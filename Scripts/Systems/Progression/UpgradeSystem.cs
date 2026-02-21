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

	// Cached runtime dependencies.
	private Player _player;
	private PlayerHealth _playerHealth;
	private ProgressionSystem _progressionSystem;
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
		_player = GetNodeOrNull<Player>(PlayerPath);
		if (_player == null)
		{
			DebugSystem.Error("[UpgradeSystem] Player not found.");
			return;
		}

		_playerHealth = _player.GetNodeOrNull<PlayerHealth>("Health");

		var progressionList = GetTree().GetNodesInGroup("ProgressionSystem");
		if (progressionList.Count > 0)
			_progressionSystem = progressionList[0] as ProgressionSystem;

		RebuildDefinitionIndex();
	}

	public bool ApplyUpgrade(UpgradeId id)
	{
		if (!IsUpgradeCompatibleWithCurrentCharacter(id))
		{
			DebugSystem.Warn("[UpgradeSystem] Upgrade incompatible with active character: " + id);
			return false;
		}

		bool hasDefinition = TryGetDefinition(id, out var definition);
		if (hasDefinition && !CanApplyDefinition(definition))
		{
			DebugSystem.Warn("[UpgradeSystem] Upgrade blocked by stack/prerequisite/exclusive: " + id);
			return false;
		}

		// One place where all numeric gameplay mutations are applied.
		int nextStack = GetStack(id) + 1;
		switch (id)
		{
			case UpgradeId.AtkSpeedUp15:
				_player?.MultiplyPrimaryCooldown(GetAttackSpeedCooldownFactor(nextStack));
				break;
			case UpgradeId.AtkCooldownDown10:
				_player?.MultiplyPrimaryCooldown(GetCooldownReductionFactor(nextStack));
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
				_progressionSystem?.EnableKillChanceLifesteal(1, 0.25f);
				break;
			case UpgradeId.EcoExpGainUp20:
				_progressionSystem?.MultiplyKillProgressGain(GetExpGainMultiplier(nextStack));
				break;
			case UpgradeId.EcoPickupRadiusUp25:
				_progressionSystem?.MultiplyPickupRadius(GetPickupRadiusMultiplier(nextStack));
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

	private static float GetAttackSpeedCooldownFactor(int stack)
	{
		return stack switch
		{
			1 => 1f / 1.15f,
			2 => 1f / 1.12f,
			_ => 1f / 1.08f
		};
	}

	private static float GetCooldownReductionFactor(int stack)
	{
		return stack switch
		{
			1 => 0.90f,
			2 => 0.92f,
			_ => 0.94f
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
