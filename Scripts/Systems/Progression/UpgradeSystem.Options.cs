using Godot;
using System.Collections.Generic;

public partial class UpgradeSystem
{
	// Fallback list used when catalog data is missing.
	private static readonly List<UpgradeOptionData> FallbackOptions = new()
	{
		new UpgradeOptionData(UpgradeId.PrimaryDamageUp, "主武器傷害提升", "遠距射擊傷害 +1", UpgradeCategory.WeaponModifier, UpgradeRarity.Common, 0, 4),
		new UpgradeOptionData(UpgradeId.PrimaryFasterFire, "主武器加速", "遠距射擊冷卻 -12%", UpgradeCategory.WeaponModifier, UpgradeRarity.Common, 0, 4),
		new UpgradeOptionData(UpgradeId.SecondaryDamageUp, "近戰傷害提升", "近戰傷害 +1", UpgradeCategory.WeaponModifier, UpgradeRarity.Common, 0, 4),
		new UpgradeOptionData(UpgradeId.PressureKillProgressUp, "壓力收斂演算", "擊殺升級進度 +18%", UpgradeCategory.PressureModifier, UpgradeRarity.Common, 0, 3),
		new UpgradeOptionData(UpgradeId.StabilityDecayDown, "宇宙穩定調諧", "穩定度衰減 -10%", UpgradeCategory.AnomalySpecialist, UpgradeRarity.Rare, 0, 2),
		new UpgradeOptionData(UpgradeId.DashFasterCooldown, "衝刺加速", "衝刺冷卻 -12%", UpgradeCategory.SpatialControl, UpgradeRarity.Common, 0, 4),
		new UpgradeOptionData(UpgradeId.MaxHpUp, "最大生命值提升", "最大生命值 +1", UpgradeCategory.RiskAmplifier, UpgradeRarity.Common, 0, 3)
	};

	public bool TryPickOptions(RandomNumberGenerator rng, int count, out List<UpgradeOptionData> picks)
	{
		picks = new List<UpgradeOptionData>();
		if (count <= 0)
			return false;

		var candidates = BuildOptionPool();
		if (candidates.Count < count)
		{
			return false;
		}

		for (int i = 0; i < count; i++)
		{
			int idx = PickWeightedIndex(rng, candidates);
			if (idx < 0 || idx >= candidates.Count)
				return false;

			picks.Add(candidates[idx]);
			candidates.RemoveAt(idx);
		}

		UpdatePityCounters(picks);
		return true;
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
					entry.Title,
					entry.Description,
					entry.Category,
					entry.Rarity,
					stack,
					Mathf.Max(1, entry.MaxStack),
					entry.Icon));
			}
		}

		// Fallback source: hardcoded options for editor/runtime safety.
		if (pool.Count == 0)
		{
			DebugSystem.Warn("[UpgradeSystem] Catalog missing/empty. Using fallback options.");
			foreach (var option in FallbackOptions)
			{
				if (IsUpgradeCompatibleWithCurrentCharacter(option.Id))
					pool.Add(option);
			}
		}

		return pool;
	}

	private bool IsUpgradeCompatibleWithCurrentCharacter(UpgradeId id)
	{
		if (_player == null)
			return true;

		return id switch
		{
			UpgradeId.PrimaryDamageUp => _player.PrimarySupportsRanged() || _player.PrimarySupportsMelee(),
			UpgradeId.PrimaryFasterFire => _player.PrimarySupportsRanged() || _player.PrimarySupportsMelee(),
			UpgradeId.PrimaryProjectileSpeedUp => _player.PrimarySupportsRanged(),
			UpgradeId.SecondaryDamageUp => _player.SecondarySupportsRanged() || _player.SecondarySupportsMelee(),
			UpgradeId.SecondaryRangeUp => _player.SecondarySupportsMelee(),
			UpgradeId.SecondaryWiderArc => _player.SecondarySupportsMelee(),
			UpgradeId.DashFasterCooldown => _player.HasDashAbility(),
			UpgradeId.DashSpeedUp => _player.HasDashAbility(),
			UpgradeId.DashLonger => _player.HasDashAbility(),
			UpgradeId.DashIFrameUp => _player.HasDashAbility(),
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
		float rarityWeight = option.Rarity switch
		{
			UpgradeRarity.Common => 1f,
			UpgradeRarity.Rare => 0.75f,
			UpgradeRarity.Epic => 0.45f,
			_ => 1f
		};

		int categoryPicks = 0;
		_categoryPickCounts.TryGetValue(option.Category, out categoryPicks);
		float categoryBias = 1f + (categoryPicks * Mathf.Max(0f, CategoryBiasPerPick));

		float pityBonus = 1f;
		if (option.Rarity == UpgradeRarity.Rare && _offersWithoutRare >= Mathf.Max(1, RarePityThreshold))
			pityBonus = 1.7f;
		else if (option.Rarity == UpgradeRarity.Epic && _offersWithoutEpic >= Mathf.Max(1, EpicPityThreshold))
			pityBonus = 2.2f;

		if (!TryGetDefinition(option.Id, out var def))
			return Mathf.Max(0.01f, rarityWeight * categoryBias * pityBonus);

		int baseWeight = Mathf.Max(1, def.Weight);
		return baseWeight * rarityWeight * categoryBias * pityBonus;
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
