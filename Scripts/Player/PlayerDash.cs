using Godot;

/*
 * PlayerDash:
 * - Handles dash input, cooldown gate, dash motion, and iframe window.
 * - Returns true while dash owns movement for current frame.
 */
public partial class PlayerDash : Node
{
	[Export] public string DashAction = "dash";
	[Export] public bool EnabledInCurrentCharacter = true;
	[Export] public float DashSpeed = 900f;
	[Export] public float DashDuration = 0.12f;
	[Export] public float DashCooldown = 0.6f;
	[Export] public float DashIFrame = 0.08f;

	private Player _player;
	private StabilitySystem _stabilitySystem;
	private bool _isDashing = false;
	private float _dashTimer = 0f;
	private float _cooldownTimer = 0f;
	private Vector2 _dashDir = Vector2.Right;
	private bool _isEnabled = true;

	public float CurrentCooldown => DashCooldown;
	public float CurrentSpeed => DashSpeed;
	public float CurrentDuration => DashDuration;

	public void Setup(Player player)
	{
		_player = player;
		ResolveStabilitySystem();
		_isEnabled = EnabledInCurrentCharacter;
	}

	public bool Tick(float dt, Vector2 inputDir)
	{
		if (!_isEnabled)
			return false;

		if (!IsInstanceValid(_stabilitySystem))
			ResolveStabilitySystem();

		if (_cooldownTimer > 0f)
			_cooldownTimer -= dt;

		if (!_isDashing && _cooldownTimer <= 0f && Input.IsActionJustPressed(DashAction))
			StartDash(inputDir);

		if (!_isDashing)
			return false;

		_dashTimer -= dt;
		float powerMult = _stabilitySystem?.GetPlayerPowerMultiplier() ?? 1f;
		_player.Velocity = _dashDir * DashSpeed * (1f + ((powerMult - 1f) * 0.5f));

		if (DashIFrame > 0f)
			_player.SetInvincible(DashIFrame);

		_player.MoveAndSlide();

		if (_dashTimer <= 0f)
			StopDash();

		return true;
	}

	private void ResolveStabilitySystem()
	{
		var list = GetTree().GetNodesInGroup("StabilitySystem");
		if (list.Count > 0)
			_stabilitySystem = list[0] as StabilitySystem;
	}

	public void SetEnabled(bool enabled)
	{
		_isEnabled = enabled;
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
