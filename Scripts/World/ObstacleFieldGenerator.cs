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
	private bool _hasClusterCenter = false;
	private Vector2 _clusterCenter = Vector2.Zero;
	private int _clusterRemaining = 0;
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

	private void SpawnBatch(int count)
	{
		if (_variants.Count == 0)
			return;
		if (MaxObstacleCount > 0 && _placed.Count >= MaxObstacleCount)
			return;

		Node2D player = GetNodeOrNull<Node2D>(PlayerPath);
		Vector2 playerPos = player != null ? player.GlobalPosition : Vector2.Zero;
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		Camera2D camera = GetViewport().GetCamera2D();
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		Vector2 halfVisible = new Vector2(viewport.X * 0.5f * zoom.X, viewport.Y * 0.5f * zoom.Y);
		float distanceScale = Mathf.Clamp(SpawnDistanceScale, 0.5f, 1.2f);
		float minDist = (Mathf.Max(halfVisible.X, halfVisible.Y) * distanceScale) + Mathf.Max(0f, SpawnOutsideMargin);
		float maxDist = minDist + Mathf.Max(60f, SpawnRingThickness);
		PrepareCluster(playerPos, minDist, maxDist);

		for (int i = 0; i < count; i++)
		{
			if (MaxObstacleCount > 0 && _placed.Count >= MaxObstacleCount)
				break;
			bool spawned = false;
			int retries = Mathf.Clamp(SpawnVariantRetryCount, 1, 12);
			for (int attempt = 0; attempt < retries; attempt++)
			{
				int variantIndex = PickVariantIndex();
				ObstacleVariant variant = _variants[variantIndex];
				if (!TryFindPlacement(playerPos, minDist, maxDist, variant.Radius, variant.SpacingBias, out Vector2 pos))
					continue;
				if (!PassesTerrainAffinity(variant, pos))
					continue;
				if (!PassesEnvironmentRule(variant, pos, variant.Radius))
					continue;

				if (CreateObstacle(variant.Scene, pos))
				{
					_placed.Add(new PlacedObstacle(pos, variant.Radius, variantIndex, variant.SpacingBias));
					_lastVariantIndex = variantIndex;
					if (_clusterRemaining > 0)
						_clusterRemaining--;
					spawned = true;
					break;
				}
			}
			if (!spawned)
				continue;
		}
	}

	private int PickVariantIndex()
	{
		if (_variants.Count <= 1)
			return 0;

		float total = 0f;
		var weights = new float[_variants.Count];
		for (int i = 0; i < _variants.Count; i++)
		{
			float w = Mathf.Max(0.01f, _variants[i].Weight);
			// Soften streaks, but keep natural randomness.
			if (EnforceAlternatingTypes && _lastVariantIndex == i)
				w *= 0.45f;
			weights[i] = w;
			total += w;
		}

		float roll = _rng.RandfRange(0f, total);
		for (int i = 0; i < weights.Length; i++)
		{
			roll -= weights[i];
			if (roll <= 0f)
				return i;
		}

		return weights.Length - 1;
	}

	private bool TryFindPlacement(Vector2 playerPos, float minDist, float maxDist, float radius, float spacingBias, out Vector2 pos)
	{
		float effectiveRadius = Mathf.Max(24f, radius);
		for (int attempt = 0; attempt < PlacementAttemptsPerSpawn; attempt++)
		{
			if (TrySamplePosition(playerPos, minDist, maxDist, out pos) &&
				IsPlacementValid(pos, effectiveRadius, spacingBias))
				return true;
		}

		pos = Vector2.Zero;
		return false;
	}

	private void PrepareCluster(Vector2 playerPos, float minDist, float maxDist)
	{
		if (!UseNaturalClusters)
		{
			_hasClusterCenter = false;
			_clusterRemaining = 0;
			return;
		}

		if (_hasClusterCenter && _clusterRemaining > 0)
			return;

		float angle = _rng.RandfRange(0f, Mathf.Tau);
		float dist = _rng.RandfRange(minDist, maxDist);
		Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
		_clusterCenter = playerPos + (dir * dist);
		_clusterRemaining = Mathf.Max(1, ClusterLifetimeSpawns);
		_hasClusterCenter = true;
	}

	private bool TrySamplePosition(Vector2 playerPos, float minDist, float maxDist, out Vector2 pos)
	{
		if (UseNaturalClusters && _hasClusterCenter && _clusterRemaining > 0 && _rng.Randf() <= Mathf.Clamp(ClusterStickiness, 0f, 1f))
		{
			float radius = _rng.RandfRange(Mathf.Max(8f, ClusterRadiusMin), Mathf.Max(ClusterRadiusMin, ClusterRadiusMax));
			float angle = _rng.RandfRange(0f, Mathf.Tau);
			float r = Mathf.Sqrt(_rng.Randf()) * radius;
			pos = _clusterCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
			float toPlayer = pos.DistanceTo(playerPos);
			if (toPlayer >= minDist && toPlayer <= maxDist)
				return true;
		}

		float ringAngle = _rng.RandfRange(0f, Mathf.Tau);
		float ringDist = _rng.RandfRange(minDist, maxDist);
		Vector2 ringDir = new Vector2(Mathf.Cos(ringAngle), Mathf.Sin(ringAngle));
		pos = playerPos + (ringDir * ringDist);
		return true;
	}

	private bool IsPlacementValid(Vector2 pos, float radius, float spacingBias)
	{
		foreach (PlacedObstacle existing in _placed)
		{
			float pairBias = (spacingBias + existing.SpacingBias) * 0.5f;
			float spacing = (radius + existing.Radius) * Mathf.Max(0.8f, ObstacleSpacingMultiplier * pairBias);
			if (pos.DistanceTo(existing.Position) < spacing)
				return false;
		}

		return true;
	}

	private bool IsDecorationPlacementValid(Vector2 pos, float radius, float spacingBias)
	{
		foreach (PlacedObstacle existing in _placed)
		{
			float pairBias = (spacingBias + existing.SpacingBias) * 0.5f;
			float spacing = (radius + existing.Radius) * Mathf.Max(0.45f, DecorationSpacingMultiplier * pairBias);
			if (pos.DistanceTo(existing.Position) < spacing)
				return false;
		}

		foreach (PlacedObstacle existing in _placedDecor)
		{
			float pairBias = (spacingBias + existing.SpacingBias) * 0.5f;
			float spacing = (radius + existing.Radius) * Mathf.Max(0.35f, DecorationSpacingMultiplier * pairBias);
			if (pos.DistanceTo(existing.Position) < spacing)
				return false;
		}

		return true;
	}

	private void SpawnInitialInsideView()
	{
		int target = Mathf.Max(0, InitialInsideViewSpawnCount);
		if (target == 0 || _variants.Count == 0)
			return;

		Node2D player = GetNodeOrNull<Node2D>(PlayerPath);
		Vector2 playerPos = player != null ? player.GlobalPosition : Vector2.Zero;
		Camera2D camera = GetViewport().GetCamera2D();
		Vector2 center = camera != null ? camera.GetScreenCenterPosition() : playerPos;
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		Vector2 halfVisible = new Vector2(viewport.X * 0.5f * zoom.X, viewport.Y * 0.5f * zoom.Y);
		float pad = Mathf.Max(0f, InitialInsidePadding);
		Rect2 rect = new(
			center - halfVisible + new Vector2(pad, pad),
			(halfVisible * 2f) - new Vector2(pad * 2f, pad * 2f));
		if (rect.Size.X <= 0f || rect.Size.Y <= 0f)
			return;

		float safeRadius = Mathf.Max(0f, InitialInsideSafeRadius);
		int attempts = Mathf.Max(PlacementAttemptsPerSpawn * 2, 32);
		int spawned = 0;
		for (int i = 0; i < target; i++)
		{
			if (MaxObstacleCount > 0 && _placed.Count >= MaxObstacleCount)
				break;
			int retries = Mathf.Clamp(SpawnVariantRetryCount, 1, 12);
			for (int attempt = 0; attempt < retries; attempt++)
			{
				int variantIndex = PickVariantIndex();
				ObstacleVariant variant = _variants[variantIndex];
				if (!TryFindPlacementInRect(rect, playerPos, safeRadius, variant.Radius, variant.SpacingBias, attempts, out Vector2 pos))
					continue;
				if (!PassesTerrainAffinity(variant, pos))
					continue;
				if (!PassesEnvironmentRule(variant, pos, variant.Radius))
					continue;

				if (!CreateObstacle(variant.Scene, pos))
					continue;
				_placed.Add(new PlacedObstacle(pos, variant.Radius, variantIndex, variant.SpacingBias));
				_lastVariantIndex = variantIndex;
				spawned++;
				break;
			}
		}
	}

	private bool TryFindPlacementInRect(
		Rect2 rect,
		Vector2 playerPos,
		float safeRadius,
		float radius,
		float spacingBias,
		int attempts,
		out Vector2 pos)
	{
		float effectiveRadius = Mathf.Max(24f, radius);
		for (int i = 0; i < attempts; i++)
		{
			pos = new Vector2(
				_rng.RandfRange(rect.Position.X, rect.End.X),
				_rng.RandfRange(rect.Position.Y, rect.End.Y));
			if (pos.DistanceTo(playerPos) < safeRadius)
				continue;
			if (IsPlacementValid(pos, effectiveRadius, spacingBias))
				return true;
		}

		pos = Vector2.Zero;
		return false;
	}

	private void SpawnInitialDecorationsInsideView()
	{
		SpawnDecorationInView(Mathf.Max(0, InitialDecorationInsideViewCount));
	}

	private void SpawnDecorationBatch(int count)
	{
		if (count <= 0)
			return;
		SpawnDecorationInView(count);
	}

	private void SpawnDecorationInView(int count)
	{
		if (_decoVariants.Count == 0)
			return;
		if (MaxDecorationCount > 0 && _placedDecor.Count >= MaxDecorationCount)
			return;

		Node2D player = GetNodeOrNull<Node2D>(PlayerPath);
		Vector2 playerPos = player != null ? player.GlobalPosition : Vector2.Zero;
		Camera2D camera = GetViewport().GetCamera2D();
		Vector2 center = camera != null ? camera.GetScreenCenterPosition() : playerPos;
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		Vector2 halfVisible = new Vector2(viewport.X * 0.5f * zoom.X, viewport.Y * 0.5f * zoom.Y);
		Rect2 rect = new(center - halfVisible, halfVisible * 2f);
		float safeRadius = Mathf.Max(0f, InitialInsideSafeRadius * 0.5f);

		int attempts = Mathf.Max(PlacementAttemptsPerSpawn, 18);
		for (int i = 0; i < count; i++)
		{
			if (MaxDecorationCount > 0 && _placedDecor.Count >= MaxDecorationCount)
				break;
			ObstacleVariant variant = PickDecorationVariant();
			if (!TryFindDecorationPlacementInRect(rect, playerPos, safeRadius, variant, attempts, out Vector2 pos))
				continue;
			if (!PassesEnvironmentRule(variant, pos, variant.Radius))
				continue;
			if (!CreateDecoration(variant.Scene, pos))
				continue;
			_placedDecor.Add(new PlacedObstacle(pos, variant.Radius, -1, variant.SpacingBias));
		}
	}

	private ObstacleVariant PickDecorationVariant()
	{
		float total = 0f;
		foreach (ObstacleVariant v in _decoVariants)
			total += Mathf.Max(0.01f, v.Weight);
		float roll = _rng.RandfRange(0f, total);
		foreach (ObstacleVariant v in _decoVariants)
		{
			roll -= Mathf.Max(0.01f, v.Weight);
			if (roll <= 0f)
				return v;
		}
		return _decoVariants[_decoVariants.Count - 1];
	}

	private bool TryFindDecorationPlacementInRect(
		Rect2 rect,
		Vector2 playerPos,
		float safeRadius,
		ObstacleVariant variant,
		int attempts,
		out Vector2 pos)
	{
		for (int i = 0; i < attempts; i++)
		{
			pos = new Vector2(
				_rng.RandfRange(rect.Position.X, rect.End.X),
				_rng.RandfRange(rect.Position.Y, rect.End.Y));
			if (pos.DistanceTo(playerPos) < safeRadius)
				continue;
			if (!IsDecorationPlacementValid(pos, variant.Radius, variant.SpacingBias))
				continue;
			return true;
		}

		pos = Vector2.Zero;
		return false;
	}

	private bool PassesTerrainAffinity(ObstacleVariant variant, Vector2 worldPos)
	{
		if (variant.Affinity == TerrainAffinity.Any || !GodotObject.IsInstanceValid(_terrainBackground))
			return true;

		int probeRadius = Mathf.Clamp(TerrainAffinityProbeRadiusTiles, 0, 16);
		float dirtRatio = SampleDirtRatio(worldPos, probeRadius);
		float strength = Mathf.Clamp(TerrainAffinityStrength, 0f, 1f);
		float target = variant.Affinity == TerrainAffinity.PreferDirt ? dirtRatio : 1f - dirtRatio;
		// Keep soft randomness, but strongly favor matching terrain to avoid large empty zones.
		float accept = Mathf.Lerp(0.85f, Mathf.Clamp(target, 0f, 1f), strength);
		accept = Mathf.Clamp(accept, 0.08f, 0.98f);
		return _rng.Randf() <= accept;
	}

	private bool PassesEnvironmentRule(ObstacleVariant variant, Vector2 worldPos, float radiusWorld)
	{
		if (GodotObject.IsInstanceValid(_terrainBackground) && AvoidTerrainEdges)
		{
			int guardTiles = Mathf.Clamp(TerrainEdgeGuardTiles, 1, 4);
			float edgeRadius = radiusWorld;
			// Trees have large colliders; using full radius over-rejects them near valid interiors.
			// Keep anti-edge rule, but sample with a reduced footprint for large foliage.
			if (variant.Kind == ObstacleKind.Tree)
				edgeRadius *= 0.35f;
			else if (variant.Kind == ObstacleKind.Bush)
				edgeRadius *= 0.55f;
			if (IsNearTerrainBoundary(worldPos, guardTiles, edgeRadius))
				return false;
		}

		if (variant.Kind == ObstacleKind.GrassDeco && GodotObject.IsInstanceValid(_terrainBackground))
		{
			// Deco grass should stay on grass tiles.
			if (_terrainBackground.IsDirtAtWorldPosition(worldPos))
				return false;
		}

		if (variant.Kind == ObstacleKind.Tree || variant.Kind == ObstacleKind.Bush)
		{
			float biome = SampleVegetationBiome(worldPos);
			float threshold = Mathf.Clamp(VegetationBiomeThreshold, 0f, 1f);
			float strength = Mathf.Clamp(VegetationBiomeStrength, 0f, 1f);
			float localChance = Mathf.Clamp((biome - threshold) / Mathf.Max(0.001f, 1f - threshold), 0f, 1f);
			float accept = Mathf.Lerp(1f, localChance, strength);
			// Prevent very large grass zones from becoming visually empty.
			if (variant.Kind == ObstacleKind.Tree)
				accept = Mathf.Max(accept, 0.68f);
			else
				accept = Mathf.Max(accept, 0.60f);
			if (_rng.Randf() > accept)
				return false;

			// Tree species should prefer local species biome, creating same-species groves.
			if (variant.Kind == ObstacleKind.Tree && variant.SpeciesId >= 0)
			{
				int preferredSpecies = SampleTreeSpeciesGroup(worldPos);
				float clusterStrength = Mathf.Clamp(TreeSpeciesClusterStrength, 0f, 1f);
				float speciesAccept = preferredSpecies == variant.SpeciesId
					? Mathf.Lerp(1f, 0.96f, clusterStrength)
					: Mathf.Lerp(1f, 0.72f, clusterStrength);
				if (_rng.Randf() > speciesAccept)
					return false;
			}
		}

		return true;
	}

	private float SampleDirtRatio(Vector2 worldPos, int probeRadiusTiles)
	{
		if (!GodotObject.IsInstanceValid(_terrainBackground))
			return 0.5f;

		float tileSpan = _terrainBackground.GetTileWorldSpan();
		int total = 0;
		int dirt = 0;
		for (int oy = -probeRadiusTiles; oy <= probeRadiusTiles; oy++)
		{
			for (int ox = -probeRadiusTiles; ox <= probeRadiusTiles; ox++)
			{
				total++;
				Vector2 p = worldPos + new Vector2(ox * tileSpan, oy * tileSpan);
				if (_terrainBackground.IsDirtAtWorldPosition(p))
					dirt++;
			}
		}

		if (total <= 0)
			return 0.5f;
		return (float)dirt / total;
	}

	private bool IsNearTerrainBoundary(Vector2 worldPos, int guardTiles, float radiusWorld)
	{
		if (!GodotObject.IsInstanceValid(_terrainBackground))
			return false;

		float step = _terrainBackground.GetTileWorldSpan();
		int footprintTiles = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(0f, radiusWorld) / Mathf.Max(1f, step * 3f)), 0, 2);
		int scanTiles = Mathf.Clamp(guardTiles + footprintTiles, 1, 6);
		bool centerIsDirt = _terrainBackground.IsDirtAtWorldPosition(worldPos);

		// Strict boundary guard:
		// if any sampled tile in guard area belongs to the opposite terrain type,
		// this position is treated as an edge and rejected.
		for (int oy = -scanTiles; oy <= scanTiles; oy++)
		{
			for (int ox = -scanTiles; ox <= scanTiles; ox++)
			{
				if (ox == 0 && oy == 0)
					continue;
				Vector2 p = worldPos + new Vector2(ox * step, oy * step);
				if (_terrainBackground.IsDirtAtWorldPosition(p) != centerIsDirt)
					return true;
			}
		}

		return false;
	}

	private float SampleVegetationBiome(Vector2 worldPos)
	{
		float scale = Mathf.Max(6f, VegetationBiomeScaleTiles);
		Vector2 p = worldPos / scale;
		int x0 = Mathf.FloorToInt(p.X);
		int y0 = Mathf.FloorToInt(p.Y);
		int x1 = x0 + 1;
		int y1 = y0 + 1;
		float tx = p.X - x0;
		float ty = p.Y - y0;
		float sx = tx * tx * (3f - (2f * tx));
		float sy = ty * ty * (3f - (2f * ty));

		float v00 = Hash01(x0, y0, _biomeSalt);
		float v10 = Hash01(x1, y0, _biomeSalt);
		float v01 = Hash01(x0, y1, _biomeSalt);
		float v11 = Hash01(x1, y1, _biomeSalt);
		float ix0 = Mathf.Lerp(v00, v10, sx);
		float ix1 = Mathf.Lerp(v01, v11, sx);
		return Mathf.Lerp(ix0, ix1, sy);
	}

	private int SampleTreeSpeciesGroup(Vector2 worldPos)
	{
		float scale = Mathf.Max(8f, TreeSpeciesBiomeScaleTiles);
		Vector2 p = worldPos / scale;
		int gx = Mathf.FloorToInt(p.X);
		int gy = Mathf.FloorToInt(p.Y);
		float n = Hash01(gx, gy, _speciesSalt);
		return Mathf.Clamp(Mathf.FloorToInt(n * 3f), 0, 2);
	}

	private static float Hash01(int x, int y, int salt)
	{
		unchecked
		{
			uint h = (uint)(x * 73856093) ^ (uint)(y * 19349663) ^ (uint)(salt * 83492791);
			h ^= h >> 13;
			h *= 1274126177u;
			h ^= h >> 16;
			return (h & 65535u) / 65535f;
		}
	}

	private bool CreateObstacle(PackedScene scene, Vector2 globalPos)
	{
		if (scene == null)
			return false;

		Node node = scene.Instantiate();
		if (node is not Node2D node2D)
		{
			node.QueueFree();
			return false;
		}

		node2D.Name = "Obstacle";
		EnsureGeneratedRoot();
		_generatedRoot.AddChild(node2D);
		node2D.GlobalPosition = globalPos;
		node2D.AddToGroup("World");
		node2D.AddToGroup("Obstacle");

		if (node2D is PhysicsBody2D physics)
		{
			physics.CollisionLayer = 1u;
			physics.CollisionMask = 0u;
		}

		return true;
	}

	private bool CreateDecoration(PackedScene scene, Vector2 globalPos)
	{
		if (scene == null)
			return false;

		Node node = scene.Instantiate();
		if (node is not Node2D node2D)
		{
			node.QueueFree();
			return false;
		}

		node2D.Name = "Decoration";
		EnsureGeneratedDecorationRoot();
		_generatedDecorationRoot.AddChild(node2D);
		// Snap to pixel grid to avoid sub-pixel shimmer/flicker in tiny foliage.
		node2D.GlobalPosition = new Vector2(Mathf.Round(globalPos.X), Mathf.Round(globalPos.Y));
		node2D.AddToGroup("World");
		node2D.AddToGroup("Decoration");
		return true;
	}

	private void EnsureStabilitySystem()
	{
		_stabilitySystem = GroupServiceResolver.ResolveFirstInGroup(this, "StabilitySystem", _stabilitySystem);
	}

	private void EnsureTerrainBackground()
	{
		if (GodotObject.IsInstanceValid(_terrainBackground))
			return;
		_terrainBackground = GetNodeOrNull<ProceduralTerrainBackground>(TerrainBackgroundPath);
	}

	private void RebuildVariants()
	{
		_variants.Clear();
		foreach (PackedScene scene in ObstacleScenes)
		{
			if (scene == null)
				continue;

			float radius = EstimateSceneRadius(scene);
			if (radius <= 0f)
				continue;

			float weight = GuessVariantWeight(scene);
			float spacingBias = GuessVariantSpacingBias(scene);
			TerrainAffinity affinity = GuessTerrainAffinity(scene);
			ObstacleKind kind = GuessObstacleKind(scene);
			int speciesId = GuessTreeSpeciesId(scene);
			_variants.Add(new ObstacleVariant(scene, radius, weight, spacingBias, affinity, kind, speciesId));
		}
	}

	private void RebuildDecorationVariants()
	{
		_decoVariants.Clear();
		foreach (PackedScene scene in DecorationScenes)
		{
			if (scene == null)
				continue;

			float radius = EstimateSceneRadius(scene);
			float spacingBias = GuessVariantSpacingBias(scene);
			float weight = Mathf.Max(0.2f, GuessVariantWeight(scene));
			_decoVariants.Add(new ObstacleVariant(
				scene,
				Mathf.Max(10f, radius * 0.6f),
				weight,
				spacingBias,
				TerrainAffinity.PreferGrass,
				ObstacleKind.GrassDeco,
				-1));
		}
	}

	private static float GuessVariantWeight(PackedScene scene)
	{
		string path = scene?.ResourcePath?.ToLowerInvariant() ?? string.Empty;
		if (path.Contains("tree"))
			return 2.1f;
		if (path.Contains("bush"))
			return 1.2f;
		if (path.Contains("rock"))
			return 1.25f;
		return 1f;
	}

	private static float GuessVariantSpacingBias(PackedScene scene)
	{
		string path = scene?.ResourcePath?.ToLowerInvariant() ?? string.Empty;
		if (path.Contains("tree"))
			return 0.72f;
		if (path.Contains("bush"))
			return 0.86f;
		if (path.Contains("rock"))
			return 1.10f;
		return 1f;
	}

	private static TerrainAffinity GuessTerrainAffinity(PackedScene scene)
	{
		string path = scene?.ResourcePath?.ToLowerInvariant() ?? string.Empty;
		if (path.Contains("tree"))
			return TerrainAffinity.PreferGrass;
		if (path.Contains("bush"))
			return TerrainAffinity.PreferGrass;
		if (path.Contains("rock"))
			return TerrainAffinity.PreferDirt;
		return TerrainAffinity.Any;
	}

	private static ObstacleKind GuessObstacleKind(PackedScene scene)
	{
		string path = scene?.ResourcePath?.ToLowerInvariant() ?? string.Empty;
		if (path.Contains("tree"))
			return ObstacleKind.Tree;
		if (path.Contains("bush"))
			return ObstacleKind.Bush;
		if (path.Contains("rock"))
			return ObstacleKind.Rock;
		return ObstacleKind.Generic;
	}

	private static int GuessTreeSpeciesId(PackedScene scene)
	{
		string path = scene?.ResourcePath?.ToLowerInvariant() ?? string.Empty;
		if (!path.Contains("tree"))
			return -1;
		if (path.Contains("small_c") || path.Contains("tree3"))
			return 2;
		if (path.Contains("small_b") || path.Contains("tree2"))
			return 1;
		return 0;
	}

	private float EstimateSceneRadius(PackedScene scene)
	{
		Node node = scene.Instantiate();
		if (node is not Node2D node2D)
		{
			node.QueueFree();
			return 0f;
		}

		float radius = EstimateNodeRadius(node2D);
		node2D.QueueFree();
		return Mathf.Max(24f, radius);
	}

	private static float EstimateNodeRadius(Node2D node)
	{
		float maxRadius = 0f;
		foreach (Node child in node.GetChildren())
		{
			if (child is not CollisionShape2D collider || collider.Shape == null)
				continue;

			Vector2 scale = collider.Scale.Abs();
			float scaleMul = Mathf.Max(scale.X, scale.Y);
			switch (collider.Shape)
			{
				case RectangleShape2D rect:
					maxRadius = Mathf.Max(maxRadius, rect.Size.Length() * 0.5f * scaleMul);
					break;
				case CircleShape2D circle:
					maxRadius = Mathf.Max(maxRadius, circle.Radius * scaleMul);
					break;
				case CapsuleShape2D capsule:
					maxRadius = Mathf.Max(maxRadius, Mathf.Max(capsule.Radius, capsule.Height * 0.5f) * scaleMul);
					break;
			}
		}

		return maxRadius > 0f ? maxRadius : 48f;
	}

	private void CacheExistingObstacles()
	{
		_placed.Clear();
		_placedDecor.Clear();
		EnsureGeneratedRoot();
		EnsureGeneratedDecorationRoot();
		foreach (Node child in _generatedRoot.GetChildren())
		{
			if (child is not Node2D node2D)
				continue;

			float radius = EstimateNodeRadius(node2D);
			_placed.Add(new PlacedObstacle(node2D.GlobalPosition, Mathf.Max(24f, radius), -1, 1f));
		}

		foreach (Node child in _generatedDecorationRoot.GetChildren())
		{
			if (child is not Node2D node2D)
				continue;
			float radius = EstimateNodeRadius(node2D);
			_placedDecor.Add(new PlacedObstacle(node2D.GlobalPosition, Mathf.Max(8f, radius), -1, 0.8f));
		}

		// Also include authored/static obstacles under this generator root so spawned objects keep spacing.
		foreach (Node child in GetChildren())
		{
			if (child == _generatedRoot || child is not Node2D node2D)
				continue;

			float radius = EstimateNodeRadius(node2D);
			_placed.Add(new PlacedObstacle(node2D.GlobalPosition, Mathf.Max(24f, radius), -1, 1f));
		}
	}

	public void ResetField()
	{
		EnsureGeneratedRoot();
		EnsureGeneratedDecorationRoot();
		foreach (Node child in _generatedRoot.GetChildren())
		{
			_generatedRoot.RemoveChild(child);
			child.QueueFree();
		}
		foreach (Node child in _generatedDecorationRoot.GetChildren())
		{
			_generatedDecorationRoot.RemoveChild(child);
			child.QueueFree();
		}

		_placed.Clear();
		_placedDecor.Clear();
		_lastVariantIndex = -1;
		_hasClusterCenter = false;
		_clusterCenter = Vector2.Zero;
		_clusterRemaining = 0;
		_spawnTimer = Mathf.Max(0.05f, SpawnIntervalSeconds);
		_wasPaused = true;
	}

	private void EnsureGeneratedRoot()
	{
		string nodeName = string.IsNullOrWhiteSpace(GeneratedContainerName) ? "GeneratedObstacles" : GeneratedContainerName;
		_generatedRoot ??= GetNodeOrNull<Node2D>(nodeName);
		if (!GodotObject.IsInstanceValid(_generatedRoot))
		{
			_generatedRoot = new Node2D { Name = nodeName };
			AddChild(_generatedRoot);
		}
	}

	private void EnsureGeneratedDecorationRoot()
	{
		string nodeName = string.IsNullOrWhiteSpace(GeneratedDecorationContainerName) ? "GeneratedDecor" : GeneratedDecorationContainerName;
		_generatedDecorationRoot ??= GetNodeOrNull<Node2D>(nodeName);
		if (!GodotObject.IsInstanceValid(_generatedDecorationRoot))
		{
			_generatedDecorationRoot = new Node2D { Name = nodeName };
			AddChild(_generatedDecorationRoot);
		}
		_generatedDecorationRoot.ZIndex = -1;
	}
}
