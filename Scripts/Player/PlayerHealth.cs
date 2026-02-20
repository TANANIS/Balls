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
	[Export] public int RegenAmount = 0;
	[Export] public float RegenIntervalSeconds = 60f;

	private int _hp;
	private bool _isDead = false;
	private float _invincibleTimer = 0f;
	private float _regenTimer = 0f;

	public int Hp => _hp;
	public bool IsDead => _isDead;
	public bool IsInvincible => _invincibleTimer > 0f;

	public override void _Ready()
	{
		_hp = MaxHp;
		_regenTimer = Mathf.Max(0f, RegenIntervalSeconds);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (_invincibleTimer > 0f)
			_invincibleTimer -= dt;
		TickRegen(dt);
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
		if (RegenAmount > 0 && RegenIntervalSeconds > 0f)
			_regenTimer = RegenIntervalSeconds;

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
		_regenTimer = Mathf.Max(0f, RegenIntervalSeconds);
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

	public void SetBaseStats(int maxHp, float hurtIFrame, bool refill = true)
	{
		MaxHp = Mathf.Max(1, maxHp);
		HurtIFrame = Mathf.Max(0f, hurtIFrame);
		if (refill)
		{
			_hp = MaxHp;
			_isDead = false;
			_invincibleTimer = 0f;
			UpdateLowHpAudio();
		}
		else if (_hp > MaxHp)
		{
			_hp = MaxHp;
		}
	}

	public void SetRegen(int amount, float intervalSeconds)
	{
		RegenAmount = Mathf.Max(0, amount);
		RegenIntervalSeconds = Mathf.Max(0f, intervalSeconds);
		_regenTimer = Mathf.Max(0f, RegenIntervalSeconds);
	}

	private void TickRegen(float dt)
	{
		if (_isDead || RegenAmount <= 0 || RegenIntervalSeconds <= 0f)
			return;
		if (_hp >= MaxHp)
			return;

		_regenTimer -= dt;
		if (_regenTimer > 0f)
			return;

		_hp = Mathf.Min(MaxHp, _hp + RegenAmount);
		_regenTimer = RegenIntervalSeconds;
		UpdateLowHpAudio();
	}
}
