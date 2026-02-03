using Godot;
using System;

public partial class DebugSystem : Node
{
	[Export] public bool Enabled = true;
	[Export] public bool EnableWarnings = true;
	[Export] public bool EnableErrors = true;

	public static DebugSystem Instance { get; private set; }

	private static bool _enabled = true;
	private static bool _warnEnabled = true;
	private static bool _errorEnabled = true;

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
	}

	public static void Warn(string message)
	{
		if (!_enabled || !_warnEnabled) return;
		GD.PushWarning(message);
	}

	public static void Error(string message)
	{
		if (!_errorEnabled) return;
		GD.PushError(message);
	}
}
