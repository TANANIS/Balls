using Godot;

/*
 * Player.cs
 *
 * 職責定位（Role & Responsibility）
 * ------------------------------------------------------------
 * Player.cs 是「玩家 Entity 的狀態機（State Machine）＋門面（Facade）」。
 *
 * - 持有玩家自身狀態（死亡、無敵、方向記憶...）並推進時間
 * - 統一調度 Player 內部模組（Movement / Dash / Weapon）
 * - 不實作行為細節（行為細節在模組）
 * - 不做戰鬥裁決（CombatSystem 才做）
 */

public partial class Player : CharacterBody2D
{
	// =========================================================
	// 一、玩家核心狀態（Core Player State）
	// =========================================================

	/*
	 * 是否死亡
	 * - 屬於玩家的「終態狀態」
	 * - 一旦為 true，玩家不再處理移動 / 行為
	 */
	private bool _isDead = false;

	/*
	 * 無敵剩餘時間（秒）
	 * - 這是「玩家狀態」，不是戰鬥規則
	 * - 誰可以設定它：
	 *   - PlayerDash（Dash 給短暫無敵）
	 *   - CombatSystem（未來可能有受擊保護）
	 * - 誰不能維護它：
	 *   - Dash / CombatSystem 不得自行倒數
	 *   - 只能由 Player 自己在每個 frame 推進
	 */
	private float _invincibleTimer = 0f;

	/*
	 * 上一次非零移動方向
	 * - 用途：
	 *   - Dash 在沒有輸入時仍有合理方向
	 * - 這是「玩家狀態記憶」，不屬於 Movement 行為本身
	 */
	private Vector2 _lastMoveDir = Vector2.Right;

	// =========================================================
	// 二、對外公開狀態（只讀查詢）
	// =========================================================

	// 給 CombatSystem / UI / Debug 查詢：玩家是否死亡
	public bool IsDead => _isDead;

	// 給 CombatSystem 查詢：玩家是否無敵
	public bool IsInvincible => _invincibleTimer > 0f;

	// 給 Dash / Weapon 讀取：最後移動方向
	public Vector2 LastMoveDir => _lastMoveDir;

	// =========================================================
	// 三、Player 子模組（Internal Modules）
	// =========================================================

	private PlayerMovement _movement;
	private PlayerDash _dash;
	private PlayerWeapon _weapon;

	// =========================================================
	// 四、初始化：組裝模組（Composition）
	// =========================================================

	public override void _Ready()
	{
		// 固定以子節點名稱取得模組（結構就是合約）
		_movement = GetNode<PlayerMovement>("Movement");
		_dash = GetNode<PlayerDash>("Dash");
		_weapon = GetNode<PlayerWeapon>("Weapon");

		// 將 Player 自己傳給模組
		_movement.Setup(this);
		_dash.Setup(this);
		_weapon.Setup(this);
	}

	// =========================================================
	// 五、主狀態迴圈（State Tick）
	// =========================================================

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// 1) 推進玩家自身狀態（時間性）
		if (_invincibleTimer > 0f)
			_invincibleTimer -= dt;

		// 2) 死亡狀態處理：死亡後停止模組，只允許重開/提示
		if (_isDead)
		{
			CheckRestartInput();
			return;
		}

		// 3) 讀取輸入，更新方向記憶（不在這裡實作移動）
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		if (inputDir != Vector2.Zero)
			_lastMoveDir = inputDir.Normalized();

		// 4) Dash（高優先權）：若接管則直接 return
		if (_dash.Tick(dt, inputDir))
			return;

		// 5) 一般移動
		_movement.Tick(dt, inputDir);

		// 6) 攻擊 / 射擊
		_weapon.Tick(dt);
	}

	// =========================================================
	// 六、對外狀態修改 API（只能透過方法改）
	// =========================================================

	/*
	 * 設定無敵時間
	 * - 只允許「覆蓋或延長」
	 * - 不允許縮短（避免互相踩狀態）
	 */
	public void SetInvincible(float duration)
	{
		if (duration <= 0f)
			return;

		_invincibleTimer = Mathf.Max(_invincibleTimer, duration);
	}

	// =========================================================
	// Dash 碰撞模式切換（預留 hook）
	// =========================================================

	public void EnterDashCollisionMode()
	{
		// 目前不做任何事
		// 之後你可以在這裡：
		// - 關閉某些碰撞 layer
		// - 調整 collision mask
		// - 或開啟「穿敵」模式
	}

	public void ExitDashCollisionMode()
	{
		// 復原 EnterDashCollisionMode 所做的事情
	}


	// =========================================================
	// 七、死亡 / 重開（暫時保留）
	// =========================================================

	private void CheckRestartInput()
	{
		// TODO: 之後可搬到 UI / GameManager
	}
}
