using Godot;
using System;

/*
 * PlayerWeapon.cs
 * 
 * 職責定位（Role & Responsibility）
 * ------------------------------------------------------------
 * Weapon 模組：負責射擊行為與生成子彈。
 * 
 * - 管理射擊冷卻（FireCooldown）
 * - 決定射擊方向（目前：Player.LastMoveDir）
 * - Instantiate Bullet 並加入 Projectiles 容器
 * 
 * 禁止事項：
 * - 不做傷害裁決（命中後由 Bullet 感測器送 DamageRequest 給 CombatSystem）
 * - 不直接扣血
 */

public partial class PlayerWeapon : Node
{
	// =========================================================
	// 一、可調參數（Weapon Tuning）
	// =========================================================

	[Export] public string FireAction = "fire";

	[Export] public PackedScene BulletScene;     // Inspector 指定 Bullet.tscn
	[Export] public NodePath ProjectilesPath;    // 指向 Game/Projectiles

	[Export] public float FireCooldown = 0.12f;

	// 子彈初速 / 傷害（先留著，之後由 Bullet.Init 接收）
	[Export] public float BulletSpeed = 900f;
	[Export] public int BulletDamage = 1;

	// =========================================================
	// 二、依賴引用（Dependency）
	// =========================================================

	private Player _player;
	private Node _projectiles;

	// =========================================================
	// 三、內部狀態（Weapon Internal State）
	// =========================================================

	private float _cooldownTimer = 0f;

	// =========================================================
	// 四、初始化（Setup）
	// =========================================================

	public void Setup(Player player)
	{
		_player = player;

		// 快取 Projectiles 容器，避免每次射擊都 GetNode
		if (ProjectilesPath != null && !ProjectilesPath.IsEmpty)
			_projectiles = GetNode(ProjectilesPath);
	}

	// =========================================================
	// 五、每幀更新（Tick）
	// =========================================================

	public void Tick(float dt)
	{
		// -------------------------
		// 1) 冷卻倒數
		// -------------------------
		if (_cooldownTimer > 0f)
			_cooldownTimer -= dt;

		// -------------------------
		// 2) 冷卻未結束：不可射擊
		// -------------------------
		if (_cooldownTimer > 0f)
			return;

		// -------------------------
		// 3) 觸發射擊（你可以改成 JustPressed 或 Pressed）
		// -------------------------
		// - JustPressed：點一下射一下（半自動）
		// - Pressed：按住連射（自動）
		if (!Input.IsActionPressed(FireAction))
			return;

		// -------------------------
		// 4) 執行射擊 → 生成子彈
		// -------------------------
		Fire();

		// -------------------------
		// 5) 重置冷卻
		// -------------------------
		_cooldownTimer = FireCooldown;
	}

	// =========================================================
	// 六、射擊：生成子彈（Spawn Bullet）
	// =========================================================

	private void Fire()
	{
		// 防呆：沒指定 BulletScene 就不要射
		if (BulletScene == null)
			return;

		// 防呆：沒指定 Projectiles 容器就不要射
		if (_projectiles == null)
			return;

		// --------------------------------------------------
		// 射擊方向：滑鼠瞄準（世界座標）
		// --------------------------------------------------
		Vector2 mouseWorld = _player.GetGlobalMousePosition();
		Vector2 dir = mouseWorld - _player.GlobalPosition;

		// 避免滑鼠剛好在玩家中心造成 Zero 向量
		if (dir.LengthSquared() < 0.0001f)
			dir = Vector2.Right;
		else
			dir = dir.Normalized();


		Node bullet = BulletScene.Instantiate();

		// 出生位置：暫用玩家中心（後續可加 muzzle offset）
		if (bullet is Node2D b2d)
			b2d.GlobalPosition = _player.GlobalPosition;

		// 與 Bullet 的初始化協議：
		// - 你等下建立 Bullet.cs 時，只要提供這個方法即可
		bullet.Call("InitFromPlayer", _player, dir, BulletSpeed, BulletDamage);

		_projectiles.AddChild(bullet);
	}
}
