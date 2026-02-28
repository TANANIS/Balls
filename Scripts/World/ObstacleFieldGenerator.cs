using Godot;
using System.Collections.Generic;

/*
 * ObstacleFieldGenerator:
 * - Spawns obstacle scenes outside current view.
 * - Obstacle visuals/collision are owned by obstacle prefabs.
 * - Randomization is limited to obstacle type and position.
 */
public partial class ObstacleFieldGenerator : Node2D
{
	private enum TerrainAffinity
	{
		Any = 0,
		PreferGrass = 1,
		PreferDirt = 2
	}

	private enum ObstacleKind
	{
		Generic = 0,
		Tree = 1,
		Bush = 2,
		Rock = 3,
		GrassDeco = 4
	}

	[Export] public string GeneratedContainerName = "GeneratedObstacles";
	[Export] public string GeneratedDecorationContainerName = "GeneratedDecor";
	[Export] public Godot.Collections.Array<PackedScene> ObstacleScenes = new();
	[Export] public Godot.Collections.Array<PackedScene> DecorationScenes = new();
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public NodePath TerrainBackgroundPath = "../Background";

	[Export] public float SpawnIntervalSeconds = 2.8f;
	[Export] public int SpawnPerTickMin = 1;
	[Export] public int SpawnPerTickMax = 1;
	[Export(PropertyHint.Range, "1,16,1")] public int InitialBurstMultiplier = 4;
	[Export(PropertyHint.Range, "0,48,1")] public int InitialInsideViewSpawnCount = 10;
	[Export] public float InitialInsideSafeRadius = 140f;
	[Export] public float InitialInsidePadding = 24f;
	[Export] public int MaxObstacleCount = 90;
	[Export] public float SpawnOutsideMargin = 220f;
	[Export] public float SpawnRingThickness = 560f;
	[Export(PropertyHint.Range, "0.5,1.2,0.01")] public float SpawnDistanceScale = 0.72f;
	[Export] public float ObstacleSpacingMultiplier = 2.75f;
	[Export] public int PlacementAttemptsPerSpawn = 24;
	[Export] public bool EnforceAlternatingTypes = true;
	[Export] public bool UseNaturalClusters = true;
	[Export(PropertyHint.Range, "0,1,0.01")] public float ClusterStickiness = 0.72f;
	[Export] public float ClusterRadiusMin = 120f;
	[Export] public float ClusterRadiusMax = 260f;
	[Export] public int ClusterLifetimeSpawns = 7;
	[Export(PropertyHint.Range, "0,1,0.01")] public float TerrainAffinityStrength = 0.75f;
	[Export(PropertyHint.Range, "1,16,1")] public int TerrainAffinityProbeRadiusTiles = 2;
	[Export] public bool AvoidTerrainEdges = true;
	[Export(PropertyHint.Range, "1,4,1")] public int TerrainEdgeGuardTiles = 1;
	[Export(PropertyHint.Range, "6,48,1")] public int VegetationBiomeScaleTiles = 18;
	[Export(PropertyHint.Range, "0,1,0.01")] public float VegetationBiomeThreshold = 0.54f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float VegetationBiomeStrength = 0.82f;
	[Export(PropertyHint.Range, "8,64,1")] public int TreeSpeciesBiomeScaleTiles = 28;
	[Export(PropertyHint.Range, "0,1,0.01")] public float TreeSpeciesClusterStrength = 0.78f;
	[Export(PropertyHint.Range, "0,1200,1")] public int MaxDecorationCount = 380;
	[Export(PropertyHint.Range, "0,32,1")] public int InitialDecorationInsideViewCount = 20;
	[Export(PropertyHint.Range, "0,24,1")] public int DecorationPerTick = 4;
	[Export(PropertyHint.Range, "0.4,2.0,0.01")] public float DecorationSpacingMultiplier = 0.78f;
	[Export] public bool FreezeDecorationsAfterInitial = true;
	[Export(PropertyHint.Range, "1,12,1")] public int SpawnVariantRetryCount = 6;

