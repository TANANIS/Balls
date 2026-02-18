using Godot;

public partial class GameFlowUI : Control
{
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public NodePath StartPanelPath = "StartPanel";
	[Export] public NodePath StartMainVBoxPath = "StartPanel/Panel/VBox";
	[Export] public NodePath StartSettingsPanelPath = "StartPanel/Panel/SettingsPanel";
	[Export] public NodePath RestartPanelPath = "RestartPanel";
	[Export] public NodePath PausePanelPath = "PausePanel";
	[Export] public NodePath PauseMainVBoxPath = "PausePanel/Panel/VBox";
	[Export] public NodePath PauseSettingsPanelPath = "PausePanel/Panel/SettingsPanel";
	[Export] public NodePath StartButtonPath = "StartPanel/Panel/VBox/StartButton";
	[Export] public NodePath StartSettingsButtonPath = "StartPanel/Panel/VBox/SettingsButton";
	[Export] public NodePath StartQuitButtonPath = "StartPanel/Panel/VBox/QuitButton";
	[Export] public NodePath StartSettingsBackButtonPath = "StartPanel/Panel/SettingsPanel/VBox/BackButton";
	[Export] public NodePath StartSettingsBgmSliderPath = "StartPanel/Panel/SettingsPanel/VBox/BgmSlider";
	[Export] public NodePath StartSettingsSfxSliderPath = "StartPanel/Panel/SettingsPanel/VBox/SfxSlider";
	[Export] public NodePath StartSettingsWindowSizePath = "StartPanel/Panel/SettingsPanel/VBox/WindowSizeOption";
	[Export] public NodePath StartSettingsWindowModePath = "StartPanel/Panel/SettingsPanel/VBox/WindowModeOption";
	[Export] public NodePath RestartButtonPath = "RestartPanel/Panel/VBox/RestartButton";
	[Export] public NodePath PauseResumeButtonPath = "PausePanel/Panel/VBox/ResumeButton";
	[Export] public NodePath PauseSettingsButtonPath = "PausePanel/Panel/VBox/SettingsButton";
	[Export] public NodePath PauseRestartButtonPath = "PausePanel/Panel/VBox/RestartButton";
	[Export] public NodePath PauseToTitleButtonPath = "PausePanel/Panel/VBox/ToTitleButton";
	[Export] public NodePath PauseQuitButtonPath = "PausePanel/Panel/VBox/QuitButton";
	[Export] public NodePath SettingsBackButtonPath = "PausePanel/Panel/SettingsPanel/VBox/BackButton";
	[Export] public NodePath SettingsBgmSliderPath = "PausePanel/Panel/SettingsPanel/VBox/BgmSlider";
	[Export] public NodePath SettingsSfxSliderPath = "PausePanel/Panel/SettingsPanel/VBox/SfxSlider";
	[Export] public NodePath SettingsWindowSizePath = "PausePanel/Panel/SettingsPanel/VBox/WindowSizeOption";
	[Export] public NodePath SettingsWindowModePath = "PausePanel/Panel/SettingsPanel/VBox/WindowModeOption";
	[Export] public NodePath UpgradeMenuPath = "UpgradeMenu";
	[Export] public NodePath LowHealthVignettePath = "../LowHealthVignette";
	[Export] public NodePath ScoreLabelPath = "ScoreLabel";
	[Export] public NodePath EventCountdownLabelPath = "EventCountdownLabel";
	[Export] public NodePath EventNoticeLabelPath = "EventNoticeLabel";
	[Export] public NodePath FinalScoreLabelPath = "RestartPanel/Panel/VBox/Score";
	[Export] public NodePath RestartTitleLabelPath = "RestartPanel/Panel/VBox/Title";
	[Export] public NodePath RestartHintLabelPath = "RestartPanel/Panel/VBox/Hint";
	[Export] public NodePath BackgroundPath = "../../World/Background";
	[Export] public NodePath BackgroundDimmerPath = "../../World/BackgroundDimmer";
	[Export] public NodePath MenuBackgroundPath = "../../World/MenuBackground";
	[Export] public NodePath MenuDimmerPath = "../../World/MenuDimmer";
	[Export] public NodePath EnemiesPath = "../../Enemies";
	[Export] public NodePath ProjectilesPath = "../../Projectiles";
	[Export] public NodePath ObstaclesPath = "../../World/Obstacles";
	[Export] public float LowHealthMaxIntensity = 0.9f;
	[Export] public float LowHealthPower = 1.6f;

