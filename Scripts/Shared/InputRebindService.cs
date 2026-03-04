using Godot;
using System;
using System.Collections.Generic;

public enum InputBindingSlot
{
	KeyboardMouse = 0,
	Gamepad = 1
}

public static class InputRebindService
{
	private const string SavePath = "user://input_rebinds.cfg";
	private const string KeyboardKey = "keyboard";
	private const string GamepadKey = "gamepad";
	private const float DefaultJoyDeadzone = 0.2f;
	private const float JoyMotionThreshold = 0.5f;

	public static event Action BindingsChanged;

	private readonly struct BindingPair
	{
		public readonly InputEvent KeyboardMouse;
		public readonly InputEvent Gamepad;

		public BindingPair(InputEvent keyboardMouse, InputEvent gamepad)
		{
			KeyboardMouse = keyboardMouse;
			Gamepad = gamepad;
		}

		public InputEvent GetSlot(InputBindingSlot slot)
		{
			return slot == InputBindingSlot.KeyboardMouse ? KeyboardMouse : Gamepad;
		}
	}

	private static readonly string[] ManagedActions =
	{
		InputActions.MoveUp,
		InputActions.MoveDown,
		InputActions.MoveLeft,
		InputActions.MoveRight,
		InputActions.AttackPrimary,
		InputActions.AttackSecondary,
		InputActions.Dash,
		InputActions.Pause
	};

	private static readonly Dictionary<string, BindingPair> DefaultBindings = new(StringComparer.Ordinal);
	private static readonly Dictionary<string, BindingPair> ActiveBindings = new(StringComparer.Ordinal);
	private static bool _initialized;

	public static IReadOnlyList<string> GetManagedActions()
	{
		return ManagedActions;
	}

	public static void Initialize()
	{
		if (_initialized)
			return;

		InputMapBootstrap.EnsureDefaultMappings();
		CaptureCurrentAsDefaultBindings();
		LoadBindingsFromDisk();
		RefreshActiveBindingsFromInputMap();
		_initialized = true;
		BindingsChanged?.Invoke();
	}

	public static InputEvent GetBindingEvent(string actionName, InputBindingSlot slot)
	{
		EnsureInitialized();
		if (string.IsNullOrWhiteSpace(actionName))
			return null;
		if (!ActiveBindings.TryGetValue(actionName, out BindingPair pair))
			return null;
		return CloneInputEvent(pair.GetSlot(slot));
	}

	public static bool TryRebindAction(string actionName, InputBindingSlot slot, InputEvent sourceEvent, out string errorMessage)
	{
		EnsureInitialized();
		errorMessage = string.Empty;

		if (string.IsNullOrWhiteSpace(actionName) || !InputMap.HasAction(actionName))
		{
			errorMessage = "Invalid action.";
			return false;
		}

		if (!TryNormalizeForSlot(slot, sourceEvent, out InputEvent normalizedEvent))
		{
			errorMessage = slot == InputBindingSlot.KeyboardMouse
				? "Please press a keyboard key or mouse button."
				: "Please press a gamepad button or axis.";
			return false;
		}

		ApplyBindingInternal(actionName, slot, normalizedEvent, enforceUniqueWithinManagedSlot: true);
		SaveBindingsToDisk();
		BindingsChanged?.Invoke();
		return true;
	}

	public static void ResetAllToDefault()
	{
		EnsureInitialized();
		foreach (string action in ManagedActions)
			ResetActionToDefault(action);
		SaveBindingsToDisk();
		BindingsChanged?.Invoke();
	}

	public static void ResetActionToDefault(string actionName)
	{
		EnsureInitialized();
		if (string.IsNullOrWhiteSpace(actionName))
			return;
		if (!DefaultBindings.TryGetValue(actionName, out BindingPair defaults))
			return;

		ApplyBindingInternal(actionName, InputBindingSlot.KeyboardMouse, defaults.KeyboardMouse, enforceUniqueWithinManagedSlot: false);
		ApplyBindingInternal(actionName, InputBindingSlot.Gamepad, defaults.Gamepad, enforceUniqueWithinManagedSlot: false);
	}

