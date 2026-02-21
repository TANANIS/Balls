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
		if (_startClearLeaderboardButton != null) _startClearLeaderboardButton.Text = Tr("UI.START.CLEAR_LEADERBOARD");
		if (_startCharacterBackButton != null) _startCharacterBackButton.Text = Tr("UI.COMMON.BACK");
		if (_startCharacterConfirmButton != null) _startCharacterConfirmButton.Text = Tr("UI.START.CONFIRM_START_RUN");
		if (_startSettingsBackButton != null) _startSettingsBackButton.Text = Tr("UI.COMMON.BACK");
		if (_startCardsBackButton != null) _startCardsBackButton.Text = Tr("UI.COMMON.BACK");

		if (_pauseResumeButton != null) _pauseResumeButton.Text = Tr("UI.PAUSE.RESUME");
		if (_pauseSettingsButton != null) _pauseSettingsButton.Text = Tr("UI.COMMON.SETTINGS");
		if (_pauseRestartButton != null) _pauseRestartButton.Text = Tr("UI.PAUSE.RESTART_RUN");
		if (_pauseToTitleButton != null) _pauseToTitleButton.Text = Tr("UI.PAUSE.BACK_TO_MENU");
		if (_pauseQuitButton != null) _pauseQuitButton.Text = Tr("UI.COMMON.QUIT_GAME");
		if (_settingsBackButton != null) _settingsBackButton.Text = Tr("UI.COMMON.BACK");
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
		var startSettingsTitle = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/VBox/Title");
		if (startSettingsTitle != null) startSettingsTitle.Text = Tr("UI.COMMON.SETTINGS");
		var startCardsTitle = GetNodeOrNull<Label>("Panels/StartPanel/Panel/CardsPanel/VBox/Title");
		if (startCardsTitle != null) startCardsTitle.Text = TrOrDefault("UI.START.CARDS_TITLE", "Upgrade Cards", "Upgrade Cards");
		var pauseTitle = GetNodeOrNull<Label>("Panels/PausePanel/Panel/VBox/Title");
		if (pauseTitle != null) pauseTitle.Text = Tr("UI.PAUSE.TITLE");
		var pauseSettingsTitle = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/VBox/Title");
		if (pauseSettingsTitle != null) pauseSettingsTitle.Text = Tr("UI.COMMON.SETTINGS");

		var startBgmLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/VBox/BgmLabel");
		if (startBgmLabel != null) startBgmLabel.Text = Tr("UI.SETTINGS.BGM");
		var startSfxLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/VBox/SfxLabel");
		if (startSfxLabel != null) startSfxLabel.Text = Tr("UI.SETTINGS.SFX");
		var startWindowModeLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/VBox/WindowModeLabel");
		if (startWindowModeLabel != null) startWindowModeLabel.Text = Tr("UI.SETTINGS.WINDOW_MODE");
		var startWindowSizeLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/VBox/WindowSizeLabel");
		if (startWindowSizeLabel != null) startWindowSizeLabel.Text = Tr("UI.SETTINGS.WINDOW_SIZE");
		var startLanguageLabel = GetNodeOrNull<Label>("Panels/StartPanel/Panel/SettingsPanel/VBox/LanguageLabel");
		if (startLanguageLabel != null) startLanguageLabel.Text = Tr("UI.SETTINGS.LANGUAGE");

		var pauseBgmLabel = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/VBox/BgmLabel");
		if (pauseBgmLabel != null) pauseBgmLabel.Text = Tr("UI.SETTINGS.BGM");
		var pauseSfxLabel = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/VBox/SfxLabel");
		if (pauseSfxLabel != null) pauseSfxLabel.Text = Tr("UI.SETTINGS.SFX");
		var pauseWindowModeLabel = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/VBox/WindowModeLabel");
		if (pauseWindowModeLabel != null) pauseWindowModeLabel.Text = Tr("UI.SETTINGS.WINDOW_MODE");
		var pauseWindowSizeLabel = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/VBox/WindowSizeLabel");
		if (pauseWindowSizeLabel != null) pauseWindowSizeLabel.Text = Tr("UI.SETTINGS.WINDOW_SIZE");
		var pauseLanguageLabel = GetNodeOrNull<Label>("Panels/PausePanel/Panel/SettingsPanel/VBox/LanguageLabel");
		if (pauseLanguageLabel != null) pauseLanguageLabel.Text = Tr("UI.SETTINGS.LANGUAGE");

		var clearDialog = GetNodeOrNull<ConfirmationDialog>("Panels/StartPanel/ClearLeaderboardConfirmDialog");
		if (clearDialog != null)
		{
			clearDialog.Title = Tr("UI.START.CLEAR_DIALOG_TITLE");
			clearDialog.DialogText = Tr("UI.START.CLEAR_DIALOG_TEXT");
			clearDialog.OkButtonText = Tr("UI.START.CLEAR_DIALOG_OK");
		}

		if (_startCardsOpen)
			RefreshStartCardsCompendium();
	}
}
