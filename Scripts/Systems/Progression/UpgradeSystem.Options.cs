using Godot;
using System.Collections.Generic;

public partial class UpgradeSystem
{
	// Fallback list used when catalog data is missing.
	private static readonly List<UpgradeOptionData> FallbackOptions = new();

	public bool TryPickOptions(RandomNumberGenerator rng, int count, out List<UpgradeOptionData> picks)
	{
		picks = new List<UpgradeOptionData>();
		if (count <= 0)
			return false;

		var candidates = BuildOptionPool();
		if (candidates.Count <= 0)
		{
			return false;
		}

		int pickCount = Mathf.Min(count, candidates.Count);
		for (int i = 0; i < pickCount; i++)
		{
			int idx = PickWeightedIndex(rng, candidates);
			if (idx < 0 || idx >= candidates.Count)
				return false;

			picks.Add(candidates[idx]);
			candidates.RemoveAt(idx);
		}

		UpdatePityCounters(picks);
		return picks.Count > 0;
	}

	private List<UpgradeOptionData> BuildOptionPool()
	{
		// Preferred source: authored catalog entries.
		var pool = new List<UpgradeOptionData>();
		if (_definitions.Count == 0)
			RebuildDefinitionIndex();

		if (Catalog != null && Catalog.Entries != null)
		{
			foreach (var entry in Catalog.Entries)
			{
				if (entry == null)
					continue;
				if (string.IsNullOrWhiteSpace(entry.Title))
					continue;
				if (!IsUpgradeCompatibleWithCurrentCharacter(entry.Id))
					continue;
				if (!CanApplyDefinition(entry))
					continue;

				int stack = GetStack(entry.Id);
				pool.Add(new UpgradeOptionData(
					entry.Id,
					entry.GetLocalizedTitle(),
					entry.GetLocalizedDescription(),
					entry.Category,
					entry.GetResolvedLayer(),
					entry.Rarity,
					stack,
					Mathf.Max(1, entry.MaxStack),
					entry.Icon));
			}
		}

		// Fallback source: hardcoded options for editor/runtime safety.
		if (pool.Count == 0)
		{
			foreach (var option in FallbackOptions)
			{
				if (IsUpgradeCompatibleWithCurrentCharacter(option.Id))
					pool.Add(option);
			}
		}

		if (EnablePhasePoolRouter && pool.Count > 0)
			pool = ApplyPhasePoolRouting(pool);

		return pool;
	}

	private List<UpgradeOptionData> ApplyPhasePoolRouting(List<UpgradeOptionData> source)
	{
		var routed = new List<UpgradeOptionData>(source.Count);
		UpgradePoolPhase phase = GetCurrentPoolPhase();

		for (int i = 0; i < source.Count; i++)
		{
			UpgradeOptionData option = source[i];
			float phaseWeight = GetPhaseLayerWeight(phase, option.Layer);
			UpgradeOptionData withPhaseWeight = option.WithPhasePoolWeight(phaseWeight);
			if (withPhaseWeight.PhasePoolWeight > 0f)
				routed.Add(withPhaseWeight);
		}

		if (routed.Count > 0)
			return routed;
		if (PhasePoolStrictFilter)
			return routed;

		// Safety fallback: if strict filter empties the pool, keep run stable.
		var relaxed = new List<UpgradeOptionData>(source.Count);
		for (int i = 0; i < source.Count; i++)
			relaxed.Add(source[i].WithPhasePoolWeight(1f));
		return relaxed;
	}

	private bool IsUpgradeCompatibleWithCurrentCharacter(UpgradeId id)
	{
		if (_player == null)
			return true;

		return id switch
		{
			UpgradeId.AtkSpeedUp15 => _player.PrimarySupportsRanged() || _player.PrimarySupportsMelee(),
			UpgradeId.AtkDamageUp20 => _player.PrimarySupportsRanged() || _player.PrimarySupportsMelee(),
			UpgradeId.AtkProjectilePlus1 => _player.PrimarySupportsRanged(),
			UpgradeId.AtkSplitShot => _player.PrimarySupportsRanged(),
			UpgradeId.AtkCritChanceUp10 => _player.PrimarySupportsRanged(),
			UpgradeId.ModElementalBurst => _player.PrimarySupportsRanged(),
			_ => true
		};
	}

