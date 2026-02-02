using Godot;
using System;
/*
 * PlayerMovement.cs
 * 
 * 職責定位（Role & Responsibility）
 * ------------------------------------------------------------
 * 移動模組：只負責「一般移動」的輸出。
 * 
 * - 輸入：inputDir（由 Player 讀取 Input 後傳入）
 * - 輸出：設定 Player.Velocity 並呼叫 Player.MoveAndSlide()
 * 
 * 禁止事項：
 * - 不處理 Dash
 * - 不處理傷害/無敵/死亡
 * - 不處理射擊
 */

public partial class PlayerMovement : Node
{
	// =========================================================
	// 一、可調參數（Movement Tuning）
	// =========================================================

	[Export] public float MaxSpeed = 320f;
	[Export] public float Accel = 2200f;
	[Export] public float Friction = 2600f;
	[Export] public float StopThreshold = 5f;

	// =========================================================
	// 二、依賴引用（Dependency）
	// =========================================================

	private Player _player;

	// =========================================================
	// 三、初始化（Setup）
	// =========================================================

	public void Setup(Player player)
	{
		_player = player;
	}

	// =========================================================
	// 四、每幀更新（Tick）
	// =========================================================

	public void Tick(float dt, Vector2 inputDir)
	{
		// -------------------------
		// 1) 計算目標速度
		// -------------------------
		// inputDir 由 Player 統一讀取 Input，再傳入這裡
		Vector2 targetVel = inputDir * MaxSpeed;

		// -------------------------
		// 2) 決定「加速」或「摩擦」速率
		// -------------------------
		// 有輸入 → 加速；沒輸入 → 摩擦（回到 0）
		float rate = (inputDir.LengthSquared() > 0f) ? Accel : Friction;

		// -------------------------
		// 3) 平滑逼近目標速度（保留手感）
		// -------------------------
		_player.Velocity = _player.Velocity.MoveToward(targetVel, rate * dt);

		// -------------------------
		// 4) 靜止閾值（避免微小速度導致抖動）
		// -------------------------
		if (inputDir == Vector2.Zero && _player.Velocity.Length() < StopThreshold)
			_player.Velocity = Vector2.Zero;

		// -------------------------
		// 5) 實際移動
		// -------------------------
		_player.MoveAndSlide();
	}
}
