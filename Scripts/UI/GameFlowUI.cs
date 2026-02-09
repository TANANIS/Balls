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
	private bool _started;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

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

		var scoreList = GetTree().GetNodesInGroup("ScoreSystem");
		if (scoreList.Count > 0)
			_scoreSystem = scoreList[0] as ScoreSystem;

		if (_startButton != null)
			_startButton.Pressed += OnStartPressed;
		if (_restartButton != null)
			_restartButton.Pressed += OnRestartPressed;
		if (_playerHealth != null)
			_playerHealth.Died += OnPlayerDied;
		if (_scoreSystem != null)
			_scoreSystem.ScoreChanged += OnScoreChanged;

		ShowStartPanel();
		AudioManager.Instance?.PlayBgmMenu();
	}

	public override void _Process(double delta)
	{
		UpdateLowHealthVignette();
	}

	private void UpdateLowHealthVignette()
	{
		if (_lowHealthMaterial == null || _playerHealth == null || _playerHealth.MaxHp <= 0)
			return;

		float hpRatio = Mathf.Clamp((float)_playerHealth.Hp / _playerHealth.MaxHp, 0f, 1f);
		float raw = 1f - hpRatio;
		float intensity = Mathf.Clamp(Mathf.Pow(raw, LowHealthPower) * LowHealthMaxIntensity, 0f, 1f);
		_lowHealthMaterial.SetShaderParameter("intensity", intensity);
	}

	private void ShowStartPanel()
	{
		_started = false;
		if (_startPanel != null) _startPanel.Visible = true;
		if (_restartPanel != null) _restartPanel.Visible = false;
		if (_scoreLabel != null) _scoreLabel.Visible = false;
		GetTree().Paused = true;
		_startButton?.GrabFocus();
	}

	private void OnStartPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		AudioManager.Instance?.PlayBgmGameplay();

		_started = true;
		if (_startPanel != null) _startPanel.Visible = false;
		if (_restartPanel != null) _restartPanel.Visible = false;
		if (_scoreLabel != null) _scoreLabel.Visible = false;
		GetTree().Paused = false;
		RespawnPlayerAtViewportCenter();

		_scoreSystem?.ResetScore();
		OnScoreChanged(_scoreSystem != null ? _scoreSystem.Score : 0);
	}

	private void OnPlayerDied()
	{
		if (!_started)
			return;

		if (_restartPanel != null)
			_restartPanel.Visible = true;

		if (_finalScoreLabel != null)
			_finalScoreLabel.Text = $"Score: {(_scoreSystem != null ? _scoreSystem.Score : 0)}";

		if (_lowHealthMaterial != null)
			_lowHealthMaterial.SetShaderParameter("intensity", 0f);

		GetTree().Paused = true;
		_restartButton?.GrabFocus();
	}

	private void OnRestartPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		AudioManager.Instance?.PlayBgmGameplay();

		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void RespawnPlayerAtViewportCenter()
	{
		if (_player == null)
			return;

		Rect2 rect = GetViewport().GetVisibleRect();
		Vector2 center = rect.Position + (rect.Size * 0.5f);
		_player.RespawnAt(center);
	}

	private void OnScoreChanged(int score)
	{
		if (_scoreLabel != null)
			_scoreLabel.Text = $"Score: {score}";
	}
}
