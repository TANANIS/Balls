using Godot;

public partial class GameFlowUI
{
	private void ResolveNodeReferences()
	{
		// Resolve scene dependencies once to keep runtime logic clean.
		_player = GetNodeOrNull<Player>(PlayerPath);
		_cursorRing = GetNodeOrNull<CursorRing>(CursorRingPath);
		if (_player != null)
			_playerHealth = _player.GetNodeOrNull<PlayerHealth>("Health");

		_titleScreenPanel = GetNodeOrNull<Control>(TitleScreenPath);
		_startPanel = GetNodeOrNull<Control>(StartPanelPath);
		_startMainBackToTitleMask = GetNodeOrNull<ColorRect>(StartMainBackToTitleMaskPath);
		_startMainPageController = GetNodeOrNull<StartMainPageController>(StartMainPageControllerPath);
		_startSettingsPageController = GetNodeOrNull<StartSettingsPageController>(StartSettingsPageControllerPath);
		_startCardsPageController = GetNodeOrNull<StartCardsPageController>(StartCardsPageControllerPath);
		_startControlsPageController = GetNodeOrNull<StartControlsPageController>(StartControlsPageControllerPath);
		_startCharacterPageController = GetNodeOrNull<StartCharacterSelectPageController>(StartCharacterPageControllerPath);
		_startMainVBox = GetNodeOrNull<Control>(StartMainVBoxPath);
		_startSettingsPanel = GetNodeOrNull<Control>(StartSettingsPanelPath);
		_startSettingsTopLetterbox = GetNodeOrNull<ColorRect>(StartSettingsTopLetterboxPath);
		_startSettingsBottomLetterbox = GetNodeOrNull<ColorRect>(StartSettingsBottomLetterboxPath);
		_startCardsPanel = GetNodeOrNull<Control>(StartCardsPanelPath);
		_startControlsPanel = GetNodeOrNull<Control>(StartControlsPanelPath);
		_startCharacterSelectPanel = GetNodeOrNull<Control>(StartCharacterSelectPanelPath);
		_startEventLoadoutPanel = GetNodeOrNull<Control>(StartEventLoadoutPanelPath);
		_restartPanel = GetNodeOrNull<Control>(RestartPanelPath);
		_pausePanel = GetNodeOrNull<Control>(PausePanelPath);
		_pauseMainVBox = GetNodeOrNull<Control>(PauseMainVBoxPath);
		_pauseSettingsPanel = GetNodeOrNull<Control>(PauseSettingsPanelPath);
		_startButton = _startMainPageController?.StartButton ?? GetNodeOrNull<Button>(StartButtonPath);
		_startSettingsButton = _startMainPageController?.SettingsButton ?? GetNodeOrNull<Button>(StartSettingsButtonPath);
		_startCardsButton = _startMainPageController?.CardsButton ?? GetNodeOrNull<Button>(StartCardsButtonPath);
		_startLeaderboardButton = _startMainPageController?.LeaderboardButton ?? GetNodeOrNull<Button>(StartLeaderboardButtonPath);
		_startQuitButton = _startMainPageController?.QuitButton ?? GetNodeOrNull<Button>(StartQuitButtonPath);
		_startSettingsControlsButton = _startSettingsPageController?.ControlsButton ?? GetNodeOrNull<Button>(StartSettingsControlsButtonPath);
		_startDeleteSaveButton = _startSettingsPageController?.DeleteSaveButton ?? GetNodeOrNull<Button>(StartDeleteSaveButtonPath);
		_startDeleteSaveDialog = GetNodeOrNull<ConfirmationDialog>(StartDeleteSaveDialogPath);
		_startPerfectLeaderboardLabel = _startMainPageController?.PerfectLeaderboardLabel ?? GetNodeOrNull<Label>(StartPerfectLeaderboardPath);
		_startSettingsBackButton = _startSettingsPageController?.BackButton ?? GetNodeOrNull<Button>(StartSettingsBackButtonPath);
		_startCardsBackButton = _startCardsPageController?.BackButton ?? GetNodeOrNull<Button>(StartCardsBackButtonPath);
		_startControlsBackButton = _startControlsPageController?.BackButton ?? GetNodeOrNull<Button>(StartControlsBackButtonPath);
		_startCardsContentLabel = _startCardsPageController?.ContentLabel ?? GetNodeOrNull<Label>(StartCardsContentPath);
		_startCharacterRangedButton = _startCharacterPageController?.RangedButton ?? GetNodeOrNull<Button>(StartCharacterRangedButtonPath);
		_startCharacterSwordsmanButton = _startCharacterPageController?.SwordsmanButton ?? GetNodeOrNull<Button>(StartCharacterSwordsmanButtonPath);
		_startCharacterTankButton = _startCharacterPageController?.TankButton ?? GetNodeOrNull<Button>(StartCharacterTankButtonPath);
		_startCharacterArcherButton = _startCharacterPageController?.ArcherButton ?? GetNodeOrNull<Button>(StartCharacterArcherButtonPath);
		_startCharacterFluxValueLabel = _startCharacterPageController?.FluxValueLabel ?? GetNodeOrNull<Label>(StartCharacterFluxValuePath);
		_startCharacterBackButton = _startCharacterPageController?.BackButton ?? GetNodeOrNull<Button>(StartCharacterBackButtonPath);
		_startCharacterConfirmButton = _startCharacterPageController?.ConfirmButton ?? GetNodeOrNull<Button>(StartCharacterConfirmButtonPath);
		_startEventLoadoutBackButton = GetNodeOrNull<Button>(StartEventLoadoutBackButtonPath);
		_startEventLoadoutRollAllButton = GetNodeOrNull<Button>(StartEventLoadoutRollAllButtonPath);
		_startEventLoadoutStartRunButton = GetNodeOrNull<Button>(StartEventLoadoutStartRunButtonPath);
		_startEventLoadoutInventoryLabel = GetNodeOrNull<Label>(StartEventLoadoutInventoryLabelPath);
		_startEventLoadoutSummaryLabel = GetNodeOrNull<Label>(StartEventLoadoutSummaryLabelPath);
		_restartBackToMetaButton = GetNodeOrNull<Button>(RestartBackToMetaButtonPath);
		_restartButton = GetNodeOrNull<Button>(RestartButtonPath);
		_pauseResumeButton = GetNodeOrNull<Button>(PauseResumeButtonPath);
		_pauseSettingsButton = GetNodeOrNull<Button>(PauseSettingsButtonPath);
		_pauseRestartButton = GetNodeOrNull<Button>(PauseRestartButtonPath);
		_pauseToTitleButton = GetNodeOrNull<Button>(PauseToTitleButtonPath);
		_pauseQuitButton = GetNodeOrNull<Button>(PauseQuitButtonPath);
		_settingsBackButton = GetNodeOrNull<Button>(SettingsBackButtonPath);
		_settingsBgmSlider = GetNodeOrNull<HSlider>(SettingsBgmSliderPath);
		_settingsSfxSlider = GetNodeOrNull<HSlider>(SettingsSfxSliderPath);
		_settingsWindowSizeOption = GetNodeOrNull<OptionButton>(SettingsWindowSizePath);
		_settingsWindowModeOption = GetNodeOrNull<OptionButton>(SettingsWindowModePath);
		_startSettingsBgmSlider = _startSettingsPageController?.BgmSlider ?? GetNodeOrNull<HSlider>(StartSettingsBgmSliderPath);
		_startSettingsSfxSlider = _startSettingsPageController?.SfxSlider ?? GetNodeOrNull<HSlider>(StartSettingsSfxSliderPath);
		_startSettingsWindowSizeOption = _startSettingsPageController?.WindowSizeOption ?? GetNodeOrNull<OptionButton>(StartSettingsWindowSizePath);
		_startSettingsWindowModeOption = _startSettingsPageController?.WindowModeOption ?? GetNodeOrNull<OptionButton>(StartSettingsWindowModePath);
		_startSettingsLanguageOption = _startSettingsPageController?.LanguageOption ?? GetNodeOrNull<OptionButton>(StartSettingsLanguagePath);
		_settingsLanguageOption = GetNodeOrNull<OptionButton>(SettingsLanguagePath);
		_upgradeMenu = GetNodeOrNull<UpgradeMenu>(UpgradeMenuPath);
		_hudOverlayRoot = GetNodeOrNull<Control>(HudOverlayRootPath);
		if (_hudOverlayRoot != null)
			_hudOverlayRoot.Visible = true;
		_scoreLabel = GetNodeOrNull<Label>(ScoreLabelPath);
		_playerHealthBar = GetNodeOrNull<Control>(PlayerHealthBarPath);
		_experienceBarRoot = GetNodeOrNull<Control>(ExperienceBarRootPath);
		_experienceBar = GetNodeOrNull<ProgressBar>(ExperienceBarPath);
		_experienceLabel = GetNodeOrNull<Label>(ExperienceLabelPath);
		_matchCountdownLabel = GetNodeOrNull<Label>(MatchCountdownLabelPath);
		_eventBannerLabel = GetNodeOrNull<Label>(EventBannerLabelPath);
		_eventHintLabel = GetNodeOrNull<Label>(EventHintLabelPath);
		_hybridToastLabel = GetNodeOrNull<Label>(HybridToastLabelPath);
		_finalScoreLabel = GetNodeOrNull<Label>(FinalScoreLabelPath);
		_finalSurvivalLabel = GetNodeOrNull<Label>(FinalSurvivalLabelPath);
		_finalFluxGainLabel = GetNodeOrNull<Label>(FinalFluxGainLabelPath);
		_finalFluxWalletLabel = GetNodeOrNull<Label>(FinalFluxWalletLabelPath);
		_finalShardBreakdownLabel = GetNodeOrNull<Label>(FinalShardBreakdownLabelPath);
		_pauseBuildSummaryLabel = GetNodeOrNull<Label>(PauseBuildSummaryLabelPath);
		_finalBuildSummaryLabel = GetNodeOrNull<Label>(FinalBuildSummaryLabelPath);
		_restartTitleLabel = GetNodeOrNull<Label>(RestartTitleLabelPath);
		_restartPerfectBannerLabel = GetNodeOrNull<Label>(RestartPerfectBannerPath);
		_restartHintLabel = GetNodeOrNull<Label>(RestartHintLabelPath);
		_startCharacterDescriptionLabel = _startCharacterPageController?.DescriptionLabel ?? GetNodeOrNull<Label>(StartCharacterDescriptionPath);
		_background = GetNodeOrNull<CanvasItem>(BackgroundPath);
		_backgroundDimmer = GetNodeOrNull<ColorRect>(BackgroundDimmerPath);
		_menuBackground = GetNodeOrNull<Sprite2D>(MenuBackgroundPath);
		_menuDimmer = GetNodeOrNull<ColorRect>(MenuDimmerPath);
		_enemiesRoot = GetNodeOrNull<CanvasItem>(EnemiesPath);
		_projectilesRoot = GetNodeOrNull<CanvasItem>(ProjectilesPath);
		_obstaclesRoot = GetNodeOrNull<CanvasItem>(ObstaclesPath);

		if (_menuBackground != null)
		{
			_menuBackground.TopLevel = true;
			FitMenuBackground();
		}
		if (_menuDimmer != null)
			_menuDimmer.TopLevel = true;
		InitializeStartMainBackToTitleMask();
		InitializeStartSubpanelDimmerFx();
		InitializeStartSettingsLetterboxFx();
		SetPausePanels(showPausePanel: false, showMain: true, showSettings: false);
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		ResolveEventLoadoutNodes();
		ResolveEventUnlockNodes();
		RefreshPerfectLeaderboardUi();

		_rangedCharacter = LoadCharacterDefinitionOrFallback(
			RangedCharacterResourcePath,
			BuildMageFallbackDefinition());
		_swordsmanCharacter = LoadCharacterDefinitionOrFallback(
			SwordsmanCharacterResourcePath,
			BuildSwordsmanFallbackDefinition());
		_tankCharacter = LoadCharacterDefinitionOrFallback(
			TankCharacterResourcePath,
			BuildBulwarkFallbackDefinition());
		_archerCharacter = LoadCharacterDefinitionOrFallback(
			ArcherCharacterResourcePath,
			BuildArcherFallbackDefinition());
		_selectedCharacterDefinition = RunContext.Instance?.GetSelectedOrDefault() ?? _rangedCharacter ?? _swordsmanCharacter ?? _tankCharacter ?? _archerCharacter;
		_sharedState.SelectedCharacterDefinition = _selectedCharacterDefinition;

		var scoreList = GetTree().GetNodesInGroup(RuntimeGroups.ScoreSystem);
		if (scoreList.Count > 0)
			_scoreSystem = scoreList[0] as ScoreSystem;

		var stabilityList = GetTree().GetNodesInGroup(RuntimeGroups.StabilitySystem);
		if (stabilityList.Count > 0)
			_stabilitySystem = stabilityList[0] as StabilitySystem;

		var progressionList = GetTree().GetNodesInGroup(RuntimeGroups.ProgressionSystem);
		if (progressionList.Count > 0)
			_progressionSystem = progressionList[0] as ProgressionSystem;

		var upgradeList = GetTree().GetNodesInGroup(RuntimeGroups.UpgradeSystem);
		if (upgradeList.Count > 0)
			_upgradeSystem = upgradeList[0] as UpgradeSystem;

		var eventDirectorList = GetTree().GetNodesInGroup(RuntimeGroups.EventDirector);
		if (eventDirectorList.Count > 0)
			_eventDirector = eventDirectorList[0] as EventDirector;
	}

