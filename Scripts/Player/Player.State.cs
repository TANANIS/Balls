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
		_pendingMovementFreezeSeconds = 0f;
		_pendingHurtAnimationSeconds = 0f;
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
