using Godot;

/*
 * PlayerDash:
 * - Handles dash input, cooldown gate, dash motion, and iframe window.
 * - Returns true while dash owns movement for current frame.
 */
public partial class PlayerDash : Node
{
	[Export] public string DashAction = "dash";
	[Export] public float DashSpeed = 900f;
	[Export] public float DashDuration = 0.12f;
	[Export] public float DashCooldown = 0.6f;
	[Export] public float DashIFrame = 0.08f;

	private Player _player;
	private bool _isDashing = false;
	private float _dashTimer = 0f;
	private float _cooldownTimer = 0f;
	private Vector2 _dashDir = Vector2.Right;

	public float CurrentCooldown => DashCooldown;
	public float CurrentSpeed => DashSpeed;
	public float CurrentDuration => DashDuration;

	public void Setup(Player player)
	{
		_player = player;
	}

	public bool Tick(float dt, Vector2 inputDir)
	{
		if (_cooldownTimer > 0f)
			_cooldownTimer -= dt;

		if (!_isDashing && _cooldownTimer <= 0f && Input.IsActionJustPressed(DashAction))
			StartDash(inputDir);

		if (!_isDashing)
			return false;

		_dashTimer -= dt;
		_player.Velocity = _dashDir * DashSpeed;

		if (DashIFrame > 0f)
			_player.SetInvincible(DashIFrame);

		_player.MoveAndSlide();

		if (_dashTimer <= 0f)
			StopDash();

		return true;
	}
}
