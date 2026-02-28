public sealed class EventUnlockDef
{
	public string EventId { get; init; } = string.Empty;
	public string DomainId { get; init; } = string.Empty;
	public int DomainShardCost { get; init; }
	public int ChargeBundleAmount { get; init; } = 3;
	public int DefaultChargeCount { get; init; }
	public bool IsDefaultUnlocked { get; init; }
}
