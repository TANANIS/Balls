

using Godot;
using System;

public partial class Player : CharacterBody2D
{

	/* =========================================================
	 * 移動參數（在 Inspector 調手感）
	 * ========================================================= */

	/// <summary>
	/// 最高移動速度（px/s）
	/// </summary>
	[Export] public float MaxSpeed = 320f;

	/// <summary>
	/// 加速度（px/s^2）
	/// </summary>
	[Export] public float Accel = 2400f;

	/// <summary>
	/// 摩擦力（px/s^2）
	/// </summary>
	[Export] public float Friction = 2800f;

	/// <summary>
	/// 停止門檻（px/s）
	/// </summary>
	[Export] public float StopThreshold = 8f;


	/* =========================================================
	 * 生命 / 受傷系統
	 * ========================================================= */

	[Export] public int MaxHp = 3;
	[Export] public float InvincibleTime = 0.6f;

	private int _hp;
	private float _invTimer = 0f;

	private Area2D _hurtbox;
	private bool _hurtboxMonitoringBeforeDash;
	private PlayerHurtbox _playerHurtbox;



	/* =========================
	 * 射擊參數
	 * ========================= */

	/// <summary>
	/// 子彈場景（拖 Bullet.tscn 進來）
	/// </summary>
	[Export] public PackedScene BulletScene;

	/// <summary>
	/// 射速限制（秒）
	/// </summary>
	[Export] public float FireCooldown = 0.22f;


	private float _fireTimer = 0f;
	private Marker2D _muzzle;

	/* =========================
* Dash / 閃避（空白鍵）
* ========================= */


	/// <summary>
	/// Dash 速度（px/s）
	/// </summary>
	[Export] public float DashSpeed = 900f;


	/// <summary>
	/// Dash 持續時間（秒）
	/// </summary>
	[Export] public float DashDuration = 0.12f;


	/// <summary>
	/// Dash 冷卻（秒）
	/// </summary>
	[Export] public float DashCooldown = 0.55f;


	/// <summary>
	/// Dash 是否給短暫無敵（秒），0=不給
	/// </summary>
	[Export] public float DashIFrame = 0.18f;


	private bool _isDashing = false;
	private float _dashTimer = 0f;
	private float _dashCooldownTimer = 0f;
	private Vector2 _dashDir = Vector2.Right;
	private Vector2 _lastMoveDir = Vector2.Right;

	private bool _isDead = false;

	// Dash 期間穿越敵人：暫存玩家原本的碰撞設定
	private uint _maskBeforeDash;
	private uint _layerBeforeDash;


	/* =========================================================
	 * Godot Life Cycle
	 * ========================================================= */

	public override void _Ready()
	{
		// 預設生成在畫面中央（之後可刪）
		GlobalPosition = GetViewportRect().Size * 0.5f;

		_hp = MaxHp;

		// 找槍口點（Player 節點下的 Muzzle）
		_muzzle = GetNodeOrNull<Marker2D>("Muzzle");

		if (BulletScene == null)
			GD.PrintErr("BulletScene is NULL. Please assign Bullet.tscn in Inspector.");
		if (_muzzle == null)
			GD.PrintErr("Muzzle node not found. Please add Marker2D named 'Muzzle' under Player.");

		_hurtbox = GetNodeOrNull<Area2D>("Hurtbox");
		if (_hurtbox == null)
			GD.PrintErr("Hurtbox node not found. Please add Area2D named 'Hurtbox' under Player.");

		_playerHurtbox = GetNodeOrNull<PlayerHurtbox>("Hurtbox");
		if (_playerHurtbox == null)
			GD.PrintErr("PlayerHurtbox script not found on node 'Hurtbox'.");


	}


	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// =========================
		// 無敵幀計時
		// =========================
		if (_invTimer > 0f)
			_invTimer -= dt;


		// Dash 冷卻計時
		if (_dashCooldownTimer > 0f)
			_dashCooldownTimer -= dt;


		if (_isDead)
		{
			CheckRestartInput();
			return;
		}


		// =========================
		// 移動
		// =========================
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
		if (inputDir != Vector2.Zero)
			_lastMoveDir = inputDir.Normalized();

		// =========================
		// Dash（空白鍵）
		// =========================
		if (!_isDashing && _dashCooldownTimer <= 0f && Input.IsActionJustPressed("dash"))
		{
			StartDash(inputDir);
		}


