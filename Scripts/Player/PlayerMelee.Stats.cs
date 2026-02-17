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
}
