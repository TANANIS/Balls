using Godot;
using System.Threading.Tasks;

public partial class GameFlowUI
{
	[ExportGroup("Title Branding/Prompt FX")]
	[Export] private bool EnableBootPromptIdleBlink = true;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BootPromptIdleMinAlpha = 0.48f;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BootPromptIdleMaxAlpha = 1.00f;
	[Export(PropertyHint.Range, "0.1,4,0.01")] private float BootPromptIdleCycleSeconds = 1.10f;
	[Export] private Color BootPromptConfirmGlowColor = new Color(1.0f, 0.95f, 0.72f, 1.0f);
	[Export(PropertyHint.Range, "0.05,1,0.01")] private float BootPromptConfirmDurationSeconds = 0.18f;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BootPromptProceedDelaySeconds = 0.18f;

	[ExportGroup("Title Branding/Boot Transition FX")]
	[Export] private bool EnableBootOpeningFade = true;
	[Export] private bool PlayBootOpeningFadeOnlyOnce = true;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BootOpeningFadeFromAlpha = 1.0f;
	[Export(PropertyHint.Range, "0.05,3,0.01")] private float BootOpeningFadeDurationSeconds = 3.0f;
	[Export] private bool EnableBootLetterboxCloseFx = true;
	[Export(PropertyHint.Range, "0.05,2,0.01")] private float BootLetterboxCloseDurationSeconds = 0.24f;
	[Export(PropertyHint.Range, "0.05,3,0.01")] private float BootTitleBgmFadeDurationSeconds = 0.45f;

	[ExportGroup("Title Branding/Background FX")]
	[Export] private bool EnableBootBackgroundSway = true;
	[Export(PropertyHint.Range, "1,1.2,0.01")] private float BootBackgroundSwayScale = 1.02f;
	[Export(PropertyHint.Range, "0,40,0.5")] private float BootBackgroundSwayOffsetX = 8f;
	[Export(PropertyHint.Range, "0,40,0.5")] private float BootBackgroundSwayOffsetY = 4f;
	[Export(PropertyHint.Range, "0,3,0.05")] private float BootBackgroundSwayRotationDegrees = 0.10f;
	[Export(PropertyHint.Range, "0.5,12,0.1")] private float BootBackgroundSwayCycleSeconds = 8.5f;

	private Tween _bootPromptIdleTween;
	private Tween _bootPromptConfirmTween;
	private Tween _bootOpeningMaskTween;
	private Tween _bootLetterboxCloseTween;
	private bool _hasPlayedBootOpeningFade;
	private bool _bootBackgroundBaseCached;
	private bool _bootBackgroundSwayActive;
	private Vector2 _bootBackgroundBasePosition;
	private Vector2 _bootBackgroundBaseScale = Vector2.One;
	private float _bootBackgroundBaseRotationDegrees;
	private float _bootBackgroundSwayElapsed;
	private float _bootBackgroundSwayPhaseX;
	private float _bootBackgroundSwayPhaseY;
	private float _bootBackgroundSwayPhaseRot;
	private float _bootBackgroundSwayPhaseScale;

	private void StartBootPromptIdleFx()
	{
		Label prompt = GetNodeOrNull<Label>(BootTitlePromptPath);
		if (prompt == null)
			return;

		StopBootPromptTweens();
		prompt.SelfModulate = Colors.White;
		prompt.Modulate = new Color(1f, 1f, 1f, Mathf.Clamp(BootPromptIdleMaxAlpha, 0f, 1f));
		if (!EnableBootPromptIdleBlink)
			return;

		float cycle = Mathf.Max(0.10f, BootPromptIdleCycleSeconds);
		float half = cycle * 0.5f;
		float minAlpha = Mathf.Clamp(BootPromptIdleMinAlpha, 0f, 1f);
		float maxAlpha = Mathf.Clamp(BootPromptIdleMaxAlpha, 0f, 1f);
		_bootPromptIdleTween = CreateTween();
		_bootPromptIdleTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_bootPromptIdleTween.SetLoops();
		_bootPromptIdleTween.TweenProperty(prompt, "modulate:a", minAlpha, half).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		_bootPromptIdleTween.TweenProperty(prompt, "modulate:a", maxAlpha, half).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
	}

