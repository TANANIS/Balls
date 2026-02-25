using Godot;

[GlobalClass]
public partial class UpgradeDefinition : Resource
{
	[Export] public UpgradeId Id = UpgradeId.AtkSpeedUp15;
	[Export] public string Title = "";
	[Export(PropertyHint.MultilineText)] public string Description = "";
	[Export] public string TitleKey = "";
	[Export] public string DescriptionKey = "";
	[Export] public Texture2D Icon;
	[Export] public UpgradeCategory Category = UpgradeCategory.WeaponModifier;
	[Export] public UpgradeLayer Layer = UpgradeLayer.Auto;
	[Export] public UpgradeRarity Rarity = UpgradeRarity.Common;
	[Export(PropertyHint.Range, "1,100,1")] public int Weight = 10;
	[Export(PropertyHint.Range, "1,10,1")] public int MaxStack = 1;
	[Export(PropertyHint.Range, "0,100,1")] public int MinUpgradeCount = 0;
	[Export] public UpgradePoolPhase MinPhase = UpgradePoolPhase.Early;
	[Export] public bool UseMaxPhaseGate = false;
	[Export] public UpgradePoolPhase MaxPhase = UpgradePoolPhase.Late;
	[Export] public Godot.Collections.Array<UpgradeId> Prerequisites = new();
	[Export] public Godot.Collections.Array<UpgradeId> ExclusiveWith = new();

	public string GetLocalizedTitle()
	{
		if (!string.IsNullOrWhiteSpace(TitleKey))
		{
			string translated = Tr(TitleKey);
			if (!string.IsNullOrWhiteSpace(translated) && translated != TitleKey)
				return translated;
		}

		return Title;
	}

	public string GetLocalizedDescription()
	{
		if (!string.IsNullOrWhiteSpace(DescriptionKey))
		{
			string translated = Tr(DescriptionKey);
			if (!string.IsNullOrWhiteSpace(translated) && translated != DescriptionKey)
				return translated;
		}

		return Description;
	}

	public UpgradeLayer GetResolvedLayer()
	{
		if (Layer != UpgradeLayer.Auto)
			return Layer;

		// No implicit inference from category: unresolved layer defaults to CoreAttack.
		return UpgradeLayer.CoreAttack;
	}
}
