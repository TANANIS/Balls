using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export] public float MaxSpeed = 160f;
	[Export] public float Accel = 1200f;
	[Export] public float Friction = 900f;
	[Export] public NodePath PlayerPath = new NodePath("../../Player");
	[Export] public NodePath BehaviorPath = new NodePath("Behavior");
	[Export] public NodePath SeparationPath = new NodePath("Separation");

	private EnemyHealth _health;
	private Node2D _player;
	private EnemyBehaviorModule _behavior;
	private EnemySeparationModule _separation;

	public override void _Ready()
	{
		_health = GetNodeOrNull<EnemyHealth>("Health");
		ResolvePlayer();
		ResolveBehavior();
		ResolveSeparation();
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (_health != null && _health.IsDead)
		{
			QueueFree();
			return;
		}

		if (!IsInstanceValid(_player))
			ResolvePlayer();

		Vector2 desired = GetDesiredVelocity(delta);
		float moveRate = desired == Vector2.Zero ? Friction : Accel;
		Velocity = Velocity.MoveToward(desired, Mathf.Max(1f, moveRate) * dt);

		Vector2 velocity = Velocity;
		_separation?.ApplyToVelocity(ref velocity, dt);
		Velocity = velocity;
		MoveAndSlide();
	}

	public void ApplySeparation(Vector2 pushDir, float strength, float duration)
	{
		_separation?.ApplyImpulse(pushDir, strength, duration);
	}

	private void ResolvePlayer()
	{
		_player = GetNodeOrNull<Node2D>(PlayerPath);
		if (_player == null)
			_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
	}

	private void ResolveBehavior()
	{
		_behavior = GetNodeOrNull<EnemyBehaviorModule>(BehaviorPath);

		if (_behavior == null)
		{
			foreach (Node child in GetChildren())
			{
				if (child is EnemyBehaviorModule module)
				{
					_behavior = module;
					break;
				}
			}
		}

		_behavior?.OnInitialized(this);
	}

	private void ResolveSeparation()
	{
		_separation = GetNodeOrNull<EnemySeparationModule>(SeparationPath);

		if (_separation == null)
		{
			foreach (Node child in GetChildren())
			{
				if (child is EnemySeparationModule module)
				{
					_separation = module;
					break;
				}
			}
		}
	}

	private Vector2 GetDesiredVelocity(double delta)
	{
		if (_behavior != null && _behavior.Active)
			return _behavior.GetDesiredVelocity(this, _player, delta);

		// Fallback: simple chase behavior.
		if (_player == null)
			return Vector2.Zero;

		Vector2 toPlayer = _player.GlobalPosition - GlobalPosition;
		if (toPlayer.LengthSquared() < 0.0001f)
			return Vector2.Zero;

		return toPlayer.Normalized() * MaxSpeed;
	}
}
