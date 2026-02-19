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
		public readonly UpgradeRarity Rarity;
		public readonly int CurrentStack;
		public readonly int MaxStack;

		public UpgradeOptionData(
			UpgradeId id,
			string title,
			string description,
			UpgradeCategory category,
			UpgradeRarity rarity,
			int currentStack,
			int maxStack,
			Texture2D icon = null)
		{
			Id = id;
			Title = title;
			Description = description;
			Category = category;
			Rarity = rarity;
			CurrentStack = currentStack;
			MaxStack = maxStack;
			Icon = icon;
		}
	}
}
