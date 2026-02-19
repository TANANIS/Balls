using Godot;

public partial class EnemyTankBlockBehavior : EnemyBehaviorModule
{
	[Export] public float ApproachSpeedMultiplier = 0.85f;
	[Export] public float OrbitSpeedMultiplier = 0.45f;
	[Export] public float DesiredBlockDistance = 140f;
	[Export] public float DistanceTolerance = 24f;
	[Export] public float InterceptSeconds = 0.55f;
	[Export] public float CutoffAheadDistance = 110f;
	[Export] public float RetreatSpeedMultiplier = 0.45f;
	[Export] public bool Clockwise = true;

	public override Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta)
	{
		if (enemy == null || player == null)
			return Vector2.Zero;

		Vector2 playerVelocity = Vector2.Zero;
		if (player is CharacterBody2D movingPlayer)
			playerVelocity = movingPlayer.Velocity;

		Vector2 predictedPlayerPos = player.GlobalPosition + (playerVelocity * Mathf.Max(0f, InterceptSeconds));
		Vector2 heading = playerVelocity.LengthSquared() > 0.01f
			? playerVelocity.Normalized()
			: (predictedPlayerPos - enemy.GlobalPosition).Normalized();
		Vector2 cutoffTarget = predictedPlayerPos + (heading * Mathf.Max(0f, CutoffAheadDistance));

		Vector2 toPlayer = predictedPlayerPos - enemy.GlobalPosition;
		float distance = toPlayer.Length();
		if (distance < 0.001f)
			return Vector2.Zero;

		Vector2 forward = toPlayer / distance;
		Vector2 toCutoff = cutoffTarget - enemy.GlobalPosition;
		Vector2 cutoffDir = toCutoff.LengthSquared() > 0.01f ? toCutoff.Normalized() : forward;
		float speed = enemy.MaxSpeed;

		if (distance > DesiredBlockDistance + DistanceTolerance)
			return cutoffDir * speed * ApproachSpeedMultiplier;

		if (distance < DesiredBlockDistance - DistanceTolerance)
		{
			Vector2 tangentRetreat = Clockwise ? new Vector2(forward.Y, -forward.X) : new Vector2(-forward.Y, forward.X);
			Vector2 retreatDir = ((-forward * 0.78f) + (tangentRetreat * 0.22f)).Normalized();
			return retreatDir * speed * Mathf.Max(0f, RetreatSpeedMultiplier);
		}

		Vector2 tangent = Clockwise ? new Vector2(forward.Y, -forward.X) : new Vector2(-forward.Y, forward.X);
		Vector2 pressureDir = ((tangent * Mathf.Max(0f, OrbitSpeedMultiplier)) + (cutoffDir * 0.36f)).Normalized();
		return pressureDir * speed;
	}
}