	private bool CanApplyDefinition(UpgradeDefinition definition)
	{
		if (definition == null)
			return false;

		int maxStack = Mathf.Max(1, definition.MaxStack);
		if (GetStack(definition.Id) >= maxStack)
			return false;
		if (!IsDefinitionGateOpen(definition))
			return false;

		if (definition.Prerequisites != null)
		{
			foreach (var pre in definition.Prerequisites)
			{
				if (GetStack(pre) <= 0)
					return false;
			}
		}

		if (definition.ExclusiveWith != null)
		{
			foreach (var ex in definition.ExclusiveWith)
			{
				if (GetStack(ex) > 0)
					return false;
			}
		}

		// Runtime safety fuse: keep projectile volley and split-shot as mutually exclusive archetypes.
		if (definition.Id == UpgradeId.AtkProjectilePlus1 && GetStack(UpgradeId.AtkSplitShot) > 0)
			return false;
		if (definition.Id == UpgradeId.AtkSplitShot && GetStack(UpgradeId.AtkProjectilePlus1) > 0)
			return false;

		foreach (var pair in _definitions)
		{
			if (GetStack(pair.Key) <= 0)
				continue;

			var selectedDef = pair.Value;
			if (selectedDef?.ExclusiveWith == null)
				continue;

			foreach (var ex in selectedDef.ExclusiveWith)
			{
				if (ex == definition.Id)
					return false;
			}
		}

		return true;
	}

	private bool IsDefinitionGateOpen(UpgradeDefinition definition)
	{
		if (definition == null)
			return false;

		int requiredPickCount = Mathf.Max(0, definition.MinUpgradeCount);
		bool hasMinCountGate = requiredPickCount > 0;
		bool hasMinPhaseGate = definition.MinPhase > UpgradePoolPhase.Early;
		UpgradePoolPhase currentPhase = GetCurrentPoolPhase();

		if (hasMinCountGate || hasMinPhaseGate)
		{
			bool passAny = false;
			if (hasMinCountGate && _appliedUpgradeCount >= requiredPickCount)
				passAny = true;
			if (hasMinPhaseGate && currentPhase >= definition.MinPhase)
				passAny = true;

			if (!passAny)
				return false;
		}

		if (definition.UseMaxPhaseGate && currentPhase > definition.MaxPhase)
			return false;

		return true;
	}

	private int PickWeightedIndex(RandomNumberGenerator rng, List<UpgradeOptionData> candidates)
	{
		if (candidates == null || candidates.Count == 0)
			return -1;

		float totalWeight = 0f;
		for (int i = 0; i < candidates.Count; i++)
			totalWeight += GetEffectiveWeight(candidates[i]);

		if (totalWeight <= 0f)
			return rng.RandiRange(0, candidates.Count - 1);

		float roll = rng.RandfRange(0f, totalWeight);
		float accum = 0f;
		for (int i = 0; i < candidates.Count; i++)
		{
			accum += GetEffectiveWeight(candidates[i]);
			if (roll <= accum)
				return i;
		}

		return candidates.Count - 1;
	}

