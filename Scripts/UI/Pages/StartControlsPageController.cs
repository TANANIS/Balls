using Godot;
using System;
using System.Collections.Generic;

public partial class StartControlsPageController : Control
{
	[ExportGroup("Node Paths")]
	[Export] private NodePath TitlePath = "VBox/Title";
	[Export] private NodePath DeviceCaptionPath = "VBox/DeviceRow/DeviceCaption";
	[Export] private NodePath DeviceValuePath = "VBox/DeviceRow/DeviceValue";
	[Export] private NodePath AutoAimTogglePath = "VBox/AutoAimToggle";
	[Export] private NodePath HintLabelPath = "VBox/HintLabel";
	[Export] private NodePath ActionsGridPath = "VBox/ActionsScroll/ActionsGrid";
	[Export] private NodePath ResetButtonPath = "VBox/ActionButtons/ResetButton";
	[Export] private NodePath BackButtonPath = "VBox/ActionButtons/BackButton";

	public event Action BackPressed;
	public event Action<bool> AutoAimToggled;

	public Button BackButton => _backButton;
	public bool IsListeningForRebind => _listenActive;

	private readonly struct ActionDefinition
	{
		public readonly string ActionName;
		public readonly string LabelKey;
		public readonly string FallbackEn;
		public readonly string FallbackZhTw;

		public ActionDefinition(string actionName, string labelKey, string fallbackEn, string fallbackZhTw)
		{
			ActionName = actionName;
			LabelKey = labelKey;
			FallbackEn = fallbackEn;
			FallbackZhTw = fallbackZhTw;
		}
	}

	private sealed class ActionRow
	{
		public ActionDefinition Definition;
		public Label Label;
		public Button KeyboardButton;
		public Button GamepadButton;
	}

	private static readonly ActionDefinition[] Definitions =
	{
		new(InputActions.MoveUp, "UI.CONTROLS.MOVE_UP", "Move Up", "\u79fb\u52d5\u4e0a"),
		new(InputActions.MoveDown, "UI.CONTROLS.MOVE_DOWN", "Move Down", "\u79fb\u52d5\u4e0b"),
		new(InputActions.MoveLeft, "UI.CONTROLS.MOVE_LEFT", "Move Left", "\u79fb\u52d5\u5de6"),
		new(InputActions.MoveRight, "UI.CONTROLS.MOVE_RIGHT", "Move Right", "\u79fb\u52d5\u53f3"),
		new(InputActions.AttackPrimary, "UI.CONTROLS.ATTACK_PRIMARY", "Primary Attack", "\u4e3b\u653b\u64ca"),
		new(InputActions.AttackSecondary, "UI.CONTROLS.ATTACK_SECONDARY", "Secondary Attack", "\u526f\u653b\u64ca"),
		new(InputActions.Dash, "UI.CONTROLS.DASH", "Dash", "\u885d\u523a"),
		new(InputActions.Pause, "UI.CONTROLS.PAUSE", "Pause", "\u66ab\u505c")
	};

	private Label _titleLabel;
	private Label _deviceCaptionLabel;
	private Label _deviceValueLabel;
	private CheckBox _autoAimToggle;
	private Label _hintLabel;
	private GridContainer _actionsGrid;
	private Button _resetButton;
	private Button _backButton;
	private readonly List<ActionRow> _rows = new();
	private bool _listenActive;
	private string _listenActionName = string.Empty;
	private InputBindingSlot _listenSlot;
	private bool _suppressSignals;
	private int _lastConnectedGamepadCount = -1;
	private InputDeviceFamily _lastDeviceFamily = InputDeviceFamily.KeyboardMouse;
	private bool _autoAimPreferredValue = true;

	public override void _EnterTree()
	{
		InputDeviceService.DeviceFamilyChanged += OnDeviceFamilyChanged;
		InputRebindService.BindingsChanged += OnBindingsChanged;
	}

	public override void _ExitTree()
	{
		InputDeviceService.DeviceFamilyChanged -= OnDeviceFamilyChanged;
		InputRebindService.BindingsChanged -= OnBindingsChanged;
	}

