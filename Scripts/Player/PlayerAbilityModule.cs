using Godot;

public abstract partial class PlayerAbilityModule : Node
{
	protected Player _player;
	protected StabilitySystem _stabilitySystem;
	protected bool _isEnabled = true;

	protected void SetupAbility(Player player, bool enabledInCurrentCharacter)
	{
		_player = player;
		_isEnabled = enabledInCurrentCharacter;
		ResolveStabilitySystem();
	}

	protected void SetEnabledState(bool enabled)
	{
		_isEnabled = enabled;
	}

	protected void EnsureStabilitySystem()
	{
		if (!IsInstanceValid(_stabilitySystem))
			ResolveStabilitySystem();
	}

	protected void TickCooldown(ref float timer, float dt)
	{
		if (timer > 0f)
			timer -= dt;
	}

	protected float GetPowerMultiplier()
	{
		EnsureStabilitySystem();
		return _stabilitySystem?.GetPlayerPowerMultiplier() ?? 1f;
	}

	protected static string ResolveInputActionOrFallback(string action)
	{
		return action;
	}

	private void ResolveStabilitySystem()
	{
		_stabilitySystem = GroupServiceResolver.ResolveFirstInGroup(this, "StabilitySystem", _stabilitySystem);
	}
}
