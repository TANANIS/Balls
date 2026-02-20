using Godot;

public partial class GameFlowUI
{
	private void UpdateLowHealthVignette()
	{
		// Drive vignette intensity from HP ratio, then layer collapse interference.
		if (_lowHealthMaterial == null)
			return;

		float hpIntensity = 0f;
		if (_playerHealth != null && _playerHealth.MaxHp > 0)
		{
			float hpRatio = Mathf.Clamp((float)_playerHealth.Hp / _playerHealth.MaxHp, 0f, 1f);
			float raw = 1f - hpRatio;
			hpIntensity = Mathf.Clamp(Mathf.Pow(raw, LowHealthPower) * LowHealthMaxIntensity, 0f, 1f);
		}

		float phaseInterference = 0f;
		if (_stabilitySystem != null && _started && !_ending)
		{
			if (_stabilitySystem.CurrentPhase == StabilitySystem.StabilityPhase.StructuralFracture)
				phaseInterference = 0.14f;
			else if (_stabilitySystem.CurrentPhase == StabilitySystem.StabilityPhase.CollapseCritical)
			{
				float wobble = (Mathf.Sin((float)_stabilitySystem.ElapsedSeconds * 7.8f) + 1f) * 0.5f;
				phaseInterference = Mathf.Lerp(0.20f, 0.44f, wobble);
			}
		}

		float intensity = Mathf.Clamp(Mathf.Max(hpIntensity, phaseInterference), 0f, 1f);
		_lowHealthMaterial.SetShaderParameter("intensity", intensity);
	}

	private void OnScoreChanged(int score)
	{
		if (_scoreLabel != null)
			_scoreLabel.Text = $"Score: {score}";
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

		if (!IsInstanceValid(_pressureSystem))
		{
			var pressureList = GetTree().GetNodesInGroup("PressureSystem");
			if (pressureList.Count > 0)
				_pressureSystem = pressureList[0] as PressureSystem;
		}

		if (!IsInstanceValid(_pressureSystem))
		{
			_experienceBar.MaxValue = 1f;
			_experienceBar.Value = 0f;
			_experienceLabel.Text = "XP --/--";
			return;
		}

		float required = Mathf.Max(1f, _pressureSystem.GetCurrentUpgradeRequirement());
		float progress = Mathf.Clamp(_pressureSystem.CurrentUpgradeProgress, 0f, required);

		_experienceBar.MaxValue = required;
		_experienceBar.Value = progress;
		_experienceLabel.Text = _pressureSystem.IsUpgradeReady
			? $"LV {_pressureSystem.CurrentUpgradeLevel}  READY x{Mathf.Max(1, _pressureSystem.PendingUpgradeCount)}"
			: $"LV {_pressureSystem.CurrentUpgradeLevel}  XP {Mathf.FloorToInt(progress)}/{Mathf.CeilToInt(required)}";
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

	private void UpdateUniverseEventUi(double delta)
	{
		if (_eventNoticeLabel != null)
			_eventNoticeLabel.Visible = false;
		if (_eventCountdownLabel != null)
			_eventCountdownLabel.Visible = false;
		_eventNoticeTimer = 0f;
	}

	private void OnUniverseEventIncoming(float secondsLeft, StabilitySystem.UniverseEventType eventType)
	{
		int seconds = Mathf.CeilToInt(secondsLeft);
		ShowEventNotice($"{StabilitySystem.GetEventDisplayName(eventType)} incoming in {seconds}s");
	}

	private void OnUniverseEventStarted(StabilitySystem.UniverseEventType eventType, float duration)
	{
		ShowEventNotice($"Universe Event: {StabilitySystem.GetEventDisplayName(eventType)}");
	}

	private void OnUniverseEventEnded(StabilitySystem.UniverseEventType eventType)
	{
		ShowEventNotice($"{StabilitySystem.GetEventDisplayName(eventType)} ended");
	}

	private void ShowEventNotice(string message)
	{
		if (_eventNoticeLabel == null)
			return;
		if (!_started || _ending)
			return;

		_eventNoticeLabel.Text = message;
		_eventNoticeLabel.Visible = true;
		_eventNoticeTimer = 2.25f;
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
