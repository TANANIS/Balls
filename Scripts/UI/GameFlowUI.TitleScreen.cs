using Godot;
using System.Threading.Tasks;

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
		_startMainPageController?.SetMainBackgroundSuppressed(true);
		_titleScreenPanel.Visible = true;
		if (_menuBackground != null)
			_menuBackground.Visible = false;
		StopStartSubpanelDimmerFx();
		ResetBootTitleContentExitFxVisual();
		ApplyBootLetterboxOverride();
		ResetBootDimmerForTitle();
		StartBootBackgroundSwayFx();
		StartBootPromptIdleFx();
		StartBootOpeningMaskFadeIfNeeded();
	}

	private async void EnterStartMenuFromBootTitle()
	{
		if (!_bootTitleScreenOpen)
			return;

		_bootTitleScreenOpen = false;
		_startMainPageController?.SetMainBackgroundSuppressed(true);
		AudioManager.Instance?.PlaySfxUiTitleConfirm();
		Task titleUiExitTask = PlayBootTitleUiExitFxAsync();
		Task letterboxFxTask = PlayBootLetterboxCloseFxAsync();
		Task backgroundScrollFxTask = PlayBootBackgroundScrollToMainAsync();
		Task bgmFadeTask = AudioManager.Instance != null
			? AudioManager.Instance.FadeOutCurrentBgmThenPlayMenuAsync(BootTitleBgmFadeDurationSeconds)
			: Task.CompletedTask;
		await Task.WhenAll(titleUiExitTask, letterboxFxTask, backgroundScrollFxTask, bgmFadeTask);
		StopBootPromptFx(resetVisual: true);
		if (_titleScreenPanel != null)
			_titleScreenPanel.Visible = false;
		if (_startPanel != null)
			_startPanel.Visible = true;
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		_startMainPageController?.SetMainBackgroundSuppressed(false);
		if (_startMainPageController != null)
			_startMainPageController.FocusDefault();
		else
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
