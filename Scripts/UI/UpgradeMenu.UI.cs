using Godot;

public partial class UpgradeMenu
{
	private void BindUi()
	{
		// Resolve mandatory UI nodes once and wire button callbacks.
		_title = GetNodeOrNull<Label>(TitlePath);
		_leftButton = GetNodeOrNull<Button>(LeftButtonPath);
		_middleButton = GetNodeOrNull<Button>(MiddleButtonPath);
		_rightButton = GetNodeOrNull<Button>(RightButtonPath);
		_panel = GetNodeOrNull<Control>(PanelPath);

		if (_title == null || _leftButton == null || _middleButton == null || _rightButton == null)
		{
			DebugSystem.Error("[UpgradeMenu] UI nodes are missing. Check TitlePath/LeftButtonPath/MiddleButtonPath/RightButtonPath.");
			return;
		}

		_leftButton.Pressed += () => ApplyOption(_leftOption);
		_middleButton.Pressed += () => ApplyOption(_middleOption);
		_rightButton.Pressed += () => ApplyOption(_rightOption);
	}

	private void RefreshButtons()
	{
		_leftButton.Text = FormatOptionText(_leftOption);
		_middleButton.Text = FormatOptionText(_middleOption);
		_rightButton.Text = FormatOptionText(_rightOption);
	}

	private static string FormatOptionText(UpgradeSystem.UpgradeOptionData option)
	{
		return $"{option.Title}\n{option.Description}";
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
