using Godot;

[GlobalClass]
public partial class UpgradeDefinition : Resource
{
	[Export] public UpgradeId Id = UpgradeId.PrimaryDamageUp;
	[Export] public string Title = "";
	[Export(PropertyHint.MultilineText)] public string Description = "";
	[Export] public Texture2D Icon;
	[Export] public UpgradeCategory Category = UpgradeCategory.WeaponModifier;
	[Export] public UpgradeRarity Rarity = UpgradeRarity.Common;
	[Export(PropertyHint.Range, "1,100,1")] public int Weight = 10;
	[Export(PropertyHint.Range, "1,10,1")] public int MaxStack = 1;
	[Export] public Godot.Collections.Array<UpgradeId> Prerequisites = new();
	[Export] public Godot.Collections.Array<UpgradeId> ExclusiveWith = new();
}