	private void BindSignals()
	{
		// Connect all one-way UI event flow here.
		GetViewport().SizeChanged += OnViewportSizeChanged;

		if (_startMainPageController != null)
		{
			_startMainPageController.StartPressed += OnStartPressed;
			_startMainPageController.SettingsPressed += OnStartSettingsPressed;
			_startMainPageController.CardsPressed += OnStartCardsPressed;
			_startMainPageController.LeaderboardPressed += OnStartLeaderboardPressed;
			_startMainPageController.QuitPressed += OnQuitGamePressed;
		}
		else
		{
			if (_startButton != null)
				_startButton.Pressed += OnStartPressed;
			if (_startSettingsButton != null)
				_startSettingsButton.Pressed += OnStartSettingsPressed;
			if (_startCardsButton != null)
				_startCardsButton.Pressed += OnStartCardsPressed;
			if (_startLeaderboardButton != null)
				_startLeaderboardButton.Pressed += OnStartLeaderboardPressed;
			if (_startQuitButton != null)
				_startQuitButton.Pressed += OnQuitGamePressed;
		}
		if (_startSettingsPageController != null)
		{
			_startSettingsPageController.ControlsPressed += OnStartControlsPressed;
			_startSettingsPageController.DeleteSavePressed += OnStartDeleteSavePressed;
			_startSettingsPageController.BackPressed += OnStartSettingsBackPressed;
		}
		else
		{
			if (_startSettingsControlsButton != null)
				_startSettingsControlsButton.Pressed += OnStartControlsPressed;
			if (_startDeleteSaveButton != null)
				_startDeleteSaveButton.Pressed += OnStartDeleteSavePressed;
			if (_startSettingsBackButton != null)
				_startSettingsBackButton.Pressed += OnStartSettingsBackPressed;
		}
		if (_startControlsPageController != null)
		{
			_startControlsPageController.AutoAimToggled += OnControlsAutoAimToggled;
			_startControlsPageController.BackPressed += OnStartControlsBackPressed;
		}
		else if (_startControlsBackButton != null)
		{
			_startControlsBackButton.Pressed += OnStartControlsBackPressed;
		}
		if (_startDeleteSaveDialog != null)
			_startDeleteSaveDialog.Confirmed += OnStartDeleteSaveConfirmed;
		if (_startCardsPageController != null)
		{
			_startCardsPageController.BackPressed += OnStartCardsBackPressed;
		}
		else if (_startCardsBackButton != null)
		{
			_startCardsBackButton.Pressed += OnStartCardsBackPressed;
		}
		if (_startCharacterPageController != null)
		{
			_startCharacterPageController.RangedPressed += OnCharacterRangedPressed;
			_startCharacterPageController.SwordsmanPressed += OnCharacterSwordsmanPressed;
			_startCharacterPageController.TankPressed += OnCharacterTankPressed;
			_startCharacterPageController.ArcherPressed += OnCharacterArcherPressed;
			_startCharacterPageController.BackPressed += OnCharacterSelectBackPressed;
			_startCharacterPageController.ConfirmPressed += OnCharacterSelectConfirmPressed;
		}
		else
		{
			if (_startCharacterRangedButton != null)
				_startCharacterRangedButton.Pressed += OnCharacterRangedPressed;
			if (_startCharacterSwordsmanButton != null)
				_startCharacterSwordsmanButton.Pressed += OnCharacterSwordsmanPressed;
			if (_startCharacterTankButton != null)
				_startCharacterTankButton.Pressed += OnCharacterTankPressed;
			if (_startCharacterArcherButton != null)
				_startCharacterArcherButton.Pressed += OnCharacterArcherPressed;
			if (_startCharacterBackButton != null)
				_startCharacterBackButton.Pressed += OnCharacterSelectBackPressed;
			if (_startCharacterConfirmButton != null)
				_startCharacterConfirmButton.Pressed += OnCharacterSelectConfirmPressed;
		}
		BindEventUnlockSignals();
		BindEventLoadoutSignals();
		if (_restartBackToMetaButton != null)
			_restartBackToMetaButton.Pressed += OnRestartBackToMetaPressed;
		if (_restartButton != null)
			_restartButton.Pressed += OnRestartPressed;
		if (_playerHealth != null)
			_playerHealth.Died += OnPlayerDied;
		if (_scoreSystem != null)
			_scoreSystem.ScoreChanged += OnScoreChanged;
		if (_pauseResumeButton != null)
			_pauseResumeButton.Pressed += OnPauseResumePressed;
		if (_pauseSettingsButton != null)
			_pauseSettingsButton.Pressed += OnPauseSettingsPressed;
		if (_pauseRestartButton != null)
			_pauseRestartButton.Pressed += OnPauseRestartPressed;
		if (_pauseToTitleButton != null)
			_pauseToTitleButton.Pressed += OnPauseToTitlePressed;
		if (_pauseQuitButton != null)
			_pauseQuitButton.Pressed += OnQuitGamePressed;
		if (_settingsBackButton != null)
			_settingsBackButton.Pressed += OnPauseSettingsBackPressed;
		if (_settingsBgmSlider != null)
			_settingsBgmSlider.ValueChanged += OnSettingsBgmChanged;
		if (_settingsSfxSlider != null)
			_settingsSfxSlider.ValueChanged += OnSettingsSfxChanged;
		if (_settingsWindowSizeOption != null)
			_settingsWindowSizeOption.ItemSelected += OnSettingsWindowSizeSelected;
		if (_settingsWindowModeOption != null)
			_settingsWindowModeOption.ItemSelected += OnSettingsWindowModeSelected;
		if (_settingsLanguageOption != null)
			_settingsLanguageOption.ItemSelected += OnSettingsLanguageSelected;
		if (_startSettingsPageController != null)
		{
			_startSettingsPageController.BgmChanged += OnSettingsBgmChanged;
			_startSettingsPageController.SfxChanged += OnSettingsSfxChanged;
			_startSettingsPageController.WindowSizeSelected += OnSettingsWindowSizeSelected;
			_startSettingsPageController.WindowModeSelected += OnSettingsWindowModeSelected;
			_startSettingsPageController.LanguageSelected += OnSettingsLanguageSelected;
		}
		else
		{
			if (_startSettingsBgmSlider != null)
				_startSettingsBgmSlider.ValueChanged += OnSettingsBgmChanged;
			if (_startSettingsSfxSlider != null)
				_startSettingsSfxSlider.ValueChanged += OnSettingsSfxChanged;
			if (_startSettingsWindowSizeOption != null)
				_startSettingsWindowSizeOption.ItemSelected += OnSettingsWindowSizeSelected;
			if (_startSettingsWindowModeOption != null)
				_startSettingsWindowModeOption.ItemSelected += OnSettingsWindowModeSelected;
			if (_startSettingsLanguageOption != null)
				_startSettingsLanguageOption.ItemSelected += OnSettingsLanguageSelected;
		}
		if (_stabilitySystem != null)
		{
			_stabilitySystem.Collapsed += OnUniverseCollapsed;
			_stabilitySystem.MatchDurationReached += OnMatchDurationReached;
		}

		InitializeSettingsUi();
		LoadSettingsFromDisk();
		ApplyLocalizedTexts();
	}

	private void InitializeStartMainBackToTitleMask()
	{
		if (_startMainBackToTitleMask == null)
			return;

		_startMainBackToTitleMask.MouseFilter = Control.MouseFilterEnum.Ignore;
		Color color = _startMainBackToTitleMask.Color;
		color.A = 0f;
		_startMainBackToTitleMask.Color = color;
		_startMainBackToTitleMask.Visible = false;
	}
}
