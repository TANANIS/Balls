using Godot;

public partial class GameFlowUI
{
	private void ResolveNodeReferences()
	{
		// Resolve scene dependencies once to keep runtime logic clean.
		_player = GetNodeOrNull<Player>(PlayerPath);
		if (_player != null)
			_playerHealth = _player.GetNodeOrNull<PlayerHealth>("Health");

		_startPanel = GetNodeOrNull<Control>(StartPanelPath);
		_startMainVBox = GetNodeOrNull<Control>(StartMainVBoxPath);
		_startSettingsPanel = GetNodeOrNull<Control>(StartSettingsPanelPath);
		_startCardsPanel = GetNodeOrNull<Control>(StartCardsPanelPath);
		_startCharacterSelectPanel = GetNodeOrNull<Control>(StartCharacterSelectPanelPath);
		_restartPanel = GetNodeOrNull<Control>(RestartPanelPath);
		_pausePanel = GetNodeOrNull<Control>(PausePanelPath);
		_pauseMainVBox = GetNodeOrNull<Control>(PauseMainVBoxPath);
		_pauseSettingsPanel = GetNodeOrNull<Control>(PauseSettingsPanelPath);
		_startButton = GetNodeOrNull<Button>(StartButtonPath);
		_startSettingsButton = GetNodeOrNull<Button>(StartSettingsButtonPath);
		_startCardsButton = GetNodeOrNull<Button>(StartCardsButtonPath);
		_startQuitButton = GetNodeOrNull<Button>(StartQuitButtonPath);
		_startDeleteSaveButton = GetNodeOrNull<Button>(StartDeleteSaveButtonPath);
		_startDeleteSaveDialog = GetNodeOrNull<ConfirmationDialog>(StartDeleteSaveDialogPath);
		_startPerfectLeaderboardLabel = GetNodeOrNull<Label>(StartPerfectLeaderboardPath);
		_startSettingsBackButton = GetNodeOrNull<Button>(StartSettingsBackButtonPath);
		_startCardsBackButton = GetNodeOrNull<Button>(StartCardsBackButtonPath);
		_startCardsContentLabel = GetNodeOrNull<Label>(StartCardsContentPath);
		_startCharacterRangedButton = GetNodeOrNull<Button>(StartCharacterRangedButtonPath);
		_startCharacterMeleeButton = GetNodeOrNull<Button>(StartCharacterMeleeButtonPath);
		_startCharacterTankButton = GetNodeOrNull<Button>(StartCharacterTankButtonPath);
		_startCharacterFluxValueLabel = GetNodeOrNull<Label>(StartCharacterFluxValuePath);
		_startCharacterBackButton = GetNodeOrNull<Button>(StartCharacterBackButtonPath);
		_startCharacterConfirmButton = GetNodeOrNull<Button>(StartCharacterConfirmButtonPath);
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
		_startSettingsBgmSlider = GetNodeOrNull<HSlider>(StartSettingsBgmSliderPath);
		_startSettingsSfxSlider = GetNodeOrNull<HSlider>(StartSettingsSfxSliderPath);
		_startSettingsWindowSizeOption = GetNodeOrNull<OptionButton>(StartSettingsWindowSizePath);
		_startSettingsWindowModeOption = GetNodeOrNull<OptionButton>(StartSettingsWindowModePath);
		_startSettingsLanguageOption = GetNodeOrNull<OptionButton>(StartSettingsLanguagePath);
		_settingsLanguageOption = GetNodeOrNull<OptionButton>(SettingsLanguagePath);
		_upgradeMenu = GetNodeOrNull<UpgradeMenu>(UpgradeMenuPath);
		_scoreLabel = GetNodeOrNull<Label>(ScoreLabelPath);
		_playerHealthBar = GetNodeOrNull<Control>(PlayerHealthBarPath);
		_experienceBarRoot = GetNodeOrNull<Control>(ExperienceBarRootPath);
		_experienceBar = GetNodeOrNull<ProgressBar>(ExperienceBarPath);
		_experienceLabel = GetNodeOrNull<Label>(ExperienceLabelPath);
		_matchCountdownLabel = GetNodeOrNull<Label>(MatchCountdownLabelPath);
		_finalScoreLabel = GetNodeOrNull<Label>(FinalScoreLabelPath);
		_finalSurvivalLabel = GetNodeOrNull<Label>(FinalSurvivalLabelPath);
		_finalFluxGainLabel = GetNodeOrNull<Label>(FinalFluxGainLabelPath);
		_finalFluxWalletLabel = GetNodeOrNull<Label>(FinalFluxWalletLabelPath);
		_pauseBuildSummaryLabel = GetNodeOrNull<Label>(PauseBuildSummaryLabelPath);
		_finalBuildSummaryLabel = GetNodeOrNull<Label>(FinalBuildSummaryLabelPath);
		_restartTitleLabel = GetNodeOrNull<Label>(RestartTitleLabelPath);
		_restartPerfectBannerLabel = GetNodeOrNull<Label>(RestartPerfectBannerPath);
		_restartHintLabel = GetNodeOrNull<Label>(RestartHintLabelPath);
		_startCharacterDescriptionLabel = GetNodeOrNull<Label>(StartCharacterDescriptionPath);
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
		SetPausePanels(showPausePanel: false, showMain: true, showSettings: false);
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		RefreshPerfectLeaderboardUi();

		_rangedCharacter = LoadCharacterDefinitionOrFallback(
			RangedCharacterResourcePath,
			BuildMageFallbackDefinition());
		_meleeCharacter = LoadCharacterDefinitionOrFallback(
			MeleeCharacterResourcePath,
			BuildBladeFallbackDefinition());
		_tankCharacter = LoadCharacterDefinitionOrFallback(
			TankCharacterResourcePath,
			BuildBulwarkFallbackDefinition());
		_selectedCharacterDefinition = RunContext.Instance?.GetSelectedOrDefault() ?? _rangedCharacter ?? _meleeCharacter ?? _tankCharacter;

		var scoreList = GetTree().GetNodesInGroup("ScoreSystem");
		if (scoreList.Count > 0)
			_scoreSystem = scoreList[0] as ScoreSystem;

		var stabilityList = GetTree().GetNodesInGroup("StabilitySystem");
		if (stabilityList.Count > 0)
			_stabilitySystem = stabilityList[0] as StabilitySystem;

		var progressionList = GetTree().GetNodesInGroup("ProgressionSystem");
		if (progressionList.Count > 0)
			_progressionSystem = progressionList[0] as ProgressionSystem;

		var upgradeList = GetTree().GetNodesInGroup("UpgradeSystem");
		if (upgradeList.Count > 0)
			_upgradeSystem = upgradeList[0] as UpgradeSystem;
	}

