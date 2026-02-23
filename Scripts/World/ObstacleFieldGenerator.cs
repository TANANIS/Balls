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
	[Export] public Godot.Collections.Array<PackedScene> ObstacleScenes = new();
	[Export] public NodePath PlayerPath = "../../Player";

	[Export] public float SpawnIntervalSeconds = 2.8f;
	[Export] public int SpawnPerTickMin = 1;
	[Export] public int SpawnPerTickMax = 1;
	[Export] public int MaxObstacleCount = 90;
	[Export] public float SpawnOutsideMargin = 220f;
	[Export] public float SpawnRingThickness = 560f;
	[Export] public float ObstacleSpacingMultiplier = 2.75f;
	[Export] public int PlacementAttemptsPerSpawn = 24;
	[Export] public bool EnforceAlternatingTypes = true;
	[Export] public bool UseNaturalClusters = true;
	[Export(PropertyHint.Range, "0,1,0.01")] public float ClusterStickiness = 0.72f;
	[Export] public float ClusterRadiusMin = 120f;
	[Export] public float ClusterRadiusMax = 260f;
	[Export] public int ClusterLifetimeSpawns = 7;

	private readonly RandomNumberGenerator _rng = new();
	private readonly List<PlacedObstacle> _placed = new();
	private readonly List<ObstacleVariant> _variants = new();

	private float _spawnTimer;
	private bool _wasPaused = true;
	private StabilitySystem _stabilitySystem;
	private int _lastVariantIndex = -1;
	private bool _hasClusterCenter = false;
	private Vector2 _clusterCenter = Vector2.Zero;
	private int _clusterRemaining = 0;

	private readonly struct ObstacleVariant
	{
		public readonly PackedScene Scene;
		public readonly float Radius;
		public readonly float Weight;
		public readonly float SpacingBias;

		public ObstacleVariant(PackedScene scene, float radius, float weight, float spacingBias)
		{
			Scene = scene;
			Radius = radius;
			Weight = weight;
			SpacingBias = spacingBias;
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
		EnsureStabilitySystem();
		RebuildVariants();
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
		float phaseRate = _stabilitySystem?.GetObstacleSpawnMultiplier() ?? 1f;
		_spawnTimer -= (float)delta * Mathf.Max(0.05f, phaseRate);

		// Generate a small initial burst right after leaving pause/start menu.
		if (_wasPaused)
		{
			_wasPaused = false;
			SpawnBatch(1);
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
		float minDist = Mathf.Max(halfVisible.X, halfVisible.Y) + Mathf.Max(0f, SpawnOutsideMargin);
		float maxDist = minDist + Mathf.Max(60f, SpawnRingThickness);
		PrepareCluster(playerPos, minDist, maxDist);

		for (int i = 0; i < count; i++)
		{
			if (MaxObstacleCount > 0 && _placed.Count >= MaxObstacleCount)
				break;

			int variantIndex = PickVariantIndex();
			ObstacleVariant variant = _variants[variantIndex];
			if (!TryFindPlacement(playerPos, minDist, maxDist, variant.Radius, variant.SpacingBias, out Vector2 pos))
				continue;

			if (CreateObstacle(variant.Scene, pos))
			{
				_placed.Add(new PlacedObstacle(pos, variant.Radius, variantIndex, variant.SpacingBias));
				_lastVariantIndex = variantIndex;
				if (_clusterRemaining > 0)
					_clusterRemaining--;
			}
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

	private bool CreateObstacle(PackedScene scene, Vector2 globalPos)
	{
		if (scene == null)
			return false;

		Node node = scene.Instantiate();
		if (node is not Node2D node2D)
		{
			node.QueueFree();
			DebugSystem.Warn("[ObstacleFieldGenerator] Obstacle scene root must be Node2D.");
			return false;
		}

		node2D.Name = "Obstacle";
		AddChild(node2D);
		node2D.GlobalPosition = globalPos;
		node2D.AddToGroup("World");

		if (node2D is PhysicsBody2D physics)
		{
			physics.CollisionLayer = 1u;
			physics.CollisionMask = 0u;
		}

		return true;
	}

	private void EnsureStabilitySystem()
	{
		_stabilitySystem = GroupServiceResolver.ResolveFirstInGroup(this, "StabilitySystem", _stabilitySystem);
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
			_variants.Add(new ObstacleVariant(scene, radius, weight, spacingBias));
		}
	}

	private static float GuessVariantWeight(PackedScene scene)
	{
		string path = scene?.ResourcePath?.ToLowerInvariant() ?? string.Empty;
		if (path.Contains("tree"))
			return 1.35f;
		if (path.Contains("rock"))
			return 0.95f;
		return 1f;
	}

	private static float GuessVariantSpacingBias(PackedScene scene)
	{
		string path = scene?.ResourcePath?.ToLowerInvariant() ?? string.Empty;
		if (path.Contains("tree"))
			return 0.88f;
		if (path.Contains("rock"))
			return 1.10f;
		return 1f;
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
		foreach (Node child in GetChildren())
		{
			if (child is not Node2D node2D)
				continue;

			float radius = EstimateNodeRadius(node2D);
			_placed.Add(new PlacedObstacle(node2D.GlobalPosition, Mathf.Max(24f, radius), -1, 1f));
		}
	}
}
