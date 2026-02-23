using System;
using System.Collections.Generic;

public static class ProgressionDefs
{
	private static readonly Dictionary<string, CharacterDef> CharacterDefs = new(StringComparer.Ordinal)
	{
		["ranged"] = new CharacterDef
		{
			CharacterId = "ranged",
			IsDefaultUnlocked = true,
			UnlockCost = 70,
			MinLevel = 1,
			MaxLevel = 20,
			BaseLevelUpCost = 18,
			LevelUpGrowth = 1.20f,
			AbilityNodes = new List<AbilityNodeDef>()
		},
		["melee"] = new CharacterDef
		{
			CharacterId = "melee",
			IsDefaultUnlocked = false,
			UnlockCost = 70,
			MinLevel = 1,
			MaxLevel = 20,
			BaseLevelUpCost = 22,
			LevelUpGrowth = 1.22f,
			AbilityNodes = new List<AbilityNodeDef>()
		},
		["tank_burst"] = new CharacterDef
		{
			CharacterId = "tank_burst",
			IsDefaultUnlocked = false,
			UnlockCost = 70,
			MinLevel = 1,
			MaxLevel = 20,
			BaseLevelUpCost = 24,
			LevelUpGrowth = 1.25f,
			AbilityNodes = new List<AbilityNodeDef>()
		}
	};

	public static IEnumerable<CharacterDef> GetAllCharacters()
	{
		return CharacterDefs.Values;
	}

	public static bool TryGetCharacter(string characterId, out CharacterDef def)
	{
		if (string.IsNullOrWhiteSpace(characterId))
		{
			def = null;
			return false;
		}

		return CharacterDefs.TryGetValue(characterId, out def);
	}

	public static bool TryGetAbilityNode(string characterId, string nodeId, out AbilityNodeDef node)
	{
		node = null;
		if (!TryGetCharacter(characterId, out CharacterDef def) || def.AbilityNodes == null)
			return false;

		foreach (AbilityNodeDef item in def.AbilityNodes)
		{
			if (item == null || string.IsNullOrWhiteSpace(item.NodeId))
				continue;
			if (string.Equals(item.NodeId, nodeId, StringComparison.Ordinal))
			{
				node = item;
				return true;
			}
		}

		return false;
	}
}
