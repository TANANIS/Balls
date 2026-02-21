using Godot;
using System;

public partial class SpawnSystem
{
	private void EnsurePressureSystem()
	{
		if (IsInstanceValid(_pressureSystem))
			return;

		var list = GetTree().GetNodesInGroup("PressureSystem");
		if (list.Count > 0)
			_pressureSystem = list[0] as PressureSystem;
	}

	private void EnsureUpgradeSystem()
	{
		if (IsInstanceValid(_upgradeSystem))
			return;

		var list = GetTree().GetNodesInGroup("UpgradeSystem");
		if (list.Count > 0)
			_upgradeSystem = list[0] as UpgradeSystem;
	}

	private void EnsureStabilitySystem()
	{
		if (IsInstanceValid(_stabilitySystem))
			return;

		var list = GetTree().GetNodesInGroup("StabilitySystem");
		if (list.Count > 0)
			_stabilitySystem = list[0] as StabilitySystem;
	}

	private StabilitySystem.StabilityPhase GetCurrentPhase()
	{
		return _stabilitySystem?.CurrentPhase ?? StabilitySystem.StabilityPhase.Stable;
	}

	private void ApplyFallbackRuntimeSettings()
	{
		_activeSpawnIntervalMin = Mathf.Max(0.05f, SpawnInterval);
		_activeSpawnIntervalMax = _activeSpawnIntervalMin;
		_activeBudgetMin = Mathf.Max(1, SpawnBudgetMin);
		_activeBudgetMax = Mathf.Max(_activeBudgetMin, SpawnBudgetMax);
		_activeMaxAlive = Mathf.Max(1, MaxAliveEnemies);
		_baseSpawnIntervalMin = _activeSpawnIntervalMin;
		_baseSpawnIntervalMax = _activeSpawnIntervalMax;
		_baseBudgetMin = _activeBudgetMin;
		_baseBudgetMax = _activeBudgetMax;
		_baseMaxAlive = _activeMaxAlive;
		_activeSpawnRadiusMin = Mathf.Max(1f, SpawnRadiusMin);
		_activeSpawnRadiusMax = Mathf.Max(_activeSpawnRadiusMin, SpawnRadiusMax);
	}

	private void ResetSpawnTimer()
	{
		float min = Mathf.Max(0.05f, GetPhaseSpawnInterval(_baseSpawnIntervalMin));
		float max = Mathf.Max(min, GetPhaseSpawnInterval(_baseSpawnIntervalMax));
		_timer = _rng.RandfRange(min, max);
	}

	private void UpdateTierRuntimeSettings()
	{
		if (!UseTierRulesCsv || _tierRules.Count == 0)
			return;

		float pressure = _pressureSystem?.CurrentPressure ?? 0f;
		int idx = FindTierRuleIndex(pressure);
		if (idx < 0)
			return;
		if (idx == _activeTierRuleIndex)
			return;

		_activeTierRuleIndex = idx;
		TierRule active = _tierRules[idx];
		_activeTier = active.Tier;
		_activeSpawnIntervalMin = Mathf.Max(0.05f, active.SpawnIntervalMin);
		_activeSpawnIntervalMax = Mathf.Max(_activeSpawnIntervalMin, active.SpawnIntervalMax);
		_activeBudgetMin = Mathf.Max(1, active.BudgetMin);
		_activeBudgetMax = Mathf.Max(_activeBudgetMin, active.BudgetMax);
		_activeMaxAlive = Mathf.Max(1, active.MaxAlive);
		_baseSpawnIntervalMin = _activeSpawnIntervalMin;
		_baseSpawnIntervalMax = _activeSpawnIntervalMax;
		_baseBudgetMin = _activeBudgetMin;
		_baseBudgetMax = _activeBudgetMax;
		_baseMaxAlive = _activeMaxAlive;
		_activeSpawnRadiusMin = Mathf.Max(1f, active.SpawnRadiusMin);
		_activeSpawnRadiusMax = Mathf.Max(_activeSpawnRadiusMin, active.SpawnRadiusMax);

		if (VerboseLog)
		{
			DebugSystem.Log(
				$"[SpawnSystem] Tier={active.Tier} pressure={pressure:F1} " +
				$"spawn={_activeSpawnIntervalMin:F2}-{_activeSpawnIntervalMax:F2}s " +
				$"budget={_activeBudgetMin}-{_activeBudgetMax} maxAlive={_activeMaxAlive} " +
				$"radius={_activeSpawnRadiusMin:F0}-{_activeSpawnRadiusMax:F0} phase={GetCurrentPhase()}");
		}

		ResetSpawnTimer();
	}

