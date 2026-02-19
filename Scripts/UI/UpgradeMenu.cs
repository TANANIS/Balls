using Godot;

public partial class UpgradeMenu : Control
{
	[Export] public bool DebugOpenWithKey = true;
	[Export] public Key DebugOpenKey = Key.U;
	private const string TitlePath = "Panel/VBox/Title";
	private const string LeftButtonPath = "Panel/VBox/Options/LeftButton";
	private const string MiddleButtonPath = "Panel/VBox/Options/MiddleButton";
	private const string RightButtonPath = "Panel/VBox/Options/RightButton";
	private const string PanelPath = "Panel";

	private UpgradeSystem _upgradeSystem;
	private readonly RandomNumberGenerator _rng = new();

	private Label _title;
	private Button _leftButton;
	private Button _middleButton;
	private Button _rightButton;
	private Control _panel;

	private bool _isOpen = false;
	private UpgradeSystem.UpgradeOptionData _leftOption;
	private UpgradeSystem.UpgradeOptionData _middleOption;
	private UpgradeSystem.UpgradeOptionData _rightOption;
	public bool IsOpen => _isOpen;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Visible = false;

		var list = GetTree().GetNodesInGroup("UpgradeSystem");
		if (list.Count > 0)
			_upgradeSystem = list[0] as UpgradeSystem;

		if (_upgradeSystem == null)
			DebugSystem.Error("[UpgradeMenu] UpgradeSystem not found.");

		BindUi();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (DebugOpenWithKey && @event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == DebugOpenKey)
		{
			if (_isOpen)
				CloseMenu();
			else
				OpenMenu();
		}

		if (_isOpen && @event.IsActionPressed("ui_cancel"))
		{
			AudioManager.Instance?.PlaySfxUiExit();
			ApplyRandomCurrentOption();
		}
	}

	public void OpenMenu()
	{
		if (_isOpen || _upgradeSystem == null)
			return;

		if (!PickOptions())
			return;

		RefreshButtons();
		_isOpen = true;
		Visible = true;
		GetTree().Paused = true;
		AudioManager.Instance?.PlaySfxUiButton();
		_leftButton?.GrabFocus();
	}

	private void CloseMenu()
	{
		_isOpen = false;
		Visible = false;
		GetTree().Paused = false;
	}
}
