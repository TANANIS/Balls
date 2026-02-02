using Godot;
using System;

/*
 * Enemy.cs
 *
 * 職責定位
 * ------------------------------------------------------------
 * - 敵人行為本體（移動/AI）
 * - 不持有 HP / 無敵 timer / 死亡旗標（交給 EnemyHealth）
 * - 每幀只需要關心：Health.IsDead 時做收尾（QueueFree / 特效）
 */

public partial class Enemy : CharacterBody2D
{
    private EnemyHealth _health;

    public override void _Ready()
    {
        _health = GetNode<EnemyHealth>("Health");
    }

    public override void _PhysicsProcess(double delta)
    {
        // 1) 死亡 gating：死亡後停止行為，並收尾
        if (_health != null && _health.IsDead)
        {
            // TODO: 播放死亡效果 / 掉落 / 計分
            QueueFree();
            return;
        }

        // 2) TODO: 之後放 AI / 追蹤玩家 / 移動
        // （目前先空）
    }
}
