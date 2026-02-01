using Godot;
using System;

public partial class Enemy : CharacterBody2D
{

	[Export] public float Speed = 140f;

	// 命中後退開相關
	[Export] public float AttackCooldown = 0.6f;     // 命中後多久才能再造成傷害
	[Export] public float BackOffTime = 0.18f;       // 退開多久
	[Export] public float BackOffSpeed = 260f;       // 退開速度（通常 > Speed）
	[Export] public float ReEngageDistance = 44f;    // 太貼臉時先退開到這距離再追

	public Node2D Target;

	private float _attackCdTimer = 0f;
	private float _backOffTimer = 0f;

	public override void _Ready()
	{
		//GD.Print($"[Enemy READY] Target null? {Target == null}");
	}


	public override void _PhysicsProcess(double delta)
	{
		if (Target == null) return;
		float dt = (float)delta;

		if (_attackCdTimer > 0f) _attackCdTimer -= dt;

		Vector2 toTarget = Target.GlobalPosition - GlobalPosition;
		float distSq = toTarget.LengthSquared();
		Vector2 dir = (distSq > 0.0001f) ? toTarget / Mathf.Sqrt(distSq) : Vector2.Zero;

		// 1) 命中後退開狀態
		if (_backOffTimer > 0f)
		{
			_backOffTimer -= dt;
			Velocity = -dir * BackOffSpeed;
			MoveAndSlide();
			return;
		}

		// 2) 太貼臉：先退開一下再追（避免黏住/推擠感）
		float reEngageSq = ReEngageDistance * ReEngageDistance;
		if (distSq < reEngageSq)
		{
			Velocity = -dir * BackOffSpeed;
			MoveAndSlide();
			return;
		}

		// 3) 正常追蹤
		Velocity = dir * Speed;
		MoveAndSlide();
	}

	/// <summary>
	/// 被 Hitbox 命中玩家時呼叫：進入冷卻 + 退避
	/// </summary>
	public void NotifyHitPlayer()
	{
		// 冷卻中就別重複進狀態（避免抖動）
		if (_attackCdTimer > 0f) return;

		_attackCdTimer = AttackCooldown;
		_backOffTimer = BackOffTime;
	}

	public bool CanDealDamage() => _attackCdTimer <= 0f;
}
