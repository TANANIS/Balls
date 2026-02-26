using Godot;

public partial class Player
{
	private const float MoveInputDeadzone = 0.12f;
	private const float MoveInputDropGraceSeconds = 0.04f;

	private readonly struct FrameCommand
	{
		public readonly Vector2 MoveInput;
		public readonly bool WantDash;
		public readonly bool WantPrimaryAttack;
		public readonly bool WantSecondaryAttack;

		public FrameCommand(Vector2 moveInput, bool wantDash, bool wantPrimaryAttack, bool wantSecondaryAttack)
		{
			MoveInput = moveInput;
			WantDash = wantDash;
			WantPrimaryAttack = wantPrimaryAttack;
			WantSecondaryAttack = wantSecondaryAttack;
		}
	}

	private float _pendingMovementFreezeSeconds = 0f;
	private float _pendingHurtAnimationSeconds = 0f;
	private Vector2 _lastStableMoveInput = Vector2.Zero;
	private float _moveInputDropGraceTimer = 0f;
	private float _attackLockTimer = 0f;

	private FrameCommand BuildFrameCommand()
	{
		Vector2 moveInput = Input.GetVector(
			InputActions.MoveLeft,
			InputActions.MoveRight,
			InputActions.MoveUp,
			InputActions.MoveDown);
		if (moveInput.LengthSquared() < (MoveInputDeadzone * MoveInputDeadzone))
			moveInput = Vector2.Zero;

		bool wantDash = Input.IsActionJustPressed(_dash?.DashAction ?? InputActions.Dash);
		bool wantPrimaryAttack = Input.IsActionPressed(_primaryAttack?.AttackAction ?? InputActions.AttackPrimary);
		bool wantSecondaryAttack = Input.IsActionPressed(_secondaryAttack?.AttackAction ?? InputActions.AttackSecondary);
		return new FrameCommand(moveInput, wantDash, wantPrimaryAttack, wantSecondaryAttack);
	}

	private void ExecuteFrameCommand(float dt, in FrameCommand command)
	{
		if (_attackLockTimer > 0f)
			_attackLockTimer = Mathf.Max(0f, _attackLockTimer - dt);
		bool isAttackLocked = _attackLockTimer > 0f;

		Vector2 stableMoveInput = StabilizeMoveInput(dt, command.MoveInput);
		if (stableMoveInput.LengthSquared() > 0.0001f)
			_lastMoveDir = stableMoveInput.Normalized();

		if (IsDead)
		{
			TickVisualState(dt, stableMoveInput, isDashActive: false);
			return;
		}

		ConsumePendingHurtCommand();

		bool dashOwnsMovement = _dash.Tick(dt, stableMoveInput, command.WantDash);
		if (dashOwnsMovement)
		{
			ClampInsideBounds();
			UpdatePhaseCamera(dt);
			TickVisualState(dt, stableMoveInput, isDashActive: true);
			return;
		}

		_movement.Tick(dt, stableMoveInput);
		_primaryAttack.Tick(dt, !isAttackLocked && command.WantPrimaryAttack);
		_secondaryAttack.Tick(dt, !isAttackLocked && command.WantSecondaryAttack);
		ClampInsideBounds();
		UpdatePhaseCamera(dt);
		TickVisualState(dt, stableMoveInput, isDashActive: _dash?.IsDashing ?? false);
	}

	private Vector2 StabilizeMoveInput(float dt, Vector2 rawInput)
	{
		if (rawInput.LengthSquared() > 0.0001f)
		{
			_lastStableMoveInput = rawInput;
			_moveInputDropGraceTimer = MoveInputDropGraceSeconds;
			return rawInput;
		}

		if (_moveInputDropGraceTimer > 0f && _lastStableMoveInput.LengthSquared() > 0.0001f)
		{
			_moveInputDropGraceTimer = Mathf.Max(0f, _moveInputDropGraceTimer - dt);
			return _lastStableMoveInput;
		}

		_lastStableMoveInput = Vector2.Zero;
		_moveInputDropGraceTimer = 0f;
		return Vector2.Zero;
	}

	private void QueueHurtCommand(float movementFreezeSeconds, float hurtAnimationSeconds)
	{
		_pendingMovementFreezeSeconds = Mathf.Max(_pendingMovementFreezeSeconds, Mathf.Max(0f, movementFreezeSeconds));
		_pendingHurtAnimationSeconds = Mathf.Max(_pendingHurtAnimationSeconds, Mathf.Max(0f, hurtAnimationSeconds));
	}

	private void ConsumePendingHurtCommand()
	{
		if (_pendingMovementFreezeSeconds > 0f)
		{
			ApplyHitMovementFreeze(_pendingMovementFreezeSeconds);
			_pendingMovementFreezeSeconds = 0f;
		}

		if (_pendingHurtAnimationSeconds > 0f)
		{
			TriggerHurtAnimation(_pendingHurtAnimationSeconds);
			_pendingHurtAnimationSeconds = 0f;
		}
	}

	private void ResetCommandPipelineRuntimeState()
	{
		_pendingMovementFreezeSeconds = 0f;
		_pendingHurtAnimationSeconds = 0f;
		_lastStableMoveInput = Vector2.Zero;
		_moveInputDropGraceTimer = 0f;
		_attackLockTimer = 0f;
	}
}
