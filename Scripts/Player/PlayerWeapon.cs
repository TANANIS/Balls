using Godot;
using System.Collections.Generic;

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
	private float _cooldownTimer = 0f;
	private readonly PlayerAttackTimeline _attackTimeline = new();
	private string _resolvedAction = InputActions.AttackPrimary;
	private readonly RandomNumberGenerator _rng = new();
	private float _elementalBurstChargeTimer = 0f;
	private bool _elementalBurstCharged = false;
	private bool _elementalBurstWaitingForDetonation = false;
	private bool _isArcherCharacter = false;
	private int _archerBurstCounter = 0;

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

	private void ExecuteAttack()
	{
		if (ProjectileScene == null || _projectileContainer == null || _player == null)
			return;

		Vector2 dir = ResolveCurrentAimDirection(_player.LastMoveDir);

		float powerMult = GetPowerMultiplier();
		float speed = ProjectileSpeed * (1f + ((powerMult - 1f) * 0.35f));
		int baseDamage = Mathf.Max(1, Mathf.RoundToInt(Damage * powerMult));
		int burstExtraShots = GetBurstExtraShots(FirePattern);
		float burstIntervalSeconds = BurstShotInterval;
		bool triggerArcherRapidBurst = ConsumeArcherBurstFlagForThisAttack();
		if (_isArcherCharacter && triggerArcherRapidBurst)
		{
			// Archer's every-Nth attack becomes a quick sequential burst (not fan spread).
			burstExtraShots += Mathf.Max(0, ArcherBurstProjectiles - 1);
			burstIntervalSeconds = Mathf.Clamp(ArcherBurstShotInterval, 0.01f, 0.30f);
		}
		float baseDuration = Mathf.Clamp(AttackWindupSeconds, 0.05f, 1.5f);
		float attackDuration = _player.TriggerPrimaryAttackAnimationAndGetDuration(baseDuration, _attackAnimationSpeedMultiplier);
		_attackTimeline.BeginWindup(
			durationSeconds: attackDuration,
			fireAtNormalized: FireAtNormalizedTime,
			aimFallbackDir: dir,
			shotSpeed: speed,
			shotBaseDamage: baseDamage,
			burstExtraShots: burstExtraShots,
			burstIntervalSeconds: burstIntervalSeconds);

		_cooldownTimer = Cooldown / Mathf.Max(0.1f, powerMult);
	}

	private void FireVolley(Vector2 baseDir, float speed, int baseDamage)
	{
		if (ProjectileScene == null || _projectileContainer == null || _player == null)
			return;

		if (PriestProjectileScene != null && ProjectileScene == PriestProjectileScene)
			AudioManager.Instance?.PlaySfxPlayerFirePriest();
		else if (_isArcherCharacter || (ArcherProjectileScene != null && ProjectileScene == ArcherProjectileScene))
			AudioManager.Instance?.PlaySfxPlayerFireArcher();
		else
			AudioManager.Instance?.PlaySfxPlayerFire();

		foreach (float angleDeg in BuildVolleyAngles())
		{
			Vector2 dir = baseDir.Rotated(Mathf.DegToRad(angleDeg)).Normalized();
			int damage = RollDamage(baseDamage);
			SpawnProjectile(dir, speed, damage);
		}
	}

	private List<float> BuildVolleyAngles()
	{
		var angles = new List<float>();
		int count = Mathf.Max(1, 1 + ExtraProjectiles);
		if (count == 1)
		{
			angles.Add(0f);
		}
		else
		{
			// Skill Split remains same-axis, but spread is widened for clearer lane coverage.
			float spacing = PrecisionSingleLine ? 8f : 14f;
			float start = -spacing * (count - 1) * 0.5f;
			for (int i = 0; i < count; i++)
				angles.Add(start + (spacing * i));
		}

		return angles;
	}

	private int RollDamage(int baseDamage)
	{
		float chance = Mathf.Clamp(CritChance, 0f, 0.95f);
		bool crit = _rng.Randf() < chance;
		if (!crit)
			return baseDamage;

		float mult = Mathf.Max(1f, CritDamageMultiplier);
		return Mathf.Max(baseDamage, Mathf.RoundToInt(baseDamage * mult));
	}

	private void SpawnProjectile(Vector2 dir, float speed, int damage)
	{
		bool useElementalBurstShot = ElementalBurstEnabled && _elementalBurstCharged && !_elementalBurstWaitingForDetonation;
		bool useArcaneTracking = ArcaneTrackingEnabled;
		Vector2 shotDir = dir.Normalized();
		Vector2 spawnPos = _player.GlobalPosition;
		if (_isArcherCharacter)
			spawnPos += shotDir * Mathf.Max(0f, ArcherSpawnForwardOffset);
		Node bullet = ProjectileScene.Instantiate();
		if (bullet is Node2D bullet2D)
			bullet2D.GlobalPosition = spawnPos;
		if (bullet is Bullet typedBullet)
		{
			float homingTurnRate = useArcaneTracking ? ArcaneTrackingTurnRateDegrees : 0f;
			if (useArcaneTracking)
				typedBullet.HomingForwardDotThreshold = Mathf.Clamp(ArcaneTrackingForwardDotThreshold, 0f, 1f);

			typedBullet.InitFromPlayer(
				_player,
				shotDir,
				speed,
				damage,
				Mathf.Clamp(SplitShotLevel, 0, 4),
				canSplitOnHit: true,
				projectileScene: ProjectileScene,
				hitArmDelaySeconds: _isArcherCharacter ? Mathf.Max(0f, ArcherHitArmDelaySeconds) : 0f,
				isElementalBurstShot: useElementalBurstShot,
				elementalBurstRadius: ElementalBurstExplosionRadius,
				elementalBurstDamageMultiplier: ElementalBurstDamageMultiplier,
				elementalBurstMaxDistance: ElementalBurstMaxDistance,
				elementalBurstMaxTargets: ElementalBurstMaxTargets,
				elementalBurstOwner: useElementalBurstShot ? this : null,
				homingTarget: null,
				homingTurnRateDegrees: homingTurnRate,
				pierceCount: Mathf.Max(0, PiercingCount),
				ricochetCount: Mathf.Max(0, RicochetCount),
				ricochetSearchRadius: Mathf.Max(64f, RicochetSearchRadius));
		}
		else
		{
			bullet.Call("InitFromPlayer", _player, dir.Normalized(), speed, damage);
		}

		if (useElementalBurstShot)
		{
			_elementalBurstCharged = false;
			_elementalBurstWaitingForDetonation = bullet is Bullet;
			if (!_elementalBurstWaitingForDetonation)
				_elementalBurstChargeTimer = 0f;
		}

		_projectileContainer.AddChild(bullet);
	}

	private void ResolveInputAction()
	{
		_resolvedAction = ResolveInputActionOrFallback(AttackAction);
	}

	private void ResolveProjectileScenes()
	{
		WizardProjectileScene ??= GD.Load<PackedScene>("res://Prefabs/WizardProjectile.tscn");
		PriestProjectileScene ??= GD.Load<PackedScene>("res://Prefabs/PriestProjectile.tscn");
		ArcherProjectileScene ??= GD.Load<PackedScene>("res://Prefabs/ArcherProjectile.tscn");
		ProjectileScene ??= WizardProjectileScene ?? PriestProjectileScene ?? ArcherProjectileScene;
	}

	public void ApplyProjectileByCharacterId(string characterId)
	{
		_isArcherCharacter = false;
		_archerBurstCounter = 0;

		if (string.Equals(characterId, TankCharacterId))
		{
			if (PriestProjectileScene != null)
				ProjectileScene = PriestProjectileScene;
			return;
		}

		if (string.Equals(characterId, ArcherCharacterId))
		{
			_isArcherCharacter = true;
			if (ArcherProjectileScene != null)
				ProjectileScene = ArcherProjectileScene;
			return;
		}

		if (string.Equals(characterId, RangedCharacterId))
		{
			if (WizardProjectileScene != null)
				ProjectileScene = WizardProjectileScene;
			return;
		}

		// Keep current scene for non-ranged archetypes (e.g. melee-only).
	}

	public void SetEnabled(bool enabled)
	{
		SetEnabledState(enabled);
		EnabledInCurrentCharacter = enabled;
		if (!enabled)
		{
			_attackTimeline.Reset();
		}
	}

	public void SetAttackAction(string action)
	{
		if (string.IsNullOrWhiteSpace(action))
			return;
		AttackAction = action;
		ResolveInputAction();
	}

	public void AddDamage(float amount)
	{
		Damage = Mathf.Max(0.1f, Damage + amount);
	}

	public void MultiplyDamage(float factor)
	{
		Damage = Mathf.Max(0.1f, Damage * Mathf.Max(0.1f, factor));
	}

	public void AddProjectileSpeed(float amount)
	{
		ProjectileSpeed = Mathf.Max(50f, ProjectileSpeed + amount);
	}

	public void MultiplyCooldown(float factor)
	{
		float safeFactor = Mathf.Clamp(factor, 0.05f, 20f);
		Cooldown = Mathf.Clamp(Cooldown * safeFactor, 0.02f, 10f);
		_attackAnimationSpeedMultiplier = Mathf.Clamp(_attackAnimationSpeedMultiplier / safeFactor, 0.2f, 6f);
	}

	public void SetBaseStats(float damage, float cooldown, float projectileSpeed)
	{
		Damage = Mathf.Max(0.1f, damage);
		Cooldown = Mathf.Clamp(cooldown, 0.02f, 10f);
		_attackAnimationSpeedMultiplier = 1f;
		ProjectileSpeed = Mathf.Max(50f, projectileSpeed);
		CritChance = 0f;
		CritDamageMultiplier = 1.5f;
		ExtraProjectiles = 0;
		SplitShotLevel = 0;
		ArcaneTrackingEnabled = false;
		ArcaneTrackingTurnRateDegrees = 620f;
		ArcaneTrackingForwardDotThreshold = 0.20f;
		PiercingCount = 0;
		RicochetCount = 0;
		RicochetSearchRadius = 640f;
		ElementalBurstEnabled = false;
		ResetElementalBurstState();
		_archerBurstCounter = 0;
	}

	public void SetFirePattern(PrimaryFirePattern pattern, float burstShotInterval)
	{
		FirePattern = pattern;
		BurstShotInterval = Mathf.Clamp(burstShotInterval, 0.01f, 0.5f);
	}

	private static int GetBurstExtraShots(PrimaryFirePattern pattern)
	{
		return pattern switch
		{
			PrimaryFirePattern.Burst2 => 1,
			PrimaryFirePattern.Burst3 => 2,
			_ => 0
		};
	}

	public void AddProjectileCount(int amount)
	{
		ExtraProjectiles = Mathf.Clamp(ExtraProjectiles + amount, 0, 10);
	}

	public void AddSplitShotLevel(int amount)
	{
		SplitShotLevel = Mathf.Clamp(SplitShotLevel + amount, 0, 4);
	}

	public void AddCritChance(float amount)
	{
		CritChance = Mathf.Clamp(CritChance + amount, 0f, 0.95f);
	}

	public void EnableArcaneTracking(float turnRateDegrees, float forwardDotThreshold)
	{
		ArcaneTrackingEnabled = true;
		ArcaneTrackingTurnRateDegrees = Mathf.Clamp(turnRateDegrees, 60f, 1440f);
		ArcaneTrackingForwardDotThreshold = Mathf.Clamp(forwardDotThreshold, 0f, 1f);
	}

	public void AddPierceCount(int amount)
	{
		PiercingCount = Mathf.Clamp(PiercingCount + amount, 0, 3);
	}

	public void AddRicochetCount(int amount)
	{
		RicochetCount = Mathf.Clamp(RicochetCount + amount, 0, 3);
	}

	public void ResetRuntimeState()
	{
		_attackAnimationSpeedMultiplier = 1f;
		_cooldownTimer = 0f;
		_attackTimeline.Reset();
		ResetElementalBurstState();
		_archerBurstCounter = 0;
	}

	public void InterruptCurrentAttack()
	{
		_attackTimeline.Reset();
	}

	public void EnableElementalBurst(
		float chargeSeconds,
		float explosionRadius,
		float damageMultiplier,
		float maxDistance,
		int maxTargets)
	{
		ElementalBurstEnabled = true;
		ElementalBurstChargeSeconds = Mathf.Clamp(chargeSeconds, 1f, 30f);
		ElementalBurstExplosionRadius = Mathf.Clamp(explosionRadius, 16f, 400f);
		ElementalBurstDamageMultiplier = Mathf.Clamp(damageMultiplier, 0.1f, 3f);
		ElementalBurstMaxDistance = Mathf.Clamp(maxDistance, 32f, 2000f);
		ElementalBurstMaxTargets = Mathf.Clamp(maxTargets, 1, 32);
	}

	public void NotifyElementalBurstDetonated()
	{
		if (!_elementalBurstWaitingForDetonation)
			return;

		_elementalBurstWaitingForDetonation = false;
		_elementalBurstChargeTimer = 0f;
		_elementalBurstCharged = false;
	}

	private void TickElementalBurst(float dt)
	{
		if (!ElementalBurstEnabled)
			return;
		if (_elementalBurstCharged || _elementalBurstWaitingForDetonation)
			return;

		_elementalBurstChargeTimer += dt;
		if (_elementalBurstChargeTimer >= ElementalBurstChargeSeconds)
		{
			_elementalBurstChargeTimer = ElementalBurstChargeSeconds;
			_elementalBurstCharged = true;
		}
	}

	private void ResetElementalBurstState()
	{
		_elementalBurstChargeTimer = 0f;
		_elementalBurstCharged = false;
		_elementalBurstWaitingForDetonation = false;
	}

	private Vector2 ResolveCurrentAimDirection(Vector2 fallback)
	{
		if (_player == null)
			return fallback;
		Vector2 mouseWorld = _player.GetGlobalMousePosition();
		Vector2 dir = mouseWorld - _player.GlobalPosition;
		if (dir.LengthSquared() < 0.0001f)
			return fallback.LengthSquared() < 0.0001f ? Vector2.Right : fallback.Normalized();
		return dir.Normalized();
	}

	private bool ConsumeArcherBurstFlagForThisAttack()
	{
		if (!_isArcherCharacter)
			return false;
		int cycle = Mathf.Max(2, ArcherBurstCycle);
		_archerBurstCounter++;
		if (_archerBurstCounter < cycle)
			return false;

		_archerBurstCounter = 0;
		return true;
	}
}
