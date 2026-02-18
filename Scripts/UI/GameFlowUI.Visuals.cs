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

	private void UpdateUniverseEventUi(double delta)
	{
		if (_eventNoticeTimer > 0f)
		{
			_eventNoticeTimer = Mathf.Max(0f, _eventNoticeTimer - (float)delta);
			if (_eventNoticeTimer <= 0f && _eventNoticeLabel != null)
				_eventNoticeLabel.Visible = false;
		}

		if (_eventCountdownLabel == null)
			return;

		if (!_started || _ending || _stabilitySystem == null)
		{
			_eventCountdownLabel.Visible = false;
			return;
		}

		_eventCountdownLabel.Visible = true;
		if (_stabilitySystem.IsUniverseEventActive)
		{
			string name = StabilitySystem.GetEventDisplayName(_stabilitySystem.ActiveEvent);
			int remain = Mathf.CeilToInt(_stabilitySystem.ActiveEventRemainingSeconds);
			_eventCountdownLabel.Text = $"Event: {name} ({remain}s)";
		}
		else
		{
			int remain = Mathf.CeilToInt(_stabilitySystem.SecondsUntilNextEvent);
			_eventCountdownLabel.Text = $"Next Event In: {remain}s";
		}
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
