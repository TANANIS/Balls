using Godot;

public partial class Bullet
{
	private void TrySpawnSplitShotsOnHit(Node hitTarget = null)
	{
		if (!_canSplitOnHit || _splitShotLevel <= 0)
			return;

		Node parent = GetParent();
		if (parent == null)
			return;

		// Default behavior: split child uses the same projectile prefab as the current character.
		// Keep SplitChildProjectileScene as an explicit override hook only.
		PackedScene scene = SplitChildProjectileScene;
		if (scene == null)
			scene = _projectileScene;
		if (scene == null && !string.IsNullOrWhiteSpace(SceneFilePath))
			scene = GD.Load<PackedScene>(SceneFilePath);
		if (scene == null)
			return;

		float baseAngle = Mathf.Clamp(SplitBaseAngleDegrees, 1f, 85f);
		float stepAngle = Mathf.Clamp(SplitAngleStepDegrees, 0f, 45f);
		int level = Mathf.Max(1, _splitShotLevel);
		int splitCount = GetSplitProjectileCount(level);
		bool radial360 = splitCount >= 6;
		float childDamageFactor = Mathf.Clamp(SplitChildDamageMultiplier, 0.01f, 1f);
		float childSpeedFactor = Mathf.Max(0.1f, SplitChildSpeedMultiplier);
		int childDamage = Mathf.Max(1, _damage);
		float childDamageScale = Mathf.Clamp(_damageScale * childDamageFactor, 0.001f, 1f);
		float childSpeed = Mathf.Max(1f, _speed * childSpeedFactor);
		ulong ignoreTargetId = (ulong)(hitTarget?.GetInstanceId() ?? 0);
		Vector2 splitOrigin = (hitTarget as Node2D)?.GlobalPosition ?? GlobalPosition;
		// Push split spawn a bit past the current target to avoid instant re-hit on the same enemy.
		splitOrigin += _dir * Mathf.Max(0f, SplitSpawnForwardOffset);

		if (radial360)
		{
			float step360 = 360f / splitCount;
			for (int i = 0; i < splitCount; i++)
			{
				float angle = step360 * i;
				SpawnSplitChild(scene, parent, splitOrigin, angle, childSpeed, childDamage, childDamageScale, ignoreTargetId);
			}
		}
		else
		{
			float halfSpan = baseAngle + ((splitCount - 3) * stepAngle);
			halfSpan = Mathf.Clamp(halfSpan, 1f, 179f);
			for (int i = 0; i < splitCount; i++)
			{
				float t = splitCount <= 1 ? 0.5f : (float)i / (splitCount - 1);
				float angle = Mathf.Lerp(-halfSpan, halfSpan, t);
				SpawnSplitChild(scene, parent, splitOrigin, angle, childSpeed, childDamage, childDamageScale, ignoreTargetId);
			}
		}

		_canSplitOnHit = false;
	}

	private void SpawnSplitChild(
		PackedScene scene,
		Node parent,
		Vector2 spawnPos,
		float angleDegrees,
		float speed,
		int damage,
		float damageScale,
		ulong ignoreTargetInstanceId)
	{
		Node spawned = scene.Instantiate();
		if (spawned is Node2D child2D)
		{
			if (parent is Node2D parent2D)
				child2D.Position = parent2D.ToLocal(spawnPos);
			else
				child2D.GlobalPosition = spawnPos;
		}

		Vector2 splitDir = _dir.Rotated(Mathf.DegToRad(angleDegrees)).Normalized();
		if (spawned is Bullet splitBullet)
		{
			if (_homingEnabledRuntime)
				splitBullet.HomingForwardDotThreshold = HomingForwardDotThreshold;
			// Explicitly disable chaining to avoid runaway recursive split behavior.
			splitBullet.InitFromPlayer(
				_source,
				splitDir,
				speed,
				damage,
				splitShotLevel: 0,
				canSplitOnHit: false,
				projectileScene: scene,
				damageScale: damageScale,
				hitArmDelaySeconds: 0f,
				ignoreTargetInstanceId: ignoreTargetInstanceId,
				ignoreTargetSeconds: SplitChildHitArmDelaySeconds,
				isElementalBurstShot: false,
				elementalBurstRadius: 0f,
				elementalBurstDamageMultiplier: 1f,
				elementalBurstMaxDistance: 0f,
				elementalBurstMaxTargets: 0,
				elementalBurstOwner: null,
				homingTarget: null,
				homingTurnRateDegrees: _homingEnabledRuntime ? _homingTurnRateRuntime : 0f);
		}
		else
		{
			spawned.Call("InitFromPlayer", _source, splitDir, speed, damage);
		}

		// Collision callbacks can run while physics queries are flushing.
		// Defer child insertion to avoid runtime "Can't change this state while flushing queries".
		if (parent.IsInsideTree())
			parent.CallDeferred(Node.MethodName.AddChild, spawned);
		else
			parent.AddChild(spawned);
	}

	private static int GetSplitProjectileCount(int level)
	{
		// SplitShot stack progression: Lv1=3, Lv2=4, Lv3=5, Lv4=6.
		return Mathf.Clamp(2 + level, 3, 6);
	}
}
