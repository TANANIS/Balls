using Godot;

[GlobalClass]
public partial class UpgradeDefinition : Resource
{
	[Export] public UpgradeId Id = UpgradeId.PrimaryDamageUp;
	[Export] public string Title = "";
	[Export(PropertyHint.MultilineText)] public string Description = "";
	[Export] public Texture2D Icon;
}
