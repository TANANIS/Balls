using Godot;

public partial class SpawnSystem
{
	public override void _EnterTree()
	{
		AddToGroup(RuntimeGroups.SpawnSystem);
	}

	public override void _Ready()
	{
		_enemiesRoot = GetNodeOrNull<Node2D>(EnemiesPath);
		_player = GetNodeOrNull<Node2D>(PlayerPath);
		_rng.Randomize();

		EnsureUpgradeSystem();
		ApplyFallbackRuntimeSettings();

		if (UseTierRulesCsv)
		{
			LoadTierRulesFromCsv();
			LoadEnemyDefinitionsFromCsv();
			LoadTierWeightsFromCsv();
		}

		ResetSpawnTimer();
	}

	public override void _PhysicsProcess(double delta)
	{
		EnsureSpawnAnchors();
		if (_enemiesRoot == null || _player == null)
			return;

		_survivalSeconds += (float)delta;

		EnsureUpgradeSystem();
		EnsureStabilitySystem();
		UpdateTierRuntimeSettings();
		UpdatePhaseTailMiniBossSchedule((float)delta);
		TickFarEnemyRecycle((float)delta);

		if (_spawnFreezeTimer > 0f)
			return;

		int maxAlive = GetPhaseMaxAlive();
		TrySpawnPending((float)delta, maxAlive);
		int alive = _enemiesRoot.GetChildCount();
		int effectiveAlive = alive + _pendingSpawns.Count;
		if (effectiveAlive >= maxAlive)
			return;

		_timer -= (float)delta;
		if (_timer > 0f)
			return;

		ResetSpawnTimer();
		if (MaxPendingSpawns > 0 && _pendingSpawns.Count >= MaxPendingSpawns)
			return;

		ScheduleWave(effectiveAlive, maxAlive);
	}
}