	private readonly RandomNumberGenerator _rng = new();
	private readonly List<PlacedObstacle> _placed = new();
	private readonly List<PlacedObstacle> _placedDecor = new();
	private readonly List<ObstacleVariant> _variants = new();
	private readonly List<ObstacleVariant> _decoVariants = new();
	private Node2D _generatedRoot;
	private Node2D _generatedDecorationRoot;

	private float _spawnTimer;
	private bool _wasPaused = true;
	private StabilitySystem _stabilitySystem;
	private ProceduralTerrainBackground _terrainBackground;
	private int _lastVariantIndex = -1;
	private bool _hasClusterCenter;
	private Vector2 _clusterCenter = Vector2.Zero;
	private int _clusterRemaining;
	private readonly int _biomeSalt = 11939;
	private readonly int _speciesSalt = 17123;

	private readonly struct ObstacleVariant
	{
		public readonly PackedScene Scene;
		public readonly float Radius;
		public readonly float Weight;
		public readonly float SpacingBias;
		public readonly TerrainAffinity Affinity;
		public readonly ObstacleKind Kind;
		public readonly int SpeciesId;

		public ObstacleVariant(
			PackedScene scene,
			float radius,
			float weight,
			float spacingBias,
			TerrainAffinity affinity,
			ObstacleKind kind,
			int speciesId)
		{
			Scene = scene;
			Radius = radius;
			Weight = weight;
			SpacingBias = spacingBias;
			Affinity = affinity;
			Kind = kind;
			SpeciesId = speciesId;
		}
	}

	private readonly struct PlacedObstacle
	{
		public readonly Vector2 Position;
		public readonly float Radius;
		public readonly int VariantIndex;
		public readonly float SpacingBias;

		public PlacedObstacle(Vector2 position, float radius, int variantIndex, float spacingBias)
		{
			Position = position;
			Radius = radius;
			VariantIndex = variantIndex;
			SpacingBias = spacingBias;
		}
	}

	public override void _Ready()
	{
		_rng.Randomize();
		EnsureGeneratedRoot();
		EnsureGeneratedDecorationRoot();
		EnsureStabilitySystem();
		EnsureTerrainBackground();
		RebuildVariants();
		RebuildDecorationVariants();
		CacheExistingObstacles();
	}

	public override void _Process(double delta)
	{
		// Keep menu phase clean: no obstacle generation while paused/start UI.
		if (GetTree().Paused)
		{
			_wasPaused = true;
			return;
		}

		if (_variants.Count == 0)
			return;

		EnsureStabilitySystem();
		EnsureTerrainBackground();
		float phaseRate = _stabilitySystem?.GetObstacleSpawnMultiplier() ?? 1f;
		_spawnTimer -= (float)delta * Mathf.Max(0.05f, phaseRate);

		// Generate a small initial burst right after leaving pause/start menu.
		if (_wasPaused)
		{
			_wasPaused = false;
			// Keep startup clean: do not force an initial obstacle/deco burst.
			// Generation continues via regular interval ticks only.
			_spawnTimer = SpawnIntervalSeconds;
			return;
		}

		if (_spawnTimer > 0f)
			return;

		_spawnTimer = SpawnIntervalSeconds;
		int burst = _rng.RandiRange(Mathf.Max(1, SpawnPerTickMin), Mathf.Max(SpawnPerTickMin, SpawnPerTickMax));
		if (_stabilitySystem != null)
		{
			if (_stabilitySystem.CurrentPhase == StabilitySystem.StabilityPhase.StructuralFracture)
				burst = Mathf.Max(1, burst + 1);
			else if (_stabilitySystem.CurrentPhase == StabilitySystem.StabilityPhase.CollapseCritical)
				burst = Mathf.Max(1, burst + 2);
		}

		SpawnBatch(burst);
		if (!FreezeDecorationsAfterInitial)
			SpawnDecorationBatch(Mathf.Max(0, DecorationPerTick));
	}
}
