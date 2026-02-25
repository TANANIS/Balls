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
			return;
		}

		_leftButton.Pressed += () => ApplyOptionByIndex(0);
		_middleButton.Pressed += () => ApplyOptionByIndex(1);
		_rightButton.Pressed += () => ApplyOptionByIndex(2);
	}

	private void RefreshButtons()
	{
		if (_title != null)
			_title.Text = Tr("UI.UPGRADE.TITLE");
		RefreshOptionButton(_leftButton, _leftOption, _availableOptionCount >= 1);
		RefreshOptionButton(_middleButton, _middleOption, _availableOptionCount >= 2);
		RefreshOptionButton(_rightButton, _rightOption, _availableOptionCount >= 3);
	}

	private static string FormatOptionText(UpgradeSystem.UpgradeOptionData option)
	{
		string rarity = option.Rarity switch
		{
			UpgradeRarity.Epic => "EPIC",
			UpgradeRarity.Rare => "RARE",
			_ => "COMMON"
		};

		string stack = option.MaxStack > 1
			? $" ({option.CurrentStack + 1}/{option.MaxStack})"
			: string.Empty;

		return $"[{rarity}] {option.Title}{stack}\n{option.Description}";
	}

	private static void RefreshOptionButton(Button button, UpgradeSystem.UpgradeOptionData option, bool available)
	{
		if (button == null)
			return;

		button.Visible = available;
		button.Disabled = !available;
		button.FocusMode = available ? FocusModeEnum.All : FocusModeEnum.None;
		button.Icon = available ? option.Icon : null;
		button.Text = available ? FormatOptionText(option) : "";
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
