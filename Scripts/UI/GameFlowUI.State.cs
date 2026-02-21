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
		_startCharacterSelectOpen = false;
		SetGameplayObjectsVisible(false);
		if (_startPanel != null) _startPanel.Visible = true;
		if (_startMainVBox != null) _startMainVBox.Visible = true;
		if (_startSettingsPanel != null) _startSettingsPanel.Visible = false;
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

	private void OnPlayerDied()
	{
		// Present restart state only if the run was actually started.
		if (!_started || _ending)
			return;
		EnterEndState("Player Down", true);
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
		_startCharacterSelectOpen = false;
		if (_startMainVBox != null)
			_startMainVBox.Visible = false;
		if (_startSettingsPanel != null)
			_startSettingsPanel.Visible = true;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = false;
		_startSettingsBackButton?.GrabFocus();
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

	private void EnterCharacterSelect()
	{
		_startSettingsOpen = false;
		_startCharacterSelectOpen = true;
		if (_startMainVBox != null)
			_startMainVBox.Visible = false;
		if (_startSettingsPanel != null)
			_startSettingsPanel.Visible = false;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = true;

		if (_selectedCharacterDefinition == null)
			_selectedCharacterDefinition = RunContext.Instance?.GetSelectedOrDefault() ?? _rangedCharacter ?? _meleeCharacter ?? _tankCharacter;

		RefreshCharacterSelectUi();
		_startCharacterConfirmButton?.GrabFocus();
	}

	private void RefreshCharacterSelectUi()
	{
		if (_startCharacterDescriptionLabel != null)
		{
			if (_selectedCharacterDefinition != null)
				_startCharacterDescriptionLabel.Text = $"{_selectedCharacterDefinition.DisplayName}\n{_selectedCharacterDefinition.Description}";
			else
				_startCharacterDescriptionLabel.Text = "No character definition loaded.";
		}

		if (_startCharacterRangedButton != null && _rangedCharacter != null)
			_startCharacterRangedButton.Text = _rangedCharacter.DisplayName;
		if (_startCharacterMeleeButton != null && _meleeCharacter != null)
			_startCharacterMeleeButton.Text = _meleeCharacter.DisplayName;
		if (_startCharacterTankButton != null && _tankCharacter != null)
			_startCharacterTankButton.Text = _tankCharacter.DisplayName;
	}

	private void OnCharacterRangedPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_selectedCharacterDefinition = _rangedCharacter;
		RefreshCharacterSelectUi();
	}

	private void OnCharacterMeleePressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_selectedCharacterDefinition = _meleeCharacter;
		RefreshCharacterSelectUi();
	}

	private void OnCharacterTankPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_selectedCharacterDefinition = _tankCharacter;
		RefreshCharacterSelectUi();
	}

	private void OnCharacterSelectBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_startCharacterSelectOpen = false;
		if (_startCharacterSelectPanel != null)
			_startCharacterSelectPanel.Visible = false;
		if (_startMainVBox != null)
			_startMainVBox.Visible = true;
		_startButton?.GrabFocus();
	}

	private void OnCharacterSelectConfirmPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		RunContext.Instance?.SetSelectedCharacter(_selectedCharacterDefinition);
		StartRun();
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
		_startCharacterSelectOpen = false;
		SetGameplayObjectsVisible(true);
		if (_startPanel != null) _startPanel.Visible = false;
		if (_startMainVBox != null) _startMainVBox.Visible = true;
		if (_startSettingsPanel != null) _startSettingsPanel.Visible = false;
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

	private async void OnUniverseCollapsed()
	{
		if (!_started || _ending)
			return;
		_ending = true;

		// Freeze direct player input and stop further spawns during collapse sequence.
		if (_player != null)
		{
			_player.SetProcess(false);
			_player.SetPhysicsProcess(false);
		}

		var spawnList = GetTree().GetNodesInGroup("SpawnSystem");
		foreach (Node node in spawnList)
		{
			node.SetProcess(false);
			node.SetPhysicsProcess(false);
		}

		if (_lowHealthMaterial != null)
			_lowHealthMaterial.SetShaderParameter("intensity", 1f);

		await ToSignal(GetTree().CreateTimer(1.35f), SceneTreeTimer.SignalName.Timeout);
		EnterEndState("Universe Collapsed", true);
	}

	private void OnMatchDurationReached()
	{
		if (!_started || _ending)
			return;

		if (HasAliveMiniBoss())
		{
			_pendingFinalBossKillClear = true;
			DebugSystem.Log("[GameFlowUI] Match timer reached 00:00. Waiting for final boss kill.");
			return;
		}

		EnterEndState("Perfect 15:00 Clear", false);
	}

	private void TryResolvePendingPerfectClear()
	{
		if (!_pendingFinalBossKillClear || _ending || !_started)
			return;
		if (HasAliveMiniBoss())
			return;

		_pendingFinalBossKillClear = false;
		EnterEndState("Perfect 15:00 Clear", false);
	}

	private bool HasAliveMiniBoss()
	{
		if (_enemiesRoot is not Node enemiesNode)
			return false;

		foreach (Node child in enemiesNode.GetChildren())
		{
			if (child is not Enemy enemy)
				continue;

			string name = enemy.Name.ToString().ToLowerInvariant();
			string path = enemy.SceneFilePath?.ToLowerInvariant() ?? string.Empty;
			bool isMiniBoss = name.Contains("miniboss") || path.Contains("minibosshex");
			if (!isMiniBoss)
				continue;

			if (enemy.GetNodeOrNull<EnemyHealth>("Health") is EnemyHealth health && health.IsDead)
				continue;

			return true;
		}

		return false;
	}

	private void EnterEndState(string reason, bool isFailure)
	{
		_ending = true;

		if (_restartPanel != null)
			_restartPanel.Visible = true;

		bool isPerfectClear = !isFailure;
		if (_restartTitleLabel != null)
			_restartTitleLabel.Text = isFailure ? "SYSTEM FAILURE" : "PERFECT CLEAR";
		if (_restartPerfectBannerLabel != null)
			_restartPerfectBannerLabel.Visible = isPerfectClear;
		if (_restartHintLabel != null)
			_restartHintLabel.Text = isFailure
				? "Press the button to restart this run."
				: "You survived the full 15:00. Press to start another perfect run.";

		int score = _scoreSystem != null ? _scoreSystem.Score : 0;
		int seconds = _stabilitySystem != null ? Mathf.FloorToInt(_stabilitySystem.ElapsedSeconds) : 0;
		string survival = $"{seconds / 60:D2}:{seconds % 60:D2}";
		if (isPerfectClear)
			RecordPerfectClear(score, ResolvePerfectCharacterName());

		if (_finalScoreLabel != null)
			_finalScoreLabel.Text = $"{reason}\nSurvival: {survival}\nScore: {score}";

		RefreshFinalBuildSummary();

		if (_lowHealthMaterial != null)
			_lowHealthMaterial.SetShaderParameter("intensity", 0f);
		if (_matchCountdownLabel != null)
			_matchCountdownLabel.Visible = false;
		if (_playerHealthBar != null)
			_playerHealthBar.Visible = false;
		if (_experienceBarRoot != null)
			_experienceBarRoot.Visible = false;
		if (_pausePanel != null)
			_pausePanel.Visible = false;
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = true;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = false;

		GetTree().Paused = true;
		_restartButton?.GrabFocus();
	}

	private string ResolvePerfectCharacterName()
	{
		string name = _player?.ActiveCharacter?.DisplayName;
		if (!string.IsNullOrWhiteSpace(name))
			return name;

		name = _selectedCharacterDefinition?.DisplayName;
		if (!string.IsNullOrWhiteSpace(name))
			return name;

		return "Unknown";
	}
}
