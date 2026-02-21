using Godot;

public partial class GameFlowUI
{
	private void ShowStartPanel()
	{
		// Enter title/menu state and pause gameplay simulation.
		_started = false;
		_ending = false;
		_pauseMenuOpen = false;
		_settingsOpen = false;
		_startSettingsOpen = false;
		_startCardsOpen = false;
		_startCharacterSelectOpen = false;
		SetGameplayObjectsVisible(false);
		if (_startPanel != null) _startPanel.Visible = true;
		if (_startMainVBox != null) _startMainVBox.Visible = true;
		if (_startSettingsPanel != null) _startSettingsPanel.Visible = false;
		if (_startCardsPanel != null) _startCardsPanel.Visible = false;
		if (_startCharacterSelectPanel != null) _startCharacterSelectPanel.Visible = false;
		if (_restartPanel != null) _restartPanel.Visible = false;
		if (_scoreLabel != null) _scoreLabel.Visible = false;
		if (_playerHealthBar != null) _playerHealthBar.Visible = false;
		if (_experienceBarRoot != null) _experienceBarRoot.Visible = false;
		if (_matchCountdownLabel != null) _matchCountdownLabel.Visible = false;
		if (_pausePanel != null) _pausePanel.Visible = false;
		if (_pauseMainVBox != null) _pauseMainVBox.Visible = true;
		if (_pauseSettingsPanel != null) _pauseSettingsPanel.Visible = false;
		if (_background != null) _background.Visible = false;
		if (_backgroundDimmer != null) _backgroundDimmer.Visible = false;
		if (_menuBackground != null) _menuBackground.Visible = true;
		if (_menuDimmer != null) _menuDimmer.Visible = true;
		if (_restartPerfectBannerLabel != null) _restartPerfectBannerLabel.Visible = false;
		RefreshPerfectLeaderboardUi();
		ResetBuildSummaryLabels();
		GetTree().Paused = true;
		_startButton?.GrabFocus();
	}

	private void OnStartPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		EnterCharacterSelect();
	}

	private void OnStartClearLeaderboardPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startClearLeaderboardDialog?.PopupCentered(new Vector2I(460, 180));
	}

	private void OnStartClearLeaderboardConfirmed()
	{
		ClearPerfectLeaderboard();
		RefreshPerfectLeaderboardUi();
	}

	private void OnRestartPressed()
	{
		// Restart by reloading scene to guarantee full state reset.
		AudioManager.Instance?.PlaySfxUiButton();
		AudioManager.Instance?.PlayBgmGameplay();

		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnStartSettingsPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startSettingsOpen = true;
		_startCardsOpen = false;
		_startCharacterSelectOpen = false;
		if (_startMainVBox != null)
			_startMainVBox.Visible = false;
		if (_startSettingsPanel != null)
			_startSettingsPanel.Visible = true;
		if (_startCardsPanel != null)
			_startCardsPanel.Visible = false;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = false;
		_startSettingsBackButton?.GrabFocus();
	}

	private void OnStartCardsPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startSettingsOpen = false;
		_startCardsOpen = true;
		_startCharacterSelectOpen = false;
		if (_startMainVBox != null)
			_startMainVBox.Visible = false;
		if (_startSettingsPanel != null)
			_startSettingsPanel.Visible = false;
		if (_startCardsPanel != null)
			_startCardsPanel.Visible = true;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = false;
		RefreshStartCardsCompendium();
		_startCardsBackButton?.GrabFocus();
	}

	private void OnStartSettingsBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startSettingsOpen = false;
		if (_startMainVBox != null)
			_startMainVBox.Visible = true;
		if (_startSettingsPanel != null)
			_startSettingsPanel.Visible = false;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = false;
		_startSettingsButton?.GrabFocus();
	}

	private void OnStartCardsBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startCardsOpen = false;
		if (_startMainVBox != null)
			_startMainVBox.Visible = true;
		if (_startSettingsPanel != null)
			_startSettingsPanel.Visible = false;
		if (_startCardsPanel != null)
			_startCardsPanel.Visible = false;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = false;
		_startCardsButton?.GrabFocus();
	}

	private void StartRun()
	{
		AudioManager.Instance?.PlayBgmGameplay();

		_started = true;
		_ending = false;
		_pendingFinalBossKillClear = false;
		_pauseMenuOpen = false;
		_settingsOpen = false;
		_startSettingsOpen = false;
		_startCardsOpen = false;
		_startCharacterSelectOpen = false;
		SetGameplayObjectsVisible(true);
		if (_startPanel != null) _startPanel.Visible = false;
		if (_startMainVBox != null) _startMainVBox.Visible = true;
		if (_startSettingsPanel != null) _startSettingsPanel.Visible = false;
		if (_startCardsPanel != null) _startCardsPanel.Visible = false;
		if (_startCharacterSelectPanel != null) _startCharacterSelectPanel.Visible = false;
		if (_restartPanel != null) _restartPanel.Visible = false;
		if (_scoreLabel != null) _scoreLabel.Visible = false;
		if (_playerHealthBar != null) _playerHealthBar.Visible = true;
		if (_experienceBarRoot != null) _experienceBarRoot.Visible = true;
		if (_matchCountdownLabel != null) _matchCountdownLabel.Visible = true;
		if (_pausePanel != null) _pausePanel.Visible = false;
		if (_pauseMainVBox != null) _pauseMainVBox.Visible = true;
		if (_pauseSettingsPanel != null) _pauseSettingsPanel.Visible = false;
		if (_background != null) _background.Visible = false;
		if (_backgroundDimmer != null) _backgroundDimmer.Visible = false;
		if (_menuBackground != null) _menuBackground.Visible = false;
		if (_menuDimmer != null) _menuDimmer.Visible = false;
		if (_restartPerfectBannerLabel != null) _restartPerfectBannerLabel.Visible = false;
		ResetBuildSummaryLabels();

		if (_player != null)
		{
			_player.ApplyCharacter(RunContext.Instance?.GetSelectedOrDefault() ?? _selectedCharacterDefinition);
			_player.SetProcess(true);
			_player.SetPhysicsProcess(true);
		}

		GetTree().Paused = false;
		RespawnPlayerAtViewportCenter();

		_scoreSystem?.ResetScore();
		OnScoreChanged(_scoreSystem != null ? _scoreSystem.Score : 0);
	}

	private void RespawnPlayerAtViewportCenter()
	{
		if (_player == null)
			return;

		Rect2 rect = GetViewport().GetVisibleRect();
		Vector2 center = rect.Position + (rect.Size * 0.5f);
		_player.RespawnAt(center);
	}

	private void SetGameplayObjectsVisible(bool visible)
	{
		if (_player != null)
			_player.Visible = visible;
		if (_enemiesRoot != null)
			_enemiesRoot.Visible = visible;
		if (_projectilesRoot != null)
			_projectilesRoot.Visible = visible;
		if (_obstaclesRoot != null)
			_obstaclesRoot.Visible = visible;
	}
}
