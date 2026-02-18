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
		int eliteRequired = Mathf.Max(0, EliteUnlockUpgradeCount - GetEliteUnlockReductionForPhase());
		bool eliteUnlocked = !UseUpgradeCountUnlocks || upgradeCount >= eliteRequired;
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
		if (IsChaosPhase())
			return BuildPhaseChaosWeights(tier);

		if (_tierWeights.TryGetValue(tier, out List<WeightedEnemy> weights))
			return weights;

		for (int t = tier - 1; t >= 0; t--)
		{
			if (_tierWeights.TryGetValue(t, out weights))
				return weights;
		}

		return null;
	}

	private bool IsChaosPhase()
	{
		StabilitySystem.StabilityPhase phase = GetCurrentPhase();
		return phase == StabilitySystem.StabilityPhase.StructuralFracture || phase == StabilitySystem.StabilityPhase.CollapseCritical;
	}

	private List<WeightedEnemy> BuildPhaseChaosWeights(int tier)
	{
		var list = new List<WeightedEnemy>();

		float eliteWeight = ChaosWeightElite;
		if (GetCurrentPhase() == StabilitySystem.StabilityPhase.CollapseCritical)
			eliteWeight *= 1.35f;

		TryAddWeight(list, "swarm_circle", ChaosWeightSwarm, tier);
		TryAddWeight(list, "charger_triangle", ChaosWeightCharger, tier);
		TryAddWeight(list, "tank_square", ChaosWeightTank, tier);
		TryAddWeight(list, "elite_swarm_circle", eliteWeight, tier);

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

		int required = Mathf.Max(0, EliteUnlockUpgradeCount - GetEliteUnlockReductionForPhase());
		if (upgradeCount < required)
			return picked;
		if (!_enemyDefinitions.TryGetValue(EliteEnemyId, out EnemyDefinition eliteDef) || eliteDef.Scene == null)
			return picked;

		float min = Mathf.Clamp(EliteInjectChanceMin, 0f, 1f);
		float max = Mathf.Clamp(EliteInjectChanceMax, min, 1f);
		StabilitySystem.StabilityPhase phase = GetCurrentPhase();
		if (phase == StabilitySystem.StabilityPhase.StructuralFracture)
		{
			min = Mathf.Clamp(min * StructuralFractureEliteChanceMultiplier, 0f, 1f);
			max = Mathf.Clamp(max * StructuralFractureEliteChanceMultiplier, min, 1f);
		}
		else if (phase == StabilitySystem.StabilityPhase.CollapseCritical)
		{
			min = Mathf.Clamp(min * CollapseCriticalEliteChanceMultiplier, 0f, 1f);
			max = Mathf.Clamp(max * CollapseCriticalEliteChanceMultiplier, min, 1f);
		}

		float chance = _rng.RandfRange(min, max);
		if (_rng.Randf() <= chance)
			return eliteDef.Scene;

		return picked;
	}
}
