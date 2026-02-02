using Godot;
using System;

/*
 * EnemyHurtbox.cs
 *
 * 職責定位（Role & Responsibility）
 * ------------------------------------------------------------
 * Hurtbox 是「受傷入口」：
 * - 必須實作 IDamageable，才能被 Bullet 命中後辨識為可受傷目標
 * - 不持有 HP，不裁決戰鬥規則
 * - 只把 TakeDamage 轉交給 Enemy 本體
 */

public partial class EnemyHurtbox : Area2D, IDamageable
{
	// =========================================================
	// 一、依賴引用（Dependency）
	// =========================================================

	private Enemy _enemy;

	// =========================================================
	// 二、初始化（Setup）
	// =========================================================

	public override void _Ready()
	{
		// Hurtbox 必須掛在 Enemy 子節點底下
		_enemy = GetParent<Enemy>();

		if (_enemy == null)
			GD.PrintErr("[EnemyHurtbox] Parent is not Enemy. Please make Hurtbox a child of Enemy.");
	}

	// =========================================================
	// 三、IDamageable：查詢狀態（轉發）
	// =========================================================

	public bool IsDead => _enemy != null && _enemy.IsDead;

	public bool IsInvincible => _enemy != null && _enemy.IsInvincible;

	// =========================================================
	// 四、IDamageable：承受傷害（轉發）
	// =========================================================

	public void TakeDamage(int amount, object source)
	{
		// Hurtbox 不處理 HP，只轉交
		if (_enemy == null) return;

		_enemy.TakeDamage(amount, source);
	}
}
