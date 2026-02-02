using Godot;
using System;
using System.Collections.Generic;

/*
 * CombatSystem.cs
 *
 * 最小可用版本（MVP）：
 * - 提供單一入口：RequestDamage(DamageRequest req)
 * - 做最基本裁決：有效性 / target 是否可受傷 / 無敵 / 防同 frame 重入
 * - 成立就呼叫 IDamageable.TakeDamage(...)
 *
 * 重要：
 * - 不管碰撞，不接 AreaEntered
 * - 不維護玩家/敵人的 timer
 * - 不做 VFX/SFX（之後可發事件給其他系統）
 */

public partial class CombatSystem : Node
{
	// 用於防止同一個 Source→Target 在同一個 physics frame 重複送出造成爆擊式扣血
	// Key = (source instance id, target instance id)
	private readonly HashSet<ulong> _frameHitGuard = new();

	private ulong _lastFrame = 0;

	public override void _Ready()
	{
		AddToGroup("CombatSystem");
	}

	public override void _PhysicsProcess(double delta)
	{
		// 每個 frame 開始時清空 guard（只保護「同 frame」）
		ulong frame = Engine.GetPhysicsFrames();
		if (frame != _lastFrame)
		{
			_frameHitGuard.Clear();
			_lastFrame = frame;
		}
	}

	/// <summary>
	/// 戰鬥系統唯一入口：送出傷害請求。
	/// Sensor / Bullet / EnemyHitbox 只能呼叫這裡，不能自己扣血。
	/// </summary>
	public void RequestDamage(in DamageRequest req)
	{
		// -------------------------
		// 0) 基本資料檢查
		// -------------------------
		if (!req.IsValid())
			return;

		// -------------------------
		// 1) 防同 frame 重入（避免時靈時不靈/爆扣）
		// -------------------------
		ulong guardKey = MakeGuardKey(req.Source, req.Target);
		if (_frameHitGuard.Contains(guardKey))
			return;

		_frameHitGuard.Add(guardKey);

		// -------------------------
		// 2) 目標是否可承受傷害（介面判定）
		// -------------------------
		if (req.Target is not IDamageable damageable)
			return;

		// -------------------------
		// 3) 裁決：死亡 / 無敵
		// -------------------------
		if (damageable.IsDead)
			return;

		if (damageable.IsInvincible)
			return;

		// -------------------------
		// 4) 最終傷害計算（目前先等於 base）
		// -------------------------
		int finalDamage = req.BaseDamage;
		if (finalDamage <= 0)
			return;

		// -------------------------
		// 5) 套用傷害（唯一能扣血的地方）
		// -------------------------
		damageable.TakeDamage(finalDamage, req.Source);


		// （可選）之後你要 DebugSystem 記錄，就在這裡發事件或呼叫 log
	}

	private static ulong MakeGuardKey(Node source, Node target)
	{
		// Godot Node 有 GetInstanceId()，可做唯一辨識
		// 壓成一個 ulong，降低 tuple 配置成本
		ulong a = (ulong)source.GetInstanceId();
		ulong b = (ulong)target.GetInstanceId();
		return (a << 32) ^ b;
	}
}


/*
 * CombatSystem.cs
 * 
 * 職責定位（Role & Responsibility）
 * ------------------------------------------------------------
 * CombatSystem 是「戰鬥裁決系統（Combat Authority）」。
 * 
 * 它是整個專案中唯一允許做以下事情的地方：
 * - 判斷一次傷害請求（Damage Request）是否成立
 * - 決定成立時的最終傷害結果（數值、是否被擋、是否免疫）
 * - 呼叫目標的對外 API（例如 Player.TakeDamage / Enemy.TakeDamage）
 * 
 * 重要：CombatSystem 是「裁決者」，不是「感測器」也不是「行為執行者」。
 * 它不直接參與碰撞，也不自己產生子彈或敵人。
 * 
 * ------------------------------------------------------------
 * 它【負責】的事情（MUST）
 * ------------------------------------------------------------
 * - 接收傷害請求（RequestDamage）
 *   請求來源可能是：
 *   - EnemyHitbox / Bullet / 任何命中感測器（Sensor）
 *   - PlayerWeapon（生成子彈後命中由 Bullet 感測器回報）
 * 
 * - 執行裁決流程（Resolve）
 *   典型裁決包含：
 *   - 目標是否存在 / 是否已死亡
 *   - 目標是否無敵（例如 Player.IsInvincible()）
 *   - 是否在同一 frame 重複命中（防重入/防連擊爆炸）
 *   - 友傷/陣營規則（可選）
 *   - 命中冷卻（可選：由 CombatSystem 統一管理）
 * 
 * - 最終落實傷害（Apply）
 *   - CombatSystem 不直接改 target 的欄位
 *   - 一律透過 target 的公開 API（例如 TakeDamage）
 * 
 * ------------------------------------------------------------
 * 它【不負責】的事情（MUST NOT）
 * ------------------------------------------------------------
 * - 不處理碰撞事件本身（不接 AreaEntered/BodyEntered）
 *   碰撞/命中是 Sensor 的責任，Sensor 只需送出 RequestDamage
 * 
 * - 不維護任何「玩家或敵人的時間性狀態」
 *   例如：
 *   - 不倒數玩家無敵時間
 *   - 不管理 dash timer
 *   - 不管理移動/射擊 cooldown
 *   ※ 時間性狀態的推進由 Entity 自己（Player/Enemy）或其模組處理
 * 
 * - 不讀取輸入、不控制移動、不生成/銷毀節點（除了必要的 hit feedback 可延後）
 * 
 * - 不成為 God System
 *   CombatSystem 的責任是「裁決與呼叫 API」，不是包辦所有戰鬥相關行為
 * 
 * ------------------------------------------------------------
 * 與其他模組/物件的邊界（Boundary Contract）
 * ------------------------------------------------------------
 * - Player / Enemy：
 *   - 提供最小必要的公開狀態查詢（IsDead / IsInvincible 等）
 *   - 提供承受傷害的 API（TakeDamage）
 * 
 * - Sensor（Hitbox/Hurtbox/Bullet）：
 *   - 只負責偵測接觸/命中
 *   - 不得扣血、不做規則判斷
 *   - 命中後只呼叫 CombatSystem.RequestDamage(...)
 * 
 * ------------------------------------------------------------
 * 設計目標（Design Goals）
 * -------*
 */
