using Godot;
using System.Collections.Generic;

/*
 * ObstacleFieldGenerator:
 * - Dynamically spawns non-interactive static obstacles outside current view.
 * - Keeps spawned obstacles; never clears previous instances at runtime.
 * - Uses randomized position/rotation/scale with overlap checks.
 */
public partial class ObstacleFieldGenerator : Node2D
{
	[Export] public Texture2D ObstacleTexture;
	[Export] public NodePath PlayerPath = "../../Player";

	[Export] public float SpawnIntervalSeconds = 2.8f;
	[Export] public int SpawnPerTickMin = 1;
	[Export] public int SpawnPerTickMax = 1;
	[Export] public int MaxObstacleCount = 90;
	[Export] public float SpawnOutsideMargin = 220f;
	[Export] public float SpawnRingThickness = 560f;
	[Export] public float ScaleMin = 0.28f;
	[Export] public float ScaleMax = 0.48f;
	[Export] public float RotationMinDegrees = -22f;
	[Export] public float RotationMaxDegrees = 22f;
	[Export] public float ColliderWidthFactor = 0.58f;
	[Export] public float ColliderHeightFactor = 0.40f;
	[Export] public float ObstacleSpacingMultiplier = 2.75f;
	[Export] public int PlacementAttemptsPerSpawn = 24;

	private readonly RandomNumberGenerator _rng = new();
	private readonly List<(Vector2 pos, float radius)> _placed = new();
	private float _spawnTimer;
	private bool _wasPaused = true;
	private StabilitySystem _stabilitySystem;

	public override void _Ready()
	{
		_rng.Randomize();
		EnsureStabilitySystem();
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

		if (ObstacleTexture == null)
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
		if (ObstacleTexture == null)
			return;
		if (MaxObstacleCount > 0 && _placed.Count >= MaxObstacleCount)
			return;

		var player = GetNodeOrNull<Node2D>(PlayerPath);
		Vector2 playerPos = player != null ? player.GlobalPosition : Vector2.Zero;
		Vector2 viewport = GetViewport().GetVisibleRect().Size;
		var camera = GetViewport().GetCamera2D();
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		Vector2 halfVisible = new Vector2(viewport.X * 0.5f * zoom.X, viewport.Y * 0.5f * zoom.Y);
		float minDist = Mathf.Max(halfVisible.X, halfVisible.Y) + Mathf.Max(0f, SpawnOutsideMargin);
		float maxDist = minDist + Mathf.Max(60f, SpawnRingThickness);
		Vector2 tex = ObstacleTexture.GetSize();

		for (int i = 0; i < count; i++)
		{
			if (MaxObstacleCount > 0 && _placed.Count >= MaxObstacleCount)
				break;
			if (!TryFindPlacement(playerPos, minDist, maxDist, tex, out Vector2 pos, out float scale, out float radius))
				continue;

			CreateObstacle(pos, scale);
			_placed.Add((pos, radius));
		}
	}

	private bool TryFindPlacement(Vector2 center, float minDist, float maxDist, Vector2 tex, out Vector2 pos, out float scale, out float radius)
	{
		for (int attempt = 0; attempt < PlacementAttemptsPerSpawn; attempt++)
		{
			scale = _rng.RandfRange(ScaleMin, ScaleMax);
			float angle = _rng.RandfRange(0f, Mathf.Tau);
			float dist = _rng.RandfRange(minDist, maxDist);
			Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
			pos = center + (dir * dist);
			radius = Mathf.Max(48f, tex.Length() * scale * 0.30f);

			bool overlap = false;
			foreach (var it in _placed)
			{
				if (pos.DistanceTo(it.pos) < (radius + it.radius) * Mathf.Max(1f, ObstacleSpacingMultiplier))
				{
					overlap = true;
					break;
				}
			}

			if (!overlap)
				return true;
		}

		pos = Vector2.Zero;
		scale = 1f;
		radius = 0f;
		return false;
	}

	private void EnsureStabilitySystem()
	{
		if (IsInstanceValid(_stabilitySystem))
			return;

		var list = GetTree().GetNodesInGroup("StabilitySystem");
		if (list.Count > 0)
			_stabilitySystem = list[0] as StabilitySystem;
	}

	private void CreateObstacle(Vector2 globalPos, float scale)
	{
		var body = new StaticBody2D();
		body.Name = "Obstacle";
		body.GlobalPosition = globalPos;
		body.CollisionLayer = 1u;
		body.CollisionMask = 0u;
		body.AddToGroup("World");
		AddChild(body);

		var sprite = new Sprite2D();
		sprite.Texture = ObstacleTexture;
		sprite.Scale = new Vector2(scale, scale);
		sprite.RotationDegrees = _rng.RandfRange(RotationMinDegrees, RotationMaxDegrees);
		body.AddChild(sprite);

		Vector2 tex = ObstacleTexture.GetSize();
		var shape = new RectangleShape2D();
		shape.Size = new Vector2(
			Mathf.Max(20f, tex.X * scale * ColliderWidthFactor),
			Mathf.Max(20f, tex.Y * scale * ColliderHeightFactor));

		var collider = new CollisionShape2D();
		collider.Shape = shape;
		body.AddChild(collider);
	}

	private void CacheExistingObstacles()
	{
		_placed.Clear();
		foreach (Node child in GetChildren())
		{
			if (child is not StaticBody2D body)
				continue;

			Vector2 pos = body.GlobalPosition;
			float radius = 48f;
			var collider = body.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			if (collider?.Shape is RectangleShape2D rect)
				radius = rect.Size.Length() * 0.5f;
			_placed.Add((pos, radius));
		}
	}
}
