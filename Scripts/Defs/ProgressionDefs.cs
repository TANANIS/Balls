using System;
using System.Collections.Generic;

public static class ProgressionDefs
{
	private const string LegacyMeleeCharacterId = "melee";
	private const string LegacyTypoSowrdmanCharacterId = "sowrdman";
	private const string SwordsmanCharacterId = "swordsman";

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
		[SwordsmanCharacterId] = new CharacterDef
		{
			CharacterId = SwordsmanCharacterId,
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
		},
		["archer"] = new CharacterDef
		{
			CharacterId = "archer",
			IsDefaultUnlocked = false,
			UnlockCost = 70,
			MinLevel = 1,
			MaxLevel = 20,
			BaseLevelUpCost = 20,
			LevelUpGrowth = 1.21f,
			AbilityNodes = new List<AbilityNodeDef>()
		}
	};

	private static readonly Dictionary<string, EventUnlockDef> EventUnlockDefs = new(StringComparer.Ordinal)
	{
		["EVT_ICE_ICESTORM"] = new EventUnlockDef
		{
			EventId = "EVT_ICE_ICESTORM",
			DomainId = "Ice",
			DomainShardCost = 30,
			ChargeBundleAmount = 3,
			DefaultChargeCount = 3,
			IsDefaultUnlocked = true
		},
		["EVT_ICE_FROZEN_PULSE"] = new EventUnlockDef
		{
			EventId = "EVT_ICE_FROZEN_PULSE",
			DomainId = "Ice",
			DomainShardCost = 70,
			ChargeBundleAmount = 3,
			DefaultChargeCount = 0,
			IsDefaultUnlocked = false
		},
		["EVT_WAR_BLOOD_TIDE"] = new EventUnlockDef
		{
			EventId = "EVT_WAR_BLOOD_TIDE",
			DomainId = "War",
			DomainShardCost = 30,
			ChargeBundleAmount = 3,
			DefaultChargeCount = 3,
			IsDefaultUnlocked = true
		},
		["EVT_WAR_BERSERK_MARK"] = new EventUnlockDef
		{
			EventId = "EVT_WAR_BERSERK_MARK",
			DomainId = "War",
			DomainShardCost = 70,
			ChargeBundleAmount = 3,
			DefaultChargeCount = 0,
			IsDefaultUnlocked = false
		},
		["EVT_SPACE_EVENT_HORIZON"] = new EventUnlockDef
		{
			EventId = "EVT_SPACE_EVENT_HORIZON",
			DomainId = "Spacetime",
			DomainShardCost = 75,
			ChargeBundleAmount = 3,
			DefaultChargeCount = 0,
			IsDefaultUnlocked = false
		},
		["EVT_SPACE_GRAVITY_WELL"] = new EventUnlockDef
		{
			EventId = "EVT_SPACE_GRAVITY_WELL",
			DomainId = "Spacetime",
			DomainShardCost = 30,
			ChargeBundleAmount = 3,
			DefaultChargeCount = 3,
			IsDefaultUnlocked = true
		}
	};

	private static readonly Dictionary<string, HybridVariantDef> HybridVariantDefs = new(StringComparer.Ordinal)
	{
		["HYB_ICE_SPACE_GLACIAL_HORIZON"] = new HybridVariantDef
		{
			VariantId = "HYB_ICE_SPACE_GLACIAL_HORIZON",
			DomainId = "Spacetime",
			DomainShardCost = 120,
			IsDefaultUnlocked = false
		},
		["HYB_SPACE_WAR_WARP_ASSAULT"] = new HybridVariantDef
		{
			VariantId = "HYB_SPACE_WAR_WARP_ASSAULT",
			DomainId = "War",
			DomainShardCost = 120,
			IsDefaultUnlocked = false
		}
	};

	public static IEnumerable<CharacterDef> GetAllCharacters()
	{
		return CharacterDefs.Values;
	}

	public static IEnumerable<EventUnlockDef> GetAllEvents()
	{
		return EventUnlockDefs.Values;
	}

	public static IEnumerable<HybridVariantDef> GetAllHybridVariants()
	{
		return HybridVariantDefs.Values;
	}

	public static bool TryGetCharacter(string characterId, out CharacterDef def)
	{
		if (string.IsNullOrWhiteSpace(characterId))
		{
			def = null;
			return false;
		}

		string normalized = NormalizeCharacterId(characterId);
		return CharacterDefs.TryGetValue(normalized, out def);
	}

	public static string NormalizeCharacterId(string characterId)
	{
		if (string.IsNullOrWhiteSpace(characterId))
			return string.Empty;
		if (string.Equals(characterId, LegacyMeleeCharacterId, StringComparison.Ordinal))
			return SwordsmanCharacterId;
		if (string.Equals(characterId, LegacyTypoSowrdmanCharacterId, StringComparison.Ordinal))
			return SwordsmanCharacterId;
		return characterId;
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

	public static bool TryGetEvent(string eventId, out EventUnlockDef def)
	{
		if (string.IsNullOrWhiteSpace(eventId))
		{
			def = null;
			return false;
		}

		return EventUnlockDefs.TryGetValue(eventId, out def);
	}

	public static bool TryGetHybridVariant(string variantId, out HybridVariantDef def)
	{
		if (string.IsNullOrWhiteSpace(variantId))
		{
			def = null;
			return false;
		}

		return HybridVariantDefs.TryGetValue(variantId, out def);
	}

	public static string NormalizeDomainId(string domainId)
	{
		if (string.IsNullOrWhiteSpace(domainId))
			return string.Empty;
		if (string.Equals(domainId, "Ice", StringComparison.OrdinalIgnoreCase))
			return "Ice";
		if (string.Equals(domainId, "War", StringComparison.OrdinalIgnoreCase))
			return "War";
		if (string.Equals(domainId, "Spacetime", StringComparison.OrdinalIgnoreCase))
			return "Spacetime";
		return string.Empty;
	}
}
