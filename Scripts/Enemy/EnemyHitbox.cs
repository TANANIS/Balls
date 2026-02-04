using Godot;
using System;

public partial class EnemyHitbox : Area2D
{
	[Export] public int ContactDamage = 1;
	[Export] public float TickInterval = 0.30f;

	[Export] public float SeparationStrength = 140f;
	[Export] public float SeparationDuration = 0.12f;

	private float _tickTimer = 0f;
	private Enemy _ownerEnemy;
	private CombatSystem _combat;
	private Area2D _currentTarget;

	public override void _Ready()
	{
		_ownerEnemy = GetParent() as Enemy;
		if (_ownerEnemy == null)
		{
			DebugSystem.Error("[EnemyHitbox] Parent is not Enemy. Make Hitbox a direct child of Enemy.");
			return;
		}

		AddToGroup("EnemyHitbox");

		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combat = list[0] as CombatSystem;

		if (_combat == null)
			DebugSystem.Error("[EnemyHitbox] CombatSystem not found. Did you AddToGroup(\"CombatSystem\")?");

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
		DebugSystem.Log("[EnemyHitbox] Tick damage -> target=" + _currentTarget.Name);

		if (_currentTarget is not IDamageable)
			return;

		var req = new DamageRequest(
			source: _ownerEnemy,
			target: _currentTarget,
			baseDamage: ContactDamage,
			worldPos: GlobalPosition,
			tag: "contact"
		);

		bool didDamage = _combat.RequestDamage(req);
		if (didDamage)
			_ownerEnemy.NotifyHitPlayer(_currentTarget);

		Vector2 pushDir = _ownerEnemy.GlobalPosition - _currentTarget.GlobalPosition;
		_ownerEnemy.ApplySeparation(pushDir, SeparationStrength, SeparationDuration);
	}

	private void OnAreaEntered(Area2D other)
	{
		DebugSystem.Log("[EnemyHitbox] AreaEntered: " + other.Name + " PlayerHurtbox=" + other.IsInGroup("PlayerHurtbox"));
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