	private Player _player;
	private PlayerHealth _playerHealth;
	private Control _startPanel;
	private Control _startMainVBox;
	private Control _startSettingsPanel;
	private Control _restartPanel;
	private Control _pausePanel;
	private Control _pauseMainVBox;
	private Control _pauseSettingsPanel;
	private Button _startButton;
	private Button _startSettingsButton;
	private Button _startQuitButton;
	private Button _startSettingsBackButton;
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
	private UpgradeMenu _upgradeMenu;
	private ColorRect _lowHealthVignette;
	private ShaderMaterial _lowHealthMaterial;
	private Label _scoreLabel;
	private Label _eventCountdownLabel;
	private Label _eventNoticeLabel;
	private Label _finalScoreLabel;
	private Label _restartTitleLabel;
	private Label _restartHintLabel;
	private ScoreSystem _scoreSystem;
	private StabilitySystem _stabilitySystem;
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
	private bool _suppressSettingsSignal;
	private float _eventNoticeTimer;

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
		UpdateLowHealthVignette();
		UpdateUniverseEventUi(delta);
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
		_restartPanel = GetNodeOrNull<Control>(RestartPanelPath);
		_pausePanel = GetNodeOrNull<Control>(PausePanelPath);
		_pauseMainVBox = GetNodeOrNull<Control>(PauseMainVBoxPath);
		_pauseSettingsPanel = GetNodeOrNull<Control>(PauseSettingsPanelPath);
		_startButton = GetNodeOrNull<Button>(StartButtonPath);
		_startSettingsButton = GetNodeOrNull<Button>(StartSettingsButtonPath);
		_startQuitButton = GetNodeOrNull<Button>(StartQuitButtonPath);
		_startSettingsBackButton = GetNodeOrNull<Button>(StartSettingsBackButtonPath);
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
		_upgradeMenu = GetNodeOrNull<UpgradeMenu>(UpgradeMenuPath);
		_lowHealthVignette = GetNodeOrNull<ColorRect>(LowHealthVignettePath);
		_lowHealthMaterial = _lowHealthVignette?.Material as ShaderMaterial;
		_scoreLabel = GetNodeOrNull<Label>(ScoreLabelPath);
		_eventCountdownLabel = GetNodeOrNull<Label>(EventCountdownLabelPath);
		_eventNoticeLabel = GetNodeOrNull<Label>(EventNoticeLabelPath);
		_finalScoreLabel = GetNodeOrNull<Label>(FinalScoreLabelPath);
		_restartTitleLabel = GetNodeOrNull<Label>(RestartTitleLabelPath);
		_restartHintLabel = GetNodeOrNull<Label>(RestartHintLabelPath);
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

		var scoreList = GetTree().GetNodesInGroup("ScoreSystem");
		if (scoreList.Count > 0)
			_scoreSystem = scoreList[0] as ScoreSystem;

		var stabilityList = GetTree().GetNodesInGroup("StabilitySystem");
		if (stabilityList.Count > 0)
			_stabilitySystem = stabilityList[0] as StabilitySystem;
	}

	private void BindSignals()
	{
		// Connect all one-way UI event flow here.
		GetViewport().SizeChanged += OnViewportSizeChanged;

		if (_startButton != null)
			_startButton.Pressed += OnStartPressed;
		if (_startSettingsButton != null)
			_startSettingsButton.Pressed += OnStartSettingsPressed;
		if (_startQuitButton != null)
			_startQuitButton.Pressed += OnQuitGamePressed;
		if (_startSettingsBackButton != null)
			_startSettingsBackButton.Pressed += OnStartSettingsBackPressed;
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
		if (_stabilitySystem != null)
		{
			_stabilitySystem.Collapsed += OnUniverseCollapsed;
			_stabilitySystem.MatchDurationReached += OnMatchDurationReached;
			_stabilitySystem.EventIncoming += OnUniverseEventIncoming;
			_stabilitySystem.EventStarted += OnUniverseEventStarted;
			_stabilitySystem.EventEnded += OnUniverseEventEnded;
		}

		InitializeSettingsUi();
		LoadSettingsFromDisk();
	}
}