	public override void _Ready()
	{
		InputRebindService.Initialize();
		ResolveNodeReferences();
		BindSignals();
		BuildActionRows();
		ApplyLocalizedTexts();
		RefreshBindings();
	}

	public override void _Process(double delta)
	{
		int gamepadCount = Input.GetConnectedJoypads().Count;
		InputDeviceFamily deviceFamily = InputDeviceService.CurrentDeviceFamily;
		if (gamepadCount == _lastConnectedGamepadCount && deviceFamily == _lastDeviceFamily)
			return;

		_lastConnectedGamepadCount = gamepadCount;
		_lastDeviceFamily = deviceFamily;
		RefreshAutoAimToggleInteractivity();
	}

	public void FocusDefault()
	{
		if (_autoAimToggle != null)
		{
			_autoAimToggle.GrabFocus();
			return;
		}

		if (_rows.Count > 0)
			_rows[0].KeyboardButton?.GrabFocus();
	}

	public void FocusBackButton()
	{
		_backButton?.GrabFocus();
	}

	public void ApplyLocalizedTexts()
	{
		if (_titleLabel != null)
			_titleLabel.Text = TrOrDefault("UI.SETTINGS.CONTROLS", "Controls", "\u64cd\u4f5c\u8a2d\u5b9a");
		if (_deviceCaptionLabel != null)
			_deviceCaptionLabel.Text = $"{TrOrDefault("UI.CONTROLS.CURRENT_DEVICE", "Current Device", "\u76ee\u524d\u88dd\u7f6e")}:";
		if (_autoAimToggle != null)
			_autoAimToggle.Text = TrOrDefault("UI.CONTROLS.AUTO_LOCK", "Auto Lock (Gamepad Auto)", "\u81ea\u52d5\u9396\u5b9a (\u624b\u628a\u81ea\u52d5\u958b\u555f)");
		if (_resetButton != null)
			_resetButton.Text = TrOrDefault("UI.CONTROLS.RESET_DEFAULTS", "Reset Defaults", "\u91cd\u8a2d\u70ba\u9810\u8a2d");
		if (_backButton != null)
			_backButton.Text = Tr("UI.COMMON.BACK");

		if (_actionsGrid != null)
		{
			if (_actionsGrid.GetChildCount() >= 3)
			{
				if (_actionsGrid.GetChild(0) is Label headerAction)
					headerAction.Text = TrOrDefault("UI.CONTROLS.COL_ACTION", "Action", "\u52d5\u4f5c");
				if (_actionsGrid.GetChild(1) is Label headerKeyboard)
					headerKeyboard.Text = TrOrDefault("UI.CONTROLS.COL_KEYBOARD", "Keyboard", "\u9375\u76e4");
				if (_actionsGrid.GetChild(2) is Label headerGamepad)
					headerGamepad.Text = TrOrDefault("UI.CONTROLS.COL_GAMEPAD", "Gamepad", "\u624b\u628a");
			}
		}

		foreach (ActionRow row in _rows)
			row.Label.Text = TrOrDefault(row.Definition.LabelKey, row.Definition.FallbackEn, row.Definition.FallbackZhTw);

		UpdateHintForCurrentState();
		RefreshDeviceLabel();
	}

	public bool TryHandleRebindInput(InputEvent inputEvent)
	{
		if (!_listenActive || inputEvent == null)
			return false;

		if (!TryExtractCaptureEvent(inputEvent, out InputEvent capturedInput, out bool isCancel))
			return false;

		if (isCancel)
		{
			CancelListening();
			return true;
		}

		if (!InputRebindService.TryRebindAction(_listenActionName, _listenSlot, capturedInput, out string errorMessage))
		{
			SetHintText(errorMessage);
			return true;
		}

		AudioManager.Instance?.PlaySfxUiButton();
		string actionLabel = GetLocalizedActionLabel(_listenActionName);
		StopListening(
			TrOrDefault(
				"UI.CONTROLS.REBIND_OK",
				$"{actionLabel} updated.",
				$"\u5df2\u66f4\u65b0\uff1a{actionLabel}"));
		return true;
	}

