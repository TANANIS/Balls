using Godot;

public partial class EnemyTankBlockBehavior : EnemyBehaviorModule
{
	[Export] public float ApproachSpeedMultiplier = 0.85f;
	[Export] public float OrbitSpeedMultiplier = 0.45f;
	[Export] public float DesiredBlockDistance = 140f;
	[Export] public float DistanceTolerance = 24f;
	[Export] public bool Clockwise = true;

	public override Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta)
	{
		if (enemy == null || player == null)
			return Vector2.Zero;

		Vector2 toPlayer = player.GlobalPosition - enemy.GlobalPosition;
		float distance = toPlayer.Length();
		if (distance < 0.001f)
			return Vector2.Zero;

		Vector2 forward = toPlayer / distance;
		float speed = enemy.MaxSpeed;

		// Too far: approach player to reduce safe space.
		if (distance > DesiredBlockDistance + DistanceTolerance)
			return forward * speed * ApproachSpeedMultiplier;

		// Too close: back off slightly to keep a blocking ring.
		if (distance < DesiredBlockDistance - DistanceTolerance)
			return -forward * speed * 0.35f;

		// Near preferred range: orbit to deny paths.
		Vector2 tangent = Clockwise ? new Vector2(forward.Y, -forward.X) : new Vector2(-forward.Y, forward.X);
		return tangent * speed * OrbitSpeedMultiplier;
	}
}
