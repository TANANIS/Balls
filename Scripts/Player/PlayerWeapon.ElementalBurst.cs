using Godot;

public partial class PlayerWeapon
{
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
