using Godot;
using System;

public partial class EnemyHitbox : Area2D
{
	[Export] public int ContactDamage = 1;
	[Export] public float TickInterval = 0.30f;

	// 防黏參數（新增）
	[Export] public float SeparationStrength = 140f;
	[Export] public float SeparationDuration = 0.12f;

	private float _tickTimer = 0f;

	// ✅ 型別改成 Enemy（這是你編譯錯的根因）
	private Enemy _ownerEnemy;

	private CombatSystem _combat;

	private Area2D _currentTarget;

	public override void _Ready()
	{
		// ✅ 保守寫法：避免掛錯節點直接崩
		_ownerEnemy = GetParent() as Enemy;
		if (_ownerEnemy == null)
		{
			GD.PrintErr("[EnemyHitbox] Parent is not Enemy. Make Hitbox a direct child of Enemy.");
			return;
		}

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

		// 只打可受傷目標（保留你原本的安全檢查）
		if (_currentTarget is not IDamageable)
			return;

		var req = new DamageRequest(
			source: _ownerEnemy,         // ✅ 這裡現在是 Enemy
			target: _currentTarget,
			baseDamage: ContactDamage,
			worldPos: GlobalPosition,
			tag: "contact"
		);

		_combat.RequestDamage(req);

		// ✅ 防黏：每次 tick 後推開一下
		// Area2D 本身就是 Node2D，所以一定有 GlobalPosition
		Vector2 pushDir = _ownerEnemy.GlobalPosition - _currentTarget.GlobalPosition;
		_ownerEnemy.ApplySeparation(pushDir, SeparationStrength, SeparationDuration);
	}

	private void OnAreaEntered(Area2D other)
	{
		// ✅ 建議改：只鎖 PlayerHurtbox，避免亂鎖其他 IDamageable
		// 如果你還沒加 group，就先暫時維持 IDamageable 也行，但我建議你改成 group
		if (!other.IsInGroup("PlayerHurtbox"))
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