	private void BindSignals()
	{
		// Connect all one-way UI event flow here.
		GetViewport().SizeChanged += OnViewportSizeChanged;

		if (_startButton != null)
			_startButton.Pressed += OnStartPressed;
		if (_startSettingsButton != null)
			_startSettingsButton.Pressed += OnStartSettingsPressed;
		if (_startCardsButton != null)
			_startCardsButton.Pressed += OnStartCardsPressed;
		if (_startQuitButton != null)
			_startQuitButton.Pressed += OnQuitGamePressed;
		if (_startDeleteSaveButton != null)
			_startDeleteSaveButton.Pressed += OnStartDeleteSavePressed;
		if (_startDeleteSaveDialog != null)
			_startDeleteSaveDialog.Confirmed += OnStartDeleteSaveConfirmed;
		if (_startSettingsBackButton != null)
			_startSettingsBackButton.Pressed += OnStartSettingsBackPressed;
		if (_startCardsBackButton != null)
			_startCardsBackButton.Pressed += OnStartCardsBackPressed;
		if (_startCharacterRangedButton != null)
			_startCharacterRangedButton.Pressed += OnCharacterRangedPressed;
		if (_startCharacterMeleeButton != null)
			_startCharacterMeleeButton.Pressed += OnCharacterMeleePressed;
		if (_startCharacterTankButton != null)
			_startCharacterTankButton.Pressed += OnCharacterTankPressed;
		if (_startCharacterBackButton != null)
			_startCharacterBackButton.Pressed += OnCharacterSelectBackPressed;
		if (_startCharacterConfirmButton != null)
			_startCharacterConfirmButton.Pressed += OnCharacterSelectConfirmPressed;
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
		if (_startSettingsBgmSlider != null)
			_startSettingsBgmSlider.ValueChanged += OnSettingsBgmChanged;
		if (_settingsSfxSlider != null)
			_settingsSfxSlider.ValueChanged += OnSettingsSfxChanged;
		if (_startSettingsSfxSlider != null)
			_startSettingsSfxSlider.ValueChanged += OnSettingsSfxChanged;
		if (_settingsWindowSizeOption != null)
			_settingsWindowSizeOption.ItemSelected += OnSettingsWindowSizeSelected;
		if (_startSettingsWindowSizeOption != null)
			_startSettingsWindowSizeOption.ItemSelected += OnSettingsWindowSizeSelected;
		if (_settingsWindowModeOption != null)
			_settingsWindowModeOption.ItemSelected += OnSettingsWindowModeSelected;
		if (_startSettingsWindowModeOption != null)
			_startSettingsWindowModeOption.ItemSelected += OnSettingsWindowModeSelected;
		if (_settingsLanguageOption != null)
			_settingsLanguageOption.ItemSelected += OnSettingsLanguageSelected;
		if (_startSettingsLanguageOption != null)
			_startSettingsLanguageOption.ItemSelected += OnSettingsLanguageSelected;
		if (_stabilitySystem != null)
		{
			_stabilitySystem.Collapsed += OnUniverseCollapsed;
			_stabilitySystem.MatchDurationReached += OnMatchDurationReached;
		}

		InitializeSettingsUi();
		LoadSettingsFromDisk();
		ApplyLocalizedTexts();
	}
}

