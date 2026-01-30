using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 140f;

	public Node2D Target;

	public override void _Ready()
	{
		GD.Print($"[Enemy READY] Target null? {Target == null}");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Target == null) return;

		Vector2 dir = (Target.GlobalPosition - GlobalPosition).Normalized();
		Velocity = dir * Speed;
		MoveAndSlide();
	}

}
