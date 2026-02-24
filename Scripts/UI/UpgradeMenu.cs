using Godot;

public partial class UpgradeMenu : Control
{
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

		TryResolveUpgradeSystem();

		BindUi();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_isOpen && @event.IsActionPressed("ui_cancel"))
		{
			AudioManager.Instance?.PlaySfxUiExit();
			ApplyRandomCurrentOption();
		}
	}

	public void OpenMenu()
	{
		TryResolveUpgradeSystem();
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

	public void ForceCloseForRunReset()
	{
		if (!_isOpen && !Visible)
			return;
		_isOpen = false;
		Visible = false;
		if (GetTree() != null)
			GetTree().Paused = false;
	}

	private void TryResolveUpgradeSystem()
	{
		if (_upgradeSystem != null)
			return;

		var list = GetTree().GetNodesInGroup("UpgradeSystem");
		if (list.Count > 0)
			_upgradeSystem = list[0] as UpgradeSystem;
	}
}
