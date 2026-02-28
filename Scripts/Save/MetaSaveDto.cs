using System.Collections.Generic;

public sealed class MetaSaveDto
{
	public const int CurrentVersion = 6;

	public int Version { get; set; } = CurrentVersion;
	public int CurrencyWallet { get; set; }
	public int CurrencyEarnedTotal { get; set; }
	public int CurrencySpentTotal { get; set; }
	public Dictionary<string, int> DomainShardWalletByDomain { get; set; } = new();
	public Dictionary<string, int> ConsumableWalletById { get; set; } = new();
	public Dictionary<string, int> EventChargesByEventId { get; set; } = new();

	public List<string> UnlockedCharacterIds { get; set; } = new();
	// Legacy: kept for migration compatibility with v5 and earlier saves.
	public List<string> UnlockedEventIds { get; set; } = new();
	public List<string> UnlockedHybridVariantIds { get; set; } = new();
	public Dictionary<string, CharacterProgressDto> CharacterProgressById { get; set; } = new();
	public List<string> MetaFlags { get; set; } = new();
	public List<string> SettledRunIds { get; set; } = new();
	public List<PerfectClearRecord> PerfectClearRecords { get; set; } = new();
}

public sealed class CharacterProgressDto
{
	public int Level { get; set; } = 1;
	public List<string> UnlockedAbilityNodes { get; set; } = new();
}
