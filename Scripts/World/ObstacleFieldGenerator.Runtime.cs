using Godot;

public partial class ObstacleFieldGenerator
{
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

		node2D.Name = RuntimeGroups.Obstacle;
		EnsureGeneratedRoot();
		_generatedRoot.AddChild(node2D);
		node2D.GlobalPosition = globalPos;
		node2D.AddToGroup(RuntimeGroups.World);
		node2D.AddToGroup(RuntimeGroups.Obstacle);

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

		node2D.Name = RuntimeGroups.Decoration;
		EnsureGeneratedDecorationRoot();
		_generatedDecorationRoot.AddChild(node2D);
		// Snap to pixel grid to avoid sub-pixel shimmer/flicker in tiny foliage.
		node2D.GlobalPosition = new Vector2(Mathf.Round(globalPos.X), Mathf.Round(globalPos.Y));
		node2D.AddToGroup(RuntimeGroups.World);
		node2D.AddToGroup(RuntimeGroups.Decoration);
		return true;
	}

	private void EnsureStabilitySystem()
	{
		_stabilitySystem = GroupServiceResolver.ResolveFirstInGroup(this, RuntimeGroups.StabilitySystem, _stabilitySystem);
	}

	private void EnsureTerrainBackground()
	{
		if (GodotObject.IsInstanceValid(_terrainBackground))
			return;
		_terrainBackground = GetNodeOrNull<ProceduralTerrainBackground>(TerrainBackgroundPath);
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
			if (!node2D.IsInGroup(RuntimeGroups.Obstacle))
				node2D.AddToGroup(RuntimeGroups.Obstacle);
			if (!node2D.IsInGroup(RuntimeGroups.World))
				node2D.AddToGroup(RuntimeGroups.World);

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
		_runtimeMinObstacleSpacingWorld = 0f;
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