	private static void EnsureInitialized()
	{
		if (!_initialized)
			Initialize();
	}

	private static void CaptureCurrentAsDefaultBindings()
	{
		DefaultBindings.Clear();
		foreach (string action in ManagedActions)
		{
			EnsureActionExists(action);
			DefaultBindings[action] = ExtractBindingPair(action);
		}
	}

	private static void LoadBindingsFromDisk()
	{
		var config = new ConfigFile();
		if (config.Load(SavePath) != Error.Ok)
			return;

		foreach (string action in ManagedActions)
		{
			string keyboardSerialized = config.GetValue(action, KeyboardKey, string.Empty).AsString();
			string gamepadSerialized = config.GetValue(action, GamepadKey, string.Empty).AsString();

			if (TryDeserializeInputEvent(keyboardSerialized, out InputEvent keyboardEvent))
				ApplyBindingInternal(action, InputBindingSlot.KeyboardMouse, keyboardEvent, enforceUniqueWithinManagedSlot: false);
			if (TryDeserializeInputEvent(gamepadSerialized, out InputEvent gamepadEvent))
				ApplyBindingInternal(action, InputBindingSlot.Gamepad, gamepadEvent, enforceUniqueWithinManagedSlot: false);
		}
	}

	private static void SaveBindingsToDisk()
	{
		var config = new ConfigFile();
		foreach (string action in ManagedActions)
		{
			InputEvent keyboardEvent = GetBindingEvent(action, InputBindingSlot.KeyboardMouse);
			InputEvent gamepadEvent = GetBindingEvent(action, InputBindingSlot.Gamepad);
			config.SetValue(action, KeyboardKey, SerializeInputEvent(keyboardEvent));
			config.SetValue(action, GamepadKey, SerializeInputEvent(gamepadEvent));
		}

		config.Save(SavePath);
	}

	private static void RefreshActiveBindingsFromInputMap()
	{
		ActiveBindings.Clear();
		foreach (string action in ManagedActions)
		{
			EnsureActionExists(action);
			ActiveBindings[action] = ExtractBindingPair(action);
		}
	}

	private static BindingPair ExtractBindingPair(string actionName)
	{
		InputEvent keyboardEvent = null;
		InputEvent gamepadEvent = null;
		var actionEvents = InputMap.ActionGetEvents(actionName);
		foreach (InputEvent actionEvent in actionEvents)
		{
			if (keyboardEvent == null && IsKeyboardMouseEvent(actionEvent))
				keyboardEvent = CloneInputEvent(actionEvent);
			else if (gamepadEvent == null && IsGamepadEvent(actionEvent))
				gamepadEvent = CloneInputEvent(actionEvent);
		}

		return new BindingPair(keyboardEvent, gamepadEvent);
	}

	private static void ApplyBindingInternal(string actionName, InputBindingSlot slot, InputEvent eventToAssign, bool enforceUniqueWithinManagedSlot)
	{
		if (string.IsNullOrWhiteSpace(actionName) || eventToAssign == null)
			return;

		EnsureActionExists(actionName);
		InputEvent clonedEvent = CloneInputEvent(eventToAssign);
		if (clonedEvent == null)
			return;

		if (enforceUniqueWithinManagedSlot)
			RemoveSameSlotEquivalentEventsFromOtherManagedActions(actionName, slot, clonedEvent);
		RemoveEventsForSlot(actionName, slot);
		InputMap.ActionAddEvent(actionName, clonedEvent);
		ActiveBindings[actionName] = ExtractBindingPair(actionName);
	}

