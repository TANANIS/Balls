using Godot;

public partial class Player
{
	public void SetInvincible(float duration)
	{
		if (_health == null)
			return;
		_health.SetInvincible(duration);
	}

	public void TakeDamage(int amount, object source)
	{
		if (_health == null)
			return;
		_health.TakeDamage(amount, source);
	}

	public void ApplyHitMovementFreeze(float duration)
	{
		_movement?.ApplyMovementFreeze(duration);
	}

	public void LockAttacks(float durationSeconds, bool interruptCurrentAttack = true)
	{
		if (durationSeconds <= 0f)
			return;

		_attackLockTimer = Mathf.Max(_attackLockTimer, durationSeconds);

		if (!interruptCurrentAttack)
			return;

		_primaryAttack?.InterruptCurrentAttack();
		_secondaryAttack?.InterruptCurrentAttack();
		_attackAnimTimer = 0f;
		_attackAnimSpeedScale = 1f;

		if (!_deathAnimLocked && _hurtAnimTimer <= 0f)
		{
			_stateMachine.Force(PlayerStateMachine.State.Idle);
			ApplyVisualState(PlayerStateMachine.State.Idle);
		}
	}

	public void EnterDashCollisionMode()
	{
		// Reserved hook for dash-specific collision layer/mask changes.
	}

	public void ExitDashCollisionMode()
	{
		// Reserved hook for reverting dash collision mode changes.
	}

	public void RespawnAt(Vector2 globalPosition)
	{
		// Reset transient runtime state for a fresh run.
		GlobalPosition = globalPosition;
		ResetForNewRunState();
	}

	public void ResetForNewRunState()
	{
		Velocity = Vector2.Zero;
		_lastMoveDir = Vector2.Right;
		ResetCommandPipelineRuntimeState();
		_deathLogged = false;
		_health?.ResetToFull();
		_movement?.ResetRuntimeState();
		_dash?.ResetRuntimeState();
		_primaryAttack?.ResetRuntimeState();
		_secondaryAttack?.ResetRuntimeState();
		if (_camera != null)
			_camera.Zoom = _cameraBaseZoom;
		ResetVisualState();
	}
}
