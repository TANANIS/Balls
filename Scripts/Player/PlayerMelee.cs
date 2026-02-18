using Godot;

public partial class PlayerMelee : Node
{
	[Export] public string AttackAction = InputActions.AttackSecondary;
	[Export] public float Cooldown = 0.35f;
	[Export] public float Range = 140f;
	[Export] public float ArcDegrees = 220f;
	[Export] public int Damage = 3;
	[Export] public uint TargetMask = 1u << 5; // Layer 6: EnemyHurtbox

	[Export] public PackedScene MeleeVfxScene;
	[Export] public NodePath VfxPath;
	[Export] public float VfxDuration = 0.12f;
	[Export] public Color VfxColor = new Color(1f, 0.9f, 0.2f, 0.7f);
	[Export] public float VfxForwardOffset = 36f;
	[Export] public float VfxSideOffset = 0f;

	private Player _player;
	private CombatSystem _combat;
	private StabilitySystem _stabilitySystem;
	private float _cooldownTimer = 0f;
	private string _resolvedAction = InputActions.AttackSecondary;

	public float CurrentCooldown => Cooldown;
	public int CurrentDamage => Damage;
	public float CurrentRange => Range;
	public float CurrentArcDegrees => ArcDegrees;

	public void Setup(Player player)
	{
		_player = player;
		ResolveStabilitySystem();

		// Resolve combat service from group to keep scene wiring flexible.
		var list = GetTree().GetNodesInGroup("CombatSystem");
		if (list.Count > 0)
			_combat = list[0] as CombatSystem;

		if (_combat == null)
			DebugSystem.Error("[PlayerMelee] CombatSystem not found. Did you AddToGroup(\"CombatSystem\")?");

		ResolveInputAction();
	}

	public void Tick(float dt)
	{
		if (!IsInstanceValid(_stabilitySystem))
			ResolveStabilitySystem();

		if (_cooldownTimer > 0f)
			_cooldownTimer -= dt;
		if (_cooldownTimer > 0f)
			return;

		if (!Input.IsActionJustPressed(_resolvedAction))
			return;

		ExecuteAttack();
		float powerMult = _stabilitySystem?.GetPlayerPowerMultiplier() ?? 1f;
		_cooldownTimer = Cooldown / Mathf.Max(0.1f, powerMult);
	}

	private void ResolveInputAction()
	{
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

	private void ResolveStabilitySystem()
	{
		var list = GetTree().GetNodesInGroup("StabilitySystem");
		if (list.Count > 0)
			_stabilitySystem = list[0] as StabilitySystem;
	}
}
