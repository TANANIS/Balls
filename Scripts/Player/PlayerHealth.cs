using Godot;
using System;

/*
 * PlayerHealth.cs
 *
 * Responsibilities:
 * - Owns player health state: HP / IsDead / IsInvincible
 * - Applies damage state only; rules are decided in CombatSystem
 */

public partial class PlayerHealth : Node
{
	public event Action Died;

	[Export] public int MaxHp = 3;
	[Export] public float HurtIFrame = 0.5f;

	private int _hp;
	private bool _isDead = false;
	private float _invincibleTimer = 0f;

	public int Hp => _hp;
	public bool IsDead => _isDead;
	public bool IsInvincible => _invincibleTimer > 0f;

	public override void _Ready()
	{
		_hp = MaxHp;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (_invincibleTimer > 0f)
			_invincibleTimer -= dt;
	}

	public void SetInvincible(float duration)
	{
		if (duration <= 0f) return;
		_invincibleTimer = Mathf.Max(_invincibleTimer, duration);
	}

	public void TakeDamage(int amount, object source)
	{
		// Minimal safeguard in case someone bypasses CombatSystem
		if (_isDead) return;
		if (IsInvincible) return;

		_hp -= amount;
		DebugSystem.Log($"[PlayerHealth] Took {amount} damage. HP: {_hp}/{MaxHp}");

		if (HurtIFrame > 0f)
			SetInvincible(HurtIFrame);

		if (_hp <= 0 && !_isDead)
		{
			_isDead = true;
			Died?.Invoke();
		}
	}

	public void ResetToFull()
	{
		_hp = MaxHp;
		_isDead = false;
		_invincibleTimer = 0f;
	}
}
