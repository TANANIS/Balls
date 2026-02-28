using Godot;

public partial class ObstacleFieldGenerator
{
	private float GetMinimumObstacleSpacingWorld()
	{
		float heights = Mathf.Max(0f, MinObstacleSpacingPlayerHeights);
		if (heights <= 0f)
			return 0f;
		return GetReferencePlayerHeight() * heights;
	}

	private float GetReferencePlayerHeight()
	{
		float fallback = Mathf.Max(16f, FallbackPlayerCollisionHeight);
		Node2D player = GetNodeOrNull<Node2D>(PlayerPath);
		if (!GodotObject.IsInstanceValid(player))
			return fallback;

		foreach (Node child in player.GetChildren())
		{
			if (child is not CollisionShape2D collider || collider.Shape == null)
				continue;

			float shapeHeight = ObstacleCollisionHelper.EstimateShapeHeight(collider.Shape);
			if (shapeHeight <= 0f)
				continue;

			float scaledHeight = shapeHeight * Mathf.Max(0.01f, collider.Scale.Abs().Y);
			return Mathf.Max(16f, scaledHeight);
		}

		return fallback;
	}
}
