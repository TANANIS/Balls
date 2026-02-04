using Godot;

public partial class PlayerMelee : Node
{
	[Export] public string AttackAction = InputActions.AttackSecondary;
	[Export] public float Cooldown = 0.35f;
	[Export] public float Range = 56f;
	[Export] public float ArcDegrees = 90f;
	[Export] public int Damage = 1;
	[Export] public uint TargetMask = 1u << 5; // Layer 6: EnemyHurtbox

	[Export] public PackedScene MeleeVfxScene;
	[Export] public NodePath VfxPath;
	[Export] public float VfxDuration = 0.12f;
	[Export] public Color VfxColor = new Color(1f, 0.9f, 0.2f, 0.7f);
	[Export] public float VfxForwardOffset = 24f;
	[Export] public float VfxSideOffset = 0f;

	private Player _player;
	private CombatSystem _combat;
	private float _cooldownTimer = 0f;
	private string _resolvedAction = InputActions.AttackSecondary;

	public float CurrentCooldown => Cooldown;
	public int CurrentDamage => Damage;
	public float CurrentRange => Range;
	public float CurrentArcDegrees => ArcDegrees;

	public void Setup(Player player)
	{
		_player = player;

		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combat = list[0] as CombatSystem;

		if (_combat == null)
			DebugSystem.Error("[PlayerMelee] CombatSystem not found. Did you AddToGroup(\"CombatSystem\")?");

		if (InputMap.HasAction(AttackAction))
		{
			_resolvedAction = AttackAction;
		}
		else if (InputMap.HasAction(InputActions.LegacyAttackSecondary))
		{
			_resolvedAction = InputActions.LegacyAttackSecondary;
			DebugSystem.Warn("[PlayerMelee] attack_secondary not found. Fallback to legacy action 'RightClick'.");
		}
		else
		{
			DebugSystem.Error("[PlayerMelee] No valid secondary attack action found.");
		}
	}

	public void Tick(float dt)
	{
		if (_cooldownTimer > 0f)
			_cooldownTimer -= dt;

		if (_cooldownTimer > 0f)
			return;

		if (!Input.IsActionJustPressed(_resolvedAction))
			return;

		ExecuteAttack();
		_cooldownTimer = Cooldown;
	}

	private void SpawnVfx(Vector2 direction)
	{
		if (MeleeVfxScene == null || _player == null)
			return;

		Node vfx = MeleeVfxScene.Instantiate();
		if (vfx is Node2D vfx2d)
		{
			Vector2 right = direction;
			Vector2 up = right.Orthogonal();
			vfx2d.GlobalPosition = _player.GlobalPosition + right * VfxForwardOffset + up * VfxSideOffset;
			vfx2d.Rotation = direction.Angle();
		}

		Node parent = _player.GetParent();
		if (VfxPath != null && !VfxPath.IsEmpty)
			parent = GetNode(VfxPath);
		parent.AddChild(vfx);

		// Initialize after entering tree so MeleeVFX._Ready references are valid.
		if (vfx is MeleeVFX meleeVfx)
			meleeVfx.Init(direction, Range, ArcDegrees, VfxDuration, VfxColor);
	}

	private void ExecuteAttack()
	{
		if (_combat == null || _player == null)
			return;

		Vector2 attackDir = _player.GetGlobalMousePosition() - _player.GlobalPosition;
		if (attackDir.LengthSquared() < 0.0001f)
			attackDir = _player.LastMoveDir;
		else
			attackDir = attackDir.Normalized();

		SpawnVfx(attackDir);

		var circle = new CircleShape2D { Radius = Range };
		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = circle,
			Transform = new Transform2D(0f, _player.GlobalPosition),
			CollisionMask = TargetMask,
			CollideWithAreas = true,
			CollideWithBodies = false
		};

		var space = _player.GetWorld2D().DirectSpaceState;
		var results = space.IntersectShape(query, 32);
		float halfArcRad = Mathf.DegToRad(ArcDegrees) * 0.5f;

		foreach (var hit in results)
		{
			if (!hit.ContainsKey("collider"))
				continue;

			var colliderObj = hit["collider"].AsGodotObject();
			if (colliderObj is not Area2D area)
				continue;

			Vector2 toTarget = area.GlobalPosition - _player.GlobalPosition;
			if (toTarget.LengthSquared() < 0.0001f)
				continue;

			Vector2 targetDir = toTarget.Normalized();
			float dot = Mathf.Clamp(attackDir.Dot(targetDir), -1f, 1f);
			float angle = Mathf.Acos(dot);
			if (angle > halfArcRad)
				continue;

			if (area is not IDamageable)
				continue;

			var req = new DamageRequest(
				source: _player,
				target: area,
				baseDamage: Damage,
				worldPos: area.GlobalPosition,
				tag: "melee"
			);

			_combat.RequestDamage(req);
		}
	}

	public void AddDamage(int amount)
	{
		Damage = Mathf.Max(1, Damage + amount);
	}

	public void AddRange(float amount)
	{
		Range = Mathf.Max(4f, Range + amount);
	}

	public void AddArcDegrees(float amount)
	{
		ArcDegrees = Mathf.Clamp(ArcDegrees + amount, 5f, 180f);
	}

	public void MultiplyCooldown(float factor)
	{
		Cooldown = Mathf.Clamp(Cooldown * factor, 0.02f, 10f);
	}
}
