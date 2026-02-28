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

		if (other.IsInGroup(RuntimeGroups.World))
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

		MarkAsEffectProjectile();
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

		MarkAsEffectProjectile();
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
		MarkAsEffectProjectile();
		ApplyPostHitDamageFalloff(RicochetDamageMultiplierPerBounce);
		_ignoreTargetInstanceId = (ulong)hitTarget.GetInstanceId();
		_ignoreTargetTimer = Mathf.Max(_ignoreTargetTimer, Mathf.Max(0.01f, PostHitRetargetDelaySeconds));
		GlobalPosition += _dir * Mathf.Max(0f, PostHitForwardOffset);
		ApplyFacingByDirection();
		return true;
	}

	private void MarkAsEffectProjectile()
	{
		if (_isEffectProjectile)
			return;

		_isEffectProjectile = true;
		float baseDamage = Mathf.Max(1f, _baseDamageReference);
		int currentDamage = Mathf.Max(1, _damage);
		if (currentDamage <= baseDamage)
			return;

		float bonus = currentDamage - baseDamage;
		float ratio = Mathf.Clamp(_effectProjectileBonusRatio, 0f, 1f);
		_damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage + (bonus * ratio)));
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

		foreach (Node node in tree.GetNodesInGroup(RuntimeGroups.EnemyHurtbox))
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

	private void UpdateHomingDirection(float dt)
	{
		if (!_homingEnabledRuntime)
			return;
		if (!IsRuntimeRetargetCandidate(_homingTarget))
			_homingTarget = AcquireHomingTargetByCurrentDirection();
		if (!IsRuntimeRetargetCandidate(_homingTarget))
			return;

		Vector2 toTarget = _homingTarget.GlobalPosition - GlobalPosition;
		if (toTarget.LengthSquared() < 0.0001f)
			return;

		Vector2 desiredDir = toTarget.Normalized();
		float maxTurnRadians = Mathf.DegToRad(Mathf.Max(0f, _homingTurnRateRuntime)) * Mathf.Max(0f, dt);
		if (maxTurnRadians <= 0f)
		{
			_dir = desiredDir;
			return;
		}

		float signedAngle = _dir.AngleTo(desiredDir);
		if (Mathf.Abs(signedAngle) <= maxTurnRadians)
			_dir = desiredDir;
		else
			_dir = _dir.Rotated(Mathf.Sign(signedAngle) * maxTurnRadians).Normalized();
	}

	private EnemyHurtbox AcquireHomingTargetByCurrentDirection()
	{
		SceneTree tree = GetTree();
		if (tree == null)
			return null;

		EnemyHurtbox best = null;
		float bestDistSq = float.MaxValue;
		Vector2 forward = _dir.LengthSquared() < 0.0001f ? Vector2.Right : _dir.Normalized();
		float threshold = Mathf.Clamp(HomingForwardDotThreshold, -1f, 1f);

		foreach (Node node in tree.GetNodesInGroup(RuntimeGroups.EnemyHurtbox))
		{
			if (node is not EnemyHurtbox hurtbox || !IsRuntimeRetargetCandidate(hurtbox))
				continue;

			Vector2 toTarget = hurtbox.GlobalPosition - GlobalPosition;
			float distSq = toTarget.LengthSquared();
			if (distSq < 0.0001f)
				continue;

			float dot = forward.Dot(toTarget.Normalized());
			if (dot < threshold)
				continue;

			if (distSq < bestDistSq)
			{
				bestDistSq = distSq;
				best = hurtbox;
			}
		}

		return best;
	}

	private bool IsRuntimeRetargetCandidate(Node2D target)
	{
		if (!IsHomingTargetValid(target))
			return false;

		ulong id = (ulong)target.GetInstanceId();
		if (_hitTargetIds.Contains(id))
			return false;
		if (_ignoreTargetTimer > 0f && _ignoreTargetInstanceId != 0 && id == _ignoreTargetInstanceId)
			return false;
		return true;
	}

	private static bool IsHomingTargetValid(Node2D target)
	{
		if (target == null || !IsInstanceValid(target))
			return false;
		if (target is EnemyHurtbox hurtbox && hurtbox.IsDead)
			return false;
		return true;
	}

	private bool IsOutsideActiveCameraViewport()
	{
		Viewport viewport = GetViewport();
		if (viewport == null)
			return false;

		Camera2D camera = viewport.GetCamera2D();
		if (camera == null)
			return false;

		Vector2 screenSize = viewport.GetVisibleRect().Size;
		Vector2 worldSize = new Vector2(
			screenSize.X * Mathf.Abs(camera.Zoom.X),
			screenSize.Y * Mathf.Abs(camera.Zoom.Y));
		Vector2 half = worldSize * 0.5f;
		Rect2 worldRect = new Rect2(camera.GlobalPosition - half, worldSize)
			.Grow(Mathf.Max(0f, DespawnOutsideViewportMargin));
		return !worldRect.HasPoint(GlobalPosition);
	}
}
