using Godot;
using System.Text;

public partial class GameFlowUI : Control
{
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		ResolveNodeReferences();
		BindSignals();
		ShowStartPanel();
		AudioManager.Instance?.PlayBgmMenu();
	}

	public override void _Process(double delta)
	{
		UpdateUpgradeProgressUi();
		UpdateMatchCountdownUi();
		TryResolvePendingPerfectClear();
		HandlePauseInput();
		if (!_started)
			FitMenuBackground();
	}

	private void RefreshStartCardsCompendium()
	{
		if (_startCardsContentLabel == null)
			return;

		UpgradeCatalog catalog = _upgradeSystem?.Catalog;
		if (catalog == null)
			catalog = GD.Load<UpgradeCatalog>("res://Data/Upgrades/DefaultUpgradeCatalog.tres");
		if (catalog?.Entries == null || catalog.Entries.Count == 0)
		{
			_startCardsContentLabel.Text = TrOrDefault("UI.START.CARDS_EMPTY", "No upgrade cards configured.", "No upgrade cards configured.");
			return;
		}

		var sb = new StringBuilder();
		int index = 1;
		foreach (var entry in catalog.Entries)
		{
			if (entry == null)
				continue;

			string title = entry.GetLocalizedTitle();
			if (string.IsNullOrWhiteSpace(title))
				title = entry.Id.ToString();

			string description = entry.GetLocalizedDescription();
			string category = GetLocalizedUpgradeCategory(entry.Category);
			sb.Append(index++).Append(". ").Append(title).Append('\n');
			if (!string.IsNullOrWhiteSpace(description))
				sb.Append(description).Append('\n');
			sb.Append('[').Append(category).Append("] ")
				.Append(TrOrDefault("UI.START.CARDS_MAX_STACK", "MaxStack", "MaxStack")).Append(": ")
				.Append(Mathf.Max(1, entry.MaxStack)).Append("\n\n");
		}

		if (sb.Length == 0)
		{
			_startCardsContentLabel.Text = TrOrDefault("UI.START.CARDS_EMPTY", "No upgrade cards configured.", "No upgrade cards configured.");
			return;
		}

		_startCardsContentLabel.Text = sb.ToString().TrimEnd();
	}

	private string GetLocalizedUpgradeCategory(UpgradeCategory category)
	{
		return category switch
		{
			UpgradeCategory.WeaponModifier => TrOrDefault("UI.CATEGORY.CORE_ATTACK", "Core Attack", "Core Attack"),
			UpgradeCategory.PressureModifier => TrOrDefault("UI.CATEGORY.DIRECTOR", "Director", "Director"),
			UpgradeCategory.AnomalySpecialist => TrOrDefault("UI.CATEGORY.ANOMALY", "Anomaly", "Anomaly"),
			UpgradeCategory.SpatialControl => TrOrDefault("UI.CATEGORY.SPATIAL", "Spatial", "Spatial"),
			UpgradeCategory.RiskAmplifier => TrOrDefault("UI.CATEGORY.SURVIVAL", "Survival", "Survival"),
			UpgradeCategory.EconomyModifier => TrOrDefault("UI.CATEGORY.ECONOMY", "Economy", "Economy"),
			_ => category.ToString()
		};
	}

	private string TrOrDefault(string key, string fallback)
	{
		string translated = Tr(key);
		return string.IsNullOrWhiteSpace(translated) || translated == key ? fallback : translated;
	}

	private string TrOrDefault(string key, string fallbackEn, string fallbackZhTw)
	{
		string translated = Tr(key);
		if (!string.IsNullOrWhiteSpace(translated) && translated != key)
			return translated;
		return TranslationServer.GetLocale().StartsWith("zh") ? fallbackZhTw : fallbackEn;
	}

	private CharacterDefinition LoadCharacterDefinitionOrFallback(string path, CharacterDefinition fallback)
	{
		CharacterDefinition loaded = GD.Load<CharacterDefinition>(path);
		if (loaded != null)
			return loaded;

		Resource raw = ResourceLoader.Load(path);
		if (raw is CharacterDefinition typed)
			return typed;

		DebugSystem.Error($"[GameFlowUI] Failed to load CharacterDefinition: {path}. Using fallback.");
		return fallback;
	}

	private static CharacterDefinition BuildRangerFallbackDefinition()
	{
		return new CharacterDefinition
		{
			CharacterId = "ranged",
			DisplayName = "Ranger Core",
			DisplayNameZhTw = "遊俠核心",
			Description = "Precision ranged specialist. Maintains stable output at safe distance with fast single-shot fire.",
			DescriptionZhTw = "精準遠程特化，以快速單發維持穩定輸出，適合安全距離作戰。",
			PrimaryAbility = AttackAbilityKind.Ranged,
			SecondaryAbility = AttackAbilityKind.None,
			MobilityAbility = MobilityAbilityKind.None,
			RangedDamage = 2,
			RangedCooldown = 0.64f
		};
	}

	private static CharacterDefinition BuildBladeFallbackDefinition()
	{
		return new CharacterDefinition
		{
			CharacterId = "melee",
			DisplayName = "Blade Core",
			DisplayNameZhTw = "刃核",
			Description = "High-mobility melee duelist. Strong close-range bursts and reposition tools, but punish mistakes heavily.",
			DescriptionZhTw = "高機動近戰決鬥者，近距離爆發強，但失誤懲罰也更高。",
			PrimaryAbility = AttackAbilityKind.Melee,
			SecondaryAbility = AttackAbilityKind.None,
			MobilityAbility = MobilityAbilityKind.Dash,
			MaxHp = 2,
			MeleeDamage = 4,
			MeleeCooldown = 0.68f
		};
	}

	private static CharacterDefinition BuildBulwarkFallbackDefinition()
	{
		return new CharacterDefinition
		{
			CharacterId = "tank_burst",
			DisplayName = "Bulwark Core",
			DisplayNameZhTw = "堡壘核心",
			Description = "Frontline anchor with high durability. Fires heavy 2-round bursts with strong knockback, trading speed for control.",
			DescriptionZhTw = "前線錨點型角色，擁有較高耐久；以雙發重射與擊退換取節奏控制。",
			PrimaryAbility = AttackAbilityKind.Ranged,
			SecondaryAbility = AttackAbilityKind.None,
			MobilityAbility = MobilityAbilityKind.None,
			MaxHp = 5,
			RegenAmount = 1,
			RangedDamage = 2,
			RangedCooldown = 0.72f,
			RangedFirePattern = PrimaryFirePattern.Burst2
		};
	}
}
