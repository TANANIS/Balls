using Godot;
using System;
using System.Collections.Generic;

public partial class SpawnSystem
{
	private PackedScene PickEnemySceneForCurrentTier()
	{
		if (!UseTierRulesCsv || _enemyDefinitions.Count == 0 || _tierWeights.Count == 0)
			return EnemyScene;

		List<WeightedEnemy> weights = GetWeightsForTier(_activeTier);
		if (weights == null || weights.Count == 0)
			return EnemyScene;

		float total = 0f;
		int upgradeCount = GetUpgradeCount();
		bool eliteUnlocked = !UseUpgradeCountUnlocks || upgradeCount >= EliteUnlockUpgradeCount;
		foreach (var item in weights)
		{
			if (item.Weight <= 0f)
				continue;

			if (!_enemyDefinitions.TryGetValue(item.EnemyId, out EnemyDefinition def))
				continue;

			if (_activeTier < def.MinTier || def.Scene == null)
				continue;
			if (!eliteUnlocked && string.Equals(item.EnemyId, EliteEnemyId, StringComparison.OrdinalIgnoreCase))
				continue;

			total += item.Weight;
		}

		if (total <= 0f)
			return EnemyScene;

		float roll = _rng.RandfRange(0f, total);
		float accumulated = 0f;

		foreach (var item in weights)
		{
			if (item.Weight <= 0f)
				continue;

			if (!_enemyDefinitions.TryGetValue(item.EnemyId, out EnemyDefinition def))
				continue;

			if (_activeTier < def.MinTier || def.Scene == null)
				continue;
			if (!eliteUnlocked && string.Equals(item.EnemyId, EliteEnemyId, StringComparison.OrdinalIgnoreCase))
				continue;

			accumulated += item.Weight;
			if (roll <= accumulated)
			{
				PackedScene picked = def.Scene;
				return TryInjectElite(picked, upgradeCount);
			}
		}

		return TryInjectElite(EnemyScene, upgradeCount);
	}

	private List<WeightedEnemy> GetWeightsForTier(int tier)
	{
		if (IsLateGame())
			return BuildLateGameWeights(tier);

		if (_tierWeights.TryGetValue(tier, out List<WeightedEnemy> weights))
			return weights;

		for (int t = tier - 1; t >= 0; t--)
		{
			if (_tierWeights.TryGetValue(t, out weights))
				return weights;
		}

		return null;
	}

	private List<WeightedEnemy> BuildLateGameWeights(int tier)
	{
		var list = new List<WeightedEnemy>();

		TryAddWeight(list, "swarm_circle", LateGameWeightSwarm, tier);
		TryAddWeight(list, "charger_triangle", LateGameWeightCharger, tier);
		TryAddWeight(list, "tank_square", LateGameWeightTank, tier);
		TryAddWeight(list, "elite_swarm_circle", LateGameWeightElite, tier);

		return list;
	}

	private void TryAddWeight(List<WeightedEnemy> list, string enemyId, float weight, int tier)
	{
		if (weight <= 0f)
			return;
		if (!_enemyDefinitions.TryGetValue(enemyId, out EnemyDefinition def))
			return;
		if (def.Scene == null || tier < def.MinTier)
			return;

		list.Add(new WeightedEnemy { EnemyId = enemyId, Weight = weight });
	}

	private PackedScene TryInjectElite(PackedScene picked, int upgradeCount)
	{
		if (!UseUpgradeCountUnlocks)
			return picked;
		int required = EliteUnlockUpgradeCount;
		if (IsLateGame())
			required = Mathf.Max(0, EliteUnlockUpgradeCount - LateGameEliteUnlockReduction);
		if (upgradeCount < required)
			return picked;
		if (!_enemyDefinitions.TryGetValue(EliteEnemyId, out EnemyDefinition eliteDef) || eliteDef.Scene == null)
			return picked;

		float min = Mathf.Clamp(EliteInjectChanceMin, 0f, 1f);
		float max = Mathf.Clamp(EliteInjectChanceMax, min, 1f);
		if (IsLateGame())
		{
			min = Mathf.Clamp(min * LateGameEliteChanceMultiplier, 0f, 1f);
			max = Mathf.Clamp(max * LateGameEliteChanceMultiplier, min, 1f);
		}
		float chance = _rng.RandfRange(min, max);
		if (_rng.Randf() <= chance)
			return eliteDef.Scene;

		return picked;
	}
}
