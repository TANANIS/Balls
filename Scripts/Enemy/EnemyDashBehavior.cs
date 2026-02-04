using Godot;

public partial class EnemyDashBehavior : EnemyBehaviorModule
{
	[Export] public float ChaseSpeedMultiplier = 0.8f;
	[Export] public float DashSpeedMultiplier = 2.4f;
	[Export] public float TriggerDistance = 280f;
	[Export] public float WindupDuration = 0.28f;
	[Export] public float DashDuration = 0.18f;
	[Export] public float DashCooldown = 1.05f;
	[Export] public float MinAimDistance = 12f;

	private enum DashState
	{
		Chase,
		Windup,
		Dash,
		Cooldown
	}

	private DashState _state = DashState.Chase;
	private float _stateTimer = 0f;
	private Vector2 _dashDirection = Vector2.Right;

	public override Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta)
	{
		if (enemy == null || player == null)
			return Vector2.Zero;

		float dt = (float)delta;
		Vector2 toPlayer = player.GlobalPosition - enemy.GlobalPosition;
		float distance = toPlayer.Length();

		switch (_state)
		{
			case DashState.Chase:
				if (distance <= TriggerDistance)
				{
					_dashDirection = distance > MinAimDistance ? toPlayer / distance : Vector2.Right;
					_state = DashState.Windup;
					_stateTimer = WindupDuration;
					return Vector2.Zero;
				}

				return GetChaseVelocity(enemy, toPlayer, distance);

			case DashState.Windup:
				_stateTimer -= dt;
				if (_stateTimer <= 0f)
				{
					_state = DashState.Dash;
					_stateTimer = DashDuration;
				}
				return Vector2.Zero;

			case DashState.Dash:
				_stateTimer -= dt;
				if (_stateTimer <= 0f)
				{
					_state = DashState.Cooldown;
					_stateTimer = DashCooldown;
				}
				return _dashDirection * enemy.MaxSpeed * DashSpeedMultiplier;

			case DashState.Cooldown:
				_stateTimer -= dt;
				if (_stateTimer <= 0f)
					_state = DashState.Chase;
				return GetChaseVelocity(enemy, toPlayer, distance);
		}

		return Vector2.Zero;
	}

	private Vector2 GetChaseVelocity(Enemy enemy, Vector2 toPlayer, float distance)
	{
		if (distance < 0.0001f)
			return Vector2.Zero;

		return (toPlayer / distance) * enemy.MaxSpeed * ChaseSpeedMultiplier;
	}
}
