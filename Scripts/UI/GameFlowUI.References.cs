using Godot;

public partial class GameFlowUI
{
	private const string PlayerPath = "../../Player";
	private const string StartPanelPath = "Panels/StartPanel";
	private const string StartMainVBoxPath = "Panels/StartPanel/Panel/MainScroll/VBox";
	private const string StartSettingsPanelPath = "Panels/StartPanel/Panel/SettingsPanel";
	private const string StartCardsPanelPath = "Panels/StartPanel/Panel/CardsPanel";
	private const string StartCharacterSelectPanelPath = "Panels/StartPanel/Panel/CharacterSelectPanel";
	private const string StartCharacterRangedButtonPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ContentRow/LeftColumn/CharacterButtons/RangedButton";
	private const string StartCharacterMeleeButtonPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ContentRow/LeftColumn/CharacterButtons/MeleeButton";
	private const string StartCharacterTankButtonPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ContentRow/LeftColumn/CharacterButtons/TankButton";
	private const string StartCharacterDescriptionPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/BottomRow/DetailPanel/Margin/DescScroll/SelectedCharacterDesc";
	private const string StartCharacterFluxValuePath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/HeaderRow/FluxHeader/FluxValue";
	private const string StartCharacterBackButtonPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ActionButtons/BackButton";
	private const string StartCharacterConfirmButtonPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/ActionButtons/ConfirmButton";
	private const string RestartPanelPath = "Panels/RestartPanel";
	private const string PausePanelPath = "Panels/PausePanel";
	private const string PauseMainVBoxPath = "Panels/PausePanel/Panel/VBox";
	private const string PauseSettingsPanelPath = "Panels/PausePanel/Panel/SettingsPanel";
	private const string StartButtonPath = "Panels/StartPanel/Panel/MainScroll/VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/StartButton";
	private const string StartSettingsButtonPath = "Panels/StartPanel/Panel/MainScroll/VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/SettingsButton";
	private const string StartCardsButtonPath = "Panels/StartPanel/Panel/MainScroll/VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/CardsButton";
	private const string StartQuitButtonPath = "Panels/StartPanel/Panel/MainScroll/VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/QuitButton";
	private const string StartDeleteSaveButtonPath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/DeleteSaveButton";
	private const string StartDeleteSaveDialogPath = "Panels/StartPanel/DeleteSaveConfirmDialog";
	private const string StartPerfectLeaderboardPath = "Panels/StartPanel/Panel/MainScroll/VBox/MainBody/LeftColumn/PerfectLeaderboard";
	private const string StartSettingsBackButtonPath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/BackButton";
	private const string StartCardsBackButtonPath = "Panels/StartPanel/Panel/CardsPanel/VBox/BackButton";
	private const string StartCardsContentPath = "Panels/StartPanel/Panel/CardsPanel/VBox/CardsScroll/CardsContent";
	private const string StartSettingsBgmSliderPath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/BgmSlider";
	private const string StartSettingsSfxSliderPath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/SfxSlider";
	private const string StartSettingsWindowSizePath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowSizeOption";
	private const string StartSettingsWindowModePath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowModeOption";
	private const string StartSettingsLanguagePath = "Panels/StartPanel/Panel/SettingsPanel/SettingsScroll/VBox/LanguageOption";
	private const string RestartBackToMetaButtonPath = "Panels/RestartPanel/Panel/Margin/VBox/ActionButtons/BackToMetaButton";
	private const string RestartButtonPath = "Panels/RestartPanel/Panel/Margin/VBox/ActionButtons/RestartButton";
	private const string PauseResumeButtonPath = "Panels/PausePanel/Panel/VBox/ResumeButton";
	private const string PauseSettingsButtonPath = "Panels/PausePanel/Panel/VBox/SettingsButton";
	private const string PauseRestartButtonPath = "Panels/PausePanel/Panel/VBox/RestartButton";
	private const string PauseToTitleButtonPath = "Panels/PausePanel/Panel/VBox/ToTitleButton";
	private const string PauseQuitButtonPath = "Panels/PausePanel/Panel/VBox/QuitButton";
	private const string SettingsBackButtonPath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/BackButton";
	private const string SettingsBgmSliderPath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/BgmSlider";
	private const string SettingsSfxSliderPath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/SfxSlider";
	private const string SettingsWindowSizePath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowSizeOption";
	private const string SettingsWindowModePath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/WindowModeOption";
	private const string SettingsLanguagePath = "Panels/PausePanel/Panel/SettingsPanel/SettingsScroll/VBox/LanguageOption";
	private const string UpgradeMenuPath = "UpgradeLayer/UpgradeMenu";
	private const string ScoreLabelPath = "Overlay/HudOverlay/ScoreLabel";
	private const string PlayerHealthBarPath = "Overlay/HudOverlay/PlayerHealthBarDemo";
	private const string ExperienceBarRootPath = "Overlay/HudOverlay/ExperienceBarRoot";
	private const string ExperienceBarPath = "Overlay/HudOverlay/ExperienceBarRoot/ExperienceBar";
	private const string ExperienceLabelPath = "Overlay/HudOverlay/ExperienceBarRoot/ExperienceLabel";
	private const string MatchCountdownLabelPath = "Overlay/HudOverlay/MatchCountdownLabel";
	private const string FinalScoreLabelPath = "Panels/RestartPanel/Panel/Margin/VBox/StatsPanel/StatsMargin/StatsRow/Score";
	private const string FinalSurvivalLabelPath = "Panels/RestartPanel/Panel/Margin/VBox/StatsPanel/StatsMargin/StatsRow/Survival";
	private const string FinalFluxGainLabelPath = "Panels/RestartPanel/Panel/Margin/VBox/StatsPanel/StatsMargin/StatsRow/FluxGain";
	private const string FinalFluxWalletLabelPath = "Panels/RestartPanel/Panel/Margin/VBox/StatsPanel/StatsMargin/StatsRow/FluxWallet";
	private const string PauseBuildSummaryLabelPath = "Panels/PausePanel/Panel/VBox/BuildSummary";
	private const string FinalBuildSummaryLabelPath = "Panels/RestartPanel/Panel/Margin/VBox/BuildSection/BuildMargin/BuildVBox/BuildScroll/BuildSummary";
	private const string RestartTitleLabelPath = "Panels/RestartPanel/Panel/Margin/VBox/Header/Title";
	private const string RestartPerfectBannerPath = "Panels/RestartPanel/Panel/Margin/VBox/Header/PerfectBanner";
	private const string RestartHintLabelPath = "Panels/RestartPanel/Panel/Margin/VBox/Header/Hint";
	private const string BackgroundPath = "../../World/Background";
	private const string BackgroundDimmerPath = "../../World/BackgroundDimmer";
	private const string MenuBackgroundPath = "../../World/MenuBackground";
	private const string MenuDimmerPath = "../../World/MenuDimmer";
	private const string EnemiesPath = "../../Enemies";
	private const string ProjectilesPath = "../../Projectiles";
	private const string ObstaclesPath = "../../World/Obstacles";
	private const string RangedCharacterResourcePath = "res://Data/Characters/RangedCharacter.tres";
	private const string MeleeCharacterResourcePath = "res://Data/Characters/MeleeCharacter.tres";
	private const string TankCharacterResourcePath = "res://Data/Characters/TankBurstCharacter.tres";

