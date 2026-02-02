using Godot;
using System;

/*
 * Bullet.cs
 *
 * 職責定位（Role & Responsibility）
 * ------------------------------------------------------------
 * Bullet 是「命中感測器（Sensor）」的一種：
 * - 負責移動（直線飛行）
 * - 負責偵測命中（AreaEntered / BodyEntered）
 * - 命中後只做一件事：送出 DamageRequest 給 CombatSystem
 *
 * 禁止事項：
 * - 不得直接扣血（不能呼叫 target.TakeDamage）
 * - 不得裁決規則（無敵/死亡/友傷等都交給 CombatSystem）
 */

public partial class Bullet : Area2D
{
	// =========================================================
	// 一、可調參數（Tuning）
	// =========================================================

	[Export] public float LifeTime = 1.5f;     // 子彈存在時間（秒）
	[Export] public string DamageTag = "bullet";

	// =========================================================
	// 二、內部狀態（Runtime State）
	// =========================================================

	private Vector2 _dir = Vector2.Right;
	private float _speed = 900f;
	private int _damage = 1;

	// 來源（通常是 Player），作為 DamageRequest.Source
	private Node _source;

	private float _lifeTimer = 0f;

	// 防止同一顆子彈在同一幀/同一瞬間觸發多次命中事件
	private bool _hasHit = false;

	// CombatSystem 快取
	private CombatSystem _combat;

	// =========================================================
	// 三、初始化（由 Weapon 呼叫）
	// =========================================================

	/*
	 * Weapon 會呼叫這個方法：
	 * bullet.Call("InitFromPlayer", _player, dir, speed, damage);
	 *
	 * 注意：
	 * - 這裡只做「資料設定」，不做裁決
	 */
	public void InitFromPlayer(Node source, Vector2 dir, float speed, int damage)
	{
		_source = source;
		_dir = (dir == Vector2.Zero) ? Vector2.Right : dir.Normalized();
		_speed = speed;
		_damage = damage;
	}

	// =========================================================
	// 四、Godot 生命週期
	// =========================================================

	public override void _Ready()
	{
		// 找 CombatSystem：用 group（建議）
		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combat = list[0] as CombatSystem;

		// 綁定命中事件（Area2D 可同時接 AreaEntered 與 BodyEntered）
		AreaEntered += OnAreaEntered;
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// -------------------------
		// 1) 壽命倒數
		// -------------------------
		_lifeTimer += dt;
		if (_lifeTimer >= LifeTime)
		{
			QueueFree();
			return;
		}

		// -------------------------
		// 2) 直線飛行
		// -------------------------
		GlobalPosition += _dir * _speed * dt;
	}

	// =========================================================
	// 五、命中處理（只送 DamageRequest）
	// =========================================================

	private void OnAreaEntered(Area2D other)
	{
		TryHit(other);
	}

	private void OnBodyEntered(Node2D other)
	{
		TryHit(other);
	}
	private void TryHit(Node other)
	{
		// --------------------------------------------------
		// 防止任何重複命中 / 重入
		// --------------------------------------------------
		if (_hasHit)
			return;

		// --------------------------------------------------
		// 撞到世界（地形、牆壁）→ 子彈消失
		// --------------------------------------------------
		if (other.IsInGroup("World"))
		{
			_hasHit = true;
			QueueFree();
			return;
		}

		// CombatSystem 不存在就不要做事（避免 NullRef）
		if (_combat == null)
			return;

		// 防呆：來源或目標不存在
		if (_source == null || other == null)
			return;

		// 不打自己
		if (other == _source)
			return;

		// 不是可受傷對象 → 不處理（穿透）
		if (other is not IDamageable)
			return;

		// --------------------------------------------------
		// 送出傷害請求（裁決在 CombatSystem）
		// --------------------------------------------------
		var req = new DamageRequest(
			source: _source,
			target: other,
			baseDamage: _damage,
			worldPos: GlobalPosition,
			tag: DamageTag
		);

		_combat.RequestDamage(req);

		_hasHit = true;
		QueueFree();
	}

}
