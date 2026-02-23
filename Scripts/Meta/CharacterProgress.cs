using System.Collections.Generic;

public sealed class CharacterProgress
{
	public int Level { get; private set; } = 1;
	public HashSet<string> UnlockedAbilityNodes { get; } = new();

	public void SetLevel(int level)
	{
		Level = level < 1 ? 1 : level;
	}

	public bool UnlockAbilityNode(string nodeId)
	{
		if (string.IsNullOrWhiteSpace(nodeId))
			return false;

		return UnlockedAbilityNodes.Add(nodeId);
	}
}
