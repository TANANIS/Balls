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

		public UpgradeOptionData(UpgradeId id, string title, string description, Texture2D icon = null)
		{
			Id = id;
			Title = title;
			Description = description;
			Icon = icon;
		}
	}
}
