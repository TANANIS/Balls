using Godot;

public partial class Player
{
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

	private FrameCommand BuildFrameCommand()
	{
		Vector2 moveInput = Input.GetVector("left", "right", "up", "down");
		bool wantDash = Input.IsActionJustPressed(_dash?.DashAction ?? "dash");
		bool wantPrimaryAttack = Input.IsActionPressed(_primaryAttack?.AttackAction ?? InputActions.AttackPrimary);
		bool wantSecondaryAttack = Input.IsActionPressed(_secondaryAttack?.AttackAction ?? InputActions.AttackSecondary);
		return new FrameCommand(moveInput, wantDash, wantPrimaryAttack, wantSecondaryAttack);
	}

	private void ExecuteFrameCommand(float dt, in FrameCommand command)
	{
		if (command.MoveInput != Vector2.Zero)
			_lastMoveDir = command.MoveInput.Normalized();

		if (IsDead)
		{
			TickVisualState(dt, command.MoveInput, isDashActive: false);
			return;
		}

		ConsumePendingHurtCommand();

		bool dashOwnsMovement = _dash.Tick(dt, command.MoveInput, command.WantDash);
		if (dashOwnsMovement)
		{
			ClampInsideBounds();
			UpdatePhaseCamera(dt);
			TickVisualState(dt, command.MoveInput, isDashActive: true);
			return;
		}

		_movement.Tick(dt, command.MoveInput);
		_primaryAttack.Tick(dt, command.WantPrimaryAttack);
		_secondaryAttack.Tick(dt, command.WantSecondaryAttack);
		ClampInsideBounds();
		UpdatePhaseCamera(dt);
		TickVisualState(dt, command.MoveInput, isDashActive: _dash?.IsDashing ?? false);
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
}
