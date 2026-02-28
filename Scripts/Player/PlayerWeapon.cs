using Godot;

public partial class PlayerWeapon : PlayerAbilityModule
{
	private const string ArcherCharacterId = "archer";
	private const string TankCharacterId = "tank_burst";
	private const string RangedCharacterId = "ranged";

	[Export] public string AttackAction = InputActions.AttackPrimary;
	[Export] public bool EnabledInCurrentCharacter = true;

	[Export] public PackedScene ProjectileScene;
	[Export] public PackedScene WizardProjectileScene;
	[Export] public PackedScene PriestProjectileScene;
	[Export] public PackedScene ArcherProjectileScene;
	[Export] public NodePath ProjectileContainerPath;

	[Export] public float Cooldown = 0.12f;
	[Export] public float ProjectileSpeed = 900f;
	[Export] public float Damage = 1f;
	[Export] public float CritChance = 0f;
	[Export] public float CritDamageMultiplier = 1.5f;
	[Export] public bool PrecisionSingleLine = true;
	[Export] public int ExtraProjectiles = 0;
	[Export] public int SplitShotLevel = 0;
	[Export] public PrimaryFirePattern FirePattern = PrimaryFirePattern.Single;
	[Export] public float BurstShotInterval = 0.08f;
	[Export(PropertyHint.Range, "0.05,1.50,0.01")] public float AttackWindupSeconds = 0.42f;
	[Export(PropertyHint.Range, "0.05,0.95,0.01")] public float FireAtNormalizedTime = 0.60f;
	[Export] public bool AimAtFireMoment = true;
	[Export] public bool AimEachBurstShot = false;
	[ExportGroup("Card Modifiers")]
	[Export] public bool ArcaneTrackingEnabled = false;
	[Export(PropertyHint.Range, "60,1440,1")] public float ArcaneTrackingTurnRateDegrees = 620f;
	[Export(PropertyHint.Range, "0.00,1.00,0.01")] public float ArcaneTrackingForwardDotThreshold = 0.20f;
	[Export(PropertyHint.Range, "0,6,1")] public int PiercingCount = 0;
	[Export(PropertyHint.Range, "0,6,1")] public int RicochetCount = 0;
	[Export(PropertyHint.Range, "64,2400,1")] public float RicochetSearchRadius = 640f;
	[ExportGroup("Elemental Burst")]
	[Export] public bool ElementalBurstEnabled = false;
	[Export(PropertyHint.Range, "1.0,30.0,0.1")] public float ElementalBurstChargeSeconds = 5f;
	[Export(PropertyHint.Range, "16,400,1")] public float ElementalBurstExplosionRadius = 130f;
	[Export(PropertyHint.Range, "0.1,3.0,0.05")] public float ElementalBurstDamageMultiplier = 1.20f;
	[Export(PropertyHint.Range, "32,2000,1")] public float ElementalBurstMaxDistance = 280f;
	[Export(PropertyHint.Range, "1,32,1")] public int ElementalBurstMaxTargets = 5;
	[ExportGroup("Archer")]
	[Export(PropertyHint.Range, "2,12,1")] public int ArcherBurstCycle = 3;
	[Export(PropertyHint.Range, "1,8,1")] public int ArcherBurstProjectiles = 3;
	[Export(PropertyHint.Range, "0.01,0.30,0.005")] public float ArcherBurstShotInterval = 0.060f;
	[Export(PropertyHint.Range, "0,64,1")] public float ArcherSpawnForwardOffset = 22f;
	[Export(PropertyHint.Range, "0.00,0.20,0.005")] public float ArcherHitArmDelaySeconds = 0.03f;

	private Node _projectileContainer;
	private float _attackAnimationSpeedMultiplier = 1f;
	private float _cooldownTimer;
	private readonly PlayerAttackTimeline _attackTimeline = new();
	private string _resolvedAction = InputActions.AttackPrimary;
	private readonly RandomNumberGenerator _rng = new();
	private float _elementalBurstChargeTimer;
	private bool _elementalBurstCharged;
	private bool _elementalBurstWaitingForDetonation;
	private bool _isArcherCharacter;
	private int _archerBurstCounter;

	public float CurrentCooldown => Cooldown;
	public float CurrentDamage => Damage;
	public float CurrentProjectileSpeed => ProjectileSpeed;

	public void Setup(Player player)
	{
		SetupAbility(player, EnabledInCurrentCharacter);
		_rng.Randomize();
		ResolveProjectileScenes();

		if (ProjectileContainerPath != null && !ProjectileContainerPath.IsEmpty)
			_projectileContainer = GetNode(ProjectileContainerPath);

		ResolveInputAction();
	}

	public void Tick(float dt)
	{
		Tick(dt, Input.IsActionPressed(_resolvedAction));
	}

	public void Tick(float dt, bool wantAttack)
	{
		if (!_isEnabled)
			return;

		EnsureStabilitySystem();
		TickCooldown(ref _cooldownTimer, dt);
		_attackTimeline.Tick(
			dt,
			aimAtFireMoment: AimAtFireMoment,
			aimEachBurstShot: AimEachBurstShot,
			resolveCurrentAimDirection: ResolveCurrentAimDirection,
			fireVolley: FireVolley);
		TickElementalBurst(dt);

		if (_cooldownTimer > 0f || _attackTimeline.IsBusy)
			return;
		if (!wantAttack)
			return;

		ExecuteAttack();
	}
}
