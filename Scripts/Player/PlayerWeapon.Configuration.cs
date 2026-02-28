using Godot;

public partial class PlayerWeapon
{
	private void ResolveInputAction()
	{
		_resolvedAction = ResolveInputActionOrFallback(AttackAction);
	}

	private void ResolveProjectileScenes()
	{
		WizardProjectileScene ??= GD.Load<PackedScene>("res://Scenes/Projectiles/WizardProjectile.tscn");
		PriestProjectileScene ??= GD.Load<PackedScene>("res://Scenes/Projectiles/PriestProjectile.tscn");
		ArcherProjectileScene ??= GD.Load<PackedScene>("res://Scenes/Projectiles/ArcherProjectile.tscn");
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
			_attackTimeline.Reset();
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
}
