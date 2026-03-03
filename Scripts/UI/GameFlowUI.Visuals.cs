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
			var progressionList = GetTree().GetNodesInGroup(RuntimeGroups.ProgressionSystem);
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

	private void UpdateEventBannerUi()
	{
		if (_eventBannerLabel == null)
			return;

		if (!_started || _ending)
		{
			_eventBannerLabel.Visible = false;
			return;
		}

		if (!IsInstanceValid(_eventDirector))
		{
			var eventDirectorList = GetTree().GetNodesInGroup(RuntimeGroups.EventDirector);
			if (eventDirectorList.Count > 0)
				_eventDirector = eventDirectorList[0] as EventDirector;
		}

		if (!IsInstanceValid(_eventDirector) || !_eventDirector.IsEventActive)
		{
			_eventBannerLabel.Visible = false;
			return;
		}

		_eventBannerLabel.Visible = true;
		_eventBannerLabel.Text = _eventDirector.ActiveEventBannerText;
	}

	private void UpdateEventHintUi()
	{
		if (_eventHintLabel == null)
			return;

		if (!_started || _ending)
		{
			_eventHintLabel.Visible = false;
			return;
		}

		if (!IsInstanceValid(_eventDirector))
		{
			var eventDirectorList = GetTree().GetNodesInGroup(RuntimeGroups.EventDirector);
			if (eventDirectorList.Count > 0)
				_eventDirector = eventDirectorList[0] as EventDirector;
		}

		if (!IsInstanceValid(_eventDirector) || !_eventDirector.IsEventActive)
		{
			_eventHintLabel.Visible = false;
			return;
		}

		string text = _eventDirector.ActiveEventHintText ?? string.Empty;
		if (string.IsNullOrWhiteSpace(text))
		{
			_eventHintLabel.Visible = false;
			return;
		}

		_eventHintLabel.Visible = true;
		_eventHintLabel.Text = text;
	}

	private void UpdateHybridToastUi()
	{
		if (_hybridToastLabel == null)
			return;

		if (!_started || _ending)
		{
			_hybridToastLabel.Visible = false;
			return;
		}

		if (!IsInstanceValid(_eventDirector))
		{
			var eventDirectorList = GetTree().GetNodesInGroup(RuntimeGroups.EventDirector);
			if (eventDirectorList.Count > 0)
				_eventDirector = eventDirectorList[0] as EventDirector;
		}

		if (!IsInstanceValid(_eventDirector))
		{
			_hybridToastLabel.Visible = false;
			return;
		}

		string text = _eventDirector.ActiveHybridToastText ?? string.Empty;
		if (string.IsNullOrWhiteSpace(text))
		{
			_hybridToastLabel.Visible = false;
			return;
		}

		_hybridToastLabel.Visible = true;
		_hybridToastLabel.Text = text;
	}

	private void OnViewportSizeChanged()
	{
		FitMenuBackground();
		UpdateEventLoadoutResponsiveLayout();
	}

	private void FitMenuBackground()
	{
		// Pixel-art cover fit:
		// - compute world-visible area from viewport + camera zoom
		// - snap to integer scale to avoid shimmer/blur
		// - add a small bleed so edges never reveal while camera jitters by sub-pixel
		if (_menuBackground?.Texture == null)
			return;

		Vector2 texSize = _menuBackground.Texture.GetSize();
		if (texSize.X <= 0 || texSize.Y <= 0)
			return;

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		Camera2D camera = GetViewport().GetCamera2D();
		Vector2 zoom = camera != null ? camera.Zoom : Vector2.One;
		Vector2 visibleWorld = new Vector2(
			Mathf.Max(1f, viewportSize.X * zoom.X),
			Mathf.Max(1f, viewportSize.Y * zoom.Y));
		float bleed = Mathf.Max(0f, _menuBackgroundBleed);
		Vector2 coverTarget = new Vector2(visibleWorld.X + bleed * 2f, visibleWorld.Y + bleed * 2f);
		float coverScale = Mathf.Max(coverTarget.X / texSize.X, coverTarget.Y / texSize.Y);
		float scale = Mathf.Max(1f, Mathf.Ceil(coverScale));
		Vector2 center = GetMenuWorldCenter();
		if (_menuBackgroundSnapCenterToPixel)
			center = new Vector2(Mathf.Round(center.X), Mathf.Round(center.Y));

		_menuBackground.Centered = true;
		_menuBackground.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
		_menuBackground.Scale = new Vector2(scale, scale);
		_menuBackground.GlobalPosition = center;

		if (_menuDimmer != null)
		{
			Vector2 dimSize = coverTarget;
			_menuDimmer.Size = dimSize;
			_menuDimmer.GlobalPosition = center - (dimSize * 0.5f);
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
