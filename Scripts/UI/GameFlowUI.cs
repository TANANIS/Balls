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
			UpgradeCategory.WeaponModifier => TrOrDefault("UI.CATEGORY.CORE_ATTACK", "Battle Arts", "戰鬥技藝"),
			UpgradeCategory.PressureModifier => TrOrDefault("UI.CATEGORY.DIRECTOR", "Encounter Flow", "戰局節奏"),
			UpgradeCategory.AnomalySpecialist => TrOrDefault("UI.CATEGORY.ANOMALY", "Arcana", "奧術"),
			UpgradeCategory.SpatialControl => TrOrDefault("UI.CATEGORY.SPATIAL", "Field Control", "場域掌控"),
			UpgradeCategory.RiskAmplifier => TrOrDefault("UI.CATEGORY.SURVIVAL", "Survival", "Survival"),
			UpgradeCategory.EconomyModifier => TrOrDefault("UI.CATEGORY.ECONOMY", "Resource", "資源"),
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

		return fallback;
	}

	private static CharacterDefinition BuildMageFallbackDefinition()
	{
		return new CharacterDefinition
		{
			CharacterId = "ranged",
			DisplayName = "Mage",
			DisplayNameZhTw = "法師",
			Description = "Arcane caster who threads precise spell bolts from a safe distance.",
			DescriptionZhTw = "奧術施法者，擅長在安全距離精準連發魔彈。",
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
			DisplayName = "Knight",
			DisplayNameZhTw = "騎士",
			Description = "Swift spellblade duelist. Excels at close-range bursts and dash repositioning.",
			DescriptionZhTw = "高機動魔劍決鬥者，擅長近距離爆發與衝刺換位。",
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
			DisplayName = "Priest",
			DisplayNameZhTw = "牧師",
			Description = "Battle cleric of the front line. Fires heavy twofold holy bolts with strong knockback.",
			DescriptionZhTw = "前線戰鬥牧師，能射出厚重的雙重聖彈並附帶強力擊退。",
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
