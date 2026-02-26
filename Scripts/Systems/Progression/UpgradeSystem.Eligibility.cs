using Godot;

public partial class UpgradeSystem
{
	private bool IsUpgradeCompatibleWithCurrentCharacter(UpgradeId id)
	{
		if (_player == null)
			return true;

		return id switch
		{
			UpgradeId.AtkSpeedUp15 => _player.PrimarySupportsRanged() || _player.PrimarySupportsMelee(),
			UpgradeId.AtkDamageUp20 => _player.PrimarySupportsRanged() || _player.PrimarySupportsMelee(),
			UpgradeId.AtkProjectilePlus1 => _player.PrimarySupportsRanged(),
			UpgradeId.AtkSplitShot => _player.PrimarySupportsRanged(),
			UpgradeId.AtkCritChanceUp10 => false,
			UpgradeId.ModElementalBurst => _player.PrimarySupportsRanged(),
			UpgradeId.ModArcaneTracking => _player.PrimarySupportsRanged(),
			UpgradeId.ModPierce => _player.PrimarySupportsRanged(),
			UpgradeId.ModRicochet => _player.PrimarySupportsRanged(),
			_ => true
		};
	}

	private bool CanApplyDefinition(UpgradeDefinition definition)
	{
		if (definition == null)
			return false;

		int maxStack = Mathf.Max(1, definition.MaxStack);
		if (GetStack(definition.Id) >= maxStack)
			return false;
		if (!IsDefinitionGateOpen(definition))
			return false;

		if (definition.Prerequisites != null)
		{
			foreach (var pre in definition.Prerequisites)
			{
				if (GetStack(pre) <= 0)
					return false;
			}
		}

		if (definition.ExclusiveWith != null)
		{
			foreach (var ex in definition.ExclusiveWith)
			{
				if (GetStack(ex) > 0)
					return false;
			}
		}

		// Runtime safety fuse: keep projectile volley and split-shot as mutually exclusive archetypes.
		if (definition.Id == UpgradeId.AtkProjectilePlus1 && GetStack(UpgradeId.AtkSplitShot) > 0)
			return false;
		if (definition.Id == UpgradeId.AtkSplitShot && GetStack(UpgradeId.AtkProjectilePlus1) > 0)
			return false;

		foreach (var pair in _definitions)
		{
			if (GetStack(pair.Key) <= 0)
				continue;

			var selectedDef = pair.Value;
			if (selectedDef?.ExclusiveWith == null)
				continue;

			foreach (var ex in selectedDef.ExclusiveWith)
			{
				if (ex == definition.Id)
					return false;
			}
		}

		return true;
	}

	private bool IsDefinitionGateOpen(UpgradeDefinition definition)
	{
		if (definition == null)
			return false;

		int requiredPickCount = Mathf.Max(0, definition.MinUpgradeCount);
		bool hasMinCountGate = requiredPickCount > 0;
		bool hasMinPhaseGate = definition.MinPhase > UpgradePoolPhase.Early;
		UpgradePoolPhase currentPhase = GetCurrentPoolPhase();

		if (hasMinCountGate || hasMinPhaseGate)
		{
			bool passAny = false;
			if (hasMinCountGate && _appliedUpgradeCount >= requiredPickCount)
				passAny = true;
			if (hasMinPhaseGate && currentPhase >= definition.MinPhase)
				passAny = true;

			if (!passAny)
				return false;
		}

		if (definition.UseMaxPhaseGate && currentPhase > definition.MaxPhase)
			return false;

		return true;
	}
}
