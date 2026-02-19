using Godot;

public partial class EnemyEliteSwarmBehavior : EnemyBehaviorModule
{
	[Export] public float BaseSpeedMultiplier = 1.15f;
	[Export] public float BurstSpeedMultiplier = 1.65f;
	[Export] public float BurstDuration = 0.45f;
	[Export] public float BurstCooldownMin = 1.0f;
	[Export] public float BurstCooldownMax = 1.7f;
	[Export] public float InterceptSeconds = 0.22f;
	[Export] public float WeaveAmplitude = 0.38f;
	[Export] public float WeaveFrequency = 5.5f;

	private float _time;
	private bool _clockwise = true;
	private float _burstCooldown;
	private float _burstRemaining;
	private readonly RandomNumberGenerator _rng = new();

	public override void OnInitialized(Enemy enemy)
	{
		_rng.Randomize();
		_time = GD.Randf() * 100f;
		_clockwise = GD.Randf() > 0.5f;
		_burstCooldown = _rng.RandfRange(Mathf.Max(0.1f, BurstCooldownMin), Mathf.Max(BurstCooldownMin, BurstCooldownMax));
		_burstRemaining = 0f;
	}

	public override Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta)
	{
		if (enemy == null || player == null)
			return Vector2.Zero;

		float dt = (float)delta;
		Vector2 predicted = player.GlobalPosition;
		if (player is CharacterBody2D movingPlayer)
			predicted += movingPlayer.Velocity * Mathf.Max(0f, InterceptSeconds);

		Vector2 toPlayer = predicted - enemy.GlobalPosition;
		float distance = toPlayer.Length();
		if (distance < 0.001f)
			return Vector2.Zero;

		_time += dt;
		if (_burstRemaining > 0f)
			_burstRemaining = Mathf.Max(0f, _burstRemaining - dt);
		else
		{
			_burstCooldown -= dt;
			if (_burstCooldown <= 0f)
			{
				_burstRemaining = Mathf.Max(0.05f, BurstDuration);
				_burstCooldown = _rng.RandfRange(Mathf.Max(0.1f, BurstCooldownMin), Mathf.Max(BurstCooldownMin, BurstCooldownMax));
			}
		}

		Vector2 forward = toPlayer / distance;
		Vector2 side = _clockwise ? new Vector2(forward.Y, -forward.X) : new Vector2(-forward.Y, forward.X);
		float weave = Mathf.Sin(_time * WeaveFrequency) * WeaveAmplitude;
		Vector2 dir = (forward + side * weave).Normalized();
		float speedMult = _burstRemaining > 0f ? BurstSpeedMultiplier : BaseSpeedMultiplier;
		return dir * enemy.MaxSpeed * Mathf.Max(0f, speedMult);
	}
}
