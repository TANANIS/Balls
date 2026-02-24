using Godot;

/*
 * PlayerDash:
 * - Handles dash input, cooldown gate, dash motion, and iframe window.
 * - Returns true while dash owns movement for current frame.
 */
public partial class PlayerDash : PlayerAbilityModule
{
	[Export] public string DashAction = "dash";
	[Export] public bool EnabledInCurrentCharacter = true;
	[Export] public float DashSpeed = 900f;
	[Export] public float DashDuration = 0.12f;
	[Export] public float DashCooldown = 0.6f;
	[Export] public float DashIFrame = 0.08f;

	private bool _isDashing = false;
	private float _dashTimer = 0f;
	private float _cooldownTimer = 0f;
	private Vector2 _dashDir = Vector2.Right;

	public float CurrentCooldown => DashCooldown;
	public float CurrentSpeed => DashSpeed;
	public float CurrentDuration => DashDuration;
	public bool IsDashing => _isDashing;

	public void Setup(Player player)
	{
		SetupAbility(player, EnabledInCurrentCharacter);
	}

	public bool Tick(float dt, Vector2 inputDir)
	{
		return Tick(dt, inputDir, Input.IsActionJustPressed(DashAction));
	}

	public bool Tick(float dt, Vector2 inputDir, bool wantDash)
	{
		if (!_isEnabled)
			return false;

		EnsureStabilitySystem();
		TickCooldown(ref _cooldownTimer, dt);

		if (!_isDashing && _cooldownTimer <= 0f && wantDash)
			StartDash(inputDir);

		if (!_isDashing)
			return false;

		_dashTimer -= dt;
		float powerMult = GetPowerMultiplier();
		_player.Velocity = _dashDir * DashSpeed * (1f + ((powerMult - 1f) * 0.5f));

		if (DashIFrame > 0f)
			_player.SetInvincible(DashIFrame);

		_player.MoveAndSlide();

		if (_dashTimer <= 0f)
			StopDash();

		return true;
	}

	public void SetEnabled(bool enabled)
	{
		SetEnabledState(enabled);
		EnabledInCurrentCharacter = enabled;
		if (!enabled && _isDashing)
		{
			_isDashing = false;
			_dashTimer = 0f;
			_player.Velocity = Vector2.Zero;
		}
	}

	public void SetDashAction(string action)
	{
		if (string.IsNullOrWhiteSpace(action))
			return;
		DashAction = action;
	}
}
