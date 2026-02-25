using Godot;
using System.Collections.Generic;

public partial class UpgradeSystem
{
	private void ValidateCatalogIntegrity()
	{
		if (Catalog == null || Catalog.Entries == null)
			return;

		var seenIds = new HashSet<UpgradeId>();
		var layerCounts = new Dictionary<UpgradeLayer, int>();

		foreach (UpgradeDefinition entry in Catalog.Entries)
		{
			if (entry == null)
			{
				GD.PushWarning("[UpgradeSystem] Null entry found in catalog.");
				continue;
			}

			if (!seenIds.Add(entry.Id))
				GD.PushWarning($"[UpgradeSystem] Duplicate card id in catalog: {entry.Id}.");

			if (entry.MaxStack < 1)
				GD.PushWarning($"[UpgradeSystem] Invalid MaxStack on {entry.Id}.");
			if (entry.Layer == UpgradeLayer.Auto)
				GD.PushWarning($"[UpgradeSystem] {entry.Id} uses Layer=Auto. Explicit layer is recommended.");

			if (string.IsNullOrWhiteSpace(entry.Title) && string.IsNullOrWhiteSpace(entry.TitleKey))
				GD.PushWarning($"[UpgradeSystem] Missing title/title key on {entry.Id}.");
			if (string.IsNullOrWhiteSpace(entry.Description) && string.IsNullOrWhiteSpace(entry.DescriptionKey))
				GD.PushWarning($"[UpgradeSystem] Missing description/description key on {entry.Id}.");

			if (entry.UseMaxPhaseGate && entry.MaxPhase < entry.MinPhase)
				GD.PushWarning($"[UpgradeSystem] {entry.Id} has MaxPhase earlier than MinPhase.");

			if (entry.Prerequisites != null)
			{
				foreach (UpgradeId pre in entry.Prerequisites)
				{
					if (pre == entry.Id)
						GD.PushWarning($"[UpgradeSystem] {entry.Id} has self prerequisite.");
				}
			}

			if (entry.ExclusiveWith != null)
			{
				foreach (UpgradeId ex in entry.ExclusiveWith)
				{
					if (ex == entry.Id)
						GD.PushWarning($"[UpgradeSystem] {entry.Id} has self exclusive-with.");
				}
			}

			UpgradeLayer layer = entry.GetResolvedLayer();
			layerCounts.TryGetValue(layer, out int count);
			layerCounts[layer] = count + 1;
		}

		foreach (UpgradeDefinition entry in Catalog.Entries)
		{
			if (entry == null)
				continue;

			if (entry.Prerequisites != null)
			{
				foreach (UpgradeId pre in entry.Prerequisites)
				{
					if (!seenIds.Contains(pre))
						GD.PushWarning($"[UpgradeSystem] {entry.Id} prerequisite missing in catalog: {pre}.");
				}
			}

			if (entry.ExclusiveWith != null)
			{
				foreach (UpgradeId ex in entry.ExclusiveWith)
				{
					if (!seenIds.Contains(ex))
						GD.PushWarning($"[UpgradeSystem] {entry.Id} exclusive-with missing in catalog: {ex}.");
				}
			}
		}

		ValidatePhasePoolCoverage(layerCounts);
	}

	private void ValidatePhasePoolCoverage(Dictionary<UpgradeLayer, int> layerCounts)
	{
		if (layerCounts == null || layerCounts.Count == 0)
			return;

		UpgradePoolPhase[] phases =
		{
			UpgradePoolPhase.Early,
			UpgradePoolPhase.Mid,
			UpgradePoolPhase.Late
		};

		foreach (UpgradePoolPhase phase in phases)
		{
			bool hasEligibleLayer = false;
			foreach (var pair in layerCounts)
			{
				if (pair.Value <= 0)
					continue;
				if (GetPhaseLayerWeight(phase, pair.Key) > 0f)
				{
					hasEligibleLayer = true;
					break;
				}
			}

			if (!hasEligibleLayer)
				GD.PushWarning($"[UpgradeSystem] Phase pool {phase} has no eligible layers in current catalog.");
		}
	}
}
