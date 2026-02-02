using Godot;
using System;

/*
 * Player.cs
 *
 * 職責定位（Role & Responsibility）
 * ------------------------------------------------------------
 * Player 是「玩家 Entity 的門面（Facade）＋行為調度器」。
 *
 * - 讀取輸入、維護方向記憶
 * - 調度模組（Movement / Dash / Weapon）
 * - 透過 Health 轉發查詢狀態（IsDead / IsInvincible）與狀態修改（SetInvincible）
 *
 * 禁止：
 * - 不持有 HP / 無敵 timer / 死亡旗標（這些都在 PlayerHealth）
 * - 不做戰鬥裁決（CombatSystem 才做）
 */

public partial class Player : CharacterBody2D
{
	// =========================================================
	// 一、依賴：玩家 Health（唯一狀態持有者）
	// =========================================================

	private PlayerHealth _health;

	// =========================================================
	// 二、方向記憶（供 Dash 等模組使用）
	// =========================================================

	private Vector2 _lastMoveDir = Vector2.Right;
	public Vector2 LastMoveDir => _lastMoveDir;

	// =========================================================
	// 三、對外只讀狀態（轉發）
	// =========================================================

	public bool IsDead => _health != null && _health.IsDead;
	public bool IsInvincible => _health != null && _health.IsInvincible;

	// =========================================================
	// 四、子模組（Internal Modules）
	// =========================================================

	private PlayerMovement _movement;
	private PlayerDash _dash;
	private PlayerWeapon _weapon;

	// =========================================================
	// 五、初始化：組裝模組（Composition）
	// =========================================================

	public override void _Ready()
	{
		// 結構就是合約：節點名稱必須匹配
		_health = GetNode<PlayerHealth>("Health");
		_movement = GetNode<PlayerMovement>("Movement");
		_dash = GetNode<PlayerDash>("Dash");
		_weapon = GetNode<PlayerWeapon>("Weapon");

		// 將 Player 自己傳給模組
		_movement.Setup(this);
		_dash.Setup(this);
		_weapon.Setup(this);
	}

	// =========================================================
	// 六、主狀態迴圈（State Tick）
	// =========================================================

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// 1) 死亡狀態 gating：死亡後停止模組，只允許重開/提示
		if (IsDead)
		{
			CheckRestartInput();
			return;
		}

		// 2) 讀取輸入，更新方向記憶（不在這裡實作移動）
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		if (inputDir != Vector2.Zero)
			_lastMoveDir = inputDir.Normalized();

		// 3) Dash（高優先權）：若接管則直接 return
		if (_dash.Tick(dt, inputDir))
			return;

		// 4) 一般移動
		_movement.Tick(dt, inputDir);

		// 5) 攻擊 / 射擊
		_weapon.Tick(dt);
	}

	// =========================================================
	// 七、狀態修改 API（轉發給 Health）
	// =========================================================

	public void SetInvincible(float duration)
	{
		if (_health == null) return;
		_health.SetInvincible(duration);
	}

	// （可選）若你希望某些地方仍用 Player 作為入口，可以保留轉發
	public void TakeDamage(int amount, object source)
	{
		if (_health == null) return;
		_health.TakeDamage(amount, source);
	}

	// =========================================================
	// 八、Dash 碰撞模式切換（預留 hook）
	// =========================================================

	public void EnterDashCollisionMode()
	{
		// TODO:
		// - 關閉某些碰撞 layer
		// - 調整 collision mask
		// - 或開啟「穿敵」模式
	}

	public void ExitDashCollisionMode()
	{
		// TODO: 復原 EnterDashCollisionMode 所做的事情
	}

	// =========================================================
	// 九、死亡 / 重開（暫時保留）
	// =========================================================

	private void CheckRestartInput()
	{
		// TODO: 之後可搬到 UI / GameManager
	}
}
