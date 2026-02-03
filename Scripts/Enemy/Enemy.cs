using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float MaxSpeed = 160f;
	[Export] public float Accel = 1200f;
	[Export] public float Friction = 900f;

	[Export] public NodePath PlayerPath = new NodePath("../../Player"); // 依你實際 Enemy 在樹的位置調整

	private EnemyHealth _health;
	private Node2D _player;

	// 防黏：短暫外力推開
	private Vector2 _separationVel = Vector2.Zero;
	private float _separationTime = 0f;

	public override void _Ready()
	{
		_health = GetNode<EnemyHealth>("Health");

		_player = GetNodeOrNull<Node2D>(PlayerPath);
		if (_player == null)
		{
			// 更穩的做法：Player 加 group，這裡用 group 抓
			_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (_health != null && _health.IsDead)
		{
			QueueFree();
			return;
		}

		// 1) 追玩家
		Vector2 desired = Vector2.Zero;
		if (_player != null)
		{
			Vector2 dir = (_player.GlobalPosition - GlobalPosition);
			if (dir.LengthSquared() > 0.0001f)
				dir = dir.Normalized();

			desired = dir * MaxSpeed;
		}

		Velocity = Velocity.MoveToward(desired, Accel * dt);

		// 2) 防黏外力
		if (_separationTime > 0f)
		{
			_separationTime -= dt;
			Velocity += _separationVel;
		}
		else
		{
			_separationVel = Vector2.Zero;
		}

		MoveAndSlide();
	}

	/// <summary>
	/// 給 EnemyHitbox 呼叫：每次成功 tick 後推開一下，避免磁吸。
	/// </summary>
	public void ApplySeparation(Vector2 pushDir, float strength, float duration)
	{
		if (pushDir.LengthSquared() < 0.0001f)
			pushDir = Vector2.Right;

		_separationVel = pushDir.Normalized() * strength;
		_separationTime = Mathf.Max(duration, 0.01f);
	}
}
