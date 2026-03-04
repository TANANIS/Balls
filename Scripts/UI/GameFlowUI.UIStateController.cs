using Godot;

public partial class GameFlowUI
{
	private void ShowStartPanel()
	{
		// Enter title/menu state and pause gameplay simulation.
		ResetEventLoadoutDraftState();
		_bootTitleScreenOpen = false;
		_started = false;
		_ending = false;
		_pauseMenuOpen = false;
		_settingsOpen = false;
		_startSettingsOpen = false;
		_startCardsOpen = false;
		_startControlsOpen = false;
		_startCharacterSelectOpen = false;
		_startEventUnlockOpen = false;
		_startEventLoadoutOpen = false;
		_startMainBackToTitleTransitionActive = false;
		StopStartMainBackToTitleMaskTween();
		StopBootPromptFx(resetVisual: true);
		_currentRunId = string.Empty;
		SetGameplayObjectsVisible(false);
		if (_titleScreenPanel != null) _titleScreenPanel.Visible = false;
		if (_startPanel != null) _startPanel.Visible = true;
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		if (_restartPanel != null) _restartPanel.Visible = false;
		if (_scoreLabel != null) _scoreLabel.Visible = false;
		if (_playerHealthBar != null) _playerHealthBar.Visible = false;
		if (_experienceBarRoot != null) _experienceBarRoot.Visible = false;
		if (_matchCountdownLabel != null) _matchCountdownLabel.Visible = false;
		if (_eventBannerLabel != null) _eventBannerLabel.Visible = false;
		if (_eventHintLabel != null) _eventHintLabel.Visible = false;
		if (_hybridToastLabel != null) _hybridToastLabel.Visible = false;
		SetPausePanels(showPausePanel: false, showMain: true, showSettings: false);
		if (_background != null) _background.Visible = false;
		if (_backgroundDimmer != null) _backgroundDimmer.Visible = false;
		if (_menuBackground != null) _menuBackground.Visible = false;
		StopStartSubpanelDimmerFx();
		_startMainPageController?.SetMainBackgroundSuppressed(false);
		_startMainPageController?.HideLeaderboardDrawer(animate: false);
		PlayStartSettingsLetterboxFx(expand: false, animate: false);
		if (_restartPerfectBannerLabel != null) _restartPerfectBannerLabel.Visible = false;
		RefreshPerfectLeaderboardUi();
		ResetBuildSummaryLabels();
		GetTree().Paused = true;
		if (_startMainPageController != null)
			_startMainPageController.FocusDefault();
		else
			_startButton?.GrabFocus();
	}

