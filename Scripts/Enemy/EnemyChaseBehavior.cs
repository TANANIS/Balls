using Godot;

public partial class EnemyChaseBehavior : EnemyBehaviorModule
{
	[Export] public float SpeedMultiplier = 1f;
	[Export] public float MinDistance = 0f;

	public override Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta)
	{
		if (enemy == null || player == null)
			return Vector2.Zero;

		Vector2 toPlayer = player.GlobalPosition - enemy.GlobalPosition;
		float distance = toPlayer.Length();
		if (distance < 0.0001f || distance <= MinDistance)
			return Vector2.Zero;

		float speed = enemy.MaxSpeed * Mathf.Max(0f, SpeedMultiplier);
		return toPlayer / distance * speed;
	}
}
