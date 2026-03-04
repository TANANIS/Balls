using Godot;
using System.Threading.Tasks;

public partial class GameFlowUI
{
	[ExportGroup("Start Menu / Back To Title FX")]
	[Export] private bool EnableStartMainBackToTitleTransitionFx = true;
	[Export(PropertyHint.Range, "0.05,1.5,0.01")] private float StartMainBackToTitleFadeInSeconds = 0.25f;
	[Export(PropertyHint.Range, "0.05,1.5,0.01")] private float StartMainBackToTitleFadeOutSeconds = 0.22f;
	[Export(PropertyHint.Range, "0.05,1.5,0.01")] private float StartMainBackToTitleBgmFadeSeconds = 0.28f;

	private void HandlePauseInput()
	{
		if (!_started && _startSettingsOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnStartSettingsBackPressed();
			return;
		}
		if (!_started && _startControlsOpen && _startControlsPageController != null && _startControlsPageController.IsListeningForRebind)
			return;
		if (!_started && _startControlsOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnStartControlsBackPressed();
			return;
		}
		if (!_started && _startCharacterSelectOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnCharacterSelectBackPressed();
			return;
		}
		if (!_started && _startEventLoadoutOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnEventLoadoutBackPressed();
			return;
		}
		if (!_started && _startEventUnlockOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnEventUnlockBackPressed();
			return;
		}
		if (!_started && _startCardsOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnStartCardsBackPressed();
			return;
		}
		if (!_started && IsStartMainMenuOpen() && Input.IsActionJustPressed("ui_cancel"))
		{
			OnStartMainBackToTitlePressed();
			return;
		}

		if (!_started || _ending)
			return;
		if (Input.IsActionJustPressed(InputActions.Pause))
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

	private bool IsStartMainMenuOpen()
	{
		return !_bootTitleScreenOpen
			&& !_startMainBackToTitleTransitionActive
			&& _startPanel != null
			&& _startPanel.Visible
			&& (_startMainVBox == null || _startMainVBox.Visible)
			&& !_startSettingsOpen
			&& !_startCardsOpen
			&& !_startControlsOpen
			&& !_startCharacterSelectOpen
			&& !_startEventLoadoutOpen
			&& !_startEventUnlockOpen
			&& (_startDeleteSaveDialog == null || !_startDeleteSaveDialog.Visible);
	}

	private void OnStartMainBackToTitlePressed()
	{
		if (_startMainBackToTitleTransitionActive)
			return;
		_ = RunStartMainBackToTitleTransitionAsync();
	}

	private async Task RunStartMainBackToTitleTransitionAsync()
	{
		_startMainBackToTitleTransitionActive = true;
		try
		{
			AudioManager.Instance?.PlaySfxUiExit();
			_startMainPageController?.HideLeaderboardDrawer(animate: false);

			if (!EnableStartMainBackToTitleTransitionFx || _startMainBackToTitleMask == null)
			{
				OnStartMainBackToTitlePressedImmediate();
				return;
			}

			Task bgmFadeTask = AudioManager.Instance != null
				? AudioManager.Instance.FadeOutCurrentBgmThenPlayTitleAsync(StartMainBackToTitleBgmFadeSeconds)
				: Task.CompletedTask;

			await FadeStartMainBackToTitleMaskAsync(1f, StartMainBackToTitleFadeInSeconds);
			await bgmFadeTask;
			ShowBootTitleScreen();
			await FadeStartMainBackToTitleMaskAsync(0f, StartMainBackToTitleFadeOutSeconds);
		}
		finally
		{
			_startMainBackToTitleTransitionActive = false;
		}
	}

	private async Task FadeStartMainBackToTitleMaskAsync(float targetAlpha, float durationSeconds)
	{
		if (_startMainBackToTitleMask == null)
			return;

		if (_startMainBackToTitleMaskTween != null && _startMainBackToTitleMaskTween.IsValid())
			_startMainBackToTitleMaskTween.Kill();

		float clampedTarget = Mathf.Clamp(targetAlpha, 0f, 1f);
		Color color = _startMainBackToTitleMask.Color;
		float currentAlpha = Mathf.Clamp(color.A, 0f, 1f);
		color.A = currentAlpha;
		_startMainBackToTitleMask.Color = color;
		_startMainBackToTitleMask.Visible = true;

		_startMainBackToTitleMaskTween = CreateTween();
		_startMainBackToTitleMaskTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_startMainBackToTitleMaskTween.TweenProperty(
			_startMainBackToTitleMask,
			"color:a",
			clampedTarget,
			Mathf.Max(0.05f, durationSeconds))
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(clampedTarget >= currentAlpha ? Tween.EaseType.Out : Tween.EaseType.InOut);
		await ToSignal(_startMainBackToTitleMaskTween, Tween.SignalName.Finished);
		_startMainBackToTitleMaskTween = null;
		if (clampedTarget <= 0.001f)
			_startMainBackToTitleMask.Visible = false;
	}

	private void StopStartMainBackToTitleMaskTween()
	{
		if (_startMainBackToTitleMaskTween != null && _startMainBackToTitleMaskTween.IsValid())
			_startMainBackToTitleMaskTween.Kill();
		_startMainBackToTitleMaskTween = null;
		if (_startMainBackToTitleMask != null)
		{
			Color color = _startMainBackToTitleMask.Color;
			color.A = 0f;
			_startMainBackToTitleMask.Color = color;
			_startMainBackToTitleMask.Visible = false;
		}
	}

	private void OnStartMainBackToTitlePressedImmediate()
	{
		StopStartMainBackToTitleMaskTween();
		ShowBootTitleScreen();
		AudioManager.Instance?.PlayBgmTitleTheme();
	}
}
