using Godot;

/*
 * DamageRequest
 * - Sensors submit this to CombatSystem.
 * - CombatSystem applies final validation/rules.
 */
public readonly struct DamageRequest
{
	public readonly Node Source;
	public readonly Node Target;
	public readonly int BaseDamage;
	// Supports fractional/chip damage models without changing IDamageable int contract.
	public readonly float DamageScale;
	public readonly ulong Frame;
	public readonly Vector2 WorldPos;
	public readonly string Tag;

	public DamageRequest(
		Node source,
		Node target,
		int baseDamage,
		Vector2 worldPos,
		string tag = "",
		float damageScale = 1f)
	{
		Source = source;
		Target = target;
		BaseDamage = baseDamage;
		DamageScale = Mathf.Max(0f, damageScale);
		WorldPos = worldPos;
		Tag = tag;
		Frame = Engine.GetPhysicsFrames();
	}

	public bool IsValid()
	{
		if (Source == null || Target == null)
			return false;
		if (BaseDamage <= 0)
			return false;
		if (DamageScale <= 0f)
			return false;
		return true;
	}
}
