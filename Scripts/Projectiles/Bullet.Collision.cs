using Godot;

public partial class Bullet
{
	private void OnAreaEntered(Area2D other)
	{
		TryHit(other);
	}

	private void OnBodyEntered(Node2D other)
	{
		TryHit(other);
	}

	private void TryHit(Node other)
	{
		if (_hasHit || _impactStarted)
			return;
		if (other == null)
			return;
		if (_hitArmDelayTimer > 0f)
			return;
		if (_ignoreTargetTimer > 0f && _ignoreTargetInstanceId != 0 && (ulong)other.GetInstanceId() == _ignoreTargetInstanceId)
			return;
		if (_hitTargetIds.Contains((ulong)other.GetInstanceId()))
			return;

		if (other.IsInGroup("World"))
		{
			_hasHit = true;
			TryTriggerElementalBurstExplosion(other);
			BeginImpact();
			return;
		}

		if (_combat == null)
			TryResolveCombatSystem();
		if (_combat == null || _source == null || other == null)
			return;
		if (other == _source)
			return;
		if (other is not IDamageable)
			return;

		_hitTargetIds.Add((ulong)other.GetInstanceId());

		var req = new DamageRequest(
			source: _source,
			target: other,
			baseDamage: _damage,
			worldPos: GlobalPosition,
			tag: DamageTag,
			damageScale: _damageScale
		);

		bool didDealDamage = _combat.RequestDamage(req);
		TrySpawnSplitShotsOnHit(other);
		TryTriggerElementalBurstExplosion(other);
		if (didDealDamage)
			AudioManager.Instance?.PlaySfxPlayerHitEnemy();

		if (TryContinueAfterEnemyHit(other))
			return;

		_hasHit = true;
		BeginImpact();
	}

	private bool TryContinueAfterEnemyHit(Node hitTarget)
	{
		// Elemental Burst is a one-shot transformed projectile and should always conclude on trigger.
		if (_isElementalBurstShot)
			return false;
		if (TryApplyRicochet(hitTarget))
			return true;
		if (TryConsumePierce(hitTarget))
			return true;
		return false;
	}

	private bool TryConsumePierce(Node hitTarget)
	{
		if (_pierceRemaining <= 0)
			return false;

		_pierceRemaining--;
		ApplyPostHitDamageFalloff(PierceDamageMultiplierPerHit);
		_ignoreTargetInstanceId = (ulong)hitTarget.GetInstanceId();
		_ignoreTargetTimer = Mathf.Max(_ignoreTargetTimer, Mathf.Max(0.01f, PostHitRetargetDelaySeconds));
		GlobalPosition += _dir * Mathf.Max(0f, PostHitForwardOffset);
		return true;
	}

	private bool TryApplyRicochet(Node hitTarget)
	{
		if (_ricochetRemaining <= 0)
			return false;

		if (AcquireRicochetTarget(hitTarget) is not EnemyHurtbox nextTarget)
			return ContinueForwardOnRicochetMiss(hitTarget);

		Vector2 toTarget = nextTarget.GlobalPosition - GlobalPosition;
		if (toTarget.LengthSquared() < 0.0001f)
			return false;

		_ricochetRemaining--;
		ApplyPostHitDamageFalloff(RicochetDamageMultiplierPerBounce);
		_dir = toTarget.Normalized();
		_homingTarget = nextTarget;
		_ignoreTargetInstanceId = (ulong)hitTarget.GetInstanceId();
		_ignoreTargetTimer = Mathf.Max(_ignoreTargetTimer, Mathf.Max(0.01f, PostHitRetargetDelaySeconds));
		GlobalPosition += _dir * Mathf.Max(0f, PostHitForwardOffset);
		ApplyFacingByDirection();
		return true;
	}

	private bool ContinueForwardOnRicochetMiss(Node hitTarget)
	{
		if (hitTarget == null)
			return false;

		// No valid bounce target now: keep current heading and keep flying.
		ApplyPostHitDamageFalloff(RicochetDamageMultiplierPerBounce);
		_ignoreTargetInstanceId = (ulong)hitTarget.GetInstanceId();
		_ignoreTargetTimer = Mathf.Max(_ignoreTargetTimer, Mathf.Max(0.01f, PostHitRetargetDelaySeconds));
		GlobalPosition += _dir * Mathf.Max(0f, PostHitForwardOffset);
		ApplyFacingByDirection();
		return true;
	}

	private void ApplyPostHitDamageFalloff(float multiplierPerContinuation)
	{
		float mult = Mathf.Clamp(multiplierPerContinuation, 0.01f, 1f);
		_damageScale = Mathf.Clamp(_damageScale * mult, 0.01f, 1f);
	}

	private EnemyHurtbox AcquireRicochetTarget(Node hitTarget)
	{
		SceneTree tree = GetTree();
		if (tree == null)
			return null;

		ulong excludeId = (ulong)hitTarget.GetInstanceId();
		float maxRadius = Mathf.Max(64f, _ricochetSearchRadiusRuntime);
		float maxRadiusSq = maxRadius * maxRadius;
		float bestDistSq = float.MaxValue;
		EnemyHurtbox best = null;

		foreach (Node node in tree.GetNodesInGroup("EnemyHurtbox"))
		{
			if (node is not EnemyHurtbox hurtbox)
				continue;
			if (!IsRuntimeRetargetCandidate(hurtbox))
				continue;

			ulong id = (ulong)hurtbox.GetInstanceId();
			if (id == excludeId)
				continue;

			float distSq = hurtbox.GlobalPosition.DistanceSquaredTo(GlobalPosition);
			if (distSq > maxRadiusSq)
				continue;
			if (distSq >= bestDistSq)
				continue;

			bestDistSq = distSq;
			best = hurtbox;
		}

		return best;
	}
}
