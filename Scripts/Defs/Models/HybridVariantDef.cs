public sealed class HybridVariantDef
{
	public string VariantId { get; init; } = string.Empty;
	public string DomainId { get; init; } = string.Empty;
	public int DomainShardCost { get; init; }
	public bool IsDefaultUnlocked { get; init; }
}
