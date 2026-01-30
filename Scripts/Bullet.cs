using Godot;
using System;



/// <summary>
/// 子彈：直線飛行的最小版本
/// - 由玩家指定方向（Direction）
///— - 自己往前飛
/// - 超時自刪，避免場景塞爆
/// </summary>
public partial class Bullet : Area2D
{
	
	[Export] public float Speed = 900f;     // px/s
	[Export] public float LifeTime = 1.2f;  // 秒

	/// <summary>
	/// 子彈移動方向（必須由外部設定）
	/// - 預設向右，避免忘記設就變成 Vector2.Zero
	/// </summary>
	public Vector2 Direction { get; set; } = Vector2.Right;

	private float _lifeTimer = 0f;

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// 往指定方向移動
		GlobalPosition += Direction * Speed * dt;

		// 壽命到就刪掉
		_lifeTimer += dt;
		if (_lifeTimer >= LifeTime)
			QueueFree();
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