	private float GetEffectiveWeight(UpgradeOptionData option)
	{
		float phaseWeight = Mathf.Max(0f, option.PhasePoolWeight);
		if (phaseWeight <= 0f)
			return 0f;

		float rarityWeight = option.Rarity switch
		{
			UpgradeRarity.Common => 1f,
			UpgradeRarity.Rare => 0.75f,
			UpgradeRarity.Epic => 0.45f,
			_ => 1f
		};

		float categoryWeight = GetCategoryWeightFactor(option.Category);

		float pityBonus = 1f;
		if (option.Rarity == UpgradeRarity.Rare && _offersWithoutRare >= Mathf.Max(1, RarePityThreshold))
			pityBonus = 1.7f;
		else if (option.Rarity == UpgradeRarity.Epic && _offersWithoutEpic >= Mathf.Max(1, EpicPityThreshold))
			pityBonus = 2.2f;

		if (!TryGetDefinition(option.Id, out var def))
			return Mathf.Max(0.01f, phaseWeight * rarityWeight * categoryWeight * pityBonus);

		int baseWeight = Mathf.Max(1, def.Weight);
		return Mathf.Max(0.01f, baseWeight * phaseWeight * rarityWeight * categoryWeight * pityBonus);
	}

	private float GetCategoryWeightFactor(UpgradeCategory category)
	{
		_categoryPickCounts.TryGetValue(category, out int categoryPicks);
		if (!UseCategoryWeightDecay)
			return 1f + (categoryPicks * Mathf.Max(0f, CategoryBiasPerPick));

		float decayPerPick = Mathf.Clamp(CategoryWeightDecayPerPick, 0f, 1f);
		float floor = Mathf.Clamp(CategoryWeightDecayFloor, 0.01f, 1f);
		float decay = 1f - (categoryPicks * decayPerPick);
		return Mathf.Max(floor, decay);
	}

	private UpgradePoolPhase GetCurrentPoolPhase()
	{
		int applied = Mathf.Max(0, _appliedUpgradeCount);
		int progressionLevel = Mathf.Max(0, _progressionSystem?.CurrentUpgradeLevel ?? 0);
		int signal = Mathf.Max(applied, progressionLevel);

		int mid = Mathf.Max(0, MidPoolStartUpgradeCount);
		int late = Mathf.Max(mid, LatePoolStartUpgradeCount);
		if (signal >= late)
			return UpgradePoolPhase.Late;
		if (signal >= mid)
			return UpgradePoolPhase.Mid;
		return UpgradePoolPhase.Early;
	}

	private static float GetPhaseLayerWeight(UpgradePoolPhase phase, UpgradeLayer layer)
	{
		return phase switch
		{
			UpgradePoolPhase.Early => layer switch
			{
				UpgradeLayer.Survival => 0.40f,
				UpgradeLayer.CoreAttack => 0.40f,
				UpgradeLayer.Subsystem => 0.10f,
				UpgradeLayer.Modifier => 0.05f,
				UpgradeLayer.Economy => 0.05f,
				_ => 0f
			},
			UpgradePoolPhase.Mid => layer switch
			{
				UpgradeLayer.Survival => 0.20f,
				UpgradeLayer.CoreAttack => 0.40f,
				UpgradeLayer.Subsystem => 0.25f,
				UpgradeLayer.Modifier => 0.15f,
				_ => 0f
			},
			UpgradePoolPhase.Late => layer switch
			{
				UpgradeLayer.Survival => 0.10f,
				UpgradeLayer.CoreAttack => 0.30f,
				UpgradeLayer.Subsystem => 0.30f,
				UpgradeLayer.Modifier => 0.30f,
				_ => 0f
			},
			_ => 1f
		};
	}

	private void UpdatePityCounters(List<UpgradeOptionData> picks)
	{
		bool hasRare = false;
		bool hasEpic = false;

		foreach (var pick in picks)
		{
			if (pick.Rarity == UpgradeRarity.Epic)
				hasEpic = true;
			if (pick.Rarity == UpgradeRarity.Rare || pick.Rarity == UpgradeRarity.Epic)
				hasRare = true;
		}

		_offersWithoutRare = hasRare ? 0 : _offersWithoutRare + 1;
		_offersWithoutEpic = hasEpic ? 0 : _offersWithoutEpic + 1;
	}
}
