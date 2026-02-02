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

	// 接觸傷害間隔（秒）：避免一貼就每幀扣血
	[Export] public float TickInterval = 0.30f;

	private float _tickTimer = 0f;

	private Node _ownerEnemy;
	private CombatSystem _combat;

	// 目前正在接觸的玩家目標（只做單一目標最小版）
	private Node _currentTarget;

	public override void _Ready()
	{
		_ownerEnemy = GetParent(); // Hitbox 在 Enemy 底下即可
		AddToGroup("EnemyHitbox");

		// 找 CombatSystem（你先前用 group 的方式）
		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combat = list[0] as CombatSystem;

		AreaEntered += OnAreaEntered;
		AreaExited += OnAreaExited;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		if (_currentTarget == null)
			return;

		if (_combat == null)
			return;

		// 接觸計時：到點才送一次傷害請求
		_tickTimer -= dt;
		if (_tickTimer > 0f)
			return;

		// 重置間隔
		_tickTimer = TickInterval;

		// 只打可受傷目標（PlayerHurtbox）
		if (_currentTarget is not IDamageable)
			return;

		// 送傷害請求（裁決在 CombatSystem）
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
		// 只鎖定玩家 Hurtbox（需實作 IDamageable）
		if (other is not IDamageable)
			return;

		_currentTarget = other;
		_tickTimer = 0f; // 進入立刻可打一次（你想延遲就改成 TickInterval）
	}

	private void OnAreaExited(Area2D other)
	{
		if (other == _currentTarget)
			_currentTarget = null;
	}
}
