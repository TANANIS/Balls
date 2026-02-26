using Godot;

public partial class PlayerMelee
{
	public void AddDamage(float amount)
	{
		Damage = Mathf.Max(0.1f, Damage + amount);
	}

	public void AddRange(float amount)
	{
		Range = Mathf.Max(4f, Range + amount);
	}

	public void AddArcDegrees(float amount)
	{
		ArcDegrees = Mathf.Clamp(ArcDegrees + amount, 5f, 180f);
	}

	public void MultiplyCooldown(float factor)
	{
		float safeFactor = Mathf.Clamp(factor, 0.05f, 20f);
		Cooldown = Mathf.Clamp(Cooldown * safeFactor, 0.02f, 10f);
		_attackAnimationSpeedMultiplier = Mathf.Clamp(_attackAnimationSpeedMultiplier / safeFactor, 0.2f, 6f);
	}

	public void SetBaseStats(float damage, float cooldown, float range, float arcDegrees)
	{
		Damage = Mathf.Max(0.1f, damage);
		Cooldown = Mathf.Clamp(cooldown, 0.02f, 10f);
		_attackAnimationSpeedMultiplier = 1f;
		Range = Mathf.Max(4f, range);
		ArcDegrees = Mathf.Clamp(arcDegrees, 5f, 180f);
		DamageMultiplier = 1f;
	}

	public void MultiplyDamage(float factor)
	{
		DamageMultiplier = Mathf.Clamp(DamageMultiplier * Mathf.Max(0.1f, factor), 0.2f, 8f);
	}
}
