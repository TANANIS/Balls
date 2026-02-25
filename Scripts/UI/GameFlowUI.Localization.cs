using Godot;

public partial class GameFlowUI
{
	private const string LocaleEnglish = "en";
	private const string LocaleTraditionalChinese = "zh_TW";

	private string GetLocaleByIndex(int index)
	{
		return index switch
		{
			1 => LocaleTraditionalChinese,
			_ => LocaleEnglish
		};
	}

	private int GetLanguageIndexFromLocale(string locale)
	{
		return locale == LocaleTraditionalChinese ? 1 : 0;
	}

	private void ApplyLocale(string locale)
	{
		TranslationServer.SetLocale(locale);
		ApplyLocalizedTexts();
	}

	private void ApplyLocalizedTexts()
	{
		if (_scoreSystem != null)
			OnScoreChanged(_scoreSystem.Score);
		else if (_scoreLabel != null)
			_scoreLabel.Text = $"{Tr("UI.HUD.SCORE")}: 0";

		RefreshPerfectLeaderboardUi();
		RefreshCharacterSelectUi();
		RefreshPauseBuildSummary();
		RefreshFinalBuildSummary();
		ResetBuildSummaryLabels();

		if (_startButton != null) _startButton.Text = Tr("UI.START.BUTTON_START");
		if (_startSettingsButton != null) _startSettingsButton.Text = Tr("UI.COMMON.SETTINGS");
		if (_startCardsButton != null) _startCardsButton.Text = TrOrDefault("UI.START.BUTTON_CARDS", "Cards", "Cards");
		if (_startQuitButton != null) _startQuitButton.Text = Tr("UI.COMMON.QUIT");
		if (_startDeleteSaveButton != null) _startDeleteSaveButton.Text = TrOrDefault("UI.META.DELETE_SAVE_BUTTON", "Delete Save Data", "\u522a\u9664\u5b58\u6a94\u8cc7\u6599");
		if (_startCharacterBackButton != null) _startCharacterBackButton.Text = Tr("UI.COMMON.BACK");
		if (_startSettingsBackButton != null) _startSettingsBackButton.Text = Tr("UI.COMMON.BACK");
		if (_startCardsBackButton != null) _startCardsBackButton.Text = Tr("UI.COMMON.BACK");

		if (_pauseResumeButton != null) _pauseResumeButton.Text = Tr("UI.PAUSE.RESUME");
		if (_pauseSettingsButton != null) _pauseSettingsButton.Text = Tr("UI.COMMON.SETTINGS");
		if (_pauseRestartButton != null) _pauseRestartButton.Text = Tr("UI.PAUSE.RESTART_RUN");
		if (_pauseToTitleButton != null) _pauseToTitleButton.Text = Tr("UI.PAUSE.BACK_TO_MENU");
		if (_pauseQuitButton != null) _pauseQuitButton.Text = Tr("UI.COMMON.QUIT_GAME");
		if (_settingsBackButton != null) _settingsBackButton.Text = Tr("UI.COMMON.BACK");
		if (_restartBackToMetaButton != null) _restartBackToMetaButton.Text = TrOrDefault("UI.END.BACK_TO_META", "Back To Meta", "\u8fd4\u56de\u5c40\u5916\u990a\u6210");
		if (_restartButton != null) _restartButton.Text = Tr("UI.PAUSE.RESTART_RUN");

		if (_restartPerfectBannerLabel != null) _restartPerfectBannerLabel.Text = Tr("UI.END.PERFECT_BANNER");
		if (_restartHintLabel != null && !_ending) _restartHintLabel.Text = Tr("UI.END.HINT_RESTART");

		var startTitle = GetNodeOrNull<Label>("Panels/StartPanel/Panel/MainScroll/VBox/Header/Title");
		if (startTitle != null) startTitle.Text = Tr("UI.START.TITLE");
		var startSubtitle = GetNodeOrNull<Label>("Panels/StartPanel/Panel/MainScroll/VBox/Header/SubTitle");
		if (startSubtitle != null) startSubtitle.Text = Tr("UI.START.SUBTITLE");
		var startDesc = GetNodeOrNull<Label>("Panels/StartPanel/Panel/MainScroll/VBox/MainBody/LeftColumn/Desc");
		if (startDesc != null) startDesc.Text = Tr("UI.START.DESC");
		var boardTitle = GetNodeOrNull<Label>("Panels/StartPanel/Panel/MainScroll/VBox/MainBody/LeftColumn/PerfectBoardTitle");
		if (boardTitle != null) boardTitle.Text = Tr("UI.START.PERFECT_BOARD_TITLE");
		var characterSelectTitle = GetNodeOrNull<Label>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/HeaderRow/Title");
		if (characterSelectTitle != null)
			characterSelectTitle.Text = TrOrDefault("UI.META.TITLE", "HERO PROGRESSION", "\u82f1\u96c4\u990a\u6210");
		var characterSelectFluxLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/HeaderRow/FluxHeader/FluxLabel");
		if (characterSelectFluxLabel != null)
			characterSelectFluxLabel.Text = $"{TrOrDefault("UI.META.FLUX", "Aether", "靈塵")}:";
		var characterComingSoonButton = GetNodeOrNull<Button>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ContentRow/LeftColumn/CharacterButtons/LockedButton");
		if (characterComingSoonButton != null)
			characterComingSoonButton.Text = TrOrDefault("UI.META.NOT_AVAILABLE", "Coming Soon", "\u5c1a\u672a\u958b\u653e");
		var abilityTreeGraph = GetNodeOrNull<Label>("Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ContentRow/AbilityTreePanel/Margin/AbilityTreeVBox/AbilityTreeGraph");
		if (abilityTreeGraph != null)
			abilityTreeGraph.Text = TrOrDefault("UI.META.NOT_AVAILABLE", "Coming Soon", "\u5c1a\u672a\u958b\u653e");
		var startSettingsTitle = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/Title");
		if (startSettingsTitle != null) startSettingsTitle.Text = Tr("UI.COMMON.SETTINGS");
		var startCardsTitle = GetNodeOrNull<Label>("Panels/StartPanel/Panel/CardsPanel/VBox/Title");
		if (startCardsTitle != null) startCardsTitle.Text = TrOrDefault("UI.START.CARDS_TITLE", "Boons & Traits", "恩賜與詞條");
		var pauseTitle = GetNodeOrNull<Label>("Panels/PausePanel/Panel/VBox/Title");
		if (pauseTitle != null) pauseTitle.Text = Tr("UI.PAUSE.TITLE");
		var pauseSettingsTitle = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/Title");
		if (pauseSettingsTitle != null) pauseSettingsTitle.Text = Tr("UI.COMMON.SETTINGS");

		var startBgmLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/BgmLabel");
		if (startBgmLabel != null) startBgmLabel.Text = Tr("UI.SETTINGS.BGM");
		var startSfxLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/SfxLabel");
		if (startSfxLabel != null) startSfxLabel.Text = Tr("UI.SETTINGS.SFX");
		var startWindowModeLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowModeLabel");
		if (startWindowModeLabel != null) startWindowModeLabel.Text = Tr("UI.SETTINGS.WINDOW_MODE");
		var startWindowSizeLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowSizeLabel");
		if (startWindowSizeLabel != null) startWindowSizeLabel.Text = Tr("UI.SETTINGS.WINDOW_SIZE");
		var startLanguageLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/LanguageLabel");
		if (startLanguageLabel != null) startLanguageLabel.Text = Tr("UI.SETTINGS.LANGUAGE");

		var pauseBgmLabel = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/BgmLabel");
		if (pauseBgmLabel != null) pauseBgmLabel.Text = Tr("UI.SETTINGS.BGM");
		var pauseSfxLabel = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/SfxLabel");
		if (pauseSfxLabel != null) pauseSfxLabel.Text = Tr("UI.SETTINGS.SFX");
		var pauseWindowModeLabel = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowModeLabel");
		if (pauseWindowModeLabel != null) pauseWindowModeLabel.Text = Tr("UI.SETTINGS.WINDOW_MODE");
		var pauseWindowSizeLabel = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowSizeLabel");
		if (pauseWindowSizeLabel != null) pauseWindowSizeLabel.Text = Tr("UI.SETTINGS.WINDOW_SIZE");
		var pauseLanguageLabel = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/LanguageLabel");
		if (pauseLanguageLabel != null) pauseLanguageLabel.Text = Tr("UI.SETTINGS.LANGUAGE");

		var deleteSaveDialog = GetNodeOrNull<ConfirmationDialog>("Panels/StartPanel/DeleteSaveConfirmDialog");
		if (deleteSaveDialog != null)
		{
			deleteSaveDialog.Title = TrOrDefault("UI.META.DELETE_SAVE_TITLE", "Delete Save Data", "\u522a\u9664\u5b58\u6a94\u8cc7\u6599");
			deleteSaveDialog.DialogText = TrOrDefault(
				"UI.META.DELETE_SAVE_TEXT",
				"Delete current profile save data?\nThis will reset Aether, unlocks, levels, and hero talent progression.",
				"\u78ba\u5b9a\u522a\u9664\u76ee\u524d profile \u5b58\u6a94\u8cc7\u6599\uff1f\n\u9019\u6703\u91cd\u7f6e\u9748\u5875\u3001\u89e3\u9396\u3001\u7b49\u7d1a\u8207\u5929\u8ce6\u6a39\u9032\u5ea6\u3002");
			deleteSaveDialog.OkButtonText = TrOrDefault("UI.META.DELETE_SAVE_OK", "Delete", "\u522a\u9664");
		}

		if (_startCardsOpen)
			RefreshStartCardsCompendium();

		RefreshCharacterSelectUi();
	}
}
