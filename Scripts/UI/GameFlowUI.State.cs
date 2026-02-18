using Godot;

public partial class GameFlowUI
{
	private void ShowStartPanel()
	{
		// Enter title/menu state and pause gameplay simulation.
		_started = false;
		SetGameplayObjectsVisible(false);
		if (_startPanel != null) _startPanel.Visible = true;
		if (_restartPanel != null) _restartPanel.Visible = false;
		if (_scoreLabel != null) _scoreLabel.Visible = false;
		if (_background != null) _background.Visible = false;
		if (_backgroundDimmer != null) _backgroundDimmer.Visible = false;
		if (_menuBackground != null) _menuBackground.Visible = true;
		if (_menuDimmer != null) _menuDimmer.Visible = true;
		GetTree().Paused = true;
		_startButton?.GrabFocus();
	}

	private void OnStartPressed()
	{
		// Switch from menu state into gameplay state.
		AudioManager.Instance?.PlaySfxUiButton();
		AudioManager.Instance?.PlayBgmGameplay();

		_started = true;
		SetGameplayObjectsVisible(true);
		if (_startPanel != null) _startPanel.Visible = false;
		if (_restartPanel != null) _restartPanel.Visible = false;
		if (_scoreLabel != null) _scoreLabel.Visible = false;
		if (_background != null) _background.Visible = false;
		if (_backgroundDimmer != null) _backgroundDimmer.Visible = false;
		if (_menuBackground != null) _menuBackground.Visible = false;
		if (_menuDimmer != null) _menuDimmer.Visible = false;
		GetTree().Paused = false;
		RespawnPlayerAtViewportCenter();

		_scoreSystem?.ResetScore();
		OnScoreChanged(_scoreSystem != null ? _scoreSystem.Score : 0);
	}

	private void OnPlayerDied()
	{
		// Present restart state only if the run was actually started.
		if (!_started)
			return;

		if (_restartPanel != null)
			_restartPanel.Visible = true;

		if (_finalScoreLabel != null)
			_finalScoreLabel.Text = $"Score: {(_scoreSystem != null ? _scoreSystem.Score : 0)}";

		if (_lowHealthMaterial != null)
			_lowHealthMaterial.SetShaderParameter("intensity", 0f);

		GetTree().Paused = true;
		_restartButton?.GrabFocus();
	}

	private void OnRestartPressed()
	{
		// Restart by reloading scene to guarantee full state reset.
		AudioManager.Instance?.PlaySfxUiButton();
		AudioManager.Instance?.PlayBgmGameplay();

		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
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
