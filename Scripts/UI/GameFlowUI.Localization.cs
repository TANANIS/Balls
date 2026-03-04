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
		_sharedState.Settings.Locale = locale;
		_sharedState.Settings.LanguageIndex = GetLanguageIndexFromLocale(locale);
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
		_startMainPageController?.ApplyLocalizedTexts();
		_startSettingsPageController?.ApplyLocalizedTexts();
		_startCardsPageController?.ApplyLocalizedTexts();
		_startControlsPageController?.ApplyLocalizedTexts();
		_startCharacterPageController?.ApplyLocalizedStaticTexts();

		if (_startButton != null) _startButton.Text = Tr("UI.START.BUTTON_START");
		if (_startSettingsButton != null) _startSettingsButton.Text = Tr("UI.COMMON.SETTINGS");
		if (_startCardsButton != null) _startCardsButton.Text = TrOrDefault("UI.START.BUTTON_CARDS", "Cards", "Cards");
		if (_startLeaderboardButton != null) _startLeaderboardButton.Text = TrOrDefault("UI.START.BUTTON_LEADERBOARD", "Leaderboard", "排行榜");
		if (_startQuitButton != null) _startQuitButton.Text = Tr("UI.COMMON.QUIT");
		if (_startSettingsControlsButton != null) _startSettingsControlsButton.Text = TrOrDefault("UI.SETTINGS.CONTROLS", "Controls", "\u64cd\u4f5c\u8a2d\u5b9a");
		if (_startDeleteSaveButton != null) _startDeleteSaveButton.Text = TrOrDefault("UI.META.DELETE_SAVE_BUTTON", "Delete Save Data", "\u522a\u9664\u5b58\u6a94\u8cc7\u6599");
		if (_startCharacterBackButton != null) _startCharacterBackButton.Text = Tr("UI.COMMON.BACK");
		if (_startSettingsBackButton != null) _startSettingsBackButton.Text = Tr("UI.COMMON.BACK");
		if (_startCardsBackButton != null) _startCardsBackButton.Text = Tr("UI.COMMON.BACK");
		if (_startControlsBackButton != null) _startControlsBackButton.Text = Tr("UI.COMMON.BACK");

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

		var startTitle = GetNodeOrNull<Label>(StartTitleLabelPath);
		if (startTitle != null) startTitle.Text = Tr("UI.START.TITLE");
		var startSubtitle = GetNodeOrNull<Label>(StartSubtitleLabelPath);
		if (startSubtitle != null) startSubtitle.Text = Tr("UI.START.SUBTITLE");
		var startDesc = GetNodeOrNull<Label>(StartDescriptionLabelPath);
		if (startDesc != null)
		{
			startDesc.Text = TrOrDefault(
				"UI.START.DESC.V2",
				"Controls:\nMove: W A S D\nConfirm / Menu: Left Mouse\nPause: Esc\n\nPre-Run Flow:\n1) Choose Hero\n2) Event Purchases (Infuse Domain Power)\n3) Arrange 4 Calamity Slots (Tier 0 -> Tier 3)\n\nGoal:\nSurvive to 15:00. Defeat each phase boss to gain +1 Level and +10 EXP.",
				"操作：\n移動：W A S D\n確認 / 選單：滑鼠左鍵\n暫停：Esc\n\n開局流程：\n1) 選擇角色\n2) 事件購買（灌注神域力量）\n3) 安排 4 格災厄槽位（Tier 0 -> Tier 3）\n\n目標：\n存活到 15:00。每次擊敗階段 BOSS 立即獲得 +1 等與 +10 EXP。");
		}
		var boardTitle = GetNodeOrNull<Label>(StartPerfectBoardTitleLabelPath);
		if (boardTitle != null) boardTitle.Text = Tr("UI.START.PERFECT_BOARD_TITLE");
		var characterSelectTitle = GetNodeOrNull<Label>(StartCharacterSelectTitleLabelPath);
		if (characterSelectTitle != null)
			characterSelectTitle.Text = TrOrDefault("UI.META.TITLE", "HERO PROGRESSION", "\u82f1\u96c4\u990a\u6210");
		var characterSelectFluxLabel = GetNodeOrNull<Label>(StartCharacterFluxLabelPath);
		if (characterSelectFluxLabel != null)
			characterSelectFluxLabel.Text = $"{TrOrDefault("UI.META.FLUX", "Aether", "靈塵")}:";
		var abilityTreeGraph = GetNodeOrNull<Label>(StartAbilityTreeGraphLabelPath);
		if (abilityTreeGraph != null)
			abilityTreeGraph.Text = TrOrDefault("UI.META.NOT_AVAILABLE", "Coming Soon", "\u5c1a\u672a\u958b\u653e");
		var startSettingsTitle = GetNodeOrNull<Label>(StartSettingsTitleLabelPath);
		if (startSettingsTitle != null) startSettingsTitle.Text = Tr("UI.COMMON.SETTINGS");
		var startCardsTitle = GetNodeOrNull<Label>(StartCardsTitleLabelPath);
		if (startCardsTitle != null) startCardsTitle.Text = TrOrDefault("UI.START.CARDS_TITLE", "Boons & Traits", "恩賜與詞條");
		var pauseTitle = GetNodeOrNull<Label>(PauseTitleLabelPath);
		if (pauseTitle != null) pauseTitle.Text = Tr("UI.PAUSE.TITLE");
		var pauseSettingsTitle = GetNodeOrNull<Label>(PauseSettingsTitleLabelPath);
		if (pauseSettingsTitle != null) pauseSettingsTitle.Text = Tr("UI.COMMON.SETTINGS");

		var startBgmLabel = GetNodeOrNull<Label>(StartBgmLabelPath);
		if (startBgmLabel != null) startBgmLabel.Text = Tr("UI.SETTINGS.BGM");
		var startSfxLabel = GetNodeOrNull<Label>(StartSfxLabelPath);
		if (startSfxLabel != null) startSfxLabel.Text = Tr("UI.SETTINGS.SFX");
		var startWindowModeLabel = GetNodeOrNull<Label>(StartWindowModeLabelPath);
		if (startWindowModeLabel != null) startWindowModeLabel.Text = Tr("UI.SETTINGS.WINDOW_MODE");
		var startWindowSizeLabel = GetNodeOrNull<Label>(StartWindowSizeLabelPath);
		if (startWindowSizeLabel != null) startWindowSizeLabel.Text = Tr("UI.SETTINGS.WINDOW_SIZE");
		var startLanguageLabel = GetNodeOrNull<Label>(StartLanguageLabelPath);
		if (startLanguageLabel != null) startLanguageLabel.Text = Tr("UI.SETTINGS.LANGUAGE");

		var pauseBgmLabel = GetNodeOrNull<Label>(PauseBgmLabelPath);
		if (pauseBgmLabel != null) pauseBgmLabel.Text = Tr("UI.SETTINGS.BGM");
		var pauseSfxLabel = GetNodeOrNull<Label>(PauseSfxLabelPath);
		if (pauseSfxLabel != null) pauseSfxLabel.Text = Tr("UI.SETTINGS.SFX");
		var pauseWindowModeLabel = GetNodeOrNull<Label>(PauseWindowModeLabelPath);
		if (pauseWindowModeLabel != null) pauseWindowModeLabel.Text = Tr("UI.SETTINGS.WINDOW_MODE");
		var pauseWindowSizeLabel = GetNodeOrNull<Label>(PauseWindowSizeLabelPath);
		if (pauseWindowSizeLabel != null) pauseWindowSizeLabel.Text = Tr("UI.SETTINGS.WINDOW_SIZE");
		var pauseLanguageLabel = GetNodeOrNull<Label>(PauseLanguageLabelPath);
		if (pauseLanguageLabel != null) pauseLanguageLabel.Text = Tr("UI.SETTINGS.LANGUAGE");

		var deleteSaveDialog = _startDeleteSaveDialog;
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

		ApplyTitleBrandingOverrides();
		RefreshCharacterSelectUi();
	}
}
