// 把你當前的 Player 程式完整貼到這裡（包含 using、class、變數、_Ready/_Process/_PhysicsProcess、射擊/移動/受傷等）。
//
// 我會在同一份檔案裡直接改，並用註解標出：
// 1) 需要新增的欄位（HP、無敵幀、Timer 等）
// 2) 需要新增/調整的方法（TryTakeDamage、FlashHurt…）
// 3) 你原本邏輯哪裡要接上（例如 UpdateShooting 的呼叫點）

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
	}


	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// =========================
		// 無敵幀計時
		// =========================
		if (_invTimer > 0f)
			_invTimer -= dt;

		// =========================
		// 移動
		// =========================
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
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

	private void Die()
	{
		GD.Print("[Player] DEAD");
		// TODO: 重生 / GameOver
	}
}
