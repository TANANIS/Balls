using Godot;
using System.Collections.Generic;

public partial class UpgradeSystem
{
	// Fallback list used when catalog data is missing.
	private static readonly List<UpgradeOptionData> FallbackOptions = new()
	{
		new UpgradeOptionData(UpgradeId.PrimaryDamageUp, "主武器傷害提升", "遠距射擊傷害 +1"),
		new UpgradeOptionData(UpgradeId.PrimaryFasterFire, "主武器加速", "遠距射擊冷卻 -12%"),
		new UpgradeOptionData(UpgradeId.PrimaryProjectileSpeedUp, "主武器彈速提升", "子彈速度 +120"),
		new UpgradeOptionData(UpgradeId.SecondaryDamageUp, "近戰傷害提升", "近戰傷害 +1"),
		new UpgradeOptionData(UpgradeId.SecondaryRangeUp, "近戰範圍提升", "近戰範圍 +10"),
		new UpgradeOptionData(UpgradeId.SecondaryWiderArc, "近戰角度擴張", "近戰扇形角度 +15°"),
		new UpgradeOptionData(UpgradeId.SecondaryFaster, "近戰加速", "近戰冷卻 -12%"),
		new UpgradeOptionData(UpgradeId.DashFasterCooldown, "衝刺加速", "衝刺冷卻 -12%"),
		new UpgradeOptionData(UpgradeId.DashSpeedUp, "衝刺速度提升", "衝刺速度 +90"),
		new UpgradeOptionData(UpgradeId.DashLonger, "衝刺延長", "衝刺持續時間 +0.03 秒"),
		new UpgradeOptionData(UpgradeId.MaxHpUp, "最大生命值提升", "最大生命值 +1")
	};

	public bool TryPickTwo(RandomNumberGenerator rng, out UpgradeOptionData left, out UpgradeOptionData right)
	{
		// Build current candidate pool, then draw two distinct indices.
		var options = BuildOptionPool();
		if (options.Count < 2)
		{
			left = default;
			right = default;
			return false;
		}

		int leftIdx = rng.RandiRange(0, options.Count - 1);
		int rightIdx = rng.RandiRange(0, options.Count - 1);
		while (rightIdx == leftIdx)
			rightIdx = rng.RandiRange(0, options.Count - 1);

		left = options[leftIdx];
		right = options[rightIdx];
		return true;
	}

	private List<UpgradeOptionData> BuildOptionPool()
	{
		// Preferred source: authored catalog entries.
		var pool = new List<UpgradeOptionData>();

		if (Catalog != null && Catalog.Entries != null)
		{
			foreach (var entry in Catalog.Entries)
			{
				if (entry == null)
					continue;
				if (string.IsNullOrWhiteSpace(entry.Title))
					continue;

				pool.Add(new UpgradeOptionData(entry.Id, entry.Title, entry.Description, entry.Icon));
			}
		}

		// Fallback source: hardcoded options for editor/runtime safety.
		if (pool.Count == 0)
		{
			DebugSystem.Warn("[UpgradeSystem] Catalog missing/empty. Using fallback options.");
			pool.AddRange(FallbackOptions);
		}

		return pool;
	}
}
