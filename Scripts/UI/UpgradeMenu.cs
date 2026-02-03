using Godot;
using System;

public partial class UpgradeMenu : Control
{
	[Export] public bool DebugOpenWithKey = true;
	[Export] public Key DebugOpenKey = Key.U;
	[Export] public NodePath TitlePath = "Panel/VBox/Title";
	[Export] public NodePath LeftButtonPath = "Panel/VBox/Options/LeftButton";
	[Export] public NodePath RightButtonPath = "Panel/VBox/Options/RightButton";

	private UpgradeSystem _upgradeSystem;
	private readonly RandomNumberGenerator _rng = new();

	private Label _title;
	private Button _leftButton;
	private Button _rightButton;

	private bool _isOpen = false;
	private UpgradeSystem.UpgradeOptionData _leftOption;
	private UpgradeSystem.UpgradeOptionData _rightOption;

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
		_upgradeSystem?.ApplyUpgrade(option.Id);
		CloseMenu();
	}

	private void ApplyRandomCurrentOption()
	{
		if (_rng.RandiRange(0, 1) == 0)
			ApplyOption(_leftOption);
		else
			ApplyOption(_rightOption);
	}

}
