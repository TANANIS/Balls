using Godot;
using System;

/*
 * EnemyHurtbox.cs
 *
 * 職責定位
 * ------------------------------------------------------------
 * - 敵人受傷入口：實作 IDamageable
 * - 不持有 HP、不裁決
 * - 直接轉發到 EnemyHealth
 */

public partial class EnemyHurtbox : Area2D, IDamageable
{
    private EnemyHealth _health;

    public override void _Ready()
    {
        Node enemy = GetParent();
        _health = enemy.GetNode<EnemyHealth>("Health");

        if (_health == null)
            GD.PrintErr("[EnemyHurtbox] Cannot find EnemyHealth node at ../Health");
    }

    public bool IsDead => _health != null && _health.IsDead;
    public bool IsInvincible => _health != null && _health.IsInvincible;

    public void TakeDamage(int amount, object source)
    {
        if (_health == null) return;
        _health.TakeDamage(amount, source);
    }
}
