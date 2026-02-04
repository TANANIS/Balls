using Godot;

public partial class EnemyMiniBossBehavior : EnemyBehaviorModule
{
	[Export] public float PatrolSpeedMultiplier = 0.55f;
	[Export] public float RushSpeedMultiplier = 1.8f;
	[Export] public float TriggerDistance = 320f;
	[Export] public float WindupDuration = 0.55f;
	[Export] public float RushDuration = 0.35f;
	[Export] public float RecoveryDuration = 1.25f;

	private enum State
	{
		Patrol,
		Windup,
		Rush,
		Recover
	}

	private State _state = State.Patrol;
	private float _timer;
	private Vector2 _rushDirection = Vector2.Right;

	public override Vector2 GetDesiredVelocity(Enemy enemy, Node2D player, double delta)
	{
		if (enemy == null || player == null)
			return Vector2.Zero;

		float dt = (float)delta;
		Vector2 toPlayer = player.GlobalPosition - enemy.GlobalPosition;
		float distance = toPlayer.Length();
		Vector2 forward = distance > 0.001f ? toPlayer / distance : Vector2.Right;

		switch (_state)
		{
			case State.Patrol:
				if (distance <= TriggerDistance)
				{
					_state = State.Windup;
					_timer = WindupDuration;
					_rushDirection = forward;
					return Vector2.Zero;
				}
				return forward * enemy.MaxSpeed * PatrolSpeedMultiplier;

			case State.Windup:
				_timer -= dt;
				if (_timer <= 0f)
				{
					_state = State.Rush;
					_timer = RushDuration;
				}
				return Vector2.Zero;

			case State.Rush:
				_timer -= dt;
				if (_timer <= 0f)
				{
					_state = State.Recover;
					_timer = RecoveryDuration;
				}
				return _rushDirection * enemy.MaxSpeed * RushSpeedMultiplier;

			case State.Recover:
				_timer -= dt;
				if (_timer <= 0f)
					_state = State.Patrol;
				return forward * enemy.MaxSpeed * 0.35f;
		}

		return Vector2.Zero;
	}
}
