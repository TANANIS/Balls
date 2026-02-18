using Godot;

public partial class GameFlowUI
{
	private void UpdateLowHealthVignette()
	{
		// Drive vignette intensity from HP ratio every frame.
		if (_lowHealthMaterial == null || _playerHealth == null || _playerHealth.MaxHp <= 0)
			return;

		float hpRatio = Mathf.Clamp((float)_playerHealth.Hp / _playerHealth.MaxHp, 0f, 1f);
		float raw = 1f - hpRatio;
		float intensity = Mathf.Clamp(Mathf.Pow(raw, LowHealthPower) * LowHealthMaxIntensity, 0f, 1f);
		_lowHealthMaterial.SetShaderParameter("intensity", intensity);
	}

	private void OnScoreChanged(int score)
	{
		if (_scoreLabel != null)
			_scoreLabel.Text = $"Score: {score}";
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
