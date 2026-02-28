using Godot;

public partial class SpawnSystem
{
	public int EventSpawnDirectionalRush(Vector2 incomingDirection, int count, bool includeEliteBias = false)
	{
		EnsureSpawnAnchors();
		if (_enemiesRoot == null || _player == null)
			return 0;

		EnsureUpgradeSystem();
		UpdateTierRuntimeSettings();

		Vector2 dir = incomingDirection.LengthSquared() > 0.0001f ? incomingDirection.Normalized() : Vector2.Right;
		GetSpawnRadiusRange(out float radiusMin, out float radiusMax);
		float spawnRadius = Mathf.Lerp(radiusMin, radiusMax, 0.35f);
		Vector2 sideCenter = _player.GlobalPosition + (dir * spawnRadius);
		Vector2 tangent = new Vector2(-dir.Y, dir.X);

		int requested = Mathf.Clamp(count, 1, 24);
		int budgetLimit = Mathf.Max(1, _activeBudgetMax > 0 ? _activeBudgetMax : _baseBudgetMax);
		if (includeEliteBias)
			budgetLimit = Mathf.Max(1, Mathf.RoundToInt(budgetLimit * 1.2f));

		int scheduled = 0;
		int upgradeCount = GetUpgradeCount();
		for (int i = 0; i < requested; i++)
		{
			if (!TryPickEnemyDefinitionForCurrentTier(budgetLimit, upgradeCount, out EnemyDefinition def))
				break;

			float laneT = requested <= 1 ? 0f : (i / (float)(requested - 1));
			float lane = Mathf.Lerp(-1f, 1f, laneT);
			Vector2 laneOffset = tangent * lane * _rng.RandfRange(34f, 150f);
			Vector2 jitter = dir * _rng.RandfRange(-22f, 42f);
			Vector2 spawnPos = sideCenter + laneOffset + jitter;
			EnqueueSpawn(def, spawnPos);
			scheduled++;
		}

		if (includeEliteBias && _enemyDefinitions.TryGetValue(EliteEnemyId, out EnemyDefinition elite) && elite.Scene != null)
		{
			Vector2 elitePos = sideCenter + (tangent * _rng.RandfRange(-40f, 40f));
			EnqueueSpawn(elite, elitePos);
			scheduled++;
		}

		return scheduled;
	}
}