	private void ResolveNodeReferences()
	{
		_titleLabel = GetNodeOrNull<Label>(TitlePath);
		_deviceCaptionLabel = GetNodeOrNull<Label>(DeviceCaptionPath);
		_deviceValueLabel = GetNodeOrNull<Label>(DeviceValuePath);
		_autoAimToggle = GetNodeOrNull<CheckBox>(AutoAimTogglePath);
		_hintLabel = GetNodeOrNull<Label>(HintLabelPath);
		_actionsGrid = GetNodeOrNull<GridContainer>(ActionsGridPath);
		_resetButton = GetNodeOrNull<Button>(ResetButtonPath);
		_backButton = GetNodeOrNull<Button>(BackButtonPath);
	}

	private void BindSignals()
	{
		if (_autoAimToggle != null)
			_autoAimToggle.Toggled += OnAutoAimToggled;
		if (_resetButton != null)
			_resetButton.Pressed += OnResetPressed;
		if (_backButton != null)
			_backButton.Pressed += OnBackPressed;
	}

	public void SetAutoAimToggle(bool enabled)
	{
		if (_autoAimToggle == null)
			return;

		_lastConnectedGamepadCount = Input.GetConnectedJoypads().Count;
		_lastDeviceFamily = InputDeviceService.CurrentDeviceFamily;
		_autoAimPreferredValue = enabled;
		_suppressSignals = true;
		_autoAimToggle.ButtonPressed = enabled;
		_suppressSignals = false;
		RefreshAutoAimToggleInteractivity();
	}

	private void BuildActionRows()
	{
		if (_actionsGrid == null)
			return;

		foreach (Node child in _actionsGrid.GetChildren())
		{
			_actionsGrid.RemoveChild(child);
			child.QueueFree();
		}

		_rows.Clear();
		_actionsGrid.Columns = 3;
		_actionsGrid.AddThemeConstantOverride("h_separation", 12);
		_actionsGrid.AddThemeConstantOverride("v_separation", 8);
		_actionsGrid.AddChild(CreateHeaderLabel(TrOrDefault("UI.CONTROLS.COL_ACTION", "Action", "\u52d5\u4f5c")));
		_actionsGrid.AddChild(CreateHeaderLabel(TrOrDefault("UI.CONTROLS.COL_KEYBOARD", "Keyboard", "\u9375\u76e4"), HorizontalAlignment.Center));
		_actionsGrid.AddChild(CreateHeaderLabel(TrOrDefault("UI.CONTROLS.COL_GAMEPAD", "Gamepad", "\u624b\u628a"), HorizontalAlignment.Center));

		foreach (ActionDefinition definition in Definitions)
		{
			var label = new Label
			{
				CustomMinimumSize = new Vector2(0f, 34f),
				VerticalAlignment = VerticalAlignment.Center,
				Text = TrOrDefault(definition.LabelKey, definition.FallbackEn, definition.FallbackZhTw)
			};
			label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			_actionsGrid.AddChild(label);

			Button keyboardButton = CreateBindingButton();
			keyboardButton.Pressed += () => BeginListening(definition.ActionName, InputBindingSlot.KeyboardMouse);
			_actionsGrid.AddChild(keyboardButton);

			Button gamepadButton = CreateBindingButton();
			gamepadButton.Pressed += () => BeginListening(definition.ActionName, InputBindingSlot.Gamepad);
			_actionsGrid.AddChild(gamepadButton);

			_rows.Add(new ActionRow
			{
				Definition = definition,
				Label = label,
				KeyboardButton = keyboardButton,
				GamepadButton = gamepadButton
			});
		}
	}

	private void BeginListening(string actionName, InputBindingSlot slot)
	{
		if (string.IsNullOrWhiteSpace(actionName))
			return;

		AudioManager.Instance?.PlaySfxUiButton();
		_listenActive = true;
		_listenActionName = actionName;
		_listenSlot = slot;
		UpdateHintForCurrentState();
		RefreshBindings();
	}

