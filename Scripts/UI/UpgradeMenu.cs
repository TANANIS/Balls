using Godot;
using System;

public partial class UpgradeMenu : Control
{
	[Export] public bool DebugOpenWithKey = true;
	[Export] public Key DebugOpenKey = Key.U;
	[Export] public NodePath TitlePath = "Panel/VBox/Title";
	[Export] public NodePath LeftButtonPath = "Panel/VBox/Options/LeftButton";
	[Export] public NodePath RightButtonPath = "Panel/VBox/Options/RightButton";
	[Export] public NodePath PanelPath = "Panel";

	private UpgradeSystem _upgradeSystem;
	private readonly RandomNumberGenerator _rng = new();

	private Label _title;
	private Button _leftButton;
	private Button _rightButton;
	private Control _panel;

	private bool _isOpen = false;
	private UpgradeSystem.UpgradeOptionData _leftOption;
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
		if (!DebugOpenWithKey)
			return;

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == DebugOpenKey)
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
		CallDeferred(nameof(CenterPanel));
		AudioManager.Instance?.PlaySfxUiButton();
		_leftButton.GrabFocus();
	}

	private void CloseMenu()
	{
		_isOpen = false;
		Visible = false;
		GetTree().Paused = false;
	}

	private void BindUi()
	{
		_title = GetNodeOrNull<Label>(TitlePath);
		_leftButton = GetNodeOrNull<Button>(LeftButtonPath);
		_rightButton = GetNodeOrNull<Button>(RightButtonPath);
		_panel = GetNodeOrNull<Control>(PanelPath);

		if (_title == null || _leftButton == null || _rightButton == null)
		{
			DebugSystem.Error("[UpgradeMenu] UI nodes are missing. Check TitlePath/LeftButtonPath/RightButtonPath.");
			return;
		}

		_leftButton.Pressed += () => ApplyOption(_leftOption);
		_rightButton.Pressed += () => ApplyOption(_rightOption);
	}

	private bool PickOptions()
	{
		if (_upgradeSystem == null)
			return false;
		if (!_upgradeSystem.TryPickTwo(_rng, out _leftOption, out _rightOption))
		{
			DebugSystem.Error("[UpgradeMenu] Could not pick upgrade options.");
			return false;
		}
		return true;
	}

	private void RefreshButtons()
	{
		_leftButton.Text = _leftOption.Title + "\n" + _leftOption.Description;
		_rightButton.Text = _rightOption.Title + "\n" + _rightOption.Description;
	}

	private void ApplyOption(UpgradeSystem.UpgradeOptionData option)
	{
		AudioManager.Instance?.PlaySfxUiUpgradeSelect();
		_upgradeSystem?.ApplyUpgrade(option.Id);
		AudioManager.Instance?.PlaySfxPlayerUpgrade();
		CloseMenu();
	}

	private void ApplyRandomCurrentOption()
	{
		if (_rng.RandiRange(0, 1) == 0)
			ApplyOption(_leftOption);
		else
			ApplyOption(_rightOption);
	}

	private void CenterPanel()
	{
		if (_panel == null)
			return;

		Vector2 size = _panel.GetCombinedMinimumSize();
		if (size == Vector2.Zero)
			size = _panel.Size;

		_panel.AnchorLeft = 0.5f;
		_panel.AnchorTop = 0.5f;
		_panel.AnchorRight = 0.5f;
		_panel.AnchorBottom = 0.5f;

		_panel.OffsetLeft = -size.X * 0.5f;
		_panel.OffsetTop = -size.Y * 0.5f;
		_panel.OffsetRight = size.X * 0.5f;
		_panel.OffsetBottom = size.Y * 0.5f;
	}
}
