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
		AudioManager.Instance?.PlaySfxPlayerGetHit();
		DebugSystem.Log($"[PlayerHealth] Took {amount} damage. HP: {_hp}/{MaxHp}");

		if (HurtIFrame > 0f)
			SetInvincible(HurtIFrame);

		if (_hp <= 0 && !_isDead)
		{
			_isDead = true;
			AudioManager.Instance?.StopLowHpLoop();
			AudioManager.Instance?.PlaySfxPlayerDie();
			Died?.Invoke();
		}
		else
		{
			UpdateLowHpAudio();
		}
	}

	public void ResetToFull()
	{
		_hp = MaxHp;
		_isDead = false;
		_invincibleTimer = 0f;
		UpdateLowHpAudio();
	}

	public void AddMaxHp(int amount, bool healByAmount = true)
	{
		if (amount <= 0) return;

		MaxHp += amount;
		if (healByAmount)
			_hp += amount;

		if (_hp > MaxHp)
			_hp = MaxHp;

		UpdateLowHpAudio();
	}

	private void UpdateLowHpAudio()
	{
		if (_isDead)
			return;

		if (_hp <= 1)
			AudioManager.Instance?.StartLowHpLoop();
		else
			AudioManager.Instance?.StopLowHpLoop();
	}
}