	private void CancelListening()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		StopListening(TrOrDefault("UI.CONTROLS.REBIND_CANCEL", "Rebind canceled.", "\u53d6\u6d88\u66f4\u6539\u6309\u9375\u3002"));
	}

	private void StopListening(string hintText)
	{
		_listenActive = false;
		_listenActionName = string.Empty;
		_listenSlot = InputBindingSlot.KeyboardMouse;
		SetHintText(hintText);
		RefreshBindings();
	}

	private void UpdateHintForCurrentState()
	{
		if (_listenActive)
		{
			SetHintText(
				TrOrDefault(
					"UI.CONTROLS.REBIND_HINT_LISTEN",
					"Press a key/button now. Press Esc to cancel.",
					"\u8acb\u76f4\u63a5\u6309\u4e0b\u8981\u7d81\u5b9a\u7684\u6309\u9375\u3002Esc \u53ef\u53d6\u6d88\u3002"));
			return;
		}

		SetHintText(
			TrOrDefault(
				"UI.CONTROLS.REBIND_HINT_IDLE",
				"Select a slot and press to rebind.",
				"\u9078\u64c7\u8981\u4fee\u6539\u7684\u683c\u4f4d\uff0c\u518d\u6309\u4e0b\u8a72\u6309\u9375\u3002"));
	}

	private void SetHintText(string text)
	{
		if (_hintLabel != null)
			_hintLabel.Text = text ?? string.Empty;
	}

	private void RefreshBindings()
	{
		InputDeviceFamily family = InputDeviceService.CurrentDeviceFamily;
		foreach (ActionRow row in _rows)
		{
			if (row == null)
				continue;

			bool listeningKeyboard = _listenActive
				&& row.Definition.ActionName == _listenActionName
				&& _listenSlot == InputBindingSlot.KeyboardMouse;
			bool listeningGamepad = _listenActive
				&& row.Definition.ActionName == _listenActionName
				&& _listenSlot == InputBindingSlot.Gamepad;

			if (row.KeyboardButton != null)
			{
				row.KeyboardButton.Text = listeningKeyboard
					? TrOrDefault("UI.CONTROLS.REBIND_LISTENING", "Listening...", "\u7b49\u5f85\u8f38\u5165\u4e2d...")
					: InputGlyphService.GetBindingDisplay(row.Definition.ActionName, InputBindingSlot.KeyboardMouse, family);
				row.KeyboardButton.Disabled = _listenActive && !listeningKeyboard;
			}

			if (row.GamepadButton != null)
			{
				row.GamepadButton.Text = listeningGamepad
					? TrOrDefault("UI.CONTROLS.REBIND_LISTENING", "Listening...", "\u7b49\u5f85\u8f38\u5165\u4e2d...")
					: InputGlyphService.GetBindingDisplay(row.Definition.ActionName, InputBindingSlot.Gamepad, family);
				row.GamepadButton.Disabled = _listenActive && !listeningGamepad;
			}
		}

		if (_resetButton != null)
			_resetButton.Disabled = _listenActive;
		if (_backButton != null)
			_backButton.Disabled = _listenActive;
		RefreshAutoAimToggleInteractivity();

		RefreshDeviceLabel();
	}

	private void RefreshAutoAimToggleInteractivity()
	{
		if (_autoAimToggle == null)
			return;

		bool hasConnectedGamepad = Input.GetConnectedJoypads().Count > 0;
		bool gamepadMode = hasConnectedGamepad && InputDeviceService.IsGamepadActive;
		bool shouldDisable = _listenActive || gamepadMode;
		if (gamepadMode)
		{
			_suppressSignals = true;
			_autoAimToggle.ButtonPressed = true;
			_suppressSignals = false;
			_autoAimToggle.TooltipText = TrOrDefault(
				"UI.CONTROLS.AUTO_LOCK_GAMEPAD_LOCKED_HINT",
				"Gamepad detected. Auto lock is forced ON.",
				"\u5df2\u5075\u6e2c\u5230\u624b\u628a\uff0c\u81ea\u52d5\u9396\u5b9a\u5df2\u5f37\u5236\u958b\u555f\u3002");
		}
		else
		{
			_suppressSignals = true;
			_autoAimToggle.ButtonPressed = _autoAimPreferredValue;
			_suppressSignals = false;
			_autoAimToggle.TooltipText = string.Empty;
		}

		_autoAimToggle.Disabled = shouldDisable;
	}

	private void RefreshDeviceLabel()
	{
		if (_deviceValueLabel == null)
			return;

		bool zh = TranslationServer.GetLocale().StartsWith("zh");
		_deviceValueLabel.Text = InputDeviceService.GetDeviceFamilyDisplayName(InputDeviceService.CurrentDeviceFamily, zh);
	}

	private void OnDeviceFamilyChanged(InputDeviceFamily _)
	{
		RefreshBindings();
	}

	private void OnBindingsChanged()
	{
		RefreshBindings();
	}

	private void OnResetPressed()
	{
		if (_listenActive)
			return;

		AudioManager.Instance?.PlaySfxUiButton();
		InputRebindService.ResetAllToDefault();
		SetHintText(
			TrOrDefault(
				"UI.CONTROLS.RESET_DONE",
				"Controls reset to defaults.",
				"\u5df2\u91cd\u8a2d\u70ba\u9810\u8a2d\u6309\u9375\u3002"));
		RefreshBindings();
	}

	private void OnAutoAimToggled(bool enabled)
	{
		if (_suppressSignals || _listenActive)
			return;

		_autoAimPreferredValue = enabled;
		AutoAimToggled?.Invoke(enabled);
	}

	private void OnBackPressed()
	{
		if (_listenActive)
		{
			CancelListening();
			return;
		}

		BackPressed?.Invoke();
	}

	private static bool TryExtractCaptureEvent(InputEvent inputEvent, out InputEvent capturedInput, out bool isCancel)
	{
		capturedInput = null;
		isCancel = false;

		if (inputEvent is InputEventKey keyEvent)
		{
			if (!keyEvent.Pressed || keyEvent.Echo)
				return false;
			if (keyEvent.Keycode == Key.Escape || keyEvent.PhysicalKeycode == Key.Escape)
			{
				isCancel = true;
				return true;
			}

			capturedInput = keyEvent;
			return true;
		}

		if (inputEvent is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			capturedInput = mouseButton;
			return true;
		}

		if (inputEvent is InputEventJoypadButton joypadButton && joypadButton.Pressed)
		{
			capturedInput = joypadButton;
			return true;
		}

		if (inputEvent is InputEventJoypadMotion joypadMotion && Mathf.Abs(joypadMotion.AxisValue) >= 0.5f)
		{
			capturedInput = joypadMotion;
			return true;
		}

		return false;
	}

	private Button CreateBindingButton()
	{
		return new Button
		{
			CustomMinimumSize = new Vector2(0f, 34f),
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};
	}

	private Label CreateHeaderLabel(string text, HorizontalAlignment alignment = HorizontalAlignment.Left)
	{
		var label = new Label
		{
			CustomMinimumSize = new Vector2(0f, 30f),
			Text = text,
			HorizontalAlignment = alignment,
			VerticalAlignment = VerticalAlignment.Center
		};
		label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		label.AddThemeColorOverride("font_color", new Color(0.91f, 0.86f, 0.74f, 0.98f));
		label.AddThemeFontSizeOverride("font_size", 16);
		return label;
	}

	private string GetLocalizedActionLabel(string actionName)
	{
		foreach (ActionRow row in _rows)
		{
			if (row.Definition.ActionName == actionName)
				return row.Label?.Text ?? actionName;
		}

		return actionName;
	}

	private string TrOrDefault(string key, string fallbackEn, string fallbackZhTw)
	{
		string translated = Tr(key);
		if (!string.IsNullOrWhiteSpace(translated) && translated != key)
			return translated;
		return TranslationServer.GetLocale().StartsWith("zh") ? fallbackZhTw : fallbackEn;
	}
}
