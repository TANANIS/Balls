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
	private PlayerWeapon _weapon;

	private Vector2 _lastMoveDir = Vector2.Right;
	public Vector2 LastMoveDir => _lastMoveDir;

	public bool IsDead => _health != null && _health.IsDead;
	public bool IsInvincible => _health != null && _health.IsInvincible;

	private bool _deathLogged = false;

	public override void _Ready()
	{
		_health = GetNode<PlayerHealth>("Health");
		_movement = GetNode<PlayerMovement>("Movement");
		_dash = GetNode<PlayerDash>("Dash");
		_weapon = GetNode<PlayerWeapon>("Weapon");

		if (_health != null)
			_health.Died += OnDied;

		_movement.Setup(this);
		_dash.Setup(this);
		_weapon.Setup(this);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (IsDead)
		{
			CheckRestartInput();
			return;
		}

		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		if (inputDir != Vector2.Zero)
			_lastMoveDir = inputDir.Normalized();

		if (_dash.Tick(dt, inputDir))
			return;

		_movement.Tick(dt, inputDir);
		_weapon.Tick(dt);
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
		DebugSystem.Log("[Player] Died. Press Enter to restart.");
	}

	private void CheckRestartInput()
	{
		if (Input.IsActionJustPressed("ui_accept"))
			GetTree().ReloadCurrentScene();
	}
}