	private static void RemoveSameSlotEquivalentEventsFromOtherManagedActions(string actionName, InputBindingSlot slot, InputEvent eventToRemove)
	{
		foreach (string otherAction in ManagedActions)
		{
			if (otherAction == actionName || !InputMap.HasAction(otherAction))
				continue;

			var events = InputMap.ActionGetEvents(otherAction);
			foreach (InputEvent existingEvent in events)
			{
				if (!BelongsToSlot(existingEvent, slot))
					continue;
				if (!AreEquivalent(existingEvent, eventToRemove))
					continue;
				InputMap.ActionEraseEvent(otherAction, existingEvent);
			}
		}
	}

	private static void RemoveEventsForSlot(string actionName, InputBindingSlot slot)
	{
		if (!InputMap.HasAction(actionName))
			return;
		var events = InputMap.ActionGetEvents(actionName);
		foreach (InputEvent existingEvent in events)
		{
			if (BelongsToSlot(existingEvent, slot))
				InputMap.ActionEraseEvent(actionName, existingEvent);
		}
	}

	private static void EnsureActionExists(string actionName)
	{
		if (!InputMap.HasAction(actionName))
			InputMap.AddAction(actionName, DefaultJoyDeadzone);
	}

	private static bool TryNormalizeForSlot(InputBindingSlot slot, InputEvent sourceEvent, out InputEvent normalizedEvent)
	{
		normalizedEvent = null;
		if (sourceEvent == null)
			return false;

		if (slot == InputBindingSlot.KeyboardMouse)
		{
			if (sourceEvent is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
			{
				normalizedEvent = new InputEventKey
				{
					Keycode = keyEvent.Keycode,
					PhysicalKeycode = keyEvent.PhysicalKeycode != Key.None ? keyEvent.PhysicalKeycode : keyEvent.Keycode,
					ShiftPressed = keyEvent.ShiftPressed,
					CtrlPressed = keyEvent.CtrlPressed,
					AltPressed = keyEvent.AltPressed,
					MetaPressed = keyEvent.MetaPressed
				};
				return true;
			}

			if (sourceEvent is InputEventMouseButton mouseButton && mouseButton.Pressed)
			{
				normalizedEvent = new InputEventMouseButton
				{
					ButtonIndex = mouseButton.ButtonIndex
				};
				return true;
			}

			return false;
		}

		if (sourceEvent is InputEventJoypadButton joypadButton && joypadButton.Pressed)
		{
			normalizedEvent = new InputEventJoypadButton
			{
				ButtonIndex = joypadButton.ButtonIndex
			};
			return true;
		}

		if (sourceEvent is InputEventJoypadMotion joypadMotion && Mathf.Abs(joypadMotion.AxisValue) >= JoyMotionThreshold)
		{
			normalizedEvent = new InputEventJoypadMotion
			{
				Axis = joypadMotion.Axis,
				AxisValue = Mathf.Sign(joypadMotion.AxisValue)
			};
			return true;
		}

		return false;
	}

	private static bool IsKeyboardMouseEvent(InputEvent inputEvent)
	{
		return inputEvent is InputEventKey or InputEventMouseButton;
	}

	private static bool IsGamepadEvent(InputEvent inputEvent)
	{
		return inputEvent is InputEventJoypadButton or InputEventJoypadMotion;
	}

	private static bool BelongsToSlot(InputEvent inputEvent, InputBindingSlot slot)
	{
		return slot == InputBindingSlot.KeyboardMouse
			? IsKeyboardMouseEvent(inputEvent)
			: IsGamepadEvent(inputEvent);
	}

	private static InputEvent CloneInputEvent(InputEvent inputEvent)
	{
		if (inputEvent == null)
			return null;
		return inputEvent.Duplicate() as InputEvent;
	}

	private static bool AreEquivalent(InputEvent a, InputEvent b)
	{
		if (a == null || b == null || a.GetType() != b.GetType())
			return false;

		if (a is InputEventKey keyA && b is InputEventKey keyB)
		{
			return keyA.Keycode == keyB.Keycode
				&& keyA.PhysicalKeycode == keyB.PhysicalKeycode
				&& keyA.ShiftPressed == keyB.ShiftPressed
				&& keyA.CtrlPressed == keyB.CtrlPressed
				&& keyA.AltPressed == keyB.AltPressed
				&& keyA.MetaPressed == keyB.MetaPressed;
		}

		if (a is InputEventMouseButton mouseA && b is InputEventMouseButton mouseB)
			return mouseA.ButtonIndex == mouseB.ButtonIndex;

		if (a is InputEventJoypadButton buttonA && b is InputEventJoypadButton buttonB)
			return buttonA.ButtonIndex == buttonB.ButtonIndex;

		if (a is InputEventJoypadMotion motionA && b is InputEventJoypadMotion motionB)
		{
			return motionA.Axis == motionB.Axis
				&& Mathf.Sign(motionA.AxisValue) == Mathf.Sign(motionB.AxisValue);
		}

		return false;
	}

	private static string SerializeInputEvent(InputEvent inputEvent)
	{
		if (inputEvent == null)
			return string.Empty;

		if (inputEvent is InputEventKey keyEvent)
		{
			return string.Join(
				":",
				"key",
				(int)keyEvent.Keycode,
				(int)keyEvent.PhysicalKeycode,
				keyEvent.ShiftPressed ? 1 : 0,
				keyEvent.CtrlPressed ? 1 : 0,
				keyEvent.AltPressed ? 1 : 0,
				keyEvent.MetaPressed ? 1 : 0);
		}

		if (inputEvent is InputEventMouseButton mouseButton)
			return $"mouse_button:{(int)mouseButton.ButtonIndex}";

		if (inputEvent is InputEventJoypadButton joypadButton)
			return $"joy_button:{(int)joypadButton.ButtonIndex}";

		if (inputEvent is InputEventJoypadMotion joypadMotion)
		{
			int sign = joypadMotion.AxisValue >= 0f ? 1 : -1;
			return $"joy_motion:{(int)joypadMotion.Axis}:{sign}";
		}

		return string.Empty;
	}

	private static bool TryDeserializeInputEvent(string serialized, out InputEvent inputEvent)
	{
		inputEvent = null;
		if (string.IsNullOrWhiteSpace(serialized))
			return false;

		string[] parts = serialized.Split(':');
		if (parts.Length == 0)
			return false;

		switch (parts[0])
		{
			case "key":
			{
				if (parts.Length < 7)
					return false;
				if (!int.TryParse(parts[1], out int keyCodeValue)
					|| !int.TryParse(parts[2], out int physicalCodeValue)
					|| !int.TryParse(parts[3], out int shift)
					|| !int.TryParse(parts[4], out int ctrl)
					|| !int.TryParse(parts[5], out int alt)
					|| !int.TryParse(parts[6], out int meta))
				{
					return false;
				}

				inputEvent = new InputEventKey
				{
					Keycode = (Key)keyCodeValue,
					PhysicalKeycode = (Key)physicalCodeValue,
					ShiftPressed = shift == 1,
					CtrlPressed = ctrl == 1,
					AltPressed = alt == 1,
					MetaPressed = meta == 1
				};
				return true;
			}
			case "mouse_button":
			{
				if (parts.Length < 2 || !int.TryParse(parts[1], out int mouseButtonValue))
					return false;
				inputEvent = new InputEventMouseButton
				{
					ButtonIndex = (MouseButton)mouseButtonValue
				};
				return true;
			}
			case "joy_button":
			{
				if (parts.Length < 2 || !int.TryParse(parts[1], out int joyButtonValue))
					return false;
				inputEvent = new InputEventJoypadButton
				{
					ButtonIndex = (JoyButton)joyButtonValue
				};
				return true;
			}
			case "joy_motion":
			{
				if (parts.Length < 3
					|| !int.TryParse(parts[1], out int axisValue)
					|| !int.TryParse(parts[2], out int signValue))
				{
					return false;
				}

				inputEvent = new InputEventJoypadMotion
				{
					Axis = (JoyAxis)axisValue,
					AxisValue = signValue >= 0 ? 1f : -1f
				};
				return true;
			}
		}

		return false;
	}
}
