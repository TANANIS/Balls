using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class DebugSystem : Node
{
	[Export] public bool Enabled = true;
	[Export] public bool EnableWarnings = true;
	[Export] public bool EnableErrors = true;
	[Export] public bool ShowOverlayInGame = true;
	[Export] public bool OverlayVisibleByDefault = true;
	[Export] public int OverlayMaxLines = 18;
	[Export] public string ToggleOverlayAction = "debug_toggle_overlay";
	[Export] public int OverlayWidth = 760;
	[Export] public int OverlayHeight = 280;
	[Export] public Color OverlayBackgroundColor = new Color(0f, 0f, 0f, 0.62f);

	public static DebugSystem Instance { get; private set; }

	private static bool _enabled = true;
	private static bool _warnEnabled = true;
	private static bool _errorEnabled = true;
	private static readonly List<string> _pendingOverlayLines = new();

	private readonly Queue<string> _overlayLines = new();
	private CanvasLayer _overlayLayer;
	private Panel _overlayPanel;
	private RichTextLabel _overlayText;
	private bool _overlayVisible;
	private bool _lastTogglePressed;

	public override void _EnterTree()
	{
		Instance = this;
		AddToGroup("DebugSystem");
		ApplySettings();
	}

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;
	}

	public override void _Ready()
	{
		if (!ShowOverlayInGame)
			return;

		CreateOverlayUi();
		_overlayVisible = OverlayVisibleByDefault;
		UpdateOverlayVisibility();

		if (_pendingOverlayLines.Count > 0)
		{
			foreach (string line in _pendingOverlayLines)
				AppendOverlayLine(line);
			_pendingOverlayLines.Clear();
		}
	}

	public override void _Process(double delta)
	{
		if (_overlayLayer == null)
			return;

		bool togglePressed = false;
		if (!string.IsNullOrWhiteSpace(ToggleOverlayAction) && InputMap.HasAction(ToggleOverlayAction))
			togglePressed = Input.IsActionPressed(ToggleOverlayAction);
		else
			togglePressed = Input.IsPhysicalKeyPressed(Key.F3);

		if (togglePressed && !_lastTogglePressed)
		{
			_overlayVisible = !_overlayVisible;
			UpdateOverlayVisibility();
		}

		_lastTogglePressed = togglePressed;
	}

	private void ApplySettings()
	{
		_enabled = Enabled;
		_warnEnabled = EnableWarnings;
		_errorEnabled = EnableErrors;
	}

	public static void Log(string message)
	{
		if (!_enabled) return;
		GD.Print(message);
		WriteOverlay("[LOG] " + message);
	}

	public static void Warn(string message)
	{
		if (!_enabled || !_warnEnabled) return;
		GD.PushWarning(message);
		WriteOverlay("[WARN] " + message);
	}

	public static void Error(string message)
	{
		if (!_errorEnabled) return;
		GD.PushError(message);
		WriteOverlay("[ERROR] " + message);
	}

	private static void WriteOverlay(string message)
	{
		if (Instance != null && IsInstanceValid(Instance))
		{
			Instance.AppendOverlayLine(message);
			return;
		}

		_pendingOverlayLines.Add(message);
	}

	private void CreateOverlayUi()
	{
		_overlayLayer = new CanvasLayer();
		_overlayLayer.Name = "DebugOverlay";
		_overlayLayer.Layer = 100;
		AddChild(_overlayLayer);

		_overlayPanel = new Panel();
		_overlayPanel.Name = "Background";
		_overlayPanel.Position = new Vector2(8, 8);
		_overlayPanel.Size = new Vector2(Mathf.Max(280, OverlayWidth), Mathf.Max(140, OverlayHeight));
		_overlayPanel.Modulate = OverlayBackgroundColor;
		_overlayLayer.AddChild(_overlayPanel);

		_overlayText = new RichTextLabel();
		_overlayText.Name = "Text";
		_overlayText.BbcodeEnabled = false;
		_overlayText.FitContent = false;
		_overlayText.ScrollActive = true;
		_overlayText.ScrollFollowing = true;
		_overlayText.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		_overlayText.Position = new Vector2(8, 8);
		_overlayText.Size = _overlayPanel.Size - new Vector2(16, 16);
		_overlayPanel.AddChild(_overlayText);
	}

	private void AppendOverlayLine(string line)
	{
		_overlayLines.Enqueue(line);
		while (_overlayLines.Count > Mathf.Max(1, OverlayMaxLines))
			_overlayLines.Dequeue();

		if (_overlayText == null)
			return;

		var sb = new StringBuilder();
		foreach (string item in _overlayLines)
		{
			sb.Append(item);
			sb.Append('\n');
		}

		_overlayText.Text = sb.ToString();
	}

	private void UpdateOverlayVisibility()
	{
		if (_overlayLayer != null)
			_overlayLayer.Visible = _overlayVisible;
	}
}