	private Player _player;
	private PlayerHealth _playerHealth;
	private Control _startPanel;
	private Control _startMainVBox;
	private Control _startSettingsPanel;
	private Control _startCardsPanel;
	private Control _startCharacterSelectPanel;
	private Control _restartPanel;
	private Control _pausePanel;
	private Control _pauseMainVBox;
	private Control _pauseSettingsPanel;
	private Button _startButton;
	private Button _startSettingsButton;
	private Button _startCardsButton;
	private Button _startQuitButton;
	private Button _startDeleteSaveButton;
	private ConfirmationDialog _startDeleteSaveDialog;
	private Label _startPerfectLeaderboardLabel;
	private Button _startSettingsBackButton;
	private Button _startCardsBackButton;
	private Label _startCardsContentLabel;
	private Button _startCharacterRangedButton;
	private Button _startCharacterMeleeButton;
	private Button _startCharacterTankButton;
	private Label _startCharacterFluxValueLabel;
	private Button _startCharacterBackButton;
	private Button _startCharacterConfirmButton;
	private Button _restartBackToMetaButton;
	private Button _restartButton;
	private Button _pauseResumeButton;
	private Button _pauseSettingsButton;
	private Button _pauseRestartButton;
	private Button _pauseToTitleButton;
	private Button _pauseQuitButton;
	private Button _settingsBackButton;
	private HSlider _settingsBgmSlider;
	private HSlider _settingsSfxSlider;
	private OptionButton _settingsWindowSizeOption;
	private OptionButton _settingsWindowModeOption;
	private HSlider _startSettingsBgmSlider;
	private HSlider _startSettingsSfxSlider;
	private OptionButton _startSettingsWindowSizeOption;
	private OptionButton _startSettingsWindowModeOption;
	private OptionButton _startSettingsLanguageOption;
	private OptionButton _settingsLanguageOption;
	private UpgradeMenu _upgradeMenu;
	private Label _scoreLabel;
	private Control _playerHealthBar;
	private Control _experienceBarRoot;
	private ProgressBar _experienceBar;
	private Label _experienceLabel;
	private Label _matchCountdownLabel;
	private Label _finalScoreLabel;
	private Label _finalSurvivalLabel;
	private Label _finalFluxGainLabel;
	private Label _finalFluxWalletLabel;
	private Label _pauseBuildSummaryLabel;
	private Label _finalBuildSummaryLabel;
	private Label _restartTitleLabel;
	private Label _restartPerfectBannerLabel;
	private Label _restartHintLabel;
	private Label _startCharacterDescriptionLabel;
	private UpgradeSystem _upgradeSystem;
	private ScoreSystem _scoreSystem;
	private StabilitySystem _stabilitySystem;
	private ProgressionSystem _progressionSystem;
	private CanvasItem _background;
	private ColorRect _backgroundDimmer;
	private Sprite2D _menuBackground;
	private ColorRect _menuDimmer;
	private CanvasItem _enemiesRoot;
	private CanvasItem _projectilesRoot;
	private CanvasItem _obstaclesRoot;
	private bool _started;
	private bool _ending;
	private bool _pauseMenuOpen;
	private bool _settingsOpen;
	private bool _startSettingsOpen;
	private bool _startCardsOpen;
	private bool _startCharacterSelectOpen;
	private bool _pendingFinalBossKillClear;
	private bool _suppressSettingsSignal;
	private string _currentRunId = string.Empty;
	private CharacterDefinition _rangedCharacter;
	private CharacterDefinition _meleeCharacter;
	private CharacterDefinition _tankCharacter;
	private CharacterDefinition _selectedCharacterDefinition;

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
