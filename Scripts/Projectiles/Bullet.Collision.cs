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
		if (_hitArmDelayTimer > 0f)
			return;
		if (_ignoreTargetTimer > 0f && _ignoreTargetInstanceId != 0 && (ulong)other.GetInstanceId() == _ignoreTargetInstanceId)
			return;

		if (other.IsInGroup("World"))
		{
			_hasHit = true;
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
		if (didDealDamage)
			AudioManager.Instance?.PlaySfxPlayerHitEnemy();
		_hasHit = true;
		BeginImpact();
	}
}
