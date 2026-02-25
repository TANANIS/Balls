using Godot;
using System.Collections.Generic;

public partial class PlayerWeapon : PlayerAbilityModule
{
	[Export] public string AttackAction = InputActions.AttackPrimary;
	[Export] public bool EnabledInCurrentCharacter = true;

	[Export] public PackedScene ProjectileScene;
	[Export] public PackedScene WizardProjectileScene;
	[Export] public PackedScene PriestProjectileScene;
	[Export] public NodePath ProjectileContainerPath;

	[Export] public float Cooldown = 0.12f;
	[Export] public float ProjectileSpeed = 900f;
	[Export] public int Damage = 1;
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

	private Node _projectileContainer;
	private float _attackAnimationSpeedMultiplier = 1f;
	private float _cooldownTimer = 0f;
	private readonly PlayerAttackTimeline _attackTimeline = new();
	private string _resolvedAction = InputActions.AttackPrimary;
	private readonly RandomNumberGenerator _rng = new();

	public float CurrentCooldown => Cooldown;
	public int CurrentDamage => Damage;
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
		float baseDuration = Mathf.Clamp(AttackWindupSeconds, 0.05f, 1.5f);
		float attackDuration = _player.TriggerPrimaryAttackAnimationAndGetDuration(baseDuration, _attackAnimationSpeedMultiplier);
		_attackTimeline.BeginWindup(
			durationSeconds: attackDuration,
			fireAtNormalized: FireAtNormalizedTime,
			aimFallbackDir: dir,
			shotSpeed: speed,
			shotBaseDamage: baseDamage,
			burstExtraShots: burstExtraShots,
			burstIntervalSeconds: BurstShotInterval);

		_cooldownTimer = Cooldown / Mathf.Max(0.1f, powerMult);
	}

	private void FireVolley(Vector2 baseDir, float speed, int baseDamage)
	{
		if (ProjectileScene == null || _projectileContainer == null || _player == null)
			return;

		if (PriestProjectileScene != null && ProjectileScene == PriestProjectileScene)
			AudioManager.Instance?.PlaySfxPlayerFirePriest();
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
			// Projectile+ is a same-axis volley by design; keep spread tight for single-target focus.
			float spacing = PrecisionSingleLine ? 3f : 7f;
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
		Node bullet = ProjectileScene.Instantiate();
		if (bullet is Node2D bullet2D)
			bullet2D.GlobalPosition = _player.GlobalPosition;
		if (bullet is Bullet typedBullet)
		{
			typedBullet.InitFromPlayer(
				_player,
				dir.Normalized(),
				speed,
				damage,
				Mathf.Clamp(SplitShotLevel, 0, 4),
				canSplitOnHit: true,
				projectileScene: ProjectileScene);
		}
		else
		{
			bullet.Call("InitFromPlayer", _player, dir.Normalized(), speed, damage);
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
		ProjectileScene ??= WizardProjectileScene ?? PriestProjectileScene;
	}

	public void ApplyProjectileByCharacterId(string characterId)
	{
		if (string.Equals(characterId, "tank_burst"))
		{
			if (PriestProjectileScene != null)
				ProjectileScene = PriestProjectileScene;
			return;
		}

		if (string.Equals(characterId, "ranged"))
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

	public void AddDamage(int amount)
	{
		Damage = Mathf.Max(1, Damage + amount);
	}

	public void MultiplyDamage(float factor)
	{
		Damage = Mathf.Max(1, Mathf.RoundToInt(Damage * Mathf.Max(0.1f, factor)));
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

	public void SetBaseStats(int damage, float cooldown, float projectileSpeed)
	{
		Damage = Mathf.Max(1, damage);
		Cooldown = Mathf.Clamp(cooldown, 0.02f, 10f);
		_attackAnimationSpeedMultiplier = 1f;
		ProjectileSpeed = Mathf.Max(50f, projectileSpeed);
		CritChance = 0f;
		CritDamageMultiplier = 1.5f;
		ExtraProjectiles = 0;
		SplitShotLevel = 0;
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

	public void ResetRuntimeState()
	{
		_attackAnimationSpeedMultiplier = 1f;
		_cooldownTimer = 0f;
		_attackTimeline.Reset();
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
}