	private void OnStartPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startMainPageController?.HideLeaderboardDrawer(animate: false);
		EnterCharacterSelect();
	}

	private void OnStartDeleteSavePressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startDeleteSaveDialog?.PopupCentered(_startDeleteSaveDialogPopupSize);
	}

	private void OnStartDeleteSaveConfirmed()
	{
		MetaProgressionService.Instance.DeleteCurrentProfileSave();

		_selectedCharacterDefinition = ResolveFirstUnlockedCharacterDefinition(_selectedCharacterDefinition);
		_sharedState.SelectedCharacterDefinition = _selectedCharacterDefinition;
		RunContext.Instance?.SetSelectedCharacter(_selectedCharacterDefinition);
		RefreshCharacterSelectUi();
	}

	private void OnRestartPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		AudioManager.Instance?.PlayBgmGameplay();
		StartRun();
	}

	private void OnRestartBackToMetaPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		PrepareFreshRun();
		ShowStartPanel();
		AudioManager.Instance?.PlayBgmMenu();
	}

	private void OnStartSettingsPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startMainPageController?.HideLeaderboardDrawer(animate: false);
		_startSettingsOpen = true;
		_startCardsOpen = false;
		_startControlsOpen = false;
		_startCharacterSelectOpen = false;
		_startEventUnlockOpen = false;
		_startEventLoadoutOpen = false;
		SetStartSubPanels(showMain: false, showSettings: true, showCards: false, showCharacterSelect: false);
		if (_startSettingsPageController != null)
		{
			_startSettingsPageController.FocusControlsButton();
			if (_startSettingsPageController.ControlsButton == null)
				_startSettingsPageController.FocusBackButton();
		}
		else
		{
			if (_startSettingsControlsButton != null)
				_startSettingsControlsButton.GrabFocus();
			else
				_startSettingsBackButton?.GrabFocus();
		}
	}

	private void OnStartCardsPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startMainPageController?.HideLeaderboardDrawer(animate: false);
		_startSettingsOpen = false;
		_startCardsOpen = true;
		_startControlsOpen = false;
		_startCharacterSelectOpen = false;
		_startEventUnlockOpen = false;
		_startEventLoadoutOpen = false;
		SetStartSubPanels(showMain: false, showSettings: false, showCards: true, showCharacterSelect: false);
		RefreshStartCardsCompendium();
		if (_startCardsPageController != null)
			_startCardsPageController.FocusBackButton();
		else
			_startCardsBackButton?.GrabFocus();
	}

	private void OnStartControlsPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startMainPageController?.HideLeaderboardDrawer(animate: false);
		_startSettingsOpen = false;
		_startCardsOpen = false;
		_startControlsOpen = true;
		_startCharacterSelectOpen = false;
		_startEventUnlockOpen = false;
		_startEventLoadoutOpen = false;
		SetStartSubPanels(showMain: false, showSettings: false, showCards: false, showCharacterSelect: false, showEventLoadout: false, showEventUnlock: false, showControls: true);
		if (_startControlsPageController != null)
			_startControlsPageController.FocusDefault();
		else
			_startControlsBackButton?.GrabFocus();
	}

	private void OnStartSettingsBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startSettingsOpen = false;
		_startControlsOpen = false;
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		if (_startMainPageController != null)
			_startMainPageController.SettingsButton?.GrabFocus();
		else
			_startSettingsButton?.GrabFocus();
	}

	private void OnStartControlsBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startControlsOpen = false;
		_startSettingsOpen = true;
		_startCardsOpen = false;
		_startCharacterSelectOpen = false;
		_startEventUnlockOpen = false;
		_startEventLoadoutOpen = false;
		SetStartSubPanels(showMain: false, showSettings: true, showCards: false, showCharacterSelect: false);
		if (_startSettingsPageController != null)
			_startSettingsPageController.FocusControlsButton();
		else
			_startSettingsControlsButton?.GrabFocus();
	}

	private void OnStartCardsBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startCardsOpen = false;
		_startControlsOpen = false;
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		if (_startMainPageController != null)
			_startMainPageController.CardsButton?.GrabFocus();
		else
			_startCardsButton?.GrabFocus();
	}

	private void OnStartLeaderboardPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_startMainPageController?.ToggleLeaderboardDrawer();
	}

	private void StartRun()
	{
		AudioManager.Instance?.PlayBgmGameplay();

		_bootTitleScreenOpen = false;
		_started = true;
		_ending = false;
		_pendingFinalBossKillClear = false;
		_pauseMenuOpen = false;
		_settingsOpen = false;
		_startSettingsOpen = false;
		_startCardsOpen = false;
		_startControlsOpen = false;
		_startCharacterSelectOpen = false;
		_startEventUnlockOpen = false;
		_startEventLoadoutOpen = false;
		_startMainBackToTitleTransitionActive = false;
		StopStartMainBackToTitleMaskTween();
		StopBootPromptFx(resetVisual: true);
		_currentRunId = System.Guid.NewGuid().ToString("N");
		SetGameplayObjectsVisible(true);
		if (_titleScreenPanel != null) _titleScreenPanel.Visible = false;
		if (_startPanel != null) _startPanel.Visible = false;
		SetStartSubPanels(showMain: true, showSettings: false, showCards: false, showCharacterSelect: false);
		if (_restartPanel != null) _restartPanel.Visible = false;
		if (_scoreLabel != null) _scoreLabel.Visible = false;
		if (_playerHealthBar != null) _playerHealthBar.Visible = true;
		if (_experienceBarRoot != null) _experienceBarRoot.Visible = true;
		if (_matchCountdownLabel != null) _matchCountdownLabel.Visible = true;
		if (_eventBannerLabel != null) _eventBannerLabel.Visible = false;
		if (_eventHintLabel != null) _eventHintLabel.Visible = false;
		if (_hybridToastLabel != null) _hybridToastLabel.Visible = false;
		SetPausePanels(showPausePanel: false, showMain: true, showSettings: false);
		if (_background != null) _background.Visible = true;
		if (_background is ProceduralTerrainBackground terrainBackground)
			terrainBackground.RefreshForNewRun();
		if (_backgroundDimmer != null) _backgroundDimmer.Visible = false;
		if (_menuBackground != null) _menuBackground.Visible = false;
		StopStartSubpanelDimmerFx();
		PlayStartSettingsLetterboxFx(expand: false, animate: false);
		if (_restartPerfectBannerLabel != null) _restartPerfectBannerLabel.Visible = false;
		ResetBuildSummaryLabels();
		PrepareFreshRun();

		if (_player != null)
		{
			CharacterDefinition selectedCharacter = RunContext.Instance?.GetSelectedOrDefault()
				?? _sharedState.SelectedCharacterDefinition
				?? _selectedCharacterDefinition;
			_player.ApplyCharacter(selectedCharacter);
			_player.SetProcess(true);
			_player.SetPhysicsProcess(true);
		}

		GetTree().Paused = false;
		RespawnPlayerAtViewportCenter();

		_scoreSystem?.ResetScore();
		OnScoreChanged(_scoreSystem != null ? _scoreSystem.Score : 0);
	}

	private void PrepareFreshRun()
	{
		ResetGameplaySystemsForNewRun();
		ClearTransientRunNodes();
	}

	private void ResetGameplaySystemsForNewRun()
	{
		Engine.TimeScale = 1f;
		_upgradeMenu?.ForceCloseForRunReset();
		_player?.ResetForNewRunState();

		_stabilitySystem?.ResetForNewRun();
		_progressionSystem?.ResetForNewRun();
		_upgradeSystem?.ResetForNewRun();
		_scoreSystem?.ResetScore();

		var spawnSystems = GetTree().GetNodesInGroup(RuntimeGroups.SpawnSystem);
		foreach (Node node in spawnSystems)
		{
			if (node is SpawnSystem spawnSystem)
				spawnSystem.ResetForNewRun();
		}

		var eventDirectors = GetTree().GetNodesInGroup(RuntimeGroups.EventDirector);
		foreach (Node node in eventDirectors)
		{
			if (node is EventDirector eventDirector)
				eventDirector.ResetForNewRun();
		}
	}

	private void ClearTransientRunNodes()
	{
		if (_enemiesRoot is Node enemiesNode)
			ClearNodeChildren(enemiesNode);

		if (_projectilesRoot is Node projectilesNode)
			ClearNodeChildren(projectilesNode);

		ClearExperiencePickups();

		if (_obstaclesRoot is ObstacleFieldGenerator obstacleField)
			obstacleField.ResetField();
		else if (_obstaclesRoot is Node obstaclesNode)
			ClearNodeChildren(obstaclesNode);
	}

	private void ClearExperiencePickups()
	{
		var pickups = GetTree().GetNodesInGroup(RuntimeGroups.ExperiencePickup);
		foreach (Node node in pickups)
		{
			if (node.GetParent() != null)
				node.GetParent().RemoveChild(node);
			node.QueueFree();
		}
	}

	private static void ClearNodeChildren(Node parent)
	{
		foreach (Node child in parent.GetChildren())
		{
			parent.RemoveChild(child);
			child.QueueFree();
		}
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

	private void SetStartSubPanels(bool showMain, bool showSettings, bool showCards, bool showCharacterSelect, bool showEventLoadout = false, bool showEventUnlock = false, bool showControls = false)
	{
		if (showMain)
			_startMainPageController?.RequestMainContentEnterFx();

		if (_startMainVBox != null)
			_startMainVBox.Visible = showMain;
		if (_startSettingsPanel != null)
			_startSettingsPanel.Visible = showSettings;
		if (_startCardsPanel != null)
			_startCardsPanel.Visible = showCards;
		if (_startControlsPanel != null)
			_startControlsPanel.Visible = showControls;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = showCharacterSelect;
		if (_startEventLoadoutPanel != null)
			_startEventLoadoutPanel.Visible = showEventLoadout;
		if (_startEventUnlockPanel != null)
			_startEventUnlockPanel.Visible = showEventUnlock;

		if (showMain)
			_startMainPageController?.TryPlayPendingMainContentEnterFx();

		PlayStartSettingsLetterboxFx(expand: showSettings || showCards, animate: true);

		UpdateMenuBackgroundVisibilityForStartSubPanels(showMain, showSettings, showCards, showCharacterSelect, showEventLoadout, showEventUnlock, showControls);
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

	private void UpdateMenuBackgroundVisibilityForStartSubPanels(
		bool showMain,
		bool showSettings,
		bool showCards,
		bool showCharacterSelect,
		bool showEventLoadout,
		bool showEventUnlock,
		bool showControls)
	{
		bool shouldShowWorldMenuBackground =
			(_startPanel != null && _startPanel.Visible)
			&& (_titleScreenPanel == null || !_titleScreenPanel.Visible)
			&& !showMain
			&& (showSettings || showCards || showCharacterSelect || showEventLoadout || showEventUnlock || showControls);

		if (_menuBackground != null)
			_menuBackground.Visible = shouldShowWorldMenuBackground;

		// Settings/Cards now own their local backdrop dim masks.
		StopStartSubpanelDimmerFx();
	}
}
