using System;
using System.Collections.Generic;

public sealed class CharacterDef
{
	public string CharacterId { get; init; } = string.Empty;
	public bool IsDefaultUnlocked { get; init; }
	public int UnlockCost { get; init; }

	public int MinLevel { get; init; } = 1;
	public int MaxLevel { get; init; } = 20;
	public int BaseLevelUpCost { get; init; } = 20;
	public float LevelUpGrowth { get; init; } = 1.20f;

	public List<AbilityNodeDef> AbilityNodes { get; init; } = new();

	public int GetLevelUpCost(int currentLevel)
	{
		int clampedCurrent = Math.Clamp(currentLevel, MinLevel, MaxLevel);
		int nextLevelIndex = Math.Max(0, clampedCurrent - MinLevel);
		float scaled = BaseLevelUpCost * MathF.Pow(Math.Max(1f, LevelUpGrowth), nextLevelIndex);
		return Math.Max(1, (int)MathF.Round(scaled));
	}
}
