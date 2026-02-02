using Godot;
using System;

/*
 * EnemyHitbox.cs
 *
 * 職責定位
 * ------------------------------------------------------------
 * EnemyHitbox 是「攻擊感測器」：
 * - 偵測玩家 Hurtbox 進入/離開
 * - 在持續接觸期間，按固定頻率送 DamageRequest
 * - 不直接扣血、不判斷無敵/死亡（交給 CombatSystem）
 */

public partial class EnemyHitbox : Area2D
{
    [Export] public int ContactDamage = 1;
    [Export] public float TickInterval = 0.30f;

    private float _tickTimer = 0f;

    private Node _ownerEnemy;
    private CombatSystem _combat;

    // 目前正在接觸的玩家目標（最小版：鎖定 Hurtbox Area2D）
    private Area2D _currentTarget;

    public override void _Ready()
    {
        _ownerEnemy = GetParent();
        AddToGroup("EnemyHitbox");

        var list = GetTree().GetNodesInGroup("CombatSystem");
        if (list.Count > 0)
            _combat = list[0] as CombatSystem;

        if (_combat == null)
            GD.PrintErr("[EnemyHitbox] CombatSystem not found. Did you AddToGroup(\"CombatSystem\")?");

        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (_currentTarget == null) return;
        if (_combat == null) return;

        _tickTimer -= dt;
        if (_tickTimer > 0f) return;

        _tickTimer = TickInterval;

        // 只打可受傷目標
        if (_currentTarget is not IDamageable)
            return;

        var req = new DamageRequest(
            source: _ownerEnemy,
            target: _currentTarget,
            baseDamage: ContactDamage,
            worldPos: GlobalPosition,
            tag: "contact"
        );

        _combat.RequestDamage(req);
    }

    private void OnAreaEntered(Area2D other)
    {
        if (other is not IDamageable)
            return;

        _currentTarget = other;
        _tickTimer = 0f;
    }

    private void OnAreaExited(Area2D other)
    {
        if (other == _currentTarget)
            _currentTarget = null;
    }
}
