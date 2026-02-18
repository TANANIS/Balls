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
		_activeMaxAlive = Mathf.Max(1, MaxAliveEnemies);
		_baseSpawnIntervalMin = _activeSpawnIntervalMin;
		_baseSpawnIntervalMax = _activeSpawnIntervalMax;
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
		_activeMaxAlive = Mathf.Max(1, active.MaxAlive);
		_baseSpawnIntervalMin = _activeSpawnIntervalMin;
		_baseSpawnIntervalMax = _activeSpawnIntervalMax;
		_baseMaxAlive = _activeMaxAlive;
		_activeSpawnRadiusMin = Mathf.Max(1f, active.SpawnRadiusMin);
		_activeSpawnRadiusMax = Mathf.Max(_activeSpawnRadiusMin, active.SpawnRadiusMax);

		if (VerboseLog)
		{
			DebugSystem.Log(
				$"[SpawnSystem] Tier={active.Tier} pressure={pressure:F1} " +
				$"spawn={_activeSpawnIntervalMin:F2}-{_activeSpawnIntervalMax:F2}s " +
				$"maxAlive={_activeMaxAlive} radius={_activeSpawnRadiusMin:F0}-{_activeSpawnRadiusMax:F0} phase={GetCurrentPhase()}");
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
		float interval = baseInterval / mult;
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
		int max = Mathf.RoundToInt(_baseMaxAlive * mult);
		if (MaxAliveCap > 0)
			max = Mathf.Min(max, MaxAliveCap);
		return Mathf.Max(1, max);
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

	private void UpdateUpgradeDrivenEvents(float dt)
	{
		if (!UseUpgradeCountUnlocks)
			return;

		if (_spawnFreezeTimer > 0f)
		{
			_spawnFreezeTimer -= dt;
			if (_spawnFreezeTimer <= 0f && _miniBossScheduled && !_miniBossSpawned)
				SpawnScheduledMiniBoss();
			return;
		}

		if (_miniBossScheduled || _miniBossSpawned)
			return;

		int upgradeCount = GetUpgradeCount();
		int required = Mathf.Max(0, MiniBossUnlockUpgradeCount - GetMiniBossUnlockReductionForPhase());
		if (upgradeCount != required)
			return;

		_miniBossScheduled = true;
		_spawnFreezeTimer = Mathf.Max(0f, MiniBossFreezeSeconds);
		DebugSystem.Log($"[SpawnSystem] MiniBoss scheduled at upgrade_count={upgradeCount}, freeze={_spawnFreezeTimer:F2}s");
	}

	private void SpawnScheduledMiniBoss()
	{
		_miniBossScheduled = false;
		_miniBossSpawned = true;

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

		miniBoss.GlobalPosition = GetSpawnPositionAroundPlayer();
		_enemiesRoot.AddChild(miniBoss);
		DebugSystem.Log($"[SpawnSystem] MiniBoss spawned: {MiniBossEnemyId}");
	}

	private int GetUpgradeCount()
	{
		if (_upgradeSystem == null)
			return 0;

		return Mathf.Max(0, _upgradeSystem.AppliedUpgradeCount);
	}

	private void TryLateGameMiniBoss()
	{
		if (GetCurrentPhase() != StabilitySystem.StabilityPhase.CollapseCritical)
			return;
		if (CriticalMiniBossInterval <= 0f)
			return;
		if (_spawnFreezeTimer > 0f)
			return;
		if (_enemiesRoot == null)
			return;

		if (_nextLateMiniBossAt < 0f)
			_nextLateMiniBossAt = _survivalSeconds + CriticalMiniBossInterval;

		if (_survivalSeconds >= _nextLateMiniBossAt)
		{
			SpawnScheduledMiniBoss();
			_nextLateMiniBossAt = _survivalSeconds + CriticalMiniBossInterval;
		}
	}

	private int FindTierRuleIndex(float pressure)
	{
		for (int i = 0; i < _tierRules.Count; i++)
		{
			TierRule rule = _tierRules[i];
			if (pressure >= rule.PressureMin && pressure < rule.PressureMax)
				return i;
		}

		return _tierRules.Count > 0 ? _tierRules.Count - 1 : -1;
	}
}
