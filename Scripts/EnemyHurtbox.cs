using Godot;
using System;

public partial class EnemyHurtbox : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;
	}

		private void OnAreaEntered(Area2D area)
	{
		// 最小版本：只要碰到叫 Bullet 的東西就死
		if (area is Bullet)
		{
			// 刪子彈
			area.QueueFree();

			// 刪敵人（Hurtbox 的 parent 就是 Enemy）
			GetParent().QueueFree();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
