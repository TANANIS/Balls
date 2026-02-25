using Godot;

public partial class SpawnSystem
{
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
		_stabilitySystem = GroupServiceResolver.ResolveFirstInGroup(this, "StabilitySystem", _stabilitySystem);
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

		int idx = FindTierRuleIndex();
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

		ResetSpawnTimer();
	}

	private int GetUpgradeCount()
	{
		if (_upgradeSystem == null)
			return 0;

		return Mathf.Max(0, _upgradeSystem.AppliedUpgradeCount);
	}

	private int FindTierRuleIndex()
	{
		if (_tierRules.Count <= 0)
			return -1;

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

		// If CSV tier ids are unexpected/missing, keep runtime stable by falling back to first row.
		return 0;
	}

	public void ResetForNewRun()
	{
		_pendingSpawns.Clear();
		_spawnStepTimer = 0f;
		_timer = 0f;
		_enemyRecycleTrack.Clear();
		_recycleBuffer.Clear();
		_recycleAliveIds.Clear();
		_recyclePruneIds.Clear();
		_recycleCheckTimer = 0f;

		_miniBossScheduled = false;
		_miniBossSpawned = false;
		_pendingPhaseMiniBossIndex = -1;
		for (int i = 0; i < _phaseMiniBossSpawned.Length; i++)
			_phaseMiniBossSpawned[i] = false;
		_spawnFreezeTimer = 0f;
		_survivalSeconds = 0f;
		_nextLateMiniBossAt = -1f;

		_activeTier = -1;
		_activeTierRuleIndex = -1;
		ApplyFallbackRuntimeSettings();
		UpdateTierRuntimeSettings();
		ResetSpawnTimer();
	}
}
