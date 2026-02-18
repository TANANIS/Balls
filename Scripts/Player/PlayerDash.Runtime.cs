using Godot;

public partial class PlayerDash
{
	private void StartDash(Vector2 inputDir)
	{
		_isDashing = true;
		_dashTimer = DashDuration;
		AudioManager.Instance?.PlaySfxPlayerDash();

		// If no input this frame, preserve the last movement facing.
		_dashDir = inputDir == Vector2.Zero ? _player.LastMoveDir : inputDir.Normalized();
		_player.EnterDashCollisionMode();
	}

	private void StopDash()
	{
		_isDashing = false;
		float powerMult = _stabilitySystem?.GetPlayerPowerMultiplier() ?? 1f;
		_cooldownTimer = DashCooldown / Mathf.Max(0.1f, powerMult);
		_player.ExitDashCollisionMode();
	}

	public void MultiplyCooldown(float factor)
	{
		DashCooldown = Mathf.Clamp(DashCooldown * factor, 0.02f, 10f);
	}

	public void AddSpeed(float amount)
	{
		DashSpeed = Mathf.Max(10f, DashSpeed + amount);
	}

	public void AddDuration(float amount)
	{
		DashDuration = Mathf.Clamp(DashDuration + amount, 0.02f, 3f);
	}
}
