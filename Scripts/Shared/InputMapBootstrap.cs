using Godot;

public static class InputMapBootstrap
{
	private const float DefaultDeadzone = 0.2f;
	private const float AxisDetectThreshold = 0.5f;

	public static void EnsureDefaultMappings()
	{
		EnsureMovementActions();
		EnsureCombatActions();
		EnsureUiActions();
		EnsurePauseAction();
	}

	private static void EnsureMovementActions()
	{
		EnsureAction(InputActions.MoveUp);
		EnsureAction(InputActions.MoveDown);
		EnsureAction(InputActions.MoveLeft);
		EnsureAction(InputActions.MoveRight);

		AddIfMissing(InputActions.MoveUp, NewJoypadMotion(JoyAxis.LeftY, -1f));
		AddIfMissing(InputActions.MoveDown, NewJoypadMotion(JoyAxis.LeftY, 1f));
		AddIfMissing(InputActions.MoveLeft, NewJoypadMotion(JoyAxis.LeftX, -1f));
		AddIfMissing(InputActions.MoveRight, NewJoypadMotion(JoyAxis.LeftX, 1f));

		AddIfMissing(InputActions.MoveUp, NewJoypadButton(JoyButton.DpadUp));
		AddIfMissing(InputActions.MoveDown, NewJoypadButton(JoyButton.DpadDown));
		AddIfMissing(InputActions.MoveLeft, NewJoypadButton(JoyButton.DpadLeft));
		AddIfMissing(InputActions.MoveRight, NewJoypadButton(JoyButton.DpadRight));
	}

	private static void EnsureCombatActions()
	{
		EnsureAction(InputActions.AttackPrimary);
		EnsureAction(InputActions.AttackSecondary);
		EnsureAction(InputActions.Dash);

		AddIfMissing(InputActions.AttackPrimary, NewJoypadButton(JoyButton.RightShoulder));
		AddIfMissing(InputActions.AttackSecondary, NewJoypadButton(JoyButton.LeftShoulder));
		AddIfMissing(InputActions.Dash, NewJoypadButton(JoyButton.A));
	}

	private static void EnsureUiActions()
	{
		EnsureAction("ui_up");
		EnsureAction("ui_down");
		EnsureAction("ui_left");
		EnsureAction("ui_right");
		EnsureAction("ui_accept");
		EnsureAction("ui_cancel");

		AddIfMissing("ui_up", NewKey(Key.Up));
		AddIfMissing("ui_down", NewKey(Key.Down));
		AddIfMissing("ui_left", NewKey(Key.Left));
		AddIfMissing("ui_right", NewKey(Key.Right));
		AddIfMissing("ui_accept", NewKey(Key.Enter));
		AddIfMissing("ui_accept", NewKey(Key.Space));
		AddIfMissing("ui_cancel", NewKey(Key.Escape));

		AddIfMissing("ui_up", NewJoypadButton(JoyButton.DpadUp));
		AddIfMissing("ui_down", NewJoypadButton(JoyButton.DpadDown));
		AddIfMissing("ui_left", NewJoypadButton(JoyButton.DpadLeft));
		AddIfMissing("ui_right", NewJoypadButton(JoyButton.DpadRight));
		AddIfMissing("ui_up", NewJoypadMotion(JoyAxis.LeftY, -1f));
		AddIfMissing("ui_down", NewJoypadMotion(JoyAxis.LeftY, 1f));
		AddIfMissing("ui_left", NewJoypadMotion(JoyAxis.LeftX, -1f));
		AddIfMissing("ui_right", NewJoypadMotion(JoyAxis.LeftX, 1f));
		AddIfMissing("ui_accept", NewJoypadButton(JoyButton.A));
		AddIfMissing("ui_cancel", NewJoypadButton(JoyButton.B));
	}

	private static void EnsurePauseAction()
	{
		EnsureAction(InputActions.Pause);
		AddIfMissing(InputActions.Pause, NewKey(Key.Escape));
		AddIfMissing(InputActions.Pause, NewJoypadButton(JoyButton.Start));
	}

	private static void EnsureAction(string actionName)
	{
		if (!InputMap.HasAction(actionName))
			InputMap.AddAction(actionName, DefaultDeadzone);
	}

	private static void AddIfMissing(string actionName, InputEvent candidate)
	{
		if (candidate == null)
			return;

		foreach (InputEvent existing in InputMap.ActionGetEvents(actionName))
		{
			if (IsEquivalent(existing, candidate))
				return;
		}

		InputMap.ActionAddEvent(actionName, candidate);
	}

	private static bool IsEquivalent(InputEvent a, InputEvent b)
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

		if (a is InputEventJoypadButton buttonA && b is InputEventJoypadButton buttonB)
			return buttonA.ButtonIndex == buttonB.ButtonIndex;

		if (a is InputEventJoypadMotion motionA && b is InputEventJoypadMotion motionB)
		{
			if (motionA.Axis != motionB.Axis)
				return false;

			float signA = Mathf.Sign(motionA.AxisValue);
			float signB = Mathf.Sign(motionB.AxisValue);
			return signA == signB
				&& Mathf.Abs(motionA.AxisValue) >= AxisDetectThreshold
				&& Mathf.Abs(motionB.AxisValue) >= AxisDetectThreshold;
		}

		return false;
	}

	private static InputEventKey NewKey(Key key)
	{
		return new InputEventKey
		{
			Keycode = key,
			PhysicalKeycode = key
		};
	}

	private static InputEventJoypadButton NewJoypadButton(JoyButton button)
	{
		return new InputEventJoypadButton
		{
			ButtonIndex = button
		};
	}

	private static InputEventJoypadMotion NewJoypadMotion(JoyAxis axis, float axisValue)
	{
		return new InputEventJoypadMotion
		{
			Axis = axis,
			AxisValue = axisValue
		};
	}
}
