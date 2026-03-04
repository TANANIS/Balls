using Godot;

public static class InputGlyphService
{
	public static string GetBindingDisplay(string actionName, InputBindingSlot slot, InputDeviceFamily gamepadFamily)
	{
		InputEvent bindingEvent = InputRebindService.GetBindingEvent(actionName, slot);
		if (bindingEvent == null)
			return "-";

		InputDeviceFamily family = slot == InputBindingSlot.Gamepad
			? gamepadFamily
			: InputDeviceFamily.KeyboardMouse;
		return GetInputEventDisplay(bindingEvent, family);
	}

	public static string GetPrimaryPromptDisplay(string actionName)
	{
		if (InputDeviceService.IsGamepadActive)
		{
			string gamepadDisplay = GetBindingDisplay(
				actionName,
				InputBindingSlot.Gamepad,
				InputDeviceService.CurrentDeviceFamily);
			if (gamepadDisplay != "-")
				return gamepadDisplay;
		}

		return GetBindingDisplay(actionName, InputBindingSlot.KeyboardMouse, InputDeviceService.CurrentDeviceFamily);
	}

	public static string GetInputEventDisplay(InputEvent inputEvent, InputDeviceFamily family)
	{
		if (inputEvent == null)
			return "-";

		if (inputEvent is InputEventKey keyEvent)
			return FormatKeyboardKey(keyEvent);
		if (inputEvent is InputEventMouseButton mouseButton)
			return FormatMouseButton(mouseButton);
		if (inputEvent is InputEventJoypadButton joypadButton)
			return FormatJoypadButton(joypadButton.ButtonIndex, family);
		if (inputEvent is InputEventJoypadMotion joypadMotion)
			return FormatJoypadAxis(joypadMotion.Axis, joypadMotion.AxisValue, family);

		return inputEvent.AsText();
	}

	private static string FormatKeyboardKey(InputEventKey keyEvent)
	{
		Key keyCode = keyEvent.PhysicalKeycode != Key.None ? keyEvent.PhysicalKeycode : keyEvent.Keycode;
		if (keyCode == Key.None)
			return "Key";

		string keyLabel = OS.GetKeycodeString(keyCode);
		return string.IsNullOrWhiteSpace(keyLabel) ? keyCode.ToString() : keyLabel.ToUpperInvariant();
	}

	private static string FormatMouseButton(InputEventMouseButton mouseButton)
	{
		return mouseButton.ButtonIndex switch
		{
			MouseButton.Left => "LMB",
			MouseButton.Right => "RMB",
			MouseButton.Middle => "MMB",
			MouseButton.WheelUp => "Wheel Up",
			MouseButton.WheelDown => "Wheel Down",
			_ => $"Mouse {(int)mouseButton.ButtonIndex}"
		};
	}

	private static string FormatJoypadButton(JoyButton button, InputDeviceFamily family)
	{
		return family switch
		{
			InputDeviceFamily.PlayStation => FormatPlayStationButton(button),
			InputDeviceFamily.Xbox => FormatXboxButton(button),
			_ => FormatGenericButton(button)
		};
	}

	private static string FormatJoypadAxis(JoyAxis axis, float axisValue, InputDeviceFamily family)
	{
		bool positive = axisValue >= 0f;
		return axis switch
		{
			JoyAxis.LeftX => positive ? "LS Right" : "LS Left",
			JoyAxis.LeftY => positive ? "LS Down" : "LS Up",
			JoyAxis.RightX => positive ? "RS Right" : "RS Left",
			JoyAxis.RightY => positive ? "RS Down" : "RS Up",
			_ => FormatUnknownAxis(axis, positive, family)
		};
	}

	private static string FormatUnknownAxis(JoyAxis axis, bool positive, InputDeviceFamily family)
	{
		if (family == InputDeviceFamily.PlayStation)
		{
			if ((int)axis == 4)
				return positive ? "L2+" : "L2-";
			if ((int)axis == 5)
				return positive ? "R2+" : "R2-";
		}
		else if (family == InputDeviceFamily.Xbox)
		{
			if ((int)axis == 4)
				return positive ? "LT+" : "LT-";
			if ((int)axis == 5)
				return positive ? "RT+" : "RT-";
		}

		return $"{axis} {(positive ? "+" : "-")}";
	}

	private static string FormatXboxButton(JoyButton button)
	{
		return button switch
		{
			JoyButton.A => "A",
			JoyButton.B => "B",
			JoyButton.X => "X",
			JoyButton.Y => "Y",
			JoyButton.LeftShoulder => "LB",
			JoyButton.RightShoulder => "RB",
			JoyButton.Back => "View",
			JoyButton.Start => "Menu",
			JoyButton.LeftStick => "LS",
			JoyButton.RightStick => "RS",
			JoyButton.DpadUp => "DPad Up",
			JoyButton.DpadDown => "DPad Down",
			JoyButton.DpadLeft => "DPad Left",
			JoyButton.DpadRight => "DPad Right",
			_ => $"Btn {(int)button}"
		};
	}

	private static string FormatPlayStationButton(JoyButton button)
	{
		return button switch
		{
			JoyButton.A => "Cross",
			JoyButton.B => "Circle",
			JoyButton.X => "Square",
			JoyButton.Y => "Triangle",
			JoyButton.LeftShoulder => "L1",
			JoyButton.RightShoulder => "R1",
			JoyButton.Back => "Share",
			JoyButton.Start => "Options",
			JoyButton.LeftStick => "L3",
			JoyButton.RightStick => "R3",
			JoyButton.DpadUp => "DPad Up",
			JoyButton.DpadDown => "DPad Down",
			JoyButton.DpadLeft => "DPad Left",
			JoyButton.DpadRight => "DPad Right",
			_ => $"Btn {(int)button}"
		};
	}

	private static string FormatGenericButton(JoyButton button)
	{
		return button switch
		{
			JoyButton.A => "South",
			JoyButton.B => "East",
			JoyButton.X => "West",
			JoyButton.Y => "North",
			JoyButton.LeftShoulder => "L1",
			JoyButton.RightShoulder => "R1",
			JoyButton.Start => "Start",
			JoyButton.Back => "Back",
			JoyButton.LeftStick => "L3",
			JoyButton.RightStick => "R3",
			JoyButton.DpadUp => "DPad Up",
			JoyButton.DpadDown => "DPad Down",
			JoyButton.DpadLeft => "DPad Left",
			JoyButton.DpadRight => "DPad Right",
			_ => $"Btn {(int)button}"
		};
	}
}
