using Godot;

[GlobalClass]
public partial class UpgradeCatalog : Resource
{
	[Export] public Godot.Collections.Array<UpgradeDefinition> Entries = new();
}