	private async Task PlayBootPromptConfirmFxAsync()
	{
		Label prompt = GetNodeOrNull<Label>(BootTitlePromptPath);
		if (prompt == null)
			return;

		if (_bootPromptIdleTween != null && _bootPromptIdleTween.IsValid())
			_bootPromptIdleTween.Kill();
		_bootPromptIdleTween = null;
		if (_bootPromptConfirmTween != null && _bootPromptConfirmTween.IsValid())
			_bootPromptConfirmTween.Kill();

		prompt.Modulate = Colors.White;
		prompt.SelfModulate = Colors.White;

		float confirmDuration = Mathf.Max(0.05f, BootPromptConfirmDurationSeconds);
		_bootPromptConfirmTween = CreateTween();
		_bootPromptConfirmTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_bootPromptConfirmTween.TweenProperty(prompt, "self_modulate", BootPromptConfirmGlowColor, confirmDuration * 0.35f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_bootPromptConfirmTween.Parallel().TweenProperty(prompt, "modulate:a", 1.0f, confirmDuration * 0.35f);
		_bootPromptConfirmTween.TweenProperty(prompt, "self_modulate", Colors.White, confirmDuration * 0.65f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
		await ToSignal(_bootPromptConfirmTween, Tween.SignalName.Finished);
		_bootPromptConfirmTween = null;

		float delay = Mathf.Max(0f, BootPromptProceedDelaySeconds);
		if (delay > 0f)
		{
			SceneTreeTimer timer = GetTree().CreateTimer(delay, processAlways: true, processInPhysics: false, ignoreTimeScale: true);
			await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
		}
	}

	private void StartBootOpeningMaskFadeIfNeeded()
	{
		ColorRect openingMask = GetNodeOrNull<ColorRect>(BootOpeningMaskPath);
		if (openingMask == null)
			return;

		bool shouldFade = EnableBootOpeningFade && (!PlayBootOpeningFadeOnlyOnce || !_hasPlayedBootOpeningFade);
		if (!shouldFade)
		{
			if (_bootOpeningMaskTween != null && _bootOpeningMaskTween.IsValid())
				_bootOpeningMaskTween.Kill();
			_bootOpeningMaskTween = null;
			openingMask.Visible = false;
			openingMask.Color = Colors.Transparent;
			return;
		}

		if (_bootOpeningMaskTween != null && _bootOpeningMaskTween.IsValid())
			_bootOpeningMaskTween.Kill();

		openingMask.Visible = true;
		openingMask.MouseFilter = Control.MouseFilterEnum.Ignore;
		openingMask.Color = new Color(0f, 0f, 0f, Mathf.Clamp(BootOpeningFadeFromAlpha, 0f, 1f));

		float duration = Mathf.Max(0.05f, BootOpeningFadeDurationSeconds);
		_bootOpeningMaskTween = CreateTween();
		_bootOpeningMaskTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_bootOpeningMaskTween.TweenProperty(openingMask, "color:a", 0f, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_bootOpeningMaskTween.TweenCallback(Callable.From(() =>
		{
			openingMask.Visible = false;
			_bootOpeningMaskTween = null;
			_hasPlayedBootOpeningFade = true;
		}));
	}

	private void StartBootBackgroundSwayFx()
	{
		TextureRect background = GetNodeOrNull<TextureRect>(BootBackgroundPath);
		if (background == null)
			return;

		CacheBootBackgroundBase(background);
		StopBootBackgroundSwayFx(resetVisual: true);
		if (!EnableBootBackgroundSway)
			return;

		background.PivotOffset = background.Size * 0.5f;
		_bootBackgroundSwayActive = true;
		_bootBackgroundSwayElapsed = 0f;
		_bootBackgroundSwayPhaseX = GD.Randf() * Mathf.Tau;
		_bootBackgroundSwayPhaseY = GD.Randf() * Mathf.Tau;
		_bootBackgroundSwayPhaseRot = GD.Randf() * Mathf.Tau;
		_bootBackgroundSwayPhaseScale = GD.Randf() * Mathf.Tau;
		UpdateBootBackgroundSwayFx(0f);
	}

	private void UpdateBootBackgroundSwayFx(float deltaSeconds)
	{
		if (!_bootBackgroundSwayActive)
			return;

		TextureRect background = GetNodeOrNull<TextureRect>(BootBackgroundPath);
		if (background == null)
			return;

		float cycle = Mathf.Max(0.5f, BootBackgroundSwayCycleSeconds);
		_bootBackgroundSwayElapsed += Mathf.Max(0f, deltaSeconds);
		float t = _bootBackgroundSwayElapsed / cycle;
		float main = t * Mathf.Tau;

		float swayXPrimary = Mathf.Sin(main + _bootBackgroundSwayPhaseX);
		float swayXSecondary = Mathf.Sin(main * 0.53f + _bootBackgroundSwayPhaseX * 0.37f);
		float swayYPrimary = Mathf.Sin(main * 0.79f + _bootBackgroundSwayPhaseY);
		float swayYSecondary = Mathf.Sin(main * 1.31f + _bootBackgroundSwayPhaseY * 0.41f);
		float offsetX = BootBackgroundSwayOffsetX * (swayXPrimary * 0.65f + swayXSecondary * 0.35f);
		float offsetY = BootBackgroundSwayOffsetY * (swayYPrimary * 0.65f + swayYSecondary * 0.35f);

		float rotPrimary = Mathf.Sin(main * 0.47f + _bootBackgroundSwayPhaseRot);
		float rotSecondary = Mathf.Sin(main * 1.07f + _bootBackgroundSwayPhaseRot * 0.51f);
		float rotation = BootBackgroundSwayRotationDegrees * (rotPrimary * 0.7f + rotSecondary * 0.3f);

		float scalePulse = Mathf.Sin(main * 0.29f + _bootBackgroundSwayPhaseScale) * 0.5f + 0.5f;
		float targetScale = Mathf.Max(1f, BootBackgroundSwayScale);
		float scale = Mathf.Lerp(1f, targetScale, scalePulse);

		background.Position = _bootBackgroundBasePosition + new Vector2(offsetX, offsetY);
		background.RotationDegrees = _bootBackgroundBaseRotationDegrees + rotation;
		background.Scale = _bootBackgroundBaseScale * new Vector2(scale, scale);
	}

	private async Task PlayBootLetterboxCloseFxAsync()
	{
		ColorRect topBar = GetNodeOrNull<ColorRect>(BootTopLetterboxPath);
		ColorRect bottomBar = GetNodeOrNull<ColorRect>(BootBottomLetterboxPath);
		if (!EnableBootLetterboxCloseFx || !EnableBootLetterbox || topBar == null || bottomBar == null)
			return;

		if (_bootLetterboxCloseTween != null && _bootLetterboxCloseTween.IsValid())
			_bootLetterboxCloseTween.Kill();

		ApplyBootLetterboxOverride();
		topBar.Visible = true;
		bottomBar.Visible = true;

		float targetHalfHeight = Mathf.Max(BootLetterboxHeight, GetViewportRect().Size.Y * 0.5f);
		float duration = Mathf.Max(0.05f, BootLetterboxCloseDurationSeconds);
		_bootLetterboxCloseTween = CreateTween();
		_bootLetterboxCloseTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_bootLetterboxCloseTween.SetTrans(Tween.TransitionType.Sine);
		_bootLetterboxCloseTween.SetEase(Tween.EaseType.InOut);
		_bootLetterboxCloseTween.Parallel().TweenProperty(topBar, "offset_bottom", targetHalfHeight, duration);
		_bootLetterboxCloseTween.Parallel().TweenProperty(bottomBar, "offset_top", -targetHalfHeight, duration);
		await ToSignal(_bootLetterboxCloseTween, Tween.SignalName.Finished);
		_bootLetterboxCloseTween = null;
	}

	private void StopBootBackgroundSwayFx(bool resetVisual)
	{
		_bootBackgroundSwayActive = false;

		if (!resetVisual)
			return;

		TextureRect background = GetNodeOrNull<TextureRect>(BootBackgroundPath);
		if (background == null)
			return;

		CacheBootBackgroundBase(background);
		background.Position = _bootBackgroundBasePosition;
		background.Scale = _bootBackgroundBaseScale;
		background.RotationDegrees = _bootBackgroundBaseRotationDegrees;
	}

	private void StopBootPromptFx(bool resetVisual)
	{
		StopBootPromptTweens();
		StopBootTransitionTweens();
		StopBootBackgroundSwayFx(resetVisual);
		if (!resetVisual)
			return;

		Label prompt = GetNodeOrNull<Label>(BootTitlePromptPath);
		if (prompt == null)
			return;

		prompt.Modulate = Colors.White;
		prompt.SelfModulate = Colors.White;
	}

	private void StopBootPromptTweens()
	{
		if (_bootPromptIdleTween != null && _bootPromptIdleTween.IsValid())
			_bootPromptIdleTween.Kill();
		_bootPromptIdleTween = null;

		if (_bootPromptConfirmTween != null && _bootPromptConfirmTween.IsValid())
			_bootPromptConfirmTween.Kill();
		_bootPromptConfirmTween = null;
	}

	private void StopBootTransitionTweens()
	{
		if (_bootOpeningMaskTween != null && _bootOpeningMaskTween.IsValid())
			_bootOpeningMaskTween.Kill();
		_bootOpeningMaskTween = null;

		if (_bootLetterboxCloseTween != null && _bootLetterboxCloseTween.IsValid())
			_bootLetterboxCloseTween.Kill();
		_bootLetterboxCloseTween = null;
	}

	private void CacheBootBackgroundBase(TextureRect background)
	{
		if (background == null || _bootBackgroundBaseCached)
			return;

		_bootBackgroundBasePosition = background.Position;
		_bootBackgroundBaseScale = background.Scale;
		_bootBackgroundBaseRotationDegrees = background.RotationDegrees;
		_bootBackgroundBaseCached = true;
	}
}
