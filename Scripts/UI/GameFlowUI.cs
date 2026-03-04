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
		UpdateCursorPresentationMode();
		AudioManager.Instance?.PlayBgmTitleTheme();
	}

	public override void _Input(InputEvent @event)
	{
		InputDeviceService.NotifyInput(@event);
		if (_startControlsOpen && _startControlsPageController != null && _startControlsPageController.TryHandleRebindInput(@event))
			GetViewport().SetInputAsHandled();
	}

	public override void _Process(double delta)
	{
		UpdateBootBackgroundSwayFx((float)delta);
		UpdateUpgradeProgressUi();
		UpdateMatchCountdownUi();
		UpdateEventBannerUi();
		UpdateEventHintUi();
		UpdateHybridToastUi();
		TryResolvePendingPerfectClear();
		HandlePauseInput();
		UpdateCursorPresentationMode();
		if (!_started)
			FitMenuBackground();
	}

	private void UpdateCursorPresentationMode()
	{
		if (!GodotObject.IsInstanceValid(_cursorRing))
			return;

		bool showUiPointer = !_started
			|| _pauseMenuOpen
			|| _ending
			|| (_upgradeMenu != null && _upgradeMenu.IsOpen);
		_cursorRing.SetPresentationMode(
			showUiPointer
				? CursorRing.CursorPresentationMode.UiPointer
				: CursorRing.CursorPresentationMode.GameplayAim);
	}

	private void RefreshStartCardsCompendium()
	{
		if (_startCardsContentLabel == null)
			return;

		void ApplyCardsText(string text)
		{
			if (_startCardsContentLabel != null)
				_startCardsContentLabel.Text = text;
			_startCardsPageController?.SetCardsContent(text);
		}

		UpgradeCatalog catalog = _upgradeSystem?.Catalog;
		if (catalog == null)
			catalog = GD.Load<UpgradeCatalog>("res://Data/Upgrades/DefaultUpgradeCatalog.tres");
		if (catalog?.Entries == null || catalog.Entries.Count == 0)
		{
			ApplyCardsText(TrOrDefault("UI.START.CARDS_EMPTY", "No upgrade cards configured."));
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
				.Append(TrOrDefault("UI.START.CARDS_MAX_STACK", "MaxStack")).Append(": ")
				.Append(Mathf.Max(1, entry.MaxStack)).Append("\n\n");
		}

		if (sb.Length == 0)
		{
			ApplyCardsText(TrOrDefault("UI.START.CARDS_EMPTY", "No upgrade cards configured."));
			return;
		}

		ApplyCardsText(sb.ToString().TrimEnd());
	}

	private string GetLocalizedUpgradeCategory(UpgradeCategory category)
	{
		return category switch
		{
			UpgradeCategory.WeaponModifier => TrOrDefault("UI.CATEGORY.CORE_ATTACK", "Battle Arts"),
			UpgradeCategory.PressureModifier => TrOrDefault("UI.CATEGORY.DIRECTOR", "Encounter Flow"),
			UpgradeCategory.AnomalySpecialist => TrOrDefault("UI.CATEGORY.ANOMALY", "Arcana"),
			UpgradeCategory.SpatialControl => TrOrDefault("UI.CATEGORY.SPATIAL", "Field Control"),
			UpgradeCategory.RiskAmplifier => TrOrDefault("UI.CATEGORY.SURVIVAL", "Survival"),
			UpgradeCategory.EconomyModifier => TrOrDefault("UI.CATEGORY.ECONOMY", "Resource"),
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
			DisplayNameZhTw = "\u6cd5\u5e2b",
			Description = "Arcane caster who threads precise spell bolts from a safe distance.",
			DescriptionZhTw = "\u9060\u7a0b\u5967\u8853\u65bd\u6cd5\u8005\u3002\u4ee5\u7a69\u5b9a\u7684\u9b54\u5f48\u8f38\u51fa\u64ca\u9000\u654c\u7fa4\u3002",
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
			DisplayNameZhTw = "\u796d\u53f8",
			Description = "Battle cleric of the front line. Fires heavy twofold holy bolts with strong knockback.",
			DescriptionZhTw = "\u524d\u7dda\u6226\u9b25\u796d\u53f8\u3002\u767c\u5c04\u96d9\u767c\u8056\u5f48\u4e26\u5177\u6709\u5f37\u529b\u64ca\u9000\u6548\u679c\u3002",
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
			DisplayNameZhTw = "\u5f13\u624b",
			Description = "Mobile marksman. Every third attack fires a quick 3-shot burst.",
			DescriptionZhTw = "\u6a5f\u52d5\u5c04\u624b\u3002\u6bcf\u7b2c\u4e09\u6b21\u653b\u64ca\u6703\u89f8\u767c\u5feb\u901f\u4e09\u9023\u767c\u3002",
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
