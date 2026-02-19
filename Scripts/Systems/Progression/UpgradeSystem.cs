using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradeSystem : Node
{
	[Export] public NodePath PlayerPath = new NodePath("../../Player");
	[Export] public UpgradeCatalog Catalog;
	[Export(PropertyHint.Range, "0,2,0.01")] public float CategoryBiasPerPick = 0.18f;
	[Export(PropertyHint.Range, "1,20,1")] public int RarePityThreshold = 4;
	[Export(PropertyHint.Range, "1,30,1")] public int EpicPityThreshold = 8;

	// Cached player modules that receive upgrade effects.
	private PlayerWeapon _primaryAttack;
	private PlayerMelee _secondaryAttack;
	private PlayerDash _dash;
	private PlayerHealth _playerHealth;
	private PressureSystem _pressureSystem;
	private StabilitySystem _stabilitySystem;
	private int _appliedUpgradeCount = 0;
	private readonly Dictionary<UpgradeId, UpgradeDefinition> _definitions = new();
	private readonly Dictionary<UpgradeId, int> _stacks = new();
	private readonly Dictionary<UpgradeCategory, int> _categoryPickCounts = new();
	private int _offersWithoutRare = 0;
	private int _offersWithoutEpic = 0;

	public int AppliedUpgradeCount => _appliedUpgradeCount;

	public override void _EnterTree()
	{
		AddToGroup("UpgradeSystem");
	}

	public override void _Ready()
	{
		// Resolve player and cache all upgrade targets once.
		var player = GetNodeOrNull<Player>(PlayerPath);
		if (player == null)
		{
			DebugSystem.Error("[UpgradeSystem] Player not found.");
			return;
		}

		_primaryAttack = player.GetNodeOrNull<PlayerWeapon>("PrimaryAttack");
		_secondaryAttack = player.GetNodeOrNull<PlayerMelee>("SecondaryAttack");
		_dash = player.GetNodeOrNull<PlayerDash>("Dash");
		_playerHealth = player.GetNodeOrNull<PlayerHealth>("Health");

		var pressureList = GetTree().GetNodesInGroup("PressureSystem");
		if (pressureList.Count > 0)
			_pressureSystem = pressureList[0] as PressureSystem;

		var stabilityList = GetTree().GetNodesInGroup("StabilitySystem");
		if (stabilityList.Count > 0)
			_stabilitySystem = stabilityList[0] as StabilitySystem;

		RebuildDefinitionIndex();
	}

	public bool ApplyUpgrade(UpgradeId id)
	{
		bool hasDefinition = TryGetDefinition(id, out var definition);
		if (hasDefinition && !CanApplyDefinition(definition))
		{
			DebugSystem.Warn("[UpgradeSystem] Upgrade blocked by stack/prerequisite/exclusive: " + id);
			return false;
		}

		// One place where all numeric gameplay mutations are applied.
		switch (id)
		{
			case UpgradeId.PrimaryDamageUp:
				_primaryAttack?.AddDamage(1);
				break;
			case UpgradeId.PrimaryFasterFire:
				_primaryAttack?.MultiplyCooldown(0.88f);
				break;
			case UpgradeId.PrimaryProjectileSpeedUp:
				_primaryAttack?.AddProjectileSpeed(120f);
				break;
			case UpgradeId.SecondaryDamageUp:
				_secondaryAttack?.AddDamage(1);
				break;
			case UpgradeId.SecondaryRangeUp:
				_secondaryAttack?.AddRange(10f);
				break;
			case UpgradeId.SecondaryWiderArc:
				_secondaryAttack?.AddArcDegrees(15f);
				break;
			case UpgradeId.PressureKillProgressUp:
				_pressureSystem?.MultiplyKillProgressGain(1.18f);
				break;
			case UpgradeId.PressureThresholdDown:
				_pressureSystem?.AddTriggerThresholdOffset(-6f);
				break;
			case UpgradeId.PressureTriggerReliefUp:
				_pressureSystem?.AddPressureDropOnTrigger(6f);
				break;
			case UpgradeId.PressureTimeProgressUp:
				_pressureSystem?.MultiplyTimeProgressGain(1.15f);
				break;
			case UpgradeId.StabilityDecayDown:
				_stabilitySystem?.MultiplyDecayRate(0.90f);
				break;
			case UpgradeId.StabilityRecoveryPulse:
				_stabilitySystem?.TryRecover(8f, "upgrade pulse");
				break;
			case UpgradeId.AnomalyLongerEvents:
				_stabilitySystem?.MultiplyEventDuration(1.15f);
				break;
			case UpgradeId.AnomalyPowerResonance:
				_stabilitySystem?.AddPlayerPowerBonus(0.04f);
				break;
			case UpgradeId.DashFasterCooldown:
				_dash?.MultiplyCooldown(0.88f);
				break;
			case UpgradeId.DashSpeedUp:
				_dash?.AddSpeed(90f);
				break;
			case UpgradeId.DashLonger:
				_dash?.AddDuration(0.03f);
				break;
			case UpgradeId.DashIFrameUp:
				_dash?.AddIFrame(0.015f);
				break;
			case UpgradeId.MaxHpUp:
				_playerHealth?.AddMaxHp(1);
				break;
			case UpgradeId.RiskVolatileArms:
				_primaryAttack?.AddDamage(2);
				_secondaryAttack?.AddDamage(2);
				_pressureSystem?.AddTriggerThresholdOffset(5f);
				break;
		}

		AddStack(id, 1);
		if (hasDefinition)
			AddCategoryPick(definition.Category);
		_appliedUpgradeCount++;
		DebugSystem.Log("[UpgradeSystem] Applied upgrade: " + id);
		DebugSystem.Log("[UpgradeSystem] Applied count: " + _appliedUpgradeCount);
		return true;
	}

	private void RebuildDefinitionIndex()
	{
		_definitions.Clear();

		if (Catalog == null || Catalog.Entries == null)
			return;

		foreach (var entry in Catalog.Entries)
		{
			if (entry == null)
				continue;
			_definitions[entry.Id] = entry;
		}
	}

	private bool TryGetDefinition(UpgradeId id, out UpgradeDefinition definition)
	{
		if (_definitions.Count == 0)
			RebuildDefinitionIndex();

		return _definitions.TryGetValue(id, out definition);
	}

	private int GetStack(UpgradeId id)
	{
		return _stacks.TryGetValue(id, out int stack) ? stack : 0;
	}

	private void AddStack(UpgradeId id, int amount)
	{
		if (amount <= 0)
			return;

		int stack = GetStack(id);
		_stacks[id] = stack + amount;
	}

	private void AddCategoryPick(UpgradeCategory category)
	{
		int count = 0;
		_categoryPickCounts.TryGetValue(category, out count);
		_categoryPickCounts[category] = count + 1;
	}
}
