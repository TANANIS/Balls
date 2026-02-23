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

	protected string ResolveInputActionOrFallback(string action, string legacyAction, string moduleTag, string actionName, string legacyAlias)
	{
		if (InputMap.HasAction(action))
			return action;
		if (InputMap.HasAction(legacyAction))
		{
			DebugSystem.Warn($"[{moduleTag}] {actionName} not found. Fallback to legacy action '{legacyAlias}'.");
			return legacyAction;
		}

		DebugSystem.Error($"[{moduleTag}] No valid {actionName} action found.");
		return action;
	}

	private void ResolveStabilitySystem()
	{
		_stabilitySystem = GroupServiceResolver.ResolveFirstInGroup(this, "StabilitySystem", _stabilitySystem);
	}
}
