using Godot;
using System.Text;

public partial class GameFlowUI : Control
{
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		ResolveNodeReferences();
		BindSignals();
		ShowBootTitleScreen();
		AudioManager.Instance?.PlayBgmMenu();
	}

	public override void _Process(double delta)
	{
		UpdateUpgradeProgressUi();
		UpdateMatchCountdownUi();
		UpdateEventBannerUi();
		UpdateEventHintUi();
		UpdateHybridToastUi();
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
			_startCardsContentLabel.Text = TrOrDefault("UI.START.CARDS_EMPTY", "No upgrade cards configured.", "未設定任何升級卡片。");
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
				.Append(TrOrDefault("UI.START.CARDS_MAX_STACK", "MaxStack", "最大層數")).Append(": ")
				.Append(Mathf.Max(1, entry.MaxStack)).Append("\n\n");
		}

		if (sb.Length == 0)
		{
			_startCardsContentLabel.Text = TrOrDefault("UI.START.CARDS_EMPTY", "No upgrade cards configured.", "未設定任何升級卡片。");
			return;
		}

		_startCardsContentLabel.Text = sb.ToString().TrimEnd();
	}

	private string GetLocalizedUpgradeCategory(UpgradeCategory category)
	{
		return category switch
		{
			UpgradeCategory.WeaponModifier => TrOrDefault("UI.CATEGORY.CORE_ATTACK", "Battle Arts", "核心攻擊"),
			UpgradeCategory.PressureModifier => TrOrDefault("UI.CATEGORY.DIRECTOR", "Encounter Flow", "節奏壓力"),
			UpgradeCategory.AnomalySpecialist => TrOrDefault("UI.CATEGORY.ANOMALY", "Arcana", "異常專精"),
			UpgradeCategory.SpatialControl => TrOrDefault("UI.CATEGORY.SPATIAL", "Field Control", "空間控制"),
			UpgradeCategory.RiskAmplifier => TrOrDefault("UI.CATEGORY.SURVIVAL", "Survival", "生存"),
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
		{
			CharacterStatsCsvService.ApplyTo(loaded);
			return loaded;
		}

		Resource raw = ResourceLoader.Load(path);
		if (raw is CharacterDefinition typed)
		{
			CharacterStatsCsvService.ApplyTo(typed);
			return typed;
		}

		CharacterStatsCsvService.ApplyTo(fallback);
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
			DescriptionZhTw = "遠距施法者，擅長在安全距離以精準術彈持續輸出。",
			PrimaryAbility = AttackAbilityKind.Ranged,
			SecondaryAbility = AttackAbilityKind.None,
			MobilityAbility = MobilityAbilityKind.None,
			MoveMaxSpeed = 188f,
			RangedDamage = 2,
			RangedCooldown = 0.64f
		};
	}

	private static CharacterDefinition BuildSwordsmanFallbackDefinition()
	{
		return new CharacterDefinition
		{
			CharacterId = "swordsman",
			DisplayName = "Swordsman",
			DisplayNameZhTw = "\u528d\u58eb",
			Description = "Close-range duelist. Uses decisive melee strikes and dash repositioning.",
			DescriptionZhTw = "\u8fd1\u6230\u6c7a\u9b25\u8005\u3002\u4ee5\u9ad8\u7206\u767c\u8fd1\u8eab\u65ac\u64ca\u8207\u885d\u523a\u63db\u4f4d\u7dad\u6301\u7bc0\u594f\u3002",
			PrimaryAbility = AttackAbilityKind.Melee,
			SecondaryAbility = AttackAbilityKind.None,
			MobilityAbility = MobilityAbilityKind.Dash,
			MaxHp = 2,
			MeleeDamage = 4,
			MeleeCooldown = 2.72f,
			DashCooldown = 1.8f
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
			DescriptionZhTw = "前線戰鬥牧師，發射雙重聖彈並具備強力擊退。",
			PrimaryAbility = AttackAbilityKind.Ranged,
			SecondaryAbility = AttackAbilityKind.None,
			MobilityAbility = MobilityAbilityKind.None,
			MaxHp = 5,
			RegenAmount = 1,
			RegenIntervalSeconds = 30f,
			RangedDamage = 2,
			RangedCooldown = 0.72f
		};
	}

	private static CharacterDefinition BuildArcherFallbackDefinition()
	{
		return new CharacterDefinition
		{
			CharacterId = "archer",
			DisplayName = "Archer",
			DisplayNameZhTw = "弓箭手",
			Description = "Mobile marksman. Every third attack fires a quick 3-shot burst.",
			DescriptionZhTw = "機動型射手。每第三次攻擊會變為快速三連發。",
			PrimaryAbility = AttackAbilityKind.Ranged,
			SecondaryAbility = AttackAbilityKind.None,
			MobilityAbility = MobilityAbilityKind.None,
			MoveMaxSpeed = 216f,
			MaxHp = 2,
			RangedDamage = 2,
			RangedCooldown = 1.28f
		};
	}
}
