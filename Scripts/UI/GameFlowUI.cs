using Godot;

public partial class GameFlowUI : Control
{
	[Export] public NodePath PlayerPath = "../../Player";
	[Export] public NodePath StartPanelPath = "StartPanel";
	[Export] public NodePath RestartPanelPath = "RestartPanel";
	[Export] public NodePath StartButtonPath = "StartPanel/Panel/VBox/StartButton";
	[Export] public NodePath RestartButtonPath = "RestartPanel/Panel/VBox/RestartButton";

	private Player _player;
	private PlayerHealth _playerHealth;
	private Control _startPanel;
	private Control _restartPanel;
	private Button _startButton;
	private Button _restartButton;
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

		if (_startButton != null)
			_startButton.Pressed += OnStartPressed;
		if (_restartButton != null)
			_restartButton.Pressed += OnRestartPressed;
		if (_playerHealth != null)
			_playerHealth.Died += OnPlayerDied;

		ShowStartPanel();
	}

	private void ShowStartPanel()
	{
		_started = false;
		if (_startPanel != null) _startPanel.Visible = true;
		if (_restartPanel != null) _restartPanel.Visible = false;
		GetTree().Paused = true;
		_startButton?.GrabFocus();
	}

	private void OnStartPressed()
	{
		_started = true;
		if (_startPanel != null) _startPanel.Visible = false;
		if (_restartPanel != null) _restartPanel.Visible = false;
		GetTree().Paused = false;
		RespawnPlayerAtViewportCenter();
	}

	private void OnPlayerDied()
	{
		if (!_started)
			return;

		if (_restartPanel != null)
			_restartPanel.Visible = true;

		GetTree().Paused = true;
		_restartButton?.GrabFocus();
	}

	private void OnRestartPressed()
	{
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
}
