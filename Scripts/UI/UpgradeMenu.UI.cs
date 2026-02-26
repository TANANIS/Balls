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
		ApplyRarityTextColors(button, available ? option.Rarity : UpgradeRarity.Common, available);
	}

	private static void ApplyRarityTextColors(Button button, UpgradeRarity rarity, bool available)
	{
		if (!available)
		{
			button.AddThemeColorOverride("font_color", new Color(0.70f, 0.62f, 0.52f, 0.75f));
			button.AddThemeColorOverride("font_hover_color", new Color(0.70f, 0.62f, 0.52f, 0.75f));
			button.AddThemeColorOverride("font_pressed_color", new Color(0.70f, 0.62f, 0.52f, 0.75f));
			button.AddThemeColorOverride("font_disabled_color", new Color(0.70f, 0.62f, 0.52f, 0.75f));
			return;
		}

		Color baseColor;
		Color hoverColor;
		Color pressedColor;
		switch (rarity)
		{
			case UpgradeRarity.Epic:
				baseColor = new Color(0.96f, 0.78f, 0.60f, 1f);      // warm copper-gold
				hoverColor = new Color(0.99f, 0.86f, 0.70f, 1f);
				pressedColor = new Color(0.90f, 0.70f, 0.54f, 1f);
				break;
			case UpgradeRarity.Rare:
				baseColor = new Color(0.97f, 0.86f, 0.67f, 1f);      // amber parchment
				hoverColor = new Color(0.99f, 0.92f, 0.77f, 1f);
				pressedColor = new Color(0.92f, 0.79f, 0.60f, 1f);
				break;
			default:
				baseColor = new Color(0.97f, 0.92f, 0.82f, 1f);      // common parchment
				hoverColor = new Color(0.99f, 0.95f, 0.88f, 1f);
				pressedColor = new Color(0.94f, 0.88f, 0.76f, 1f);
				break;
		}

		button.AddThemeColorOverride("font_color", baseColor);
		button.AddThemeColorOverride("font_hover_color", hoverColor);
		button.AddThemeColorOverride("font_pressed_color", pressedColor);
		button.AddThemeColorOverride("font_disabled_color", new Color(0.70f, 0.62f, 0.52f, 0.75f));
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
