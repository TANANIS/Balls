using Godot;
using System;

public partial class StartMainPageController : ScrollContainer
{
	[ExportGroup("Node Paths")]
	[Export] private NodePath StartButtonPath = "VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/StartButton";
	[Export] private NodePath CardsButtonPath = "VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/CardsButton";
	[Export] private NodePath SettingsButtonPath = "VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/SettingsButton";
	[Export] private NodePath QuitButtonPath = "VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/QuitButton";
	[Export] private NodePath PerfectLeaderboardPath = "VBox/MainBody/LeftColumn/PerfectLeaderboard";

	public event Action StartPressed;
	public event Action CardsPressed;
	public event Action SettingsPressed;
	public event Action QuitPressed;

	public Button StartButton => _startButton;
	public Button CardsButton => _cardsButton;
	public Button SettingsButton => _settingsButton;
	public Button QuitButton => _quitButton;
	public Label PerfectLeaderboardLabel => _perfectLeaderboardLabel;

	private Button _startButton;
	private Button _cardsButton;
	private Button _settingsButton;
	private Button _quitButton;
	private Label _perfectLeaderboardLabel;

	public override void _Ready()
	{
		ResolveNodeReferences();
		BindSignals();
	}

	public void FocusDefault()
	{
		_startButton?.GrabFocus();
	}

	public void SetPerfectLeaderboardText(string text)
	{
		if (_perfectLeaderboardLabel != null)
			_perfectLeaderboardLabel.Text = text ?? string.Empty;
	}

	public bool HasPerfectLeaderboard()
	{
		return _perfectLeaderboardLabel != null;
	}

	public void ApplyLocalizedTexts()
	{
		if (_startButton != null)
			_startButton.Text = Tr("UI.START.BUTTON_START");
		if (_settingsButton != null)
			_settingsButton.Text = Tr("UI.COMMON.SETTINGS");
		if (_cardsButton != null)
			_cardsButton.Text = Tr("UI.START.BUTTON_CARDS");
		if (_quitButton != null)
			_quitButton.Text = Tr("UI.COMMON.QUIT");
	}

	private void ResolveNodeReferences()
	{
		_startButton = GetNodeOrNull<Button>(StartButtonPath);
		_cardsButton = GetNodeOrNull<Button>(CardsButtonPath);
		_settingsButton = GetNodeOrNull<Button>(SettingsButtonPath);
		_quitButton = GetNodeOrNull<Button>(QuitButtonPath);
		_perfectLeaderboardLabel = GetNodeOrNull<Label>(PerfectLeaderboardPath);
	}

	private void BindSignals()
	{
		if (_startButton != null)
			_startButton.Pressed += () => StartPressed?.Invoke();
		if (_cardsButton != null)
			_cardsButton.Pressed += () => CardsPressed?.Invoke();
		if (_settingsButton != null)
			_settingsButton.Pressed += () => SettingsPressed?.Invoke();
		if (_quitButton != null)
			_quitButton.Pressed += () => QuitPressed?.Invoke();
	}
}
