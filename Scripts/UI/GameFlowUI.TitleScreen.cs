using Godot;

public partial class GameFlowUI
{
	public override void _UnhandledInput(InputEvent @event)
	{
		if (!_bootTitleScreenOpen)
			return;

		if (!IsBootTitleDismissInput(@event))
			return;

		EnterStartMenuFromBootTitle();
		GetViewport().SetInputAsHandled();
	}

	private void ShowBootTitleScreen()
	{
		ShowStartPanel();
		if (_titleScreenPanel == null)
			return;

		_bootTitleScreenOpen = true;
		if (_startPanel != null)
			_startPanel.Visible = false;
		_titleScreenPanel.Visible = true;
	}

	private void EnterStartMenuFromBootTitle()
	{
		if (!_bootTitleScreenOpen)
			return;

		_bootTitleScreenOpen = false;
		AudioManager.Instance?.PlaySfxUiButton();
		if (_titleScreenPanel != null)
			_titleScreenPanel.Visible = false;
		if (_startPanel != null)
			_startPanel.Visible = true;
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		_startButton?.GrabFocus();
	}

	private static bool IsBootTitleDismissInput(InputEvent @event)
	{
		if (@event is InputEventKey key)
			return key.Pressed && !key.Echo;
		if (@event is InputEventMouseButton mouseButton)
			return mouseButton.Pressed;
		if (@event is InputEventJoypadButton joypadButton)
			return joypadButton.Pressed;
		return false;
	}
}
