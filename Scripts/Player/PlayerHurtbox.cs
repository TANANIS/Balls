using Godot;
using System;

/*
 * PlayerHurtbox.cs
 * - 玩家受傷入口：實作 IDamageable
 * - 不持有狀態、不裁決
 * - 直接轉發到 PlayerHealth
 */

public partial class PlayerHurtbox : Area2D, IDamageable
{
	private PlayerHealth _health;

	public override void _Ready()
	{
		Node player = GetParent();
		_health = player.GetNode<PlayerHealth>("Health");

		if (_health == null)
			GD.PrintErr("[PlayerHurtbox] Cannot find PlayerHealth node at ../Health");
	}

	public bool IsDead => _health != null && _health.IsDead;
	public bool IsInvincible => _health != null && _health.IsInvincible;

	public void TakeDamage(int amount, object source)
	{
		if (_health == null) return;
		_health.TakeDamage(amount, source);
	}
}
