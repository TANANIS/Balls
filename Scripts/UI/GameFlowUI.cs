using Godot;
using System.Text;

public partial class GameFlowUI : Control
{
	private const string PlayerPath = "../../Player";
	private const string StartPanelPath = "Panels/StartPanel";
	private const string StartMainVBoxPath = "Panels/StartPanel/Panel/MainScroll/VBox";
	private const string StartSettingsPanelPath = "Panels/StartPanel/Panel/SettingsPanel";
	private const string StartCardsPanelPath = "Panels/StartPanel/Panel/CardsPanel";
	private const string StartCharacterSelectPanelPath = "Panels/StartPanel/Panel/CharacterSelectPanel";
	private const string StartCharacterRangedButtonPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/CharacterButtons/RangedButton";
	private const string StartCharacterMeleeButtonPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/CharacterButtons/MeleeButton";
	private const string StartCharacterTankButtonPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/CharacterButtons/TankButton";
	private const string StartCharacterDescriptionPath = "Panels/StartPanel/Panel/CharacterSelectPanel/VBox/SelectedCharacterDesc";
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
	private const string StartClearLeaderboardButtonPath = "Panels/StartPanel/Panel/MainScroll/VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/ClearLeaderboardButton";
	private const string StartClearLeaderboardDialogPath = "Panels/StartPanel/ClearLeaderboardConfirmDialog";
	private const string StartPerfectLeaderboardPath = "Panels/StartPanel/Panel/MainScroll/VBox/MainBody/LeftColumn/PerfectLeaderboard";
	private const string StartSettingsBackButtonPath = "Panels/StartPanel/Panel/SettingsPanel/VBox/BackButton";
	private const string StartCardsBackButtonPath = "Panels/StartPanel/Panel/CardsPanel/VBox/BackButton";
	private const string StartCardsContentPath = "Panels/StartPanel/Panel/CardsPanel/VBox/CardsScroll/CardsContent";
	private const string StartSettingsBgmSliderPath = "Panels/StartPanel/Panel/SettingsPanel/VBox/BgmSlider";
	private const string StartSettingsSfxSliderPath = "Panels/StartPanel/Panel/SettingsPanel/VBox/SfxSlider";
	private const string StartSettingsWindowSizePath = "Panels/StartPanel/Panel/SettingsPanel/VBox/WindowSizeOption";
	private const string StartSettingsWindowModePath = "Panels/StartPanel/Panel/SettingsPanel/VBox/WindowModeOption";
	private const string StartSettingsLanguagePath = "Panels/StartPanel/Panel/SettingsPanel/VBox/LanguageOption";
	private const string RestartButtonPath = "Panels/RestartPanel/Panel/VBox/RestartButton";
	private const string PauseResumeButtonPath = "Panels/PausePanel/Panel/VBox/ResumeButton";
	private const string PauseSettingsButtonPath = "Panels/PausePanel/Panel/VBox/SettingsButton";
	private const string PauseRestartButtonPath = "Panels/PausePanel/Panel/VBox/RestartButton";
	private const string PauseToTitleButtonPath = "Panels/PausePanel/Panel/VBox/ToTitleButton";
	private const string PauseQuitButtonPath = "Panels/PausePanel/Panel/VBox/QuitButton";
	private const string SettingsBackButtonPath = "Panels/PausePanel/Panel/SettingsPanel/VBox/BackButton";
	private const string SettingsBgmSliderPath = "Panels/PausePanel/Panel/SettingsPanel/VBox/BgmSlider";
	private const string SettingsSfxSliderPath = "Panels/PausePanel/Panel/SettingsPanel/VBox/SfxSlider";
	private const string SettingsWindowSizePath = "Panels/PausePanel/Panel/SettingsPanel/VBox/WindowSizeOption";
	private const string SettingsWindowModePath = "Panels/PausePanel/Panel/SettingsPanel/VBox/WindowModeOption";
	private const string SettingsLanguagePath = "Panels/PausePanel/Panel/SettingsPanel/VBox/LanguageOption";
	private const string UpgradeMenuPath = "UpgradeLayer/UpgradeMenu";
	private const string ScoreLabelPath = "Overlay/HudOverlay/ScoreLabel";
	private const string PlayerHealthBarPath = "Overlay/HudOverlay/PlayerHealthBarDemo";
	private const string ExperienceBarRootPath = "Overlay/HudOverlay/ExperienceBarRoot";
	private const string ExperienceBarPath = "Overlay/HudOverlay/ExperienceBarRoot/ExperienceBar";
	private const string ExperienceLabelPath = "Overlay/HudOverlay/ExperienceBarRoot/ExperienceLabel";
	private const string MatchCountdownLabelPath = "Overlay/HudOverlay/MatchCountdownLabel";
	private const string FinalScoreLabelPath = "Panels/RestartPanel/Panel/VBox/Score";
	private const string PauseBuildSummaryLabelPath = "Panels/PausePanel/Panel/VBox/BuildSummary";
	private const string FinalBuildSummaryLabelPath = "Panels/RestartPanel/Panel/VBox/BuildSummary";
	private const string RestartTitleLabelPath = "Panels/RestartPanel/Panel/VBox/Title";
	private const string RestartPerfectBannerPath = "Panels/RestartPanel/Panel/VBox/PerfectBanner";
	private const string RestartHintLabelPath = "Panels/RestartPanel/Panel/VBox/Hint";
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
	private Button _startClearLeaderboardButton;
	private ConfirmationDialog _startClearLeaderboardDialog;
	private Label _startPerfectLeaderboardLabel;
	private Button _startSettingsBackButton;
	private Button _startCardsBackButton;
	private Label _startCardsContentLabel;
	private Button _startCharacterRangedButton;
	private Button _startCharacterMeleeButton;
	private Button _startCharacterTankButton;
	private Button _startCharacterBackButton;
	private Button _startCharacterConfirmButton;
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
	private CharacterDefinition _rangedCharacter;
	private CharacterDefinition _meleeCharacter;
	private CharacterDefinition _tankCharacter;
	private CharacterDefinition _selectedCharacterDefinition;

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
		_startClearLeaderboardButton = GetNodeOrNull<Button>(StartClearLeaderboardButtonPath);
		_startClearLeaderboardDialog = GetNodeOrNull<ConfirmationDialog>(StartClearLeaderboardDialogPath);
		_startPerfectLeaderboardLabel = GetNodeOrNull<Label>(StartPerfectLeaderboardPath);
		_startSettingsBackButton = GetNodeOrNull<Button>(StartSettingsBackButtonPath);
		_startCardsBackButton = GetNodeOrNull<Button>(StartCardsBackButtonPath);
		_startCardsContentLabel = GetNodeOrNull<Label>(StartCardsContentPath);
		_startCharacterRangedButton = GetNodeOrNull<Button>(StartCharacterRangedButtonPath);
		_startCharacterMeleeButton = GetNodeOrNull<Button>(StartCharacterMeleeButtonPath);
		_startCharacterTankButton = GetNodeOrNull<Button>(StartCharacterTankButtonPath);
		_startCharacterBackButton = GetNodeOrNull<Button>(StartCharacterBackButtonPath);
		_startCharacterConfirmButton = GetNodeOrNull<Button>(StartCharacterConfirmButtonPath);
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
		if (_pausePanel != null)
			_pausePanel.Visible = false;
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = true;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = false;
		if (_startMainVBox != null)
			_startMainVBox.Visible = true;
		if (_startSettingsPanel != null)
			_startSettingsPanel.Visible = false;
		if (_startCardsPanel != null)
			_startCardsPanel.Visible = false;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = false;
		RefreshPerfectLeaderboardUi();

		_rangedCharacter = GD.Load<CharacterDefinition>(RangedCharacterResourcePath);
		_meleeCharacter = GD.Load<CharacterDefinition>(MeleeCharacterResourcePath);
		_tankCharacter = GD.Load<CharacterDefinition>(TankCharacterResourcePath);
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
		if (_startClearLeaderboardButton != null)
			_startClearLeaderboardButton.Pressed += OnStartClearLeaderboardPressed;
		if (_startClearLeaderboardDialog != null)
			_startClearLeaderboardDialog.Confirmed += OnStartClearLeaderboardConfirmed;
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
}
