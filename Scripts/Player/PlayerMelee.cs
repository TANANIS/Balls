using Godot;

public partial class PlayerMelee : PlayerAbilityModule
{
	[Export] public string AttackAction = InputActions.AttackSecondary;
	[Export] public bool EnabledInCurrentCharacter = true;
	[Export] public float Cooldown = 0.35f;
	[Export] public float Range = 140f;
	[Export] public float ArcDegrees = 220f;
	[Export] public int Damage = 3;
	[Export] public float DamageMultiplier = 1f;
	[Export] public uint TargetMask = 1u << 5; // Layer 6: EnemyHurtbox

	[Export] public PackedScene MeleeVfxScene;
	[Export] public NodePath VfxPath;
	[Export] public float VfxDuration = 0.12f;
	[Export] public Color VfxColor = new Color(1f, 0.9f, 0.2f, 0.7f);
	[Export] public float VfxForwardOffset = 36f;
	[Export] public float VfxSideOffset = 0f;

	private CombatSystem _combat;
	private float _cooldownTimer = 0f;
	private string _resolvedAction = InputActions.AttackSecondary;

	public float CurrentCooldown => Cooldown;
	public int CurrentDamage => Damage;
	public float CurrentRange => Range;
	public float CurrentArcDegrees => ArcDegrees;

	public void Setup(Player player)
	{
		SetupAbility(player, EnabledInCurrentCharacter);

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
		if (!_isEnabled)
			return;

		EnsureStabilitySystem();
		TickCooldown(ref _cooldownTimer, dt);
		if (_cooldownTimer > 0f)
			return;

		if (!Input.IsActionPressed(_resolvedAction))
			return;

		ExecuteAttack();
		float powerMult = GetPowerMultiplier();
		_cooldownTimer = Cooldown / Mathf.Max(0.1f, powerMult);
	}

	private void ResolveInputAction()
	{
		_resolvedAction = ResolveInputActionOrFallback(
			AttackAction,
			InputActions.LegacyAttackSecondary,
			"PlayerMelee",
			"attack_secondary",
			"RightClick");
	}

	public void SetEnabled(bool enabled)
	{
		SetEnabledState(enabled);
		EnabledInCurrentCharacter = enabled;
	}

	public void SetAttackAction(string action)
	{
		if (string.IsNullOrWhiteSpace(action))
			return;
		AttackAction = action;
		ResolveInputAction();
	}
}
