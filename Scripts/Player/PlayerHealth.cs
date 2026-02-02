using Godot;
using System;


/*
 * PlayerHealth.cs
 *
 * 職責定位
 * ------------------------------------------------------------
 * - 唯一持有玩家生命狀態：HP / IsDead / IsInvincible
 * - 推進 invincibleTimer（倒數）
 * - TakeDamage 僅做狀態落地，不做規則裁決（裁決仍在 CombatSystem）
 */

public partial class PlayerHealth : Node
{
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
		// 最低限度保險（就算外部繞過 CombatSystem 也不至於炸狀態）
		if (_isDead) return;
		if (IsInvincible) return;

		_hp -= amount;

		if (HurtIFrame > 0f)
			SetInvincible(HurtIFrame);

		if (_hp <= 0)
		{
			_isDead = true;
			// TODO: 可 EmitSignal 通知 UI / GameManager
		}
	}
}
