using Godot;
using System;
using System.Collections.Generic;

public partial class CombatSystem : Node
{
	[Export] public string TankCharacterId = "tank_burst";
	[Export] public int TankBulletBonusDamage = 1;
	[Export] public float TankBulletBonusKnockback = 220f;
	[Export] public float TankBulletBonusKnockbackDuration = 0.14f;

	private readonly HashSet<ulong> _frameHitGuard = new();
	private ulong _lastFrame = 0;
	public event Action<Node, Node> EnemyKilled;

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

	public bool RequestDamage(in DamageRequest req)
	{
		if (!req.IsValid())
			return false;
		DebugSystem.Log("[CombatSystem] RequestDamage: valid request received");

		ulong guardKey = MakeGuardKey(req.Source, req.Target);
		if (_frameHitGuard.Contains(guardKey))
			return false;

		_frameHitGuard.Add(guardKey);

		if (req.Target is not IDamageable damageable)
			return false;

		if (damageable.IsDead)
			return false;

		if (damageable.IsInvincible)
			return false;

		int finalDamage = req.BaseDamage;
		if (finalDamage <= 0)
			return false;

		if (IsTankBulletRequest(req, out Enemy targetEnemy))
		{
			finalDamage += Mathf.Max(0, TankBulletBonusDamage);

			Vector2 pushDir = targetEnemy.GlobalPosition - req.WorldPos;
			if (pushDir.LengthSquared() < 0.0001f)
				pushDir = Vector2.Right;
			else
				pushDir = pushDir.Normalized();

			targetEnemy.ApplySeparation(pushDir, Mathf.Max(0f, TankBulletBonusKnockback), Mathf.Max(0.01f, TankBulletBonusKnockbackDuration));
		}

		bool wasDead = damageable.IsDead;
		damageable.TakeDamage(finalDamage, req.Source);

		if (!wasDead && damageable.IsDead && req.Target is EnemyHurtbox)
			EnemyKilled?.Invoke(req.Source, req.Target);

		return true;
	}

	private bool IsTankBulletRequest(in DamageRequest req, out Enemy enemy)
	{
		enemy = null;
		if (!string.Equals(req.Tag, "bullet", StringComparison.OrdinalIgnoreCase))
			return false;
		if (req.Source is not Player player || player.ActiveCharacter == null)
			return false;
		if (!string.Equals(player.ActiveCharacter.CharacterId, TankCharacterId, StringComparison.OrdinalIgnoreCase))
			return false;
		if (req.Target is not EnemyHurtbox hurtbox)
			return false;
		if (hurtbox.GetParent() is not Enemy targetEnemy)
			return false;

		enemy = targetEnemy;
		return true;
	}

	private static ulong MakeGuardKey(Node source, Node target)
	{
		ulong a = (ulong)source.GetInstanceId();
		ulong b = (ulong)target.GetInstanceId();
		return (a << 32) ^ b;
	}
}
