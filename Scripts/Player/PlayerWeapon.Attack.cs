using Godot;
using System.Collections.Generic;

public partial class PlayerWeapon
{
	private void ExecuteAttack()
	{
		if (ProjectileScene == null || _projectileContainer == null || _player == null)
			return;

		Vector2 dir = ResolveCurrentAimDirection(_player.LastMoveDir);

		float powerMult = GetPowerMultiplier();
		float speed = ProjectileSpeed * (1f + ((powerMult - 1f) * 0.35f));
		int baseDamage = Mathf.Max(1, Mathf.RoundToInt(Damage * powerMult));
		_shotBaseDamageReference = Mathf.Max(1, Mathf.RoundToInt(_baseDamageStat * powerMult));
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
				ricochetSearchRadius: Mathf.Max(64f, RicochetSearchRadius),
				baseDamageReference: Mathf.Max(1, _shotBaseDamageReference),
				effectProjectileBonusRatio: EffectProjectileBonusRatio,
				isEffectProjectile: useElementalBurstShot);
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

	private Vector2 ResolveCurrentAimDirection(Vector2 fallback)
	{
		if (_player == null)
			return fallback;
		return _player.GetAimDirection(fallback);
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

	private static int GetBurstExtraShots(PrimaryFirePattern pattern)
	{
		return pattern switch
		{
			PrimaryFirePattern.Burst2 => 1,
			PrimaryFirePattern.Burst3 => 2,
			_ => 0
		};
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
}
