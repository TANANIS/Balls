using Godot;

public partial class Player
{
	[Export] public CharacterDefinition DefaultCharacter;

	private CharacterDefinition _activeCharacter;
	private AttackAbilityKind _primaryAbility = AttackAbilityKind.None;
	private AttackAbilityKind _secondaryAbility = AttackAbilityKind.None;
	private MobilityAbilityKind _mobilityAbility = MobilityAbilityKind.None;

	public CharacterDefinition ActiveCharacter => _activeCharacter;
	public AttackAbilityKind PrimaryAbility => _primaryAbility;
	public AttackAbilityKind SecondaryAbility => _secondaryAbility;
	public MobilityAbilityKind MobilityAbility => _mobilityAbility;

	public void ApplyCharacter(CharacterDefinition character)
	{
		CharacterDefinition resolved = character ?? DefaultCharacter;
		if (resolved == null)
			return;

		_activeCharacter = resolved;
		_primaryAbility = resolved.PrimaryAbility;
		_secondaryAbility = resolved.SecondaryAbility;
		_mobilityAbility = resolved.MobilityAbility;

		_movement?.SetBaseStats(
			resolved.MoveMaxSpeed,
			resolved.MoveAccel,
			resolved.MoveFriction,
			resolved.MoveStopThreshold);

		_health?.SetBaseStats(resolved.MaxHp, resolved.HurtIFrame, refill: true);
		_health?.SetRegen(resolved.RegenAmount, resolved.RegenIntervalSeconds);

		_primaryAttack?.SetBaseStats(
			resolved.RangedDamage,
			resolved.RangedCooldown,
			resolved.RangedProjectileSpeed);
		_primaryAttack?.SetFirePattern(
			resolved.RangedFirePattern,
			resolved.RangedBurstShotInterval);

		_secondaryAttack?.SetBaseStats(
			resolved.MeleeDamage,
			resolved.MeleeCooldown,
			resolved.MeleeRange,
			resolved.MeleeArcDegrees);

		_dash?.SetBaseStats(
			resolved.DashSpeed,
			resolved.DashDuration,
			resolved.DashCooldown,
			resolved.DashIFrame);

		ConfigureAttackAbilities(resolved);
		ConfigureMobilityAbility(resolved);
	}

	private void ConfigureAttackAbilities(CharacterDefinition definition)
	{
		bool rangedEnabled = false;
		bool meleeEnabled = false;
		string rangedAction = InputActions.AttackPrimary;
		string meleeAction = InputActions.AttackSecondary;

		if (definition.PrimaryAbility == AttackAbilityKind.Ranged)
		{
			rangedEnabled = true;
			rangedAction = definition.PrimaryAction;
		}
		else if (definition.PrimaryAbility == AttackAbilityKind.Melee)
		{
			meleeEnabled = true;
			meleeAction = definition.PrimaryAction;
		}

		if (definition.SecondaryAbility == AttackAbilityKind.Ranged)
		{
			if (rangedEnabled)
			{
				DebugSystem.Warn("[Player] Secondary ranged slot ignored. Ranged already bound to primary slot.");
			}
			else
			{
				rangedEnabled = true;
				rangedAction = definition.SecondaryAction;
			}
		}
		else if (definition.SecondaryAbility == AttackAbilityKind.Melee)
		{
			if (meleeEnabled)
			{
				DebugSystem.Warn("[Player] Secondary melee slot ignored. Melee already bound to primary slot.");
			}
			else
			{
				meleeEnabled = true;
				meleeAction = definition.SecondaryAction;
			}
		}

		_primaryAttack?.SetEnabled(rangedEnabled);
		_primaryAttack?.SetAttackAction(rangedAction);

		_secondaryAttack?.SetEnabled(meleeEnabled);
		_secondaryAttack?.SetAttackAction(meleeAction);
	}

	private void ConfigureMobilityAbility(CharacterDefinition definition)
	{
		bool dashEnabled = definition.MobilityAbility == MobilityAbilityKind.Dash;
		_dash?.SetEnabled(dashEnabled);
		_dash?.SetDashAction(definition.MobilityAction);
	}

	public bool HasRangedAbility()
	{
		return _primaryAttack != null && _primaryAttack.EnabledInCurrentCharacter;
	}

	public bool HasMeleeAbility()
	{
		return _secondaryAttack != null && _secondaryAttack.EnabledInCurrentCharacter;
	}

	public bool HasDashAbility()
	{
		return _dash != null && _dash.EnabledInCurrentCharacter;
	}

	public bool PrimarySupportsRanged()
	{
		return _primaryAbility == AttackAbilityKind.Ranged;
	}

	public bool PrimarySupportsMelee()
	{
		return _primaryAbility == AttackAbilityKind.Melee;
	}

	public bool SecondarySupportsRanged()
	{
		return _secondaryAbility == AttackAbilityKind.Ranged;
	}

	public bool SecondarySupportsMelee()
	{
		return _secondaryAbility == AttackAbilityKind.Melee;
	}

	public void AddPrimaryDamage(int amount)
	{
		if (PrimarySupportsRanged())
			_primaryAttack?.AddDamage(amount);
		else if (PrimarySupportsMelee())
			_secondaryAttack?.AddDamage(amount);
	}

	public void MultiplyPrimaryCooldown(float factor)
	{
		if (PrimarySupportsRanged())
			_primaryAttack?.MultiplyCooldown(factor);
		else if (PrimarySupportsMelee())
			_secondaryAttack?.MultiplyCooldown(factor);
	}

	public void AddPrimaryProjectileSpeed(float amount)
	{
		if (PrimarySupportsRanged())
			_primaryAttack?.AddProjectileSpeed(amount);
	}

	public void AddSecondaryDamage(int amount)
	{
		if (SecondarySupportsRanged())
			_primaryAttack?.AddDamage(amount);
		else if (SecondarySupportsMelee())
			_secondaryAttack?.AddDamage(amount);
	}

	public void AddSecondaryRange(float amount)
	{
		if (SecondarySupportsMelee())
			_secondaryAttack?.AddRange(amount);
	}

	public void AddSecondaryArc(float amount)
	{
		if (SecondarySupportsMelee())
			_secondaryAttack?.AddArcDegrees(amount);
	}

	public void AddAllAttackDamage(int amount)
	{
		if (HasRangedAbility())
			_primaryAttack?.AddDamage(amount);
		if (HasMeleeAbility())
			_secondaryAttack?.AddDamage(amount);
	}
}
