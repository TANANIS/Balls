using Godot;
using System.Collections.Generic;
using System.Text;

public partial class UpgradeSystem
{
	public string GetCategoryShareSummary()
	{
		if (_appliedUpgradeCount <= 0)
			return Tr("UI.BUILD.CATEGORY_SHARE_NONE");

		var order = new[]
		{
			UpgradeCategory.WeaponModifier,
			UpgradeCategory.PressureModifier,
			UpgradeCategory.AnomalySpecialist,
			UpgradeCategory.SpatialControl,
			UpgradeCategory.RiskAmplifier,
			UpgradeCategory.EconomyModifier
		};

		var parts = new List<string>();
		foreach (var category in order)
		{
			_categoryPickCounts.TryGetValue(category, out int count);
			if (count <= 0)
				continue;

			int ratio = (int)System.Math.Round((count * 100.0) / _appliedUpgradeCount);
			parts.Add($"{GetCategoryDisplayName(category)}:{ratio}%");
		}

		return parts.Count > 0
			? Tr("UI.BUILD.CATEGORY_SHARE_PREFIX") + " " + string.Join(" | ", parts)
			: Tr("UI.BUILD.CATEGORY_SHARE_NONE");
	}

	public string GetKeyUpgradeSummary(int maxEntries = 5)
	{
		if (_stacks.Count == 0)
			return Tr("UI.BUILD.KEY_UPGRADES_NONE");

		if (_definitions.Count == 0)
			RebuildDefinitionIndex();

		var items = new List<(UpgradeId Id, int Stack, int RarityRank)>();
		foreach (var pair in _stacks)
		{
			int rank = 0;
			if (_definitions.TryGetValue(pair.Key, out var def))
				rank = (int)def.Rarity;
			items.Add((pair.Key, pair.Value, rank));
		}

		items.Sort((a, b) =>
		{
			int stackCmp = b.Stack.CompareTo(a.Stack);
			if (stackCmp != 0)
				return stackCmp;
			int rarityCmp = b.RarityRank.CompareTo(a.RarityRank);
			if (rarityCmp != 0)
				return rarityCmp;
			return a.Id.CompareTo(b.Id);
		});

		int take = System.Math.Clamp(maxEntries, 1, items.Count);
		var sb = new StringBuilder(Tr("UI.BUILD.KEY_UPGRADES_PREFIX"));
		for (int i = 0; i < take; i++)
		{
			var item = items[i];
			string title = item.Id.ToString();
			if (_definitions.TryGetValue(item.Id, out var def))
			{
				string localized = def.GetLocalizedTitle();
				if (!string.IsNullOrWhiteSpace(localized))
					title = localized;
			}
			sb.Append("\n- ").Append(title).Append(" x").Append(item.Stack);
		}

		return sb.ToString();
	}

	private static string GetCategoryDisplayName(UpgradeCategory category)
	{
		return category switch
		{
			UpgradeCategory.WeaponModifier => TranslationServer.Translate("UI.CATEGORY.CORE_ATTACK"),
			UpgradeCategory.PressureModifier => TranslationServer.Translate("UI.CATEGORY.DIRECTOR"),
			UpgradeCategory.AnomalySpecialist => TranslationServer.Translate("UI.CATEGORY.ANOMALY"),
			UpgradeCategory.SpatialControl => TranslationServer.Translate("UI.CATEGORY.SPATIAL"),
			UpgradeCategory.RiskAmplifier => TranslationServer.Translate("UI.CATEGORY.SURVIVAL"),
			UpgradeCategory.EconomyModifier => TranslationServer.Translate("UI.CATEGORY.ECONOMY"),
			_ => category.ToString()
		};
	}
}
