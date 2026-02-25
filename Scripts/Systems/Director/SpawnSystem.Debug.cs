using Godot;
using System;
using System.Collections.Generic;

public partial class SpawnSystem
{
	public string[] DebugGetEnemyIds()
	{
		if (UseTierRulesCsv && _enemyDefinitions.Count == 0)
			LoadEnemyDefinitionsFromCsv();

		var ids = new List<string>(_enemyDefinitions.Keys);
		ids.Sort(StringComparer.OrdinalIgnoreCase);
		return ids.ToArray();
	}

	public bool DebugSpawnEnemyById(string enemyId, int count = 1)
	{
		if (string.IsNullOrWhiteSpace(enemyId))
			return false;

		EnsureSpawnAnchors();
		if (_enemiesRoot == null || _player == null)
			return false;

		if (UseTierRulesCsv && _enemyDefinitions.Count == 0)
			LoadEnemyDefinitionsFromCsv();

		if (!TryResolveEnemyDefinition(enemyId, out EnemyDefinition definition))
			return false;

		count = Mathf.Clamp(count, 1, 64);
		int spawned = 0;
		for (int i = 0; i < count; i++)
		{
			Vector2 pos = GetDebugSpawnPositionNearPlayer(42f, 260f);
			if (SpawnEnemyAt(definition, pos))
				spawned++;
		}

		return spawned > 0;
	}

	private void EnsureSpawnAnchors()
	{
		if (!IsInstanceValid(_enemiesRoot))
			_enemiesRoot = GetNodeOrNull<Node2D>(EnemiesPath);
		if (!IsInstanceValid(_player))
			_player = GetNodeOrNull<Node2D>(PlayerPath);
	}

	private bool TryResolveEnemyDefinition(string enemyId, out EnemyDefinition definition)
	{
		if (_enemyDefinitions.TryGetValue(enemyId, out definition))
			return true;

		foreach (var pair in _enemyDefinitions)
		{
			if (string.Equals(pair.Key, enemyId, StringComparison.OrdinalIgnoreCase))
			{
				definition = pair.Value;
				return true;
			}
		}

		definition = default;
		return false;
	}

	private Vector2 GetDebugSpawnPositionNearPlayer(float minRadius, float maxRadius)
	{
		float radius = _rng.RandfRange(Mathf.Max(0f, minRadius), Mathf.Max(minRadius + 1f, maxRadius));
		float angle = _rng.RandfRange(0f, Mathf.Tau);
		Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		return _player.GlobalPosition + (dir * radius);
	}
}
