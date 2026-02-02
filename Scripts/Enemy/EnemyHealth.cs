using Godot;
using System;

/*
 * EnemyHealth.cs
 *
 * 職責定位（Role & Responsibility）
 * ------------------------------------------------------------
 * - 唯一持有敵人的生命狀態：HP / IsDead / IsInvincible
 * - 推進 invincibleTimer（倒數）
 * - TakeDamage 僅做狀態落地，不做規則裁決（裁決仍在 CombatSystem）
 *
 * 禁止：
 * - 不做戰鬥規則（暴擊、減傷、友傷等）
 * - 不做 AI / 移動 / 追蹤玩家
 */

public partial class EnemyHealth : Node
{
	// =========================================================
	// 一、可調參數（Tuning）
	// =========================================================

	[Export] public int MaxHp = 3;

	// 可選：受擊後短暫無敵，避免同一瞬間被多顆子彈貼臉秒殺
	[Export] public float HurtIFrame = 0.05f;

	// =========================================================
	// 二、狀態（State）
	// =========================================================

	private int _hp;
	private bool _isDead = false;
	private float _invincibleTimer = 0f;

	public int Hp => _hp;
	public bool IsDead => _isDead;
	public bool IsInvincible => _invincibleTimer > 0f;

	// =========================================================
	// 三、初始化 / 時間推進
	// =========================================================

	public override void _Ready()
	{
		_hp = MaxHp;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// 無敵倒數：狀態時間推進在 Health
		if (_invincibleTimer > 0f)
			_invincibleTimer -= dt;
	}

	// =========================================================
	// 四、狀態修改 API
	// =========================================================

	public void SetInvincible(float duration)
	{
		if (duration <= 0f) return;
		_invincibleTimer = Mathf.Max(_invincibleTimer, duration);
	}

	// CombatSystem 最終呼叫：狀態落地
	public void TakeDamage(int amount, object source)
	{
		// 最低限度保險
		if (_isDead) return;
		if (IsInvincible) return;

		_hp -= amount;

		if (HurtIFrame > 0f)
			SetInvincible(HurtIFrame);

		if (_hp <= 0)
		{
			_isDead = true;

			// TODO:
			// - 可以 EmitSignal 通知 Enemy 播放死亡效果
			// - 或由 Enemy 每幀輪詢 IsDead 來 QueueFree
		}
	}
}
