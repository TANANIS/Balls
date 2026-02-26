using Godot;
using System.Collections.Generic;

public partial class SpawnSystem
{
	[ExportGroup("Stale Enemy Recycle")]
	[Export] public bool EnableFarEnemyRecycle = true;
	[Export] public float FarEnemyRecycleDistance = 1250f;
	[Export] public float FarEnemyRecycleGraceSeconds = 3.2f;
	[Export] public float FarEnemyRecycleMinAgeSeconds = 5.5f;
	[Export] public float FarEnemyRecycleCheckIntervalSeconds = 0.25f;
	[Export] public int FarEnemyRecycleMaxPerTick = 12;
	[Export] public bool RecycleProtectMiniBoss = true;
	[Export] public bool RequeueSameTypeOnRecycle = true;

	private struct EnemyRecycleState
	{
		public string DefinitionId;
		public float SpawnedAtSeconds;
		public float FarSeconds;
		public bool Protected;
	}

	private readonly Dictionary<ulong, EnemyRecycleState> _enemyRecycleTrack = new();
	private readonly List<Node2D> _recycleBuffer = new();
	private readonly HashSet<ulong> _recycleAliveIds = new();
	private readonly List<ulong> _recyclePruneIds = new();
	private float _recycleCheckTimer = 0f;

	private void RegisterSpawnedEnemy(Node2D enemy, string definitionId, bool protectFromRecycle)
	{
		if (!IsInstanceValid(enemy))
			return;

		ulong id = enemy.GetInstanceId();
		_enemyRecycleTrack[id] = new EnemyRecycleState
		{
			DefinitionId = definitionId ?? string.Empty,
			SpawnedAtSeconds = _survivalSeconds,
			FarSeconds = 0f,
			Protected = protectFromRecycle
		};
	}

	private void TickFarEnemyRecycle(float dt)
	{
		if (!EnableFarEnemyRecycle)
		{
			_enemyRecycleTrack.Clear();
			_recycleCheckTimer = 0f;
			return;
		}

		float checkInterval = Mathf.Max(0.05f, FarEnemyRecycleCheckIntervalSeconds);
		_recycleCheckTimer -= dt;
		if (_recycleCheckTimer > 0f)
			return;
		_recycleCheckTimer = checkInterval;

		EnsureSpawnAnchors();
		if (_enemiesRoot == null || _player == null)
			return;

		_recycleBuffer.Clear();
		_recycleAliveIds.Clear();

		float minAge = Mathf.Max(0f, FarEnemyRecycleMinAgeSeconds);
		float grace = Mathf.Max(0.2f, FarEnemyRecycleGraceSeconds);
		float maxDistance = Mathf.Max(64f, FarEnemyRecycleDistance);
		float maxDistanceSq = maxDistance * maxDistance;
		int recycleLimit = Mathf.Max(1, FarEnemyRecycleMaxPerTick);

		foreach (Node child in _enemiesRoot.GetChildren())
		{
			if (child is not Node2D enemyNode)
				continue;

			ulong id = enemyNode.GetInstanceId();
			_recycleAliveIds.Add(id);

			if (!_enemyRecycleTrack.TryGetValue(id, out EnemyRecycleState state))
			{
				state = new EnemyRecycleState
				{
					DefinitionId = string.Empty,
					SpawnedAtSeconds = _survivalSeconds,
					FarSeconds = 0f,
					Protected = false
				};
			}

			if (RecycleProtectMiniBoss && state.Protected)
			{
				state.FarSeconds = 0f;
				_enemyRecycleTrack[id] = state;
				continue;
			}

			if (enemyNode is Enemy enemy && enemy.GetNodeOrNull<EnemyHealth>("Health") is EnemyHealth health && health.IsDead)
			{
				state.FarSeconds = 0f;
				_enemyRecycleTrack[id] = state;
				continue;
			}

			float age = _survivalSeconds - state.SpawnedAtSeconds;
			if (age < minAge)
			{
				state.FarSeconds = 0f;
				_enemyRecycleTrack[id] = state;
				continue;
			}

			if (enemyNode.GlobalPosition.DistanceSquaredTo(_player.GlobalPosition) <= maxDistanceSq)
			{
				state.FarSeconds = 0f;
				_enemyRecycleTrack[id] = state;
				continue;
			}

			state.FarSeconds += checkInterval;
			if (state.FarSeconds >= grace && _recycleBuffer.Count < recycleLimit)
			{
				_recycleBuffer.Add(enemyNode);
				state.FarSeconds = 0f;
			}

			_enemyRecycleTrack[id] = state;
		}

		PruneRecycleTracking();
		for (int i = 0; i < _recycleBuffer.Count; i++)
			TryRecycleEnemyNode(_recycleBuffer[i]);
	}

	private void PruneRecycleTracking()
	{
		_recyclePruneIds.Clear();
		foreach (var pair in _enemyRecycleTrack)
		{
			if (!_recycleAliveIds.Contains(pair.Key))
				_recyclePruneIds.Add(pair.Key);
		}

		for (int i = 0; i < _recyclePruneIds.Count; i++)
			_enemyRecycleTrack.Remove(_recyclePruneIds[i]);
	}

	private void TryRecycleEnemyNode(Node2D enemyNode)
	{
		if (!IsInstanceValid(enemyNode))
			return;

		ulong id = enemyNode.GetInstanceId();
		if (!_enemyRecycleTrack.TryGetValue(id, out EnemyRecycleState state))
			return;
		if (RecycleProtectMiniBoss && state.Protected)
			return;

		if (RequeueSameTypeOnRecycle &&
			TryResolveRecycleDefinition(state, out EnemyDefinition def) &&
			(MaxPendingSpawns <= 0 || _pendingSpawns.Count < MaxPendingSpawns))
		{
			EnqueueSpawn(def, GetSpawnPositionAroundPlayer());
		}

		_enemyRecycleTrack.Remove(id);
		enemyNode.QueueFree();
	}

	private bool TryResolveRecycleDefinition(EnemyRecycleState state, out EnemyDefinition definition)
	{
		if (!string.IsNullOrWhiteSpace(state.DefinitionId) &&
			_enemyDefinitions.TryGetValue(state.DefinitionId, out EnemyDefinition trackedDef) &&
			trackedDef.Scene != null)
		{
			definition = trackedDef;
			return true;
		}

		if (EnemyScene != null)
		{
			definition = new EnemyDefinition
			{
				Id = "fallback_enemy",
				ScenePath = string.Empty,
				Cost = 1,
				MinTier = 0,
				Scene = EnemyScene
			};
			return true;
		}

		definition = default;
		return false;
	}
}
