using Godot;

/*
 * Player facade:
 * - Coordinates movement, dash, and attacks through a single state flow.
 * - Delegates HP to PlayerHealth, and reacts through events.
 */
public partial class Player : CharacterBody2D
{
	private PlayerHealth _health;
	private PlayerMovement _movement;
	private PlayerDash _dash;
	private PlayerWeapon _primaryAttack;
	private PlayerMelee _secondaryAttack;
	private Sprite2D _sprite;
	private AnimatedSprite2D _animatedSprite;
	private Node2D _visualRoot;
	private Node2D _skillVfxRoot;
	private SpriteFrames _baseSpriteFrames;
	private Vector2 _baseSpriteScale = Vector2.One;
	private Camera2D _camera;
	private StabilitySystem _stabilitySystem;
	private Vector2 _cameraBaseZoom = Vector2.One;

	private Vector2 _lastMoveDir = Vector2.Right;
	private bool _deathLogged = false;
	private float _attackAnimTimer = 0f;
	private float _hurtAnimTimer = 0f;
	private bool _deathAnimLocked = false;
	private float _attackAnimSpeedScale = 1f;
	private readonly PlayerStateMachine _stateMachine = new();
	private Vector2 _aimWorldPosition = Vector2.Zero;

	[Export] public bool UseMovementBounds = false;
	[Export] public Rect2 MovementBounds = new Rect2(48f, 48f, 1184f, 624f);

	public Vector2 LastMoveDir => _lastMoveDir;
	public bool IsDead => _health != null && _health.IsDead;
	public bool IsInvincible => _health != null && _health.IsInvincible;
	public Node2D SkillVfxRoot => _skillVfxRoot;

	public override void _Ready()
	{
		ResolveModules();
		BindSignals();
		SetupModules();
		ApplyCharacter(RunContext.Instance?.GetSelectedOrDefault() ?? DefaultCharacter);
		_aimWorldPosition = GetAimWorldPosition();
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		FrameCommand command = BuildFrameCommand();
		ExecuteFrameCommand(dt, command);
	}

	private void TickVisualState(float dt, Vector2 inputDir, bool isDashActive)
	{
		if (_animatedSprite == null)
			return;

		if (Mathf.Abs(_lastMoveDir.X) > 0.01f)
			_animatedSprite.FlipH = _lastMoveDir.X < 0f;

		if (_hurtAnimTimer > 0f)
			_hurtAnimTimer = Mathf.Max(0f, _hurtAnimTimer - dt);
		if (_attackAnimTimer > 0f)
			_attackAnimTimer = Mathf.Max(0f, _attackAnimTimer - dt);

		PlayerStateMachine.State next = _stateMachine.Evaluate(
			isDeathLocked: _deathAnimLocked,
			hasHurt: _hurtAnimTimer > 0f,
			isDashActive: isDashActive,
			hasAttack: _attackAnimTimer > 0f,
			hasMoveInput: inputDir.LengthSquared() > 0.0001f);
		ApplyVisualState(next);
	}

	private void ApplyVisualState(PlayerStateMachine.State state)
	{
		switch (state)
		{
			case PlayerStateMachine.State.Attack:
				SetVisualSpeedScale(_attackAnimSpeedScale);
				PlayVisualAnimation("attack");
				break;
			case PlayerStateMachine.State.Hurt:
				SetVisualSpeedScale(1f);
				PlayVisualAnimation("hurt");
				break;
			case PlayerStateMachine.State.Dash:
				SetVisualSpeedScale(1f);
				PlayVisualAnimationWithFallback("dash", "walk");
				break;
			case PlayerStateMachine.State.Death:
				SetVisualSpeedScale(1f);
				PlayVisualAnimation("death");
				break;
			case PlayerStateMachine.State.Move:
				SetVisualSpeedScale(1f);
				PlayVisualAnimation("walk");
				break;
			default:
				SetVisualSpeedScale(1f);
				PlayVisualAnimation("idle");
				break;
		}
	}

