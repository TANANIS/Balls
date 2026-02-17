using Godot;
using System.Collections.Generic;

public partial class DebugSystem : Node
{
	[Export] public bool Enabled = true;
	[Export] public bool EnableWarnings = true;
	[Export] public bool EnableErrors = true;
	[Export] public bool ShowOverlayInGame = true;
	[Export] public bool OverlayVisibleByDefault = true;
	[Export] public bool EnableF3DebugToggle = false;
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
		// Publish singleton and apply inspector switches early.
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
		_overlayVisible = EnableF3DebugToggle ? false : OverlayVisibleByDefault;
		UpdateOverlayVisibility();

		// Flush logs generated before DebugSystem finished _Ready.
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
		if (!EnableF3DebugToggle)
		{
			_lastTogglePressed = false;
			return;
		}

		bool togglePressed = Input.IsPhysicalKeyPressed(Key.F3);
		if (!string.IsNullOrWhiteSpace(ToggleOverlayAction) && InputMap.HasAction(ToggleOverlayAction))
			togglePressed = togglePressed || Input.IsActionPressed(ToggleOverlayAction);

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
}
