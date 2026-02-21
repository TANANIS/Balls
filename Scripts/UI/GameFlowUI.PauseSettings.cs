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
		if (_pausePanel != null)
			_pausePanel.Visible = true;
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = true;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = false;
		GetTree().Paused = true;
		_pauseResumeButton?.GrabFocus();
	}

	private void ClosePauseMenu()
	{
		_pauseMenuOpen = false;
		_settingsOpen = false;
		if (_pausePanel != null)
			_pausePanel.Visible = false;
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = true;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = false;
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
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = false;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = true;
		_settingsBackButton?.GrabFocus();
	}

	private void OnPauseSettingsBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_settingsOpen = false;
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = true;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = false;
		_pauseSettingsButton?.GrabFocus();
	}

	private void OnPauseRestartPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_pauseMenuOpen = false;
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnPauseToTitlePressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
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