		if (_isDashing)
		{
			_dashTimer -= dt;
			Velocity = _dashDir * DashSpeed;


			// Dash 期間給短暫無敵（可選）
			if (DashIFrame > 0f)
				_invTimer = Mathf.Max(_invTimer, DashIFrame);


			MoveAndSlide();


			if (_dashTimer <= 0f)
			{
				_isDashing = false;
				_dashCooldownTimer = DashCooldown;

				ExitDashCollisionMode();
			}


			// Dash 時通常不允許射擊（比較乾淨），你想允許也行
			return;
		}


		// =========================
		// 一般移動
		// =========================
		Vector2 targetVel = inputDir * MaxSpeed;
		float rate = (inputDir.LengthSquared() > 0f) ? Accel : Friction;

		Velocity = Velocity.MoveToward(targetVel, rate * dt);

		if (inputDir == Vector2.Zero && Velocity.Length() < StopThreshold)
			Velocity = Vector2.Zero;

		MoveAndSlide();

		// =========================
		// 射擊
		// =========================
		UpdateShooting(dt);
	}


	/* =========================================================
	 * 射擊系統
	 * ========================================================= */

	private void UpdateShooting(float dt)
	{
		_fireTimer -= dt;

		bool wantFire = Input.IsActionPressed("fire");
		if (!wantFire) return;
		if (_fireTimer > 0f) return;
		if (BulletScene == null || _muzzle == null) return;

		_fireTimer = FireCooldown;

		SpawnBullet();
	}

	private void SpawnBullet()
	{
		var bullet = BulletScene.Instantiate<Bullet>();
		bullet.GlobalPosition = _muzzle.GlobalPosition;

		Vector2 aimDir = (GetGlobalMousePosition() - _muzzle.GlobalPosition).Normalized();
		if (aimDir == Vector2.Zero)
			aimDir = Vector2.Right;

		bullet.Direction = aimDir;

		Node projectiles = GetTree().CurrentScene.GetNodeOrNull("Projectiles");
		(projectiles ?? GetTree().CurrentScene).AddChild(bullet);
	}





	/* =========================================================
	* Dash helpers
	* ========================================================= */


	private void StartDash(Vector2 inputDir)
	{
		_isDashing = true;
		_dashTimer = DashDuration;


		// 沒有移動輸入時，沿用最後移動方向，避免原地 Dash 不知道往哪衝
		_dashDir = (inputDir != Vector2.Zero) ? inputDir.Normalized() : _lastMoveDir;
		if (_dashDir == Vector2.Zero)
			_dashDir = Vector2.Right;


		// 立即清空一般速度，避免 dash 開始瞬間被 MoveToward 拉扯
		Velocity = Vector2.Zero;

		EnterDashCollisionMode();
	}

	private void EnterDashCollisionMode()
	{
		// 1) 玩家移動時不把敵人當牆
		_maskBeforeDash = CollisionMask;
		SetCollisionMaskValue(3, false);

		// 2) 讓敵人也「看不到玩家」
		_layerBeforeDash = CollisionLayer;
		SetCollisionLayerValue(2, false);

		// 3) Dash 無敵（只針對 EnemyHitbox）
		_playerHurtbox?.SetDamageEnabled(false);
	}

	private void ExitDashCollisionMode()
	{
		CollisionMask = _maskBeforeDash;
		CollisionLayer = _layerBeforeDash;

		_playerHurtbox?.SetDamageEnabled(true);
	}




	/* =========================================================
	 * 受傷處理（由 PlayerHurtbox 呼叫）
	 * ========================================================= */

	public void TryTakeDamage(int damage, Vector2 hitFrom)
	{
		if (_invTimer > 0f)
			return;

		_hp -= damage;
		_invTimer = InvincibleTime;

		FlashHurt();

		GD.Print($"[Player] Damage {damage}, HP = {_hp}");

		if (_hp <= 0)
			Die();
	}

	private async void FlashHurt()
	{
		Color old = Modulate;
		Modulate = new Color(1f, 1f, 1f, 0.4f);
		await ToSignal(GetTree().CreateTimer(0.08f), "timeout");
		Modulate = old;
	}

	/// <summary>
	/// 死亡(尚未做死亡UI)
	/// </summary>

	private void Die()
	{
		if (_isDead) return;

		_isDead = true;

		GD.Print("[Player] DEAD - Press ENTER to Restart");

		// 停止移動
		Velocity = Vector2.Zero;

		// 可選：讓角色變暗一點，表示已死亡
		Modulate = new Color(0.6f, 0.6f, 0.6f, 1f);
	}



	private void CheckRestartInput()
	{
		// 先用 Godot 內建的確認鍵（Enter / Space）
		if (Input.IsActionJustPressed("ui_accept"))
		{
			Restart();
		}
	}

	private void Restart()
	{
		GD.Print("[Game] Restart");

		GetTree().ReloadCurrentScene();
	}

}
