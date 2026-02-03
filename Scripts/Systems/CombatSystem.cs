using Godot;
using System;
using System.Collections.Generic;

public partial class CombatSystem : Node
{
	private readonly HashSet<ulong> _frameHitGuard = new();
	private ulong _lastFrame = 0;

	public override void _EnterTree()
	{
		AddToGroup("CombatSystem");
	}

	public override void _PhysicsProcess(double delta)
	{
		ulong frame = Engine.GetPhysicsFrames();
		if (frame != _lastFrame)
		{
			_frameHitGuard.Clear();
			_lastFrame = frame;
		}
	}

	public void RequestDamage(in DamageRequest req)
	{
		if (!req.IsValid())
			return;
		DebugSystem.Log("[CombatSystem] RequestDamage: valid request received");

		ulong guardKey = MakeGuardKey(req.Source, req.Target);
		if (_frameHitGuard.Contains(guardKey))
			return;

		_frameHitGuard.Add(guardKey);

		if (req.Target is not IDamageable damageable)
			return;

		if (damageable.IsDead)
			return;

		if (damageable.IsInvincible)
			return;

		int finalDamage = req.BaseDamage;
		if (finalDamage <= 0)
			return;

		damageable.TakeDamage(finalDamage, req.Source);
	}

	private static ulong MakeGuardKey(Node source, Node target)
	{
		ulong a = (ulong)source.GetInstanceId();
		ulong b = (ulong)target.GetInstanceId();
		return (a << 32) ^ b;
	}
}
