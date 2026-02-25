using Godot;

public partial class UpgradeSystem
{
	// Lightweight immutable DTO used by UI selection.
	public readonly struct UpgradeOptionData
	{
		public readonly UpgradeId Id;
		public readonly string Title;
		public readonly string Description;
		public readonly Texture2D Icon;
		public readonly UpgradeCategory Category;
		public readonly UpgradeLayer Layer;
		public readonly UpgradeRarity Rarity;
		public readonly int CurrentStack;
		public readonly int MaxStack;
		public readonly float PhasePoolWeight;

		public UpgradeOptionData(
			UpgradeId id,
			string title,
			string description,
			UpgradeCategory category,
			UpgradeLayer layer,
			UpgradeRarity rarity,
			int currentStack,
			int maxStack,
			Texture2D icon = null,
			float phasePoolWeight = 1f)
		{
			Id = id;
			Title = title;
			Description = description;
			Category = category;
			Layer = layer;
			Rarity = rarity;
			CurrentStack = currentStack;
			MaxStack = maxStack;
			Icon = icon;
			PhasePoolWeight = Mathf.Max(0f, phasePoolWeight);
		}

		public UpgradeOptionData WithPhasePoolWeight(float phasePoolWeight)
		{
			return new UpgradeOptionData(
				Id,
				Title,
				Description,
				Category,
				Layer,
				Rarity,
				CurrentStack,
				MaxStack,
				Icon,
				phasePoolWeight);
		}
	}
}
