using Godot;

public partial class ObstacleFieldGenerator
{
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
}
