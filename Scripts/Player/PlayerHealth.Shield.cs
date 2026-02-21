using Godot;

public partial class PlayerHealth
{
	public void EnableShield(float cooldownSeconds)
	{
		float cd = Mathf.Clamp(cooldownSeconds, 1f, 120f);
		if (!_shieldEnabled)
		{
			_shieldEnabled = true;
			_shieldCooldownSeconds = cd;
			_shieldCooldownTimer = 0f;
			RefreshShieldVisual(force: true);
			DebugSystem.Log($"[PlayerHealth] Shield enabled. Cooldown={_shieldCooldownSeconds:0.##}s");
			return;
		}

		_shieldCooldownSeconds = Mathf.Min(_shieldCooldownSeconds, cd);
		RefreshShieldVisual(force: true);
		DebugSystem.Log($"[PlayerHealth] Shield cooldown updated. Cooldown={_shieldCooldownSeconds:0.##}s");
	}
}
