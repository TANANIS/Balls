using Godot;

public partial class PlayerMelee
{
	public void AddDamage(int amount)
	{
		Damage = Mathf.Max(1, Damage + amount);
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
		Cooldown = Mathf.Clamp(Cooldown * factor, 0.02f, 10f);
	}

	public void SetBaseStats(int damage, float cooldown, float range, float arcDegrees)
	{
		Damage = Mathf.Max(1, damage);
		Cooldown = Mathf.Clamp(cooldown, 0.02f, 10f);
		Range = Mathf.Max(4f, range);
		ArcDegrees = Mathf.Clamp(arcDegrees, 5f, 180f);
		DamageMultiplier = 1f;
	}

	public void MultiplyDamage(float factor)
	{
		DamageMultiplier = Mathf.Clamp(DamageMultiplier * Mathf.Max(0.1f, factor), 0.2f, 8f);
	}
}
