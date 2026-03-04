using Godot;

public partial class GameFlowUI
{
	[ExportGroup("Start Settings FX")]
	[Export] private bool EnableStartSettingsLetterboxFx = true;
	[Export(PropertyHint.Range, "0.05,0.4,0.005")] private float StartSettingsLetterboxHeightRatio = 0.14f;
	[Export(PropertyHint.Range, "0.05,1.0,0.01")] private float StartSettingsLetterboxTweenSeconds = 0.24f;
	[Export(PropertyHint.Range, "0.4,1.0,0.01")] private float StartSettingsLetterboxMaxAlpha = 1.0f;

	private Tween _startSettingsLetterboxTween;
	private float _startSettingsLetterboxT;

	private void InitializeStartSettingsLetterboxFx()
	{
		if (_startSettingsTopLetterbox != null)
			_startSettingsTopLetterbox.MouseFilter = Control.MouseFilterEnum.Ignore;
		if (_startSettingsBottomLetterbox != null)
			_startSettingsBottomLetterbox.MouseFilter = Control.MouseFilterEnum.Ignore;
		ApplyStartSettingsLetterbox(0f);
	}

	private void PlayStartSettingsLetterboxFx(bool expand, bool animate)
	{
		if (!EnableStartSettingsLetterboxFx || _startSettingsTopLetterbox == null || _startSettingsBottomLetterbox == null)
		{
			ApplyStartSettingsLetterbox(expand ? 1f : 0f);
			return;
		}

		float target = expand ? 1f : 0f;
		if (Mathf.IsEqualApprox(_startSettingsLetterboxT, target))
			return;

		if (_startSettingsLetterboxTween != null && _startSettingsLetterboxTween.IsValid())
			_startSettingsLetterboxTween.Kill();

		if (!animate || !IsInsideTree())
		{
			ApplyStartSettingsLetterbox(target);
			return;
		}

		_startSettingsLetterboxTween = CreateTween();
		_startSettingsLetterboxTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_startSettingsLetterboxTween.TweenMethod(
			Callable.From<float>(ApplyStartSettingsLetterbox),
			_startSettingsLetterboxT,
			target,
			Mathf.Max(0.05f, StartSettingsLetterboxTweenSeconds))
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(expand ? Tween.EaseType.Out : Tween.EaseType.InOut);
		_startSettingsLetterboxTween.TweenCallback(Callable.From(() => _startSettingsLetterboxTween = null));
	}

	private void RefreshStartSettingsLetterboxLayout()
	{
		ApplyStartSettingsLetterbox(_startSettingsLetterboxT);
	}

	private void ApplyStartSettingsLetterbox(float t)
	{
		_startSettingsLetterboxT = Mathf.Clamp(t, 0f, 1f);
		if (_startSettingsTopLetterbox == null || _startSettingsBottomLetterbox == null)
			return;

		float viewportHeight = Mathf.Max(1f, GetViewport().GetVisibleRect().Size.Y);
		float barHeight = viewportHeight * Mathf.Clamp(StartSettingsLetterboxHeightRatio, 0.05f, 0.4f) * _startSettingsLetterboxT;
		float alpha = Mathf.Clamp(StartSettingsLetterboxMaxAlpha * _startSettingsLetterboxT, 0f, 1f);

		_startSettingsTopLetterbox.AnchorLeft = 0f;
		_startSettingsTopLetterbox.AnchorTop = 0f;
		_startSettingsTopLetterbox.AnchorRight = 1f;
		_startSettingsTopLetterbox.AnchorBottom = 0f;
		_startSettingsTopLetterbox.OffsetLeft = 0f;
		_startSettingsTopLetterbox.OffsetTop = 0f;
		_startSettingsTopLetterbox.OffsetRight = 0f;
		_startSettingsTopLetterbox.OffsetBottom = barHeight;

		_startSettingsBottomLetterbox.AnchorLeft = 0f;
		_startSettingsBottomLetterbox.AnchorTop = 1f;
		_startSettingsBottomLetterbox.AnchorRight = 1f;
		_startSettingsBottomLetterbox.AnchorBottom = 1f;
		_startSettingsBottomLetterbox.OffsetLeft = 0f;
		_startSettingsBottomLetterbox.OffsetTop = -barHeight;
		_startSettingsBottomLetterbox.OffsetRight = 0f;
		_startSettingsBottomLetterbox.OffsetBottom = 0f;

		Color topColor = _startSettingsTopLetterbox.Color;
		topColor.A = alpha;
		_startSettingsTopLetterbox.Color = topColor;

		Color bottomColor = _startSettingsBottomLetterbox.Color;
		bottomColor.A = alpha;
		_startSettingsBottomLetterbox.Color = bottomColor;

		bool visible = _startSettingsLetterboxT > 0.001f;
		_startSettingsTopLetterbox.Visible = visible;
		_startSettingsBottomLetterbox.Visible = visible;
	}
}
