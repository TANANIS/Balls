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
		float min = Mathf.Max(0.05f, GetLateGameSpawnInterval(_baseSpawnIntervalMin));
		float max = Mathf.Max(min, GetLateGameSpawnInterval(_baseSpawnIntervalMax));
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
		_activeTier = Mathf.Max(active.Tier, GetTimeForcedTier());
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
				$"maxAlive={_activeMaxAlive} radius={_activeSpawnRadiusMin:F0}-{_activeSpawnRadiusMax:F0}");
		}

		ResetSpawnTimer();
	}

	private int GetTimeForcedTier()
	{
		if (_survivalSeconds >= 240f) return 4;
		if (_survivalSeconds >= 180f) return 4;
		if (_survivalSeconds >= 120f) return 3;
		if (_survivalSeconds >= 60f) return 2;
		return -1;
	}

	private bool IsLateGame()
	{
		return LateGameStartSeconds > 0f && _survivalSeconds >= LateGameStartSeconds;
	}

	private float GetLateGameMultiplier()
	{
		if (!IsLateGame())
			return 1f;

		float minutesSince = Mathf.Max(0f, (_survivalSeconds - LateGameStartSeconds) / 60f);
		int steps = Mathf.FloorToInt(minutesSince) + 1;
		float mult = Mathf.Pow(2f, steps);

		if (LateGameSecondRampStartSeconds > 0f && _survivalSeconds >= LateGameSecondRampStartSeconds)
		{
			float extra = Mathf.Max(0f, _survivalSeconds - LateGameSecondRampStartSeconds);
			int extraSteps = Mathf.FloorToInt(extra / Mathf.Max(1f, LateGameSecondRampStepSeconds));
			if (extraSteps > 0)
				mult *= Mathf.Pow(2f, extraSteps);
		}

		return mult;
	}

	private float GetLateGameSpawnInterval(float baseInterval)
	{
		float mult = GetLateGameMultiplier();
		float interval = baseInterval / mult;
		return Mathf.Max(LateGameSpawnIntervalMinClamp, interval);
	}

	private int GetLateGameMaxAlive()
	{
		float mult = GetLateGameMultiplier();
		int max = Mathf.RoundToInt(_baseMaxAlive * mult);
		if (LateGameMaxAliveCap > 0)
			max = Mathf.Min(max, LateGameMaxAliveCap);
		return Mathf.Max(1, max);
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
		int required = MiniBossUnlockUpgradeCount;
		if (IsLateGame())
			required = Mathf.Max(0, MiniBossUnlockUpgradeCount - LateGameMiniBossUnlockReduction);
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
		if (!IsLateGame())
			return;
		if (LateGameMiniBossInterval <= 0f)
			return;
		if (_spawnFreezeTimer > 0f)
			return;
		if (_enemiesRoot == null)
			return;

		if (_nextLateMiniBossAt < 0f)
			_nextLateMiniBossAt = LateGameStartSeconds + LateGameMiniBossInterval;

		if (_survivalSeconds >= _nextLateMiniBossAt)
		{
			SpawnScheduledMiniBoss();
			_nextLateMiniBossAt = _survivalSeconds + LateGameMiniBossInterval;
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
