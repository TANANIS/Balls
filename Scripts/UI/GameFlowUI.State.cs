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
		_currentRunId = string.Empty;
		SetGameplayObjectsVisible(false);
		if (_startPanel != null) _startPanel.Visible = true;
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		if (_restartPanel != null) _restartPanel.Visible = false;
		if (_scoreLabel != null) _scoreLabel.Visible = false;
		if (_playerHealthBar != null) _playerHealthBar.Visible = false;
		if (_experienceBarRoot != null) _experienceBarRoot.Visible = false;
		if (_matchCountdownLabel != null) _matchCountdownLabel.Visible = false;
		SetPausePanels(showPausePanel: false, showMain: true, showSettings: false);
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

	private void OnStartDeleteSavePressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startDeleteSaveDialog?.PopupCentered(new Vector2I(520, 220));
	}

	private void OnStartDeleteSaveConfirmed()
	{
		bool deleted = MetaProgressionService.Instance.DeleteCurrentProfileSave();
		if (deleted)
			DebugSystem.Log($"[MetaProgression] Save deleted for profile '{MetaProgressionService.Instance.CurrentProfileId}'.");
		else
			DebugSystem.Warn($"[MetaProgression] Save deletion failed for profile '{MetaProgressionService.Instance.CurrentProfileId}'.");

		_selectedCharacterDefinition = ResolveFirstUnlockedCharacterDefinition(_selectedCharacterDefinition);
		RunContext.Instance?.SetSelectedCharacter(_selectedCharacterDefinition);
		RefreshCharacterSelectUi();
	}

	private void OnRestartPressed()
	{
		// Restart by reloading scene to guarantee full state reset.
		AudioManager.Instance?.PlaySfxUiButton();
		AudioManager.Instance?.PlayBgmGameplay();

		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnRestartBackToMetaPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		ShowStartPanel();
		AudioManager.Instance?.PlayBgmMenu();
	}

	private void OnStartSettingsPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startSettingsOpen = true;
		_startCardsOpen = false;
		_startCharacterSelectOpen = false;
		SetStartSubPanels(showMain: false, showSettings: true, showCards: false, showCharacterSelect: false);
		_startSettingsBackButton?.GrabFocus();
	}

	private void OnStartCardsPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startSettingsOpen = false;
		_startCardsOpen = true;
		_startCharacterSelectOpen = false;
		SetStartSubPanels(showMain: false, showSettings: false, showCards: true, showCharacterSelect: false);
		RefreshStartCardsCompendium();
		_startCardsBackButton?.GrabFocus();
	}

	private void OnStartSettingsBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startSettingsOpen = false;
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		_startSettingsButton?.GrabFocus();
	}

	private void OnStartCardsBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startCardsOpen = false;
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
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
		_currentRunId = System.Guid.NewGuid().ToString("N");
		SetGameplayObjectsVisible(true);
		if (_startPanel != null) _startPanel.Visible = false;
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		if (_restartPanel != null) _restartPanel.Visible = false;
		if (_scoreLabel != null) _scoreLabel.Visible = false;
		if (_playerHealthBar != null) _playerHealthBar.Visible = true;
		if (_experienceBarRoot != null) _experienceBarRoot.Visible = true;
		if (_matchCountdownLabel != null) _matchCountdownLabel.Visible = true;
		SetPausePanels(showPausePanel: false, showMain: true, showSettings: false);
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

	private void SetStartSubPanels(bool showMain, bool showSettings, bool showCards, bool showCharacterSelect)
	{
		if (_startMainVBox != null)
			_startMainVBox.Visible = showMain;
		if (_startSettingsPanel != null)
			_startSettingsPanel.Visible = showSettings;
		if (_startCardsPanel != null)
			_startCardsPanel.Visible = showCards;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = showCharacterSelect;
	}

	private void SetPausePanels(bool showPausePanel, bool showMain, bool showSettings)
	{
		if (_pausePanel != null)
			_pausePanel.Visible = showPausePanel;
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = showMain;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = showSettings;
	}
}
