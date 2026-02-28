using Godot;
using System;

public static class ObstacleCollisionHelper
{
	private const float MinDirectionLengthSquared = 0.0001f;

	public static bool TryGetObstacleCollision(CharacterBody2D body, out KinematicCollision2D collision)
	{
		collision = null;
		if (!GodotObject.IsInstanceValid(body))
			return false;

		int count = body.GetSlideCollisionCount();
		for (int i = 0; i < count; i++)
		{
			KinematicCollision2D hit = body.GetSlideCollision(i);
			if (hit == null)
				continue;
			if (IsObstacleNode(hit.GetCollider() as Node))
			{
				collision = hit;
				return true;
			}
		}

		return false;
	}

	public static ulong GetColliderInstanceId(KinematicCollision2D collision)
	{
		if (collision?.GetCollider() is GodotObject obj)
			return obj.GetInstanceId();
		return 0ul;
	}

	public static Vector2 GetRepelDirection(Node2D self, KinematicCollision2D collision)
	{
		if (!GodotObject.IsInstanceValid(self) || collision == null)
			return Vector2.Zero;

		Vector2 normal = collision.GetNormal();
		if (normal.LengthSquared() > MinDirectionLengthSquared)
			return normal.Normalized();

		if (collision.GetCollider() is Node2D colliderNode)
		{
			Vector2 away = self.GlobalPosition - colliderNode.GlobalPosition;
			if (away.LengthSquared() > MinDirectionLengthSquared)
				return away.Normalized();
		}

		Vector2 awayFromHit = self.GlobalPosition - collision.GetPosition();
		if (awayFromHit.LengthSquared() > MinDirectionLengthSquared)
			return awayFromHit.Normalized();

		return Vector2.Zero;
	}

	public static bool IsObstacleNode(Node node)
	{
		if (!GodotObject.IsInstanceValid(node))
			return false;
		if (node.IsInGroup(RuntimeGroups.Obstacle))
			return true;
		return node.Name.ToString().Contains("Obstacle", StringComparison.OrdinalIgnoreCase);
	}

	public static float EstimateShapeHeight(Shape2D shape)
	{
		switch (shape)
		{
			case RectangleShape2D rectangle:
				return rectangle.Size.Y;
			case CircleShape2D circle:
				return circle.Radius * 2f;
			case CapsuleShape2D capsule:
				return Mathf.Max(capsule.Height, capsule.Radius * 2f);
			default:
				return 0f;
		}
	}
}
