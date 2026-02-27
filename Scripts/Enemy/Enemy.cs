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
	[Export(PropertyHint.Range, "0,1,0.01")] public float KnockbackResistance = 0f;
	[Export] public NodePath PlayerPath = new NodePath("../../Player");
	[Export] public NodePath BehaviorPath = new NodePath("Behavior");
	[Export] public NodePath SeparationPath = new NodePath("Separation");
	[Export] public NodePath EventsPath = new NodePath("Events");
	[Export] public bool AutoFlipVisualByVelocityX = false;
	[Export] public float FlipFacingDeadzone = 10f;
	[Export(PropertyHint.Range, "0,2,0.01")] public float DespawnDelayOnDeathSeconds = 0f;

	private EnemyHealth _health;
	private Node2D _player;
	private StabilitySystem _stabilitySystem;
	private EnemyBehaviorModule _behavior;
	private EnemySeparationModule _separation;
	private AnimatedSprite2D _animatedSprite;
	private Sprite2D _sprite;
	private bool _facingLeft = false;
	private float _deathDespawnTimer = 0f;
	private bool _deathDespawnArmed = false;
	private readonly Godot.Collections.Array<EnemyEventModule> _events = new();

	public override void _Ready()
	{
		_health = GetNodeOrNull<EnemyHealth>("Health");
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("Sprite2D")
			?? GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
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
			if (!_deathDespawnArmed)
			{
				_deathDespawnArmed = true;
				_deathDespawnTimer = Mathf.Max(0f, DespawnDelayOnDeathSeconds);
				Velocity = Vector2.Zero;
			}

			_deathDespawnTimer -= dt;
			if (_deathDespawnTimer <= 0f)
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

		float preSeparationSpeed = Velocity.Length();
		Vector2 velocity = Velocity;
		_separation?.ApplyToVelocity(ref velocity, dt);
		float desiredSpeed = desired.Length();
		float maxAllowedSpeed = Mathf.Max(desiredSpeed, preSeparationSpeed) * 1.10f;
		if (maxAllowedSpeed > 0.001f && velocity.Length() > maxAllowedSpeed)
			velocity = velocity.Normalized() * maxAllowedSpeed;
		Velocity = velocity;
		UpdateVisualFacing(Velocity.X);
		MoveAndSlide();
	}

	private void UpdateVisualFacing(float velocityX)
	{
		if (!AutoFlipVisualByVelocityX)
			return;

		float deadzone = Mathf.Max(0f, FlipFacingDeadzone);
		if (Mathf.Abs(velocityX) <= deadzone)
			return;

		bool shouldFaceLeft = velocityX < 0f;
		if (shouldFaceLeft == _facingLeft)
			return;

		_facingLeft = shouldFaceLeft;
		if (_animatedSprite != null)
			_animatedSprite.FlipH = shouldFaceLeft;
		if (_sprite != null)
			_sprite.FlipH = shouldFaceLeft;
	}

	public void ApplySeparation(Vector2 pushDir, float strength, float duration)
	{
		float resistance = Mathf.Clamp(KnockbackResistance, 0f, 1f);
		float adjustedStrength = strength * (1f - resistance);
		if (adjustedStrength <= 0.01f)
			return;

		_separation?.ApplyImpulse(pushDir, adjustedStrength, duration);
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
