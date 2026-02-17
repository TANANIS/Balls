using Godot;

public partial class UpgradeMenu
{
	private void BindUi()
	{
		// Resolve mandatory UI nodes once and wire button callbacks.
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

	private void RefreshButtons()
	{
		_leftButton.Text = _leftOption.Title + "\n" + _leftOption.Description;
		_rightButton.Text = _rightOption.Title + "\n" + _rightOption.Description;
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
