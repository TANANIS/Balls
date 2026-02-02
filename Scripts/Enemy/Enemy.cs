using Godot;
using System;
/*
 * Enemy.cs
 *
 * 職責定位（Role & Responsibility）
 * ------------------------------------------------------------
 * Enemy 本體是「狀態持有者」：
 * - 持有 HP / IsDead / 無敵等狀態
 * - 負責被扣血後的結果（死亡、消失、播放效果等）
 *
 * 注意：
 * - Enemy 本體不一定要實作 IDamageable（由 Hurtbox 實作即可）
 * - 但 Enemy 必須提供 TakeDamage 給 Hurtbox 轉發
 */

public partial class Enemy : CharacterBody2D
{
	// =========================================================
	// 一、狀態（State）
	// =========================================================

	[Export] public int MaxHp = 3;

	private int _hp;
	private bool _isDead = false;

	// 可選：敵人也可以有無敵（例如剛生成、或被擊退保護）
	private float _invincibleTimer = 0f;

	public bool IsDead => _isDead;
	public bool IsInvincible => _invincibleTimer > 0f;

	// =========================================================
	// 二、初始化（Init）
	// =========================================================

	public override void _Ready()
	{
		_hp = MaxHp;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// 無敵倒數：屬於敵人自身狀態（如果你不用敵人無敵，可以整段刪掉）
		if (_invincibleTimer > 0f)
			_invincibleTimer -= dt;

		if (_isDead)
			return;

		// TODO: 之後接 EnemyMovement / SeekPlayer / AI
	}

	// =========================================================
	// 三、受傷入口（由 Hurtbox 轉發進來）
	// =========================================================

	public void TakeDamage(int amount, object source)
	{
		// -------------------------
		// 1) 防守：已死或無敵就忽略
		// -------------------------
		if (_isDead) return;
		if (IsInvincible) return;

		// -------------------------
		// 2) 扣血
		// -------------------------
		_hp -= amount;

		// （可選）命中保護：避免同一瞬間多顆子彈貼臉秒殺
		// 你想要更硬派就刪掉這行
		// _invincibleTimer = Mathf.Max(_invincibleTimer, 0.05f);

		// -------------------------
		// 3) 死亡判定
		// -------------------------
		if (_hp <= 0)
		{
			Die(source);
		}
	}

	// =========================================================
	// 四、死亡處理
	// =========================================================

	private void Die(object source)
	{
		_isDead = true;

		// TODO: 播放死亡特效 / 掉落 / 計分
		QueueFree();
	}
}
