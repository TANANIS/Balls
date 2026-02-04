using Godot;

public partial class EnemyEliteSwarmBehavior : EnemyBehaviorModule
{
	[Export] public float BaseSpeedMultiplier = 1.15f;
	[Export] public float WeaveAmplitude = 0.38f;
	[Export] public float WeaveFrequency = 5.5f;

	private float _time;
	private bool _clockwise = true;

	public override void OnInitialized(Enemy enemy)
	{
		_time = GD.Randf() * 100f;
		_clockwise = GD.Randf() > 0.5f;
	}

	public override Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta)
	{
		if (enemy == null || player == null)
			return Vector2.Zero;

		Vector2 toPlayer = player.GlobalPosition - enemy.GlobalPosition;
		float distance = toPlayer.Length();
		if (distance < 0.001f)
			return Vector2.Zero;

		_time += (float)delta;

		Vector2 forward = toPlayer / distance;
		Vector2 side = _clockwise ? new Vector2(forward.Y, -forward.X) : new Vector2(-forward.Y, forward.X);
		float weave = Mathf.Sin(_time * WeaveFrequency) * WeaveAmplitude;
		Vector2 dir = (forward + side * weave).Normalized();
		return dir * enemy.MaxSpeed * BaseSpeedMultiplier;
	}
}