	private float GetPhaseSpawnRateMultiplier()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => StableSpawnRateMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalySpawnRateMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFractureSpawnRateMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalSpawnRateMultiplier,
			_ => 1f
		};
	}

	private float GetPhaseMaxAliveMultiplier()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => StableMaxAliveMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalyMaxAliveMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFractureMaxAliveMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalMaxAliveMultiplier,
			_ => 1f
		};
	}

	private float GetPhaseSpawnInterval(float baseInterval)
	{
		float mult = Mathf.Max(0.01f, GetPhaseSpawnRateMultiplier());
		float tierTailRamp = Mathf.Max(1f, GetPhaseTierTailRampMultiplier());
		float tierProgress = GetCurrentTierProgress01();
		float interval = baseInterval / mult;
		interval /= Mathf.Lerp(1f, tierTailRamp, tierProgress);
		interval *= GetOpeningSpawnIntervalMultiplier();
		if (GetCurrentPhase() == StabilitySystem.StabilityPhase.CollapseCritical)
		{
			float jitter = Mathf.Clamp(CollapseCriticalSpawnChaosJitter, 0f, 0.95f);
			float chaos = _rng.RandfRange(1f - jitter, 1f + jitter);
			interval *= chaos;
		}
		return Mathf.Max(SpawnIntervalMinClamp, interval);
	}

	private int GetPhaseMaxAlive()
	{
		float mult = Mathf.Max(0.01f, GetPhaseMaxAliveMultiplier());
		float tierTailRamp = Mathf.Max(1f, GetPhaseTierTailRampMultiplier());
		float tierProgress = GetCurrentTierProgress01();
		float openingMult = GetOpeningVolumeMultiplier(OpeningMaxAliveStartMultiplier);
		float phaseTailMult = IsInPhaseTailPrepWindow() ? Mathf.Clamp(PhaseTailMaxAliveMultiplier, 0.35f, 1f) : 1f;
		int max = Mathf.RoundToInt(_baseMaxAlive * mult * Mathf.Lerp(1f, tierTailRamp, tierProgress) * openingMult * phaseTailMult);
		if (MaxAliveCap > 0)
			max = Mathf.Min(max, MaxAliveCap);
		return Mathf.Max(1, max);
	}

	private float GetPhaseBudgetMultiplier()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => StableBudgetMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalyBudgetMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFractureBudgetMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalBudgetMultiplier,
			_ => 1f
		};
	}

	private float GetPhaseBudget(int baseBudget)
	{
		float phaseMult = Mathf.Max(0.05f, GetPhaseBudgetMultiplier());
		float tierTailRamp = Mathf.Max(1f, GetPhaseTierTailRampMultiplier());
		float tierProgress = GetCurrentTierProgress01();
		float rampMult = Mathf.Lerp(1f, tierTailRamp, tierProgress);
		float openingMult = GetOpeningVolumeMultiplier(OpeningBudgetStartMultiplier);
		float phaseTailMult = IsInPhaseTailPrepWindow() ? Mathf.Clamp(PhaseTailBudgetMultiplier, 0.35f, 1f) : 1f;
		return Mathf.Max(1f, baseBudget * phaseMult * rampMult * openingMult * phaseTailMult);
	}

	private float GetOpeningRamp01()
	{
		if (!UseOpeningRamp)
			return 1f;

		float duration = Mathf.Max(0.1f, OpeningRampSeconds);
		float t = Mathf.Clamp(_survivalSeconds / duration, 0f, 1f);
		return t * t * (3f - 2f * t); // smoothstep
	}

	private float GetOpeningSpawnIntervalMultiplier()
	{
		float start = Mathf.Max(1f, OpeningSpawnIntervalStartMultiplier);
		float ramp = GetOpeningRamp01();
		return Mathf.Lerp(start, 1f, ramp);
	}

	private float GetOpeningVolumeMultiplier(float startMultiplier)
	{
		float start = Mathf.Clamp(startMultiplier, 0.05f, 1f);
		float ramp = GetOpeningRamp01();
		return Mathf.Lerp(start, 1f, ramp);
	}

	private float GetPhaseTierTailRampMultiplier()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => StableTierTailRampMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalyTierTailRampMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFractureTierTailRampMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalTierTailRampMultiplier,
			_ => 1f
		};
	}

	private float GetCurrentTierProgress01()
	{
		if (!IsInstanceValid(_stabilitySystem))
			return GetPressureDrivenTierProgressFallback();

		float elapsed = _stabilitySystem.ElapsedSeconds;
		float stableEnd = Mathf.Max(1f, _stabilitySystem.StablePhaseEndSeconds);
		float anomalyEnd = Mathf.Max(stableEnd + 1f, _stabilitySystem.EnergyAnomalyPhaseEndSeconds);
		float fractureEnd = Mathf.Max(anomalyEnd + 1f, _stabilitySystem.StructuralFracturePhaseEndSeconds);
		float matchEnd = Mathf.Max(fractureEnd + 1f, _stabilitySystem.MatchDurationLimitSeconds);

		float phaseStart = 0f;
		float phaseEnd = stableEnd;
		switch (GetCurrentPhase())
		{
			case StabilitySystem.StabilityPhase.Stable:
				phaseStart = 0f;
				phaseEnd = stableEnd;
				break;
			case StabilitySystem.StabilityPhase.EnergyAnomaly:
				phaseStart = stableEnd;
				phaseEnd = anomalyEnd;
				break;
			case StabilitySystem.StabilityPhase.StructuralFracture:
				phaseStart = anomalyEnd;
				phaseEnd = fractureEnd;
				break;
			case StabilitySystem.StabilityPhase.CollapseCritical:
				phaseStart = fractureEnd;
				phaseEnd = matchEnd;
				break;
		}

		float duration = Mathf.Max(1f, phaseEnd - phaseStart);
		return Mathf.Clamp((elapsed - phaseStart) / duration, 0f, 1f);
	}

	private float GetPressureDrivenTierProgressFallback()
	{
		if (_activeTierRuleIndex < 0 || _activeTierRuleIndex >= _tierRules.Count)
			return 0f;

		TierRule rule = _tierRules[_activeTierRuleIndex];
		float span = rule.PressureMax - rule.PressureMin;
		if (span <= 0.01f)
			return 0f;

		float pressure = _pressureSystem?.CurrentPressure ?? rule.PressureMin;
		float normalized = (pressure - rule.PressureMin) / span;
		return Mathf.Clamp(normalized, 0f, 1f);
	}

	private int GetPhasePackCount()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => Mathf.Max(1, StablePacksPerWave),
			StabilitySystem.StabilityPhase.EnergyAnomaly => Mathf.Max(1, EnergyAnomalyPacksPerWave),
			StabilitySystem.StabilityPhase.StructuralFracture => Mathf.Max(1, StructuralFracturePacksPerWave),
			StabilitySystem.StabilityPhase.CollapseCritical => Mathf.Max(1, CollapseCriticalPacksPerWave),
			_ => 1
		};
	}

	private int GetEliteUnlockReductionForPhase()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.EnergyAnomaly => Mathf.Max(0, EnergyAnomalyEliteUnlockReduction),
			StabilitySystem.StabilityPhase.StructuralFracture => Mathf.Max(0, StructuralFractureEliteUnlockReduction),
			StabilitySystem.StabilityPhase.CollapseCritical => Mathf.Max(0, CollapseCriticalEliteUnlockReduction),
			_ => 0
		};
	}

	private int GetMiniBossUnlockReductionForPhase()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.StructuralFracture => Mathf.Max(0, StructuralFractureMiniBossUnlockReduction),
			StabilitySystem.StabilityPhase.CollapseCritical => Mathf.Max(0, CollapseCriticalMiniBossUnlockReduction),
			_ => 0
		};
	}

	private void UpdatePhaseTailMiniBossSchedule(float dt)
	{
		if (_spawnFreezeTimer > 0f)
		{
			_spawnFreezeTimer -= dt;
			if (_spawnFreezeTimer <= 0f && _pendingPhaseMiniBossIndex >= 0)
			{
				SpawnPhaseMiniBoss(_pendingPhaseMiniBossIndex);
				_pendingPhaseMiniBossIndex = -1;
			}
			return;
		}

		if (!UsePhaseTailMiniBossSchedule)
			return;

		float[] schedule =
		{
			Mathf.Max(1f, Phase1MiniBossAtSeconds),
			Mathf.Max(1f, Phase2MiniBossAtSeconds),
			Mathf.Max(1f, Phase3MiniBossAtSeconds),
			Mathf.Max(1f, Phase4MiniBossAtSeconds)
		};

		for (int i = 0; i < schedule.Length; i++)
		{
			if (_phaseMiniBossSpawned[i])
				continue;
			if (_survivalSeconds < schedule[i])
				continue;

			_phaseMiniBossSpawned[i] = true;
			_pendingPhaseMiniBossIndex = i;
			_spawnFreezeTimer = Mathf.Max(0f, PhaseMiniBossFreezeSeconds);
			DebugSystem.Log($"[SpawnSystem] Phase tail MiniBoss scheduled: stage={i + 1}, freeze={_spawnFreezeTimer:F2}s");
			return;
		}
	}

	private bool IsInPhaseTailPrepWindow()
	{
		if (!UsePhaseTailMiniBossSchedule)
			return false;
		if (_spawnFreezeTimer > 0f)
			return false;

		float prepSeconds = Mathf.Max(1f, PhaseTailPrepSeconds);
		float[] schedule =
		{
			Mathf.Max(1f, Phase1MiniBossAtSeconds),
			Mathf.Max(1f, Phase2MiniBossAtSeconds),
			Mathf.Max(1f, Phase3MiniBossAtSeconds),
			Mathf.Max(1f, Phase4MiniBossAtSeconds)
		};

		for (int i = 0; i < schedule.Length; i++)
		{
			if (_phaseMiniBossSpawned[i])
				continue;

			float until = schedule[i] - _survivalSeconds;
			if (until <= 0f)
				continue;
			return until <= prepSeconds;
		}

		return false;
	}

	private void SpawnPhaseMiniBoss(int phaseIndex)
	{
		if (!_enemyDefinitions.TryGetValue(MiniBossEnemyId, out EnemyDefinition def) || def.Scene == null)
		{
			DebugSystem.Warn($"[SpawnSystem] MiniBoss definition missing: {MiniBossEnemyId}");
			return;
		}

		if (def.Scene.Instantiate() is not Node2D miniBoss)
		{
			DebugSystem.Warn("[SpawnSystem] MiniBoss scene root is not Node2D.");
			return;
		}

		int stage = Mathf.Clamp(phaseIndex + 1, 1, 4);
		float scaleMult = Mathf.Max(0.5f, PhaseMiniBossScaleBase + ((stage - 1) * PhaseMiniBossScaleStep));
		miniBoss.Scale *= scaleMult;
		miniBoss.GlobalPosition = GetSpawnPositionAroundPlayer();
		miniBoss.Name = $"MiniBossHex_Stage{stage}";

		if (miniBoss.GetNodeOrNull<EnemyHealth>("Health") is EnemyHealth health)
		{
			int hp = Mathf.Max(1, PhaseMiniBossHpBase + ((stage - 1) * PhaseMiniBossHpStep));
			health.SetMaxHpAndRefill(hp);
		}

		if (miniBoss.GetNodeOrNull<EnemyHitbox>("Hitbox") is EnemyHitbox hitbox)
		{
			hitbox.ContactDamage = Mathf.Max(1, PhaseMiniBossContactDamageBase + ((stage - 1) * PhaseMiniBossContactDamageStep));
		}

		_enemiesRoot.AddChild(miniBoss);
		DebugSystem.Log($"[SpawnSystem] MiniBoss spawned: stage={stage}, scale={scaleMult:F2}");
	}

	private int GetUpgradeCount()
	{
		if (_upgradeSystem == null)
			return 0;

		return Mathf.Max(0, _upgradeSystem.AppliedUpgradeCount);
	}

	private int FindTierRuleIndex(float pressure)
	{
		if (IsInstanceValid(_stabilitySystem) && _tierRules.Count > 0)
		{
			int phaseTier = GetCurrentPhase() switch
			{
				StabilitySystem.StabilityPhase.Stable => 0,
				StabilitySystem.StabilityPhase.EnergyAnomaly => 1,
				StabilitySystem.StabilityPhase.StructuralFracture => 2,
				StabilitySystem.StabilityPhase.CollapseCritical => 3,
				_ => 0
			};

			for (int i = 0; i < _tierRules.Count; i++)
			{
				if (_tierRules[i].Tier == phaseTier)
					return i;
			}
		}

		for (int i = 0; i < _tierRules.Count; i++)
		{
			TierRule rule = _tierRules[i];
			if (pressure >= rule.PressureMin && pressure < rule.PressureMax)
				return i;
		}

		return _tierRules.Count > 0 ? _tierRules.Count - 1 : -1;
	}
}
