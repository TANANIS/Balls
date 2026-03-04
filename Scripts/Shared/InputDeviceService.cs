using Godot;
using System;

public enum InputDeviceFamily
{
	KeyboardMouse = 0,
	Xbox = 1,
	PlayStation = 2,
	GenericGamepad = 3
}

public static class InputDeviceService
{
	private const float JoyMotionDetectThreshold = 0.5f;

	public static event Action<InputDeviceFamily> DeviceFamilyChanged;

	public static InputDeviceFamily CurrentDeviceFamily { get; private set; } = InputDeviceFamily.KeyboardMouse;
	public static int ActiveGamepadDeviceId { get; private set; } = 0;

	public static bool IsGamepadActive => CurrentDeviceFamily != InputDeviceFamily.KeyboardMouse;

	public static void NotifyInput(InputEvent inputEvent)
	{
		if (inputEvent == null)
			return;

		if (IsKeyboardMouseInput(inputEvent))
		{
			SetCurrentDeviceFamily(InputDeviceFamily.KeyboardMouse);
			return;
		}

		if (inputEvent is InputEventJoypadButton joypadButton && joypadButton.Pressed)
		{
			ActiveGamepadDeviceId = joypadButton.Device;
			SetCurrentDeviceFamily(ResolveGamepadFamily(joypadButton.Device));
			return;
		}

		if (inputEvent is InputEventJoypadMotion joypadMotion && Mathf.Abs(joypadMotion.AxisValue) >= JoyMotionDetectThreshold)
		{
			ActiveGamepadDeviceId = joypadMotion.Device;
			SetCurrentDeviceFamily(ResolveGamepadFamily(joypadMotion.Device));
		}
	}

	public static Vector2 GetActiveRightStickVector(float deadzone = 0.25f)
	{
		var pads = Input.GetConnectedJoypads();
		if (pads == null || pads.Count == 0)
			return Vector2.Zero;

		int activePad = ActiveGamepadDeviceId;
		if (!pads.Contains(activePad))
			activePad = (int)pads[0];

		Vector2 raw = new(
			Input.GetJoyAxis(activePad, JoyAxis.RightX),
			Input.GetJoyAxis(activePad, JoyAxis.RightY));
		float dz = Mathf.Clamp(deadzone, 0.01f, 0.95f);
		return raw.LengthSquared() >= (dz * dz) ? raw : Vector2.Zero;
	}

	public static string GetDeviceFamilyDisplayName(InputDeviceFamily family, bool useTraditionalChinese)
	{
		return family switch
		{
			InputDeviceFamily.Xbox => "Xbox",
			InputDeviceFamily.PlayStation => "PlayStation",
			InputDeviceFamily.GenericGamepad => useTraditionalChinese ? "\u624b\u628a (Generic)" : "Gamepad (Generic)",
			_ => useTraditionalChinese ? "\u9375\u76e4 / \u6ed1\u9f20" : "Keyboard / Mouse"
		};
	}

	private static void SetCurrentDeviceFamily(InputDeviceFamily family)
	{
		if (CurrentDeviceFamily == family)
			return;

		CurrentDeviceFamily = family;
		DeviceFamilyChanged?.Invoke(CurrentDeviceFamily);
	}

	private static bool IsKeyboardMouseInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey key)
			return key.Pressed && !key.Echo;

		if (inputEvent is InputEventMouseButton mouseButton)
			return mouseButton.Pressed;

		return inputEvent is InputEventMouseMotion;
	}

	private static InputDeviceFamily ResolveGamepadFamily(int deviceId)
	{
		string joyName = Input.GetJoyName(deviceId) ?? string.Empty;
		string normalized = joyName.ToLowerInvariant();

		if (normalized.Contains("xbox") || normalized.Contains("xinput") || normalized.Contains("microsoft"))
			return InputDeviceFamily.Xbox;

		if (normalized.Contains("playstation")
			|| normalized.Contains("dualshock")
			|| normalized.Contains("dualsense")
			|| normalized.Contains("wireless controller")
			|| normalized.Contains("sony"))
		{
			return InputDeviceFamily.PlayStation;
		}

		return InputDeviceFamily.GenericGamepad;
	}
}
