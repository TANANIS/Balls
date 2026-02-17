using Godot;

/*
 * Player facade:
 * - Coordinates movement, dash, and attacks.
 * - Delegates health/damage state to PlayerHealth.
 */
public partial class Player : CharacterBody2D
{
	private PlayerHealth _health;
	private PlayerMovement _movement;
	private PlayerDash _dash;
	private PlayerWeapon _primaryAttack;
	private PlayerMelee _secondaryAttack;

	private Vector2 _lastMoveDir = Vector2.Right;
	private bool _deathLogged = false;

	[Export] public bool UseMovementBounds = true;
	[Export] public Rect2 MovementBounds = new Rect2(48f, 48f, 1184f, 624f);

	public Vector2 LastMoveDir => _lastMoveDir;
	public bool IsDead => _health != null && _health.IsDead;
	public bool IsInvincible => _health != null && _health.IsInvincible;

	public override void _Ready()
	{
		ResolveModules();
		BindSignals();
		SetupModules();
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		if (IsDead)
			return;

		// Collect movement intent and remember last non-zero facing.
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		if (inputDir != Vector2.Zero)
			_lastMoveDir = inputDir.Normalized();

		// Dash owns movement while active.
		if (_dash.Tick(dt, inputDir))
		{
			ClampInsideBounds();
			return;
		}

		_movement.Tick(dt, inputDir);
		_primaryAttack.Tick(dt);
		_secondaryAttack.Tick(dt);
		ClampInsideBounds();
	}
}
