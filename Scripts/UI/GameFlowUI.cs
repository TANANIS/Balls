using Godot;

public partial class GameFlowUI : Control
{
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public NodePath StartPanelPath = "StartPanel";
	[Export] public NodePath RestartPanelPath = "RestartPanel";
	[Export] public NodePath StartButtonPath = "StartPanel/Panel/VBox/StartButton";
	[Export] public NodePath RestartButtonPath = "RestartPanel/Panel/VBox/RestartButton";
	[Export] public NodePath LowHealthVignettePath = "../LowHealthVignette";
	[Export] public NodePath ScoreLabelPath = "ScoreLabel";
	[Export] public NodePath FinalScoreLabelPath = "RestartPanel/Panel/VBox/Score";
	[Export] public NodePath BackgroundPath = "../../World/Background";
	[Export] public NodePath BackgroundDimmerPath = "../../World/BackgroundDimmer";
	[Export] public NodePath MenuBackgroundPath = "../../World/MenuBackground";
	[Export] public NodePath MenuDimmerPath = "../../World/MenuDimmer";
	[Export] public float LowHealthMaxIntensity = 0.9f;
	[Export] public float LowHealthPower = 1.6f;

	private Player _player;
	private PlayerHealth _playerHealth;
	private Control _startPanel;
	private Control _restartPanel;
	private Button _startButton;
	private Button _restartButton;
	private ColorRect _lowHealthVignette;
	private ShaderMaterial _lowHealthMaterial;
	private Label _scoreLabel;
	private Label _finalScoreLabel;
	private ScoreSystem _scoreSystem;
	private CanvasItem _background;
	private ColorRect _backgroundDimmer;
	private Sprite2D _menuBackground;
	private ColorRect _menuDimmer;
	private bool _started;

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
	}

	private void ResolveNodeReferences()
	{
		// Resolve scene dependencies once to keep runtime logic clean.
		_player = GetNodeOrNull<Player>(PlayerPath);
		if (_player != null)
			_playerHealth = _player.GetNodeOrNull<PlayerHealth>("Health");

		_startPanel = GetNodeOrNull<Control>(StartPanelPath);
		_restartPanel = GetNodeOrNull<Control>(RestartPanelPath);
		_startButton = GetNodeOrNull<Button>(StartButtonPath);
		_restartButton = GetNodeOrNull<Button>(RestartButtonPath);
		_lowHealthVignette = GetNodeOrNull<ColorRect>(LowHealthVignettePath);
		_lowHealthMaterial = _lowHealthVignette?.Material as ShaderMaterial;
		_scoreLabel = GetNodeOrNull<Label>(ScoreLabelPath);
		_finalScoreLabel = GetNodeOrNull<Label>(FinalScoreLabelPath);
		_background = GetNodeOrNull<CanvasItem>(BackgroundPath);
		_backgroundDimmer = GetNodeOrNull<ColorRect>(BackgroundDimmerPath);
		_menuBackground = GetNodeOrNull<Sprite2D>(MenuBackgroundPath);
		_menuDimmer = GetNodeOrNull<ColorRect>(MenuDimmerPath);

		if (_menuBackground != null)
			FitMenuBackground();

		var scoreList = GetTree().GetNodesInGroup("ScoreSystem");
		if (scoreList.Count > 0)
			_scoreSystem = scoreList[0] as ScoreSystem;
	}

	private void BindSignals()
	{
		// Connect all one-way UI event flow here.
		GetViewport().SizeChanged += OnViewportSizeChanged;

		if (_startButton != null)
			_startButton.Pressed += OnStartPressed;
		if (_restartButton != null)
			_restartButton.Pressed += OnRestartPressed;
		if (_playerHealth != null)
			_playerHealth.Died += OnPlayerDied;
		if (_scoreSystem != null)
			_scoreSystem.ScoreChanged += OnScoreChanged;
	}
}
