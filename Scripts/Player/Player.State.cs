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
		Velocity = Vector2.Zero;
		_deathLogged = false;
		_health?.ResetToFull();
	}
}
