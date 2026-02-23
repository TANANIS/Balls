using System.Collections.Generic;

public sealed class AbilityNodeDef
{
	public string NodeId { get; init; } = string.Empty;
	public int UnlockCost { get; init; }
	public int MinCharacterLevel { get; init; } = 1;
	public List<string> PrerequisiteNodeIds { get; init; } = new();
	public string EffectKey { get; init; } = string.Empty;
}
