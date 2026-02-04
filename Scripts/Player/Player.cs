using Godot;
using System;

/*
 * Player.cs
 *
 * Thin facade for player composition and input flow.
 */

public partial class Player : CharacterBody2D
{
	private PlayerHealth _health;
	private PlayerMovement _movement;
	private PlayerDash _dash;
	private PlayerWeapon _primaryAttack;
	private PlayerMelee _secondaryAttack;

	private Vector2 _lastMoveDir = Vector2.Right;
	public Vector2 LastMoveDir => _lastMoveDir;

	public bool IsDead => _health != null && _health.IsDead;
	public bool IsInvincible => _health != null && _health.IsInvincible;

	private bool _deathLogged = false;
	[Export] public bool UseMovementBounds = true;
	[Export] public Rect2 MovementBounds = new Rect2(48f, 48f, 1184f, 624f);

	public override void _Ready()
	{
		_health = GetNode<PlayerHealth>("Health");
		_movement = GetNode<PlayerMovement>("Movement");
		_dash = GetNode<PlayerDash>("Dash");
		_primaryAttack = GetNode<PlayerWeapon>("PrimaryAttack");
		_secondaryAttack = GetNode<PlayerMelee>("SecondaryAttack");

		if (_health != null)
			_health.Died += OnDied;

		_movement.Setup(this);
		_dash.Setup(this);
		_primaryAttack.Setup(this);
		_secondaryAttack.Setup(this);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (IsDead)
		{
			return;
		}

		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		if (inputDir != Vector2.Zero)
			_lastMoveDir = inputDir.Normalized();

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

	public void SetInvincible(float duration)
	{
		if (_health == null) return;
		_health.SetInvincible(duration);
	}

	public void TakeDamage(int amount, object source)
	{
		if (_health == null) return;
		_health.TakeDamage(amount, source);
	}

	public void EnterDashCollisionMode()
	{
		// TODO:
		// - switch collision layer
		// - switch collision mask
		// - optional invincibility window
	}

	public void ExitDashCollisionMode()
	{
		// TODO: revert EnterDashCollisionMode changes
	}

	private void OnDied()
	{
		if (_deathLogged) return;
		_deathLogged = true;
		DebugSystem.Log("[Player] Died.");
	}

	private void ClampInsideBounds()
	{
		if (!UseMovementBounds)
			return;

		Vector2 p = GlobalPosition;
		p.X = Mathf.Clamp(p.X, MovementBounds.Position.X, MovementBounds.Position.X + MovementBounds.Size.X);
		p.Y = Mathf.Clamp(p.Y, MovementBounds.Position.Y, MovementBounds.Position.Y + MovementBounds.Size.Y);
		GlobalPosition = p;
	}

	public void RespawnAt(Vector2 globalPosition)
	{
		GlobalPosition = globalPosition;
		Velocity = Vector2.Zero;
		_deathLogged = false;
		_health?.ResetToFull();
	}
}
