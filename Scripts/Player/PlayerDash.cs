using Godot;
using System;

/*
 * PlayerDash.cs
 * 
 * 職責定位（Role & Responsibility）
 * ------------------------------------------------------------
 * Dash 模組：只負責 Dash 行為本身（啟動/持續/結束/冷卻）。
 * 
 * - Dash 啟動條件：按下 dash action + 冷卻結束 + 不在 dash 中
 * - Dash 輸出：接管 Player.Velocity 並 MoveAndSlide()
 * - Dash 期間若要無敵：只能呼叫 Player.SetInvincible()（倒數仍由 Player）
 * 
 * 禁止事項：
 * - 不做傷害裁決
 * - 不接觸碰撞事件（AreaEntered/BodyEntered）
 */

public partial class PlayerDash : Node
{
	// =========================================================
	// 一、可調參數（Dash Tuning）
	// =========================================================

	[Export] public string DashAction = "dash";

	[Export] public float DashSpeed = 900f;
	[Export] public float DashDuration = 0.12f;
	[Export] public float DashCooldown = 0.6f;

	// Dash 期間給短暫無敵（只設定，倒數在 Player）
	[Export] public float DashIFrame = 0.08f;

	// =========================================================
	// 二、依賴引用（Dependency）
	// =========================================================

	private Player _player;

	// =========================================================
	// 三、內部狀態（Dash Internal State）
	// =========================================================

	private bool _isDashing = false;
	private float _dashTimer = 0f;
	private float _cooldownTimer = 0f;
	private Vector2 _dashDir = Vector2.Right;

	public float CurrentCooldown => DashCooldown;
	public float CurrentSpeed => DashSpeed;
	public float CurrentDuration => DashDuration;

	// =========================================================
	// 四、初始化（Setup）
	// =========================================================

	public void Setup(Player player)
	{
		_player = player;
	}

	// =========================================================
	// 五、每幀更新（Tick）
	// =========================================================

	/// <summary>
	/// 回傳 true 代表 Dash 本幀接管移動（Player.cs 需直接 return）
	/// </summary>
	public bool Tick(float dt, Vector2 inputDir)
	{
		// -------------------------
		// 1) 冷卻倒數（Dash 行為自身狀態）
		// -------------------------
		if (_cooldownTimer > 0f)
			_cooldownTimer -= dt;

		// -------------------------
		// 2) 嘗試啟動 Dash
		// -------------------------
		if (!_isDashing && _cooldownTimer <= 0f && Input.IsActionJustPressed(DashAction))
		{
			StartDash(inputDir);
		}

		// -------------------------
		// 3) 沒有在 Dash：不接管
		// -------------------------
		if (!_isDashing)
			return false;

		// -------------------------
		// 4) Dash 中：接管速度與移動
		// -------------------------
		_dashTimer -= dt;

		// 速度直接鎖定為 dashDir * dashSpeed（典型 dash 手感）
		_player.Velocity = _dashDir * DashSpeed;

		// dash 無敵：只設定/延長，不倒數
		if (DashIFrame > 0f)
			_player.SetInvincible(DashIFrame);

		_player.MoveAndSlide();

		// -------------------------
		// 5) Dash 結束：退出狀態並進入冷卻
		// -------------------------
		if (_dashTimer <= 0f)
		{
			_isDashing = false;
			_cooldownTimer = DashCooldown;

			// 若你之後要 dash 期間變更碰撞模式，hook 仍放在 Player
			_player.ExitDashCollisionMode();
		}

		return true;
	}

	// =========================================================
	// 六、啟動 Dash（Start）
	// =========================================================

	private void StartDash(Vector2 inputDir)
	{
		_isDashing = true;
		_dashTimer = DashDuration;

		AudioManager.Instance?.PlaySfxPlayerDash();

		// 若當下沒有輸入，就用玩家最後方向，避免原地 dash 無方向
		_dashDir = (inputDir == Vector2.Zero) ? _player.LastMoveDir : inputDir.Normalized();

		_player.EnterDashCollisionMode();
	}

	public void MultiplyCooldown(float factor)
	{
		DashCooldown = Mathf.Clamp(DashCooldown * factor, 0.02f, 10f);
	}

	public void AddSpeed(float amount)
	{
		DashSpeed = Mathf.Max(10f, DashSpeed + amount);
	}

	public void AddDuration(float amount)
	{
		DashDuration = Mathf.Clamp(DashDuration + amount, 0.02f, 3f);
	}
}
