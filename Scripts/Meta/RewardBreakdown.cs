public sealed class RewardBreakdown
{
	public string RunId { get; init; } = string.Empty;
	public int InputScore { get; init; }
	public int BaseCurrency { get; init; }
	public int SoftCappedCurrency { get; init; }
	public int BonusCurrency { get; init; }
	public int FirstClearBonus { get; init; }
	public int TotalCurrency { get; init; }
	public bool IsDuplicateRun { get; init; }
}
