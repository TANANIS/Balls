using Godot;
using System;

public partial class PlayerHurtbox : Area2D, IDamageable
{
	private PlayerHealth _health;

	public override void _Ready()
	{
		Node player = GetParent();
		AddToGroup("PlayerHurtbox");
		DebugSystem.Log("[PlayerHurtbox] Ready. Added to group PlayerHurtbox.");

		_health = player.GetNode<PlayerHealth>("Health");
		if (_health == null)
			DebugSystem.Error("[PlayerHurtbox] Cannot find PlayerHealth node at ../Health");
	}

	public bool IsDead => _health != null && _health.IsDead;
	public bool IsInvincible => _health != null && _health.IsInvincible;

	public void TakeDamage(int amount, object source)
	{
		if (_health == null) return;
		_health.TakeDamage(amount, source);
	}
}
