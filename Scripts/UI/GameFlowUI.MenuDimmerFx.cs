using Godot;

public partial class GameFlowUI
{
	[ExportGroup("Start Menu / Background Dim FX")]
	[Export] private bool EnableStartSubpanelDimmerFx = true;
	[Export(PropertyHint.Range, "0,1,0.01")] private float StartSubpanelDimmerAlpha = 0.28f;
	[Export(PropertyHint.Range, "0.05,1.5,0.01")] private float StartSubpanelDimmerTweenSeconds = 0.22f;

	private Tween _startSubpanelDimmerTween;
	private Color _menuDimmerBaseColor = new Color(0f, 0f, 0f, 1f);
	private bool _menuDimmerBaseColorCached;

	private void InitializeStartSubpanelDimmerFx()
	{
		CacheMenuDimmerBaseColor();
		ApplyStartSubpanelDimmerAlpha(0f, visible: false);
	}

	private void PlayStartSubpanelDimmerFx(bool expand, bool animate)
	{
		if (_menuDimmer == null)
			return;

		CacheMenuDimmerBaseColor();
		float targetAlpha = expand ? Mathf.Clamp(StartSubpanelDimmerAlpha, 0f, 1f) : 0f;

		if (_startSubpanelDimmerTween != null && _startSubpanelDimmerTween.IsValid())
			_startSubpanelDimmerTween.Kill();
		_startSubpanelDimmerTween = null;

		if (!EnableStartSubpanelDimmerFx || !animate || !IsInsideTree())
		{
			ApplyStartSubpanelDimmerAlpha(targetAlpha, visible: targetAlpha > 0.001f);
			return;
		}

		Color color = _menuDimmer.Color;
		float currentAlpha = Mathf.Clamp(color.A, 0f, 1f);
		if (Mathf.IsEqualApprox(currentAlpha, targetAlpha))
		{
			_menuDimmer.Visible = targetAlpha > 0.001f;
			return;
		}

		if (targetAlpha > 0.001f)
			_menuDimmer.Visible = true;

		_startSubpanelDimmerTween = CreateTween();
		_startSubpanelDimmerTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_startSubpanelDimmerTween.TweenProperty(
			_menuDimmer,
			"color:a",
			targetAlpha,
			Mathf.Max(0.05f, StartSubpanelDimmerTweenSeconds))
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(targetAlpha > currentAlpha ? Tween.EaseType.Out : Tween.EaseType.InOut);
		_startSubpanelDimmerTween.TweenCallback(Callable.From(() =>
		{
			_startSubpanelDimmerTween = null;
			if (targetAlpha <= 0.001f && _menuDimmer != null)
				_menuDimmer.Visible = false;
		}));
	}

	private void StopStartSubpanelDimmerFx()
	{
		if (_startSubpanelDimmerTween != null && _startSubpanelDimmerTween.IsValid())
			_startSubpanelDimmerTween.Kill();
		_startSubpanelDimmerTween = null;
		ApplyStartSubpanelDimmerAlpha(0f, visible: false);
	}

	private void CacheMenuDimmerBaseColor()
	{
		if (_menuDimmer == null || _menuDimmerBaseColorCached)
			return;

		Color color = _menuDimmer.Color;
		color.A = 1f;
		_menuDimmerBaseColor = color;
		_menuDimmerBaseColorCached = true;
	}

	private void ApplyStartSubpanelDimmerAlpha(float alpha, bool visible)
	{
		if (_menuDimmer == null)
			return;

		Color color = _menuDimmerBaseColorCached ? _menuDimmerBaseColor : _menuDimmer.Color;
		color.A = Mathf.Clamp(alpha, 0f, 1f);
		_menuDimmer.Color = color;
		_menuDimmer.Visible = visible;
	}
}
