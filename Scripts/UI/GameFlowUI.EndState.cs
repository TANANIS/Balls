using Godot;

public partial class GameFlowUI
{
	private void OnPlayerDied()
	{
		// Present restart state only if the run was actually started.
		if (!_started || _ending)
			return;
		EnterEndState(Tr("UI.END.REASON_PLAYER_DOWN"), true);
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

		await ToSignal(GetTree().CreateTimer(1.35f), SceneTreeTimer.SignalName.Timeout);
		EnterEndState(Tr("UI.END.REASON_UNIVERSE_COLLAPSED"), true);
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

		EnterEndState(Tr("UI.END.REASON_PERFECT_CLEAR"), false);
	}

	private void TryResolvePendingPerfectClear()
	{
		if (!_pendingFinalBossKillClear || _ending || !_started)
			return;
		if (HasAliveMiniBoss())
			return;

		_pendingFinalBossKillClear = false;
		EnterEndState(Tr("UI.END.REASON_PERFECT_CLEAR"), false);
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
			_restartTitleLabel.Text = isFailure ? Tr("UI.END.TITLE_FAILURE") : Tr("UI.END.TITLE_PERFECT");
		if (_restartPerfectBannerLabel != null)
			_restartPerfectBannerLabel.Visible = isPerfectClear;
		if (_restartHintLabel != null)
			_restartHintLabel.Text = isFailure
				? Tr("UI.END.HINT_RESTART")
				: Tr("UI.END.HINT_PERFECT");

		int score = _scoreSystem != null ? _scoreSystem.Score : 0;
		int seconds = _stabilitySystem != null ? Mathf.FloorToInt(_stabilitySystem.ElapsedSeconds) : 0;
		string survival = $"{seconds / 60:D2}:{seconds % 60:D2}";
		if (isPerfectClear)
			RecordPerfectClear(score, ResolvePerfectCharacterName());

		if (_finalScoreLabel != null)
			_finalScoreLabel.Text = $"{reason}\n{Tr("UI.END.SURVIVAL")}: {survival}\n{Tr("UI.HUD.SCORE")}: {score}";

		RefreshFinalBuildSummary();

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

		return Tr("UI.COMMON.UNKNOWN");
	}
}
