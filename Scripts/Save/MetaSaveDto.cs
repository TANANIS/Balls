using System.Collections.Generic;

public sealed class MetaSaveDto
{
	public const int CurrentVersion = 1;

	public int Version { get; set; } = CurrentVersion;
	public int CurrencyWallet { get; set; }
	public int CurrencyEarnedTotal { get; set; }
	public int CurrencySpentTotal { get; set; }

	public List<string> UnlockedCharacterIds { get; set; } = new();
	public Dictionary<string, CharacterProgressDto> CharacterProgressById { get; set; } = new();
	public List<string> MetaFlags { get; set; } = new();
	public List<string> SettledRunIds { get; set; } = new();
}

public sealed class CharacterProgressDto
{
	public int Level { get; set; } = 1;
	public List<string> UnlockedAbilityNodes { get; set; } = new();
}