	public void TriggerPrimaryAttackAnimation(float duration = 0.42f)
	{
		if (_deathAnimLocked)
			return;
		_attackAnimTimer = Mathf.Max(_attackAnimTimer, Mathf.Clamp(duration, 0.05f, 1.5f));
		if (_hurtAnimTimer <= 0f)
		{
			_stateMachine.Force(PlayerStateMachine.State.Attack);
			ApplyVisualState(PlayerStateMachine.State.Attack);
		}
	}

	public float TriggerPrimaryAttackAnimationAndGetDuration(float fallbackDuration = 0.42f, float speedScale = 1f)
	{
		_attackAnimSpeedScale = Mathf.Clamp(speedScale, 0.2f, 6f);
		float resolved = GetVisualAnimationDurationSeconds("attack", fallbackDuration) / _attackAnimSpeedScale;
		TriggerPrimaryAttackAnimation(resolved);
		return resolved;
	}

	public void TriggerHurtAnimation(float duration = 0.30f)
	{
		if (_deathAnimLocked)
			return;
		_hurtAnimTimer = Mathf.Max(_hurtAnimTimer, Mathf.Clamp(duration, 0.06f, 0.8f));
		_stateMachine.Force(PlayerStateMachine.State.Hurt);
		ApplyVisualState(PlayerStateMachine.State.Hurt);
	}

	public void TriggerDeathAnimation()
	{
		_deathAnimLocked = true;
		_hurtAnimTimer = 0f;
		_attackAnimTimer = 0f;
		_attackAnimSpeedScale = 1f;
		_stateMachine.Force(PlayerStateMachine.State.Death);
		ApplyVisualState(PlayerStateMachine.State.Death);
	}

	public float GetDeathAnimationDurationSeconds(float fallbackDuration = 0.48f)
	{
		return GetVisualAnimationDurationSeconds("death", fallbackDuration);
	}

	public void ResetVisualState()
	{
		_deathAnimLocked = false;
		_hurtAnimTimer = 0f;
		_attackAnimTimer = 0f;
		_attackAnimSpeedScale = 1f;
		_stateMachine.Reset();
		ApplyVisualState(PlayerStateMachine.State.Idle);
	}

	private void PlayVisualAnimation(StringName animation)
	{
		if (_animatedSprite == null)
			return;
		if (_animatedSprite.SpriteFrames == null || !_animatedSprite.SpriteFrames.HasAnimation(animation))
			return;
		if (_animatedSprite.Animation != animation)
			_animatedSprite.Play(animation);
	}

	private void PlayVisualAnimationWithFallback(StringName primary, StringName fallback)
	{
		if (_animatedSprite?.SpriteFrames == null)
			return;
		if (_animatedSprite.SpriteFrames.HasAnimation(primary))
		{
			PlayVisualAnimation(primary);
			return;
		}
		PlayVisualAnimation(fallback);
	}

	private float GetVisualAnimationDurationSeconds(StringName animation, float fallbackDuration)
	{
		float fallback = Mathf.Clamp(fallbackDuration, 0.05f, 1.5f);
		if (_animatedSprite?.SpriteFrames == null)
			return fallback;
		if (!_animatedSprite.SpriteFrames.HasAnimation(animation))
			return fallback;

		int frameCount = _animatedSprite.SpriteFrames.GetFrameCount(animation);
		float speed = (float)_animatedSprite.SpriteFrames.GetAnimationSpeed(animation);
		if (frameCount <= 0 || speed <= 0f)
			return fallback;

		float durationUnits = 0f;
		for (int i = 0; i < frameCount; i++)
			durationUnits += (float)_animatedSprite.SpriteFrames.GetFrameDuration(animation, i);

		float seconds = durationUnits / speed;
		return Mathf.Clamp(seconds, 0.05f, 1.5f);
	}

	private void SetVisualSpeedScale(float speedScale)
	{
		if (_animatedSprite == null)
			return;
		float next = Mathf.Clamp(speedScale, 0.1f, 8f);
		if (!Mathf.IsEqualApprox(_animatedSprite.SpeedScale, next))
			_animatedSprite.SpeedScale = next;
	}
}
