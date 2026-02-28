using Godot;

public partial class Bullet
{
	public void InitFromPlayer(Node source, Vector2 dir, float speed, int damage)
	{
		InitFromPlayer(
			source,
			dir,
			speed,
			damage,
			splitShotLevel: 0,
			canSplitOnHit: true,
			projectileScene: null,
			damageScale: 1f,
			hitArmDelaySeconds: 0f,
			ignoreTargetInstanceId: 0,
			ignoreTargetSeconds: 0f,
			isElementalBurstShot: false,
			elementalBurstRadius: 0f,
			elementalBurstDamageMultiplier: 1f,
			elementalBurstMaxDistance: 0f,
			elementalBurstMaxTargets: 0,
			elementalBurstOwner: null,
			homingTarget: null,
			homingTurnRateDegrees: 0f,
			pierceCount: 0,
			ricochetCount: 0,
			ricochetSearchRadius: 0f,
			baseDamageReference: damage,
			effectProjectileBonusRatio: 0.30f,
			isEffectProjectile: false);
	}
}
