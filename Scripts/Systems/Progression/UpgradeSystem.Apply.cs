using Godot;

public partial class UpgradeSystem
{
	public bool ApplyUpgrade(UpgradeId id)
	{
		return ApplyUpgradeInternal(id, ignoreDefinitionConstraints: false);
	}

	public bool DebugApplyUpgrade(UpgradeId id)
	{
		// Debug path bypasses catalog gates/exclusive/prerequisite checks
		// but still respects character compatibility and stack caps.
		return ApplyUpgradeInternal(id, ignoreDefinitionConstraints: true);
	}

	private bool ApplyUpgradeInternal(UpgradeId id, bool ignoreDefinitionConstraints)
	{
		if (_player == null)
		{
			GD.PushWarning("[UpgradeSystem] ApplyUpgrade called without a valid player reference.");
			return false;
		}

		if (!IsUpgradeCompatibleWithCurrentCharacter(id))
			return false;

		bool hasDefinition = TryGetDefinition(id, out var definition);
		if (hasDefinition)
		{
			int maxStack = Mathf.Max(1, definition.MaxStack);
			if (GetStack(id) >= maxStack)
				return false;

			if (!ignoreDefinitionConstraints && !CanApplyDefinition(definition))
				return false;
		}

		// One place where all numeric gameplay mutations are applied.
		int nextStack = GetStack(id) + 1;
		switch (id)
		{
			case UpgradeId.AtkSpeedUp15:
				_player?.MultiplyPrimaryCooldown(GetAttackSpeedCooldownFactor(nextStack));
				break;
			case UpgradeId.AtkProjectilePlus1:
				_player?.AddPrimaryProjectileCount(1);
				break;
			case UpgradeId.AtkSplitShot:
				_player?.AddPrimarySplitShot(1);
				break;
			case UpgradeId.AtkDamageUp20:
				_player?.MultiplyPrimaryDamage(GetDamageMultiplier(nextStack));
				break;
			case UpgradeId.AtkCritChanceUp10:
				_player?.AddPrimaryCritChance(GetCritChanceAdd(nextStack));
				break;
			case UpgradeId.SurvMaxHpPlus1:
				_playerHealth?.AddMaxHp(1);
				break;
			case UpgradeId.SurvShieldCooldown:
				_playerHealth?.EnableShield(60f);
				break;
			case UpgradeId.SurvLifestealCloseKill:
				_progressionSystem?.EnableKillChanceLifesteal(1, 0.12f);
				break;
			case UpgradeId.EcoExpGainUp20:
				_progressionSystem?.MultiplyKillProgressGain(GetExpGainMultiplier(nextStack));
				break;
			case UpgradeId.EcoPickupRadiusUp25:
				_progressionSystem?.MultiplyPickupRadius(GetPickupRadiusMultiplier(nextStack));
				break;
			case UpgradeId.ModElementalBurst:
				_player?.EnablePrimaryElementalBurst(
					chargeSeconds: 5f,
					explosionRadius: 130f,
					damageMultiplier: 1.20f,
					maxDistance: 280f,
					maxTargets: 5);
				break;
		}

		AddStack(id, 1);
		if (hasDefinition)
			AddCategoryPick(definition.Category);
		_appliedUpgradeCount++;
		return true;
	}

	private static float GetAttackSpeedCooldownFactor(int stack)
	{
		return stack switch
		{
			1 => 1f / 1.15f,
			2 => 1f / 1.12f,
			_ => 1f / 1.08f
		};
	}

	private static float GetDamageMultiplier(int stack)
	{
		return stack switch
		{
			1 => 1.20f,
			2 => 1.15f,
			_ => 1.10f
		};
	}

	private static float GetCritChanceAdd(int stack)
	{
		return stack switch
		{
			1 => 0.10f,
			2 => 0.08f,
			_ => 0.06f
		};
	}

	private static float GetExpGainMultiplier(int stack)
	{
		return stack switch
		{
			1 => 1.20f,
			_ => 1.15f
		};
	}

	private static float GetPickupRadiusMultiplier(int stack)
	{
		return stack switch
		{
			1 => 1.25f,
			_ => 1.20f
		};
	}
}
