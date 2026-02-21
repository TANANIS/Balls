using Godot;

public partial class GameFlowUI
{
	private void OnScoreChanged(int score)
	{
		if (_scoreLabel != null)
			_scoreLabel.Text = $"{Tr("UI.HUD.SCORE")}: {score}";
	}

	private void UpdateUpgradeProgressUi()
	{
		if (_experienceBarRoot == null || _experienceBar == null || _experienceLabel == null)
			return;

		if (!_started || _ending)
		{
			_experienceBarRoot.Visible = false;
			return;
		}

		_experienceBarRoot.Visible = true;

		if (!IsInstanceValid(_progressionSystem))
		{
			var progressionList = GetTree().GetNodesInGroup("ProgressionSystem");
			if (progressionList.Count > 0)
				_progressionSystem = progressionList[0] as ProgressionSystem;
		}

		if (!IsInstanceValid(_progressionSystem))
		{
			_experienceBar.MaxValue = 1f;
			_experienceBar.Value = 0f;
			_experienceLabel.Text = $"{Tr("UI.HUD.XP")} --/--";
			return;
		}

		float required = Mathf.Max(1f, _progressionSystem.GetCurrentUpgradeRequirement());
		float progress = Mathf.Clamp(_progressionSystem.CurrentUpgradeProgress, 0f, required);

		_experienceBar.MaxValue = required;
		_experienceBar.Value = progress;
		_experienceLabel.Text = _progressionSystem.IsUpgradeReady
			? $"{Tr("UI.HUD.LEVEL")} {_progressionSystem.CurrentUpgradeLevel}  {Tr("UI.HUD.READY")} x{Mathf.Max(1, _progressionSystem.PendingUpgradeCount)}"
			: $"{Tr("UI.HUD.LEVEL")} {_progressionSystem.CurrentUpgradeLevel}  {Tr("UI.HUD.XP")} {Mathf.FloorToInt(progress)}/{Mathf.CeilToInt(required)}";
	}

	private void UpdateMatchCountdownUi()
	{
		if (_matchCountdownLabel == null)
			return;

		if (!_started || _ending || _stabilitySystem == null)
		{
			_matchCountdownLabel.Visible = false;
			return;
		}

		_matchCountdownLabel.Visible = true;
		float limit = Mathf.Max(1f, _stabilitySystem.MatchDurationLimitSeconds);
		float remain = Mathf.Max(0f, limit - _stabilitySystem.ElapsedSeconds);
		int total = Mathf.CeilToInt(remain);
		int mm = total / 60;
		int ss = total % 60;
		_matchCountdownLabel.Text = $"{mm:D2}:{ss:D2}";
	}

	private void OnViewportSizeChanged()
	{
		FitMenuBackground();
	}

	private void FitMenuBackground()
	{
		// Keep menu background aligned to camera center and scale-to-fill viewport.
		if (_menuBackground?.Texture == null)
			return;

		Vector2 texSize = _menuBackground.Texture.GetSize();
		if (texSize.X <= 0 || texSize.Y <= 0)
			return;

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float scale = Mathf.Max(viewportSize.X / texSize.X, viewportSize.Y / texSize.Y) * 1.03f;
		Vector2 center = GetMenuWorldCenter();

		_menuBackground.Scale = new Vector2(scale, scale);
		_menuBackground.GlobalPosition = center;

		if (_menuDimmer != null)
		{
			_menuDimmer.Size = viewportSize;
			_menuDimmer.GlobalPosition = center - (viewportSize * 0.5f);
		}
	}

	private Vector2 GetMenuWorldCenter()
	{
		var camera = GetViewport().GetCamera2D();
		if (camera != null)
			return camera.GetScreenCenterPosition();
		if (_player != null)
			return _player.GlobalPosition;
		Rect2 rect = GetViewport().GetVisibleRect();
		return rect.Position + (rect.Size * 0.5f);
	}
}
