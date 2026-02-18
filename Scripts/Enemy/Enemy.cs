using Godot;

/*
 * Enemy actor:
 * - Pulls intent from behavior module.
 * - Applies separation impulse module.
 * - Emits lifecycle events to event modules.
 */
public partial class Enemy : CharacterBody2D
{
	[Export] public float MaxSpeed = 160f;
	[Export] public float Accel = 1200f;
	[Export] public float Friction = 900f;
	[Export] public NodePath PlayerPath = new NodePath("../../Player");
	[Export] public NodePath BehaviorPath = new NodePath("Behavior");
	[Export] public NodePath SeparationPath = new NodePath("Separation");
	[Export] public NodePath EventsPath = new NodePath("Events");

	private EnemyHealth _health;
	private Node2D _player;
	private StabilitySystem _stabilitySystem;
	private EnemyBehaviorModule _behavior;
	private EnemySeparationModule _separation;
	private readonly Godot.Collections.Array<EnemyEventModule> _events = new();

	public override void _Ready()
	{
		_health = GetNodeOrNull<EnemyHealth>("Health");
		ResolvePlayer();
		ResolveStabilitySystem();
		ResolveBehavior();
		ResolveSeparation();
		ResolveEvents();
		EmitSpawned();
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
		if (!IsInstanceValid(_stabilitySystem))
			ResolveStabilitySystem();

		Vector2 desired = GetDesiredVelocity(delta);
		if (_stabilitySystem != null)
			desired *= _stabilitySystem.GetEnemySpeedMultiplier();
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

	public void NotifyDamaged(int amount, object source)
	{
		ForEachActiveEventModule(evt => evt.OnDamaged(this, amount, source));
	}

	public void NotifyHitPlayer(Node playerTarget)
	{
		ForEachActiveEventModule(evt => evt.OnHitPlayer(this, playerTarget));
	}

	public void NotifyDeath(object killer)
	{
		ForEachActiveEventModule(evt => evt.OnDeath(this, killer));
	}
}
