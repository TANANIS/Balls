using Godot;
using System;

/*
 * SpawnSystem.cs
 *
 * 職責定位
 * ------------------------------------------------------------
 * - 管理敵人生成（節奏/位置/數量上限）
 * - 不負責敵人 AI、不負責戰鬥裁決
 * - 只管「什麼時候生、在哪裡生、把誰生出來」
 */

public partial class SpawnSystem : Node
{
	// =========================================================
	// 一、Scene Reference（由 Inspector 指定）
	// =========================================================

	[Export] public PackedScene EnemyScene;

	// 指向 Game/Enemies 容器
	[Export] public NodePath EnemiesPath = "../Enemies";

	// 指向 Game/Player
	[Export] public NodePath PlayerPath = "../Player";

	// =========================================================
	// 二、生成節奏（最小版本：固定間隔）
	// =========================================================

	[Export] public float SpawnInterval = 1.0f;

	// 場上敵人上限（防止爆量）
	[Export] public int MaxAliveEnemies = 50;

	// =========================================================
	// 三、生成位置（以玩家為中心的環形外圈）
	// =========================================================

	[Export] public float SpawnRadiusMin = 420f;
	[Export] public float SpawnRadiusMax = 560f;

	// =========================================================
	// 四、內部狀態
	// =========================================================

	private Node2D _enemiesRoot;
	private Node2D _player;
	private float _timer;

	public override void _Ready()
	{
		_enemiesRoot = GetNodeOrNull<Node2D>(EnemiesPath);
		_player = GetNodeOrNull<Node2D>(PlayerPath);

		if (EnemyScene == null)
			GD.PrintErr("[SpawnSystem] EnemyScene is null. Please assign Enemy.tscn in Inspector.");

		if (_enemiesRoot == null)
			GD.PrintErr("[SpawnSystem] Enemies root not found. Check EnemiesPath.");

		if (_player == null)
			GD.PrintErr("[SpawnSystem] Player not found. Check PlayerPath.");

		_timer = SpawnInterval;
	}

	public override void _PhysicsProcess(double delta)
	{
		// 缺少必要引用就不運作
		if (EnemyScene == null || _enemiesRoot == null || _player == null)
			return;

		// 敵人上限
		if (_enemiesRoot.GetChildCount() >= MaxAliveEnemies)
			return;

		_timer -= (float)delta;
		if (_timer > 0f)
			return;

		_timer = SpawnInterval;

		SpawnOne();
	}

	private void SpawnOne()
	{
		// 1) 建立敵人實例
		var enemy = EnemyScene.Instantiate<Node2D>();

		// 2) 計算生成位置（玩家周圍環形外圈）
		Vector2 pos = GetSpawnPositionAroundPlayer();

		enemy.GlobalPosition = pos;

		// 3) 掛到 Enemies 容器
		_enemiesRoot.AddChild(enemy);
	}

	private Vector2 GetSpawnPositionAroundPlayer()
	{
		// 隨機角度 + 隨機半徑（在 min~max 之間）
		float angle = (float)GD.RandRange(0, Mathf.Tau);
		float radius = (float)GD.RandRange(SpawnRadiusMin, SpawnRadiusMax);

		Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
		return _player.GlobalPosition + offset;
	}
}
