using Godot;

public partial class GameFlowUI
{
	private void HandlePauseInput()
	{
		if (!_started && _startSettingsOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnStartSettingsBackPressed();
			return;
		}
		if (!_started && _startCharacterSelectOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnCharacterSelectBackPressed();
			return;
		}
		if (!_started && _startCardsOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnStartCardsBackPressed();
			return;
		}

		if (!_started || _ending)
			return;
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (_upgradeMenu != null && _upgradeMenu.IsOpen)
				return;
			if (_pauseMenuOpen && _settingsOpen)
			{
				OnPauseSettingsBackPressed();
				return;
			}
			if (_pauseMenuOpen)
				ClosePauseMenu();
			else
				OpenPauseMenu();
		}
	}

	private void OpenPauseMenu()
	{
		_pauseMenuOpen = true;
		_settingsOpen = false;
		RefreshPauseBuildSummary();
		SetPausePanels(showPausePanel: true, showMain: true, showSettings: false);
		GetTree().Paused = true;
		_pauseResumeButton?.GrabFocus();
	}

	private void ClosePauseMenu()
	{
		_pauseMenuOpen = false;
		_settingsOpen = false;
		SetPausePanels(showPausePanel: false, showMain: true, showSettings: false);
		GetTree().Paused = false;
	}

	private void OnPauseResumePressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		ClosePauseMenu();
	}

	private void OnPauseSettingsPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_settingsOpen = true;
		SetPausePanels(showPausePanel: true, showMain: false, showSettings: true);
		_settingsBackButton?.GrabFocus();
	}

	private void OnPauseSettingsBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_settingsOpen = false;
		SetPausePanels(showPausePanel: true, showMain: true, showSettings: false);
		_pauseSettingsButton?.GrabFocus();
	}

	private void OnPauseRestartPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_pauseMenuOpen = false;
		StartRun();
	}

	private void OnPauseToTitlePressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		PrepareFreshRun();
		ClosePauseMenu();
		ShowStartPanel();
		AudioManager.Instance?.PlayBgmMenu();
	}

	private void OnQuitGamePressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		GetTree().Quit();
	}
}
