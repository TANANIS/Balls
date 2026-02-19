using Godot;

public partial class EnemyChaseBehavior : EnemyBehaviorModule
{
	[Export] public float SpeedMultiplier = 1f;
	[Export] public float MinDistance = 0f;
	[Export] public bool UsePredictiveIntercept = false;
	[Export] public float InterceptSeconds = 0.24f;
	[Export] public bool UseEncircle = false;
	[Export] public float EncircleDistance = 165f;
	[Export] public float EncircleDistanceBand = 70f;
	[Export] public float EncircleWeight = 0.46f;
	[Export] public bool RandomizeSide = true;
	[Export] public bool Clockwise = true;

	private int _sideSign = 1;

	public override void OnInitialized(Enemy enemy)
	{
		if (enemy == null)
			return;

		if (RandomizeSide)
			_sideSign = ((enemy.GetInstanceId() & 1) == 0) ? 1 : -1;
		else
			_sideSign = Clockwise ? 1 : -1;
	}

	public override Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta)
	{
		if (enemy == null || player == null)
			return Vector2.Zero;

		Vector2 target = player.GlobalPosition;
		if (UsePredictiveIntercept && player is CharacterBody2D movingPlayer)
			target += movingPlayer.Velocity * Mathf.Max(0f, InterceptSeconds);

		Vector2 toPredicted = target - enemy.GlobalPosition;
		float distance = toPredicted.Length();
		if (distance < 0.0001f)
			return Vector2.Zero;

		float speed = enemy.MaxSpeed * Mathf.Max(0f, SpeedMultiplier);
		Vector2 forward = toPredicted / distance;

		// Default behavior for regular mobs: direct chase.
		if (!UseEncircle)
		{
			if (distance <= MinDistance)
				return Vector2.Zero;
			return forward * speed;
		}

		Vector2 tangent = _sideSign > 0
			? new Vector2(forward.Y, -forward.X)
			: new Vector2(-forward.Y, forward.X);

		float band = Mathf.Max(8f, EncircleDistanceBand);
		float ringMin = Mathf.Max(0f, EncircleDistance - band);
		float ringMax = EncircleDistance + band;

		float radialWeight;
		if (distance > ringMax)
			radialWeight = 1f;
		else if (distance < Mathf.Max(MinDistance, ringMin))
			radialWeight = -0.52f;
		else
			radialWeight = 0.22f;

		float lateral = Mathf.Max(0f, EncircleWeight);
		Vector2 dir = (forward * radialWeight) + (tangent * lateral);
		if (dir.LengthSquared() < 0.0001f)
			dir = forward;

		return dir.Normalized() * speed;
	}
}
