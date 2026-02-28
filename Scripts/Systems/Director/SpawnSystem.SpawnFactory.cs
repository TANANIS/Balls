using Godot;
using System;
using System.Collections.Generic;

public partial class SpawnSystem
{
	private void ScheduleWave(int aliveCount, int maxAlive)
	{
		int budget = RollWaveBudget(aliveCount, maxAlive);
		int spawnSlots = Mathf.Max(0, maxAlive - aliveCount);
		if (budget <= 0 || spawnSlots <= 0)
			return;
		int packs = ResolvePackCountForBudget(budget, spawnSlots);

		int spawned = 0;
		int remainingBudget = budget;
		int remainingSlots = spawnSlots;
		List<Vector2> packCenters = BuildPackCenters(packs);
		for (int i = 0; i < packs; i++)
		{
			if (remainingBudget <= 0 || remainingSlots <= 0)
				break;

			int remainingPacks = packs - i;
			int packBudget = Mathf.Max(1, remainingBudget / remainingPacks);
			int packSlots = Mathf.Max(1, remainingSlots / remainingPacks);
			Vector2 center = i < packCenters.Count ? packCenters[i] : GetSpawnPositionAroundPlayer();
			spawned += SchedulePackedGroup(center, packBudget, packSlots, GetUpgradeCount());
			remainingBudget = Mathf.Max(0, remainingBudget - packBudget);
			remainingSlots = Mathf.Max(0, remainingSlots - packSlots);
		}
	}

	private int ResolvePackCountForBudget(int budget, int spawnSlots)
	{
		int requestedPacks = Mathf.Clamp(GetPhasePackCount(), 1, Mathf.Max(1, spawnSlots));
		int minBudgetPerPack = GetPreferredMinPackBudget();
		int maxPacksByBudget = Mathf.Max(1, budget / Mathf.Max(1, minBudgetPerPack));
		return Mathf.Clamp(requestedPacks, 1, Mathf.Max(1, Mathf.Min(spawnSlots, maxPacksByBudget)));
	}

	private int GetPreferredMinPackBudget()
	{
		if (!UseTierRulesCsv || _enemyDefinitions.Count == 0 || _tierWeights.Count == 0)
			return 1;

		List<WeightedEnemy> weights = GetWeightsForTier(_activeTier);
		if (weights == null || weights.Count == 0)
			return 1;

		int cheapestNonSlimeCost = int.MaxValue;
		foreach (var item in weights)
		{
			if (item.Weight <= 0f)
				continue;
			if (!_enemyDefinitions.TryGetValue(item.EnemyId, out EnemyDefinition def))
				continue;
			if (_activeTier < def.MinTier || def.Scene == null)
				continue;
			if (string.Equals(def.Id, "slime", StringComparison.OrdinalIgnoreCase))
				continue;

			cheapestNonSlimeCost = Mathf.Min(cheapestNonSlimeCost, Mathf.Max(1, def.Cost));
		}

		if (cheapestNonSlimeCost == int.MaxValue)
			return 1;
		return Mathf.Clamp(cheapestNonSlimeCost, 1, 8);
	}

	private int SchedulePackedGroup(Vector2 center, int budget, int spawnSlots, int upgradeCount)
	{
		if (budget <= 0 || spawnSlots <= 0)
			return 0;

		int spawned = 0;
		int remainingBudget = budget;
		var usedOffsets = new List<Vector2>(Mathf.Max(1, spawnSlots));

		for (int i = 0; i < spawnSlots && remainingBudget > 0; i++)
		{
			if (!TryPickEnemyDefinitionForCurrentTier(remainingBudget, upgradeCount, out EnemyDefinition def))
				break;

			if (!TryFindPackOffset(usedOffsets, out Vector2 offset))
				break;

			EnqueueSpawn(def, center + offset);
			spawned++;
			remainingBudget -= Mathf.Max(1, def.Cost);
			usedOffsets.Add(offset);
		}

		return spawned;
	}

	private void EnqueueSpawn(EnemyDefinition definition, Vector2 position)
	{
		if (definition.Scene == null)
			return;

		_pendingSpawns.Enqueue(new PendingSpawnRequest
		{
			Definition = definition,
			Position = position
		});
	}

	private void TrySpawnPending(float dt, int maxAlive)
	{
		if (_pendingSpawns.Count == 0)
			return;

		_spawnStepTimer -= dt;
		if (_spawnStepTimer > 0f)
			return;
		if (_enemiesRoot.GetChildCount() >= maxAlive)
			return;

		PendingSpawnRequest item = _pendingSpawns.Dequeue();
		SpawnEnemyAt(item.Definition, item.Position);
		_spawnStepTimer = GetSpawnStepInterval();
	}

	private float GetSpawnStepInterval()
	{
		float min = Mathf.Max(0.01f, SpawnStepIntervalMinSeconds);
		float max = Mathf.Max(min, SpawnStepIntervalMaxSeconds);
		float baseInterval = _rng.RandfRange(min, max);

		float phaseMult = GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => StableSpawnStepMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalySpawnStepMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFractureSpawnStepMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalSpawnStepMultiplier,
			_ => 1f
		};

		float interval = baseInterval * Mathf.Max(0.1f, phaseMult);
		interval *= GetOpeningSpawnIntervalMultiplier();
		return Mathf.Max(0.01f, interval);
	}

	private bool SpawnEnemyAt(EnemyDefinition definition, Vector2 position)
	{
		PackedScene scene = definition.Scene;
		if (scene == null)
			return false;

		if (scene.Instantiate() is not Node2D enemy)
			return false;

		enemy.GlobalPosition = position;
		ApplyEnemyOverrides(enemy, definition);
		_enemiesRoot.AddChild(enemy);
		RegisterSpawnedEnemy(enemy, definition.Id, protectFromRecycle: false);
		return true;
	}

	private static void ApplyEnemyOverrides(Node2D enemyNode, EnemyDefinition definition)
	{
		if (enemyNode is Enemy enemy && definition.SpeedOverride > 0f)
			enemy.MaxSpeed = definition.SpeedOverride;

		if (definition.HpOverride > 0 && enemyNode.GetNodeOrNull<EnemyHealth>("Health") is EnemyHealth health)
			health.SetMaxHpAndRefill(definition.HpOverride);

		if (definition.ContactDamageOverride > 0 && enemyNode.GetNodeOrNull<EnemyHitbox>("Hitbox") is EnemyHitbox hitbox)
			hitbox.ContactDamage = definition.ContactDamageOverride;
	}

	private int RollWaveBudget(int aliveCount, int maxAlive)
	{
		int minBudget = Mathf.Max(1, Mathf.RoundToInt(GetPhaseBudget(_baseBudgetMin)));
		int maxBudget = Mathf.Max(minBudget, Mathf.RoundToInt(GetPhaseBudget(_baseBudgetMax)));
		int waveBudget = _rng.RandiRange(minBudget, maxBudget);

		int targetAlive = Mathf.Clamp(Mathf.RoundToInt(maxAlive * Mathf.Clamp(HordeTargetAliveRatio, 0.2f, 1f)), 1, maxAlive);
		int deficit = Mathf.Max(0, targetAlive - aliveCount);
		if (deficit > 0)
		{
			int catchUp = Mathf.RoundToInt(deficit * Mathf.Max(0f, HordeCatchUpBudgetFactor));
			waveBudget += catchUp;
		}

		return Mathf.Max(1, waveBudget);
	}
}
