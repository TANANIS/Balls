using Godot;

public partial class SpawnSystem
{
	private struct TierRule
	{
		public int Tier;
		public float PressureMin;
		public float PressureMax;
		public float SpawnIntervalMin;
		public float SpawnIntervalMax;
		public int BudgetMin;
		public int BudgetMax;
		public int MaxAlive;
		public float SpawnRadiusMin;
		public float SpawnRadiusMax;
	}

	private struct EnemyDefinition
	{
		public string Id;
		public string ScenePath;
		public int Cost;
		public int MinTier;
		public PackedScene Scene;
	}

	private struct WeightedEnemy
	{
		public string EnemyId;
		public float Weight;
	}
}
