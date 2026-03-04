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
	[Export] private bool EnableBootBackgroundScrollToMainFx = true;
	[Export(PropertyHint.Range, "0.05,3,0.01")] private float BootBackgroundScrollDurationSeconds = 1.45f;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BootBackgroundScrollDelaySeconds = 0.04f;
	[Export] private bool StopBootBackgroundSwayBeforeScroll = true;
	[Export] private bool EnableBootDimmerFadeToMainFx = true;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BootDimmerMainTargetAlpha = 0f;
	[Export(PropertyHint.Range, "0.05,3,0.01")] private float BootTitleBgmFadeDurationSeconds = 0.45f;
	[Export] private bool EnableBootTitleContentExitFx = true;
	[Export(PropertyHint.Range, "0.05,1.5,0.01")] private float BootTitleContentExitDurationSeconds = 0.22f;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BootTitleContentExitDelaySeconds = 0.00f;
	[Export(PropertyHint.Range, "0.8,1.0,0.005")] private float BootTitleContentExitTargetScale = 0.96f;

	[ExportGroup("Title Branding/Background FX")]
	[Export] private bool EnableBootBackgroundSway = true;
	[Export] private bool UseBootBackgroundHalfSlice = true;
	[Export] private bool BootBackgroundUseNearestFilter = true;
	[Export] private bool BootBackgroundQuantizeSliceToTexel = true;
	[Export(PropertyHint.Range, "0,1,0.001")] private float BootBackgroundTitleSliceT = 0f;
	[Export(PropertyHint.Range, "0,1,0.001")] private float BootBackgroundMainSliceT = 1f;
	[Export(PropertyHint.Range, "1,1.2,0.01")] private float BootBackgroundSwayScale = 1.02f;
	[Export(PropertyHint.Range, "0,40,0.5")] private float BootBackgroundSwayOffsetX = 8f;
	[Export(PropertyHint.Range, "0,40,0.5")] private float BootBackgroundSwayOffsetY = 4f;
	[Export(PropertyHint.Range, "0,3,0.05")] private float BootBackgroundSwayRotationDegrees = 0.10f;
	[Export(PropertyHint.Range, "0.5,12,0.1")] private float BootBackgroundSwayCycleSeconds = 8.5f;

	private Tween _bootPromptIdleTween;
	private Tween _bootPromptConfirmTween;
	private Tween _bootOpeningMaskTween;
	private Tween _bootLetterboxCloseTween;
	private Tween _bootBackgroundScrollTween;
	private Tween _bootDimmerFadeTween;
	private Tween _bootTitleContentExitTween;
	private bool _hasPlayedBootOpeningFade;
	private bool _bootBackgroundBaseCached;
	private bool _bootBackgroundSwayActive;
	private float _bootBackgroundSliceT;
	private Texture2D _bootBackgroundSourceTexture;
	private AtlasTexture _bootBackgroundSliceTexture;
	private Vector2 _bootBackgroundBasePosition;
	private Vector2 _bootBackgroundBaseScale = Vector2.One;
	private float _bootBackgroundBaseRotationDegrees;
	private float _bootBackgroundSwayElapsed;
	private float _bootBackgroundSwayPhaseX;
	private float _bootBackgroundSwayPhaseY;
	private float _bootBackgroundSwayPhaseRot;
	private float _bootBackgroundSwayPhaseScale;
	private bool _bootTitleContentBaseCached;
	private Vector2 _bootTitleContentBaseScale = Vector2.One;
	private Color _bootTitleContentBaseModulate = Colors.White;
	private bool _bootDimmerBaseColorCached;
	private Color _bootDimmerBaseColor = new Color(0.06f, 0.05f, 0.05f, 0.58f);

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

	private async Task PlayBootTitleUiExitFxAsync()
	{
		await PlayBootPromptConfirmFxAsync();
		await PlayBootTitleContentExitFxAsync();
	}

	private async Task PlayBootTitleContentExitFxAsync()
	{
		Control content = GetNodeOrNull<Control>(BootTitleContentPath);
		if (content == null)
			return;

		CacheBootTitleContentBase(content);
		if (_bootTitleContentExitTween != null && _bootTitleContentExitTween.IsValid())
			_bootTitleContentExitTween.Kill();

		if (!EnableBootTitleContentExitFx)
			return;

		content.Visible = true;
		content.Modulate = _bootTitleContentBaseModulate;
		content.Scale = _bootTitleContentBaseScale;
		content.PivotOffset = content.Size * 0.5f;

		float delay = Mathf.Max(0f, BootTitleContentExitDelaySeconds);
		float duration = Mathf.Max(0.05f, BootTitleContentExitDurationSeconds);
		float targetScale = Mathf.Clamp(BootTitleContentExitTargetScale, 0.8f, 1f);

		_bootTitleContentExitTween = CreateTween();
		_bootTitleContentExitTween.SetPauseMode(Tween.TweenPauseMode.Process);
		if (delay > 0f)
			_bootTitleContentExitTween.TweenInterval(delay);
		_bootTitleContentExitTween.TweenProperty(content, "modulate:a", 0f, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_bootTitleContentExitTween.Parallel().TweenProperty(content, "scale", _bootTitleContentBaseScale * targetScale, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		await ToSignal(_bootTitleContentExitTween, Tween.SignalName.Finished);
		_bootTitleContentExitTween = null;
	}

	private void ResetBootTitleContentExitFxVisual()
	{
		Control content = GetNodeOrNull<Control>(BootTitleContentPath);
		if (content == null)
			return;

		CacheBootTitleContentBase(content);
		content.Modulate = _bootTitleContentBaseModulate;
		content.Scale = _bootTitleContentBaseScale;
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

	private void ResetBootDimmerForTitle()
	{
		ColorRect dimmer = GetNodeOrNull<ColorRect>(BootDimmerPath);
		if (dimmer == null)
			return;

		if (_bootDimmerFadeTween != null && _bootDimmerFadeTween.IsValid())
			_bootDimmerFadeTween.Kill();
		_bootDimmerFadeTween = null;

		CacheBootDimmerBaseColor(dimmer);
		dimmer.Color = _bootDimmerBaseColor;
		dimmer.Visible = _bootDimmerBaseColor.A > 0.001f;
	}

	private async Task PlayBootDimmerFadeToMainAsync(float delaySeconds, float durationSeconds, bool animate)
	{
		ColorRect dimmer = GetNodeOrNull<ColorRect>(BootDimmerPath);
		if (dimmer == null)
			return;

		CacheBootDimmerBaseColor(dimmer);
		if (_bootDimmerFadeTween != null && _bootDimmerFadeTween.IsValid())
			_bootDimmerFadeTween.Kill();
		_bootDimmerFadeTween = null;

		float targetAlpha = Mathf.Clamp(BootDimmerMainTargetAlpha, 0f, 1f);
		if (!EnableBootDimmerFadeToMainFx || !animate || !IsInsideTree())
		{
			Color immediate = _bootDimmerBaseColor;
			immediate.A = targetAlpha;
			dimmer.Color = immediate;
			dimmer.Visible = targetAlpha > 0.001f;
			return;
		}

		Color start = dimmer.Color;
		if (!dimmer.Visible || start.A <= 0.001f)
			start = _bootDimmerBaseColor;

		dimmer.Color = start;
		dimmer.Visible = true;
		Color end = _bootDimmerBaseColor;
		end.A = targetAlpha;

		float delay = Mathf.Max(0f, delaySeconds);
		float duration = Mathf.Max(0.05f, durationSeconds);
		_bootDimmerFadeTween = CreateTween();
		_bootDimmerFadeTween.SetPauseMode(Tween.TweenPauseMode.Process);
		if (delay > 0f)
			_bootDimmerFadeTween.TweenInterval(delay);
		_bootDimmerFadeTween.TweenProperty(dimmer, "color", end, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		await ToSignal(_bootDimmerFadeTween, Tween.SignalName.Finished);
		_bootDimmerFadeTween = null;

		if (dimmer != null && targetAlpha <= 0.001f)
			dimmer.Visible = false;
	}

	private void StartBootBackgroundSwayFx()
	{
		TextureRect background = GetNodeOrNull<TextureRect>(BootBackgroundPath);
		if (background == null)
			return;

		ConfigureBootBackgroundSampling(background);
		CacheBootBackgroundBase(background);
		StopBootBackgroundSwayFx(resetVisual: true);
		ApplyBootBackgroundHalfSlice(BootBackgroundTitleSliceT);
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

	private void ApplyBootBackgroundHalfSlice(float sliceT)
	{
		TextureRect background = GetNodeOrNull<TextureRect>(BootBackgroundPath);
		if (background == null)
			return;

		ConfigureBootBackgroundSampling(background);
		Texture2D sourceTexture = ResolveBootBackgroundSourceTexture(background);
		if (sourceTexture == null)
			return;

		float t = QuantizeBootBackgroundSliceT(sliceT, sourceTexture);
		_bootBackgroundSliceT = t;
		if (!UseBootBackgroundHalfSlice)
		{
			background.Texture = sourceTexture;
			background.Material = null;
			return;
		}

		_bootBackgroundSliceTexture ??= new AtlasTexture();
		_bootBackgroundSliceTexture.Atlas = sourceTexture;
		_bootBackgroundSliceTexture.Region = BuildVerticalHalfSliceRegion(sourceTexture.GetSize(), t);
		background.Texture = _bootBackgroundSliceTexture;
		background.Material = null;
	}

	private void SetBootBackgroundSliceT(float value)
	{
		ApplyBootBackgroundHalfSlice(value);
	}

	private void ConfigureBootBackgroundSampling(TextureRect background)
	{
		if (background == null)
			return;

		background.TextureFilter = BootBackgroundUseNearestFilter
			? CanvasItem.TextureFilterEnum.Nearest
			: CanvasItem.TextureFilterEnum.Linear;
	}

	private float QuantizeBootBackgroundSliceT(float rawT, Texture2D sourceTexture)
	{
		float t = Mathf.Clamp(rawT, 0f, 1f);
		if (!BootBackgroundQuantizeSliceToTexel)
			return t;
		if (sourceTexture == null)
			return t;

		float texHeight = Mathf.Max(1f, sourceTexture.GetSize().Y);
		float step = 2f / texHeight;
		if (step <= 0f)
			return t;

		float snapped = Mathf.Round(t / step) * step;
		return Mathf.Clamp(snapped, 0f, 1f);
	}

	private Texture2D ResolveBootBackgroundSourceTexture(TextureRect background)
	{
		if (background == null)
			return null;

		if (_bootBackgroundSourceTexture != null)
			return _bootBackgroundSourceTexture;

		if (background.Texture is AtlasTexture atlasTexture && atlasTexture.Atlas != null)
		{
			_bootBackgroundSourceTexture = atlasTexture.Atlas;
			return _bootBackgroundSourceTexture;
		}

		_bootBackgroundSourceTexture = background.Texture;
		return _bootBackgroundSourceTexture;
	}

	private static Rect2 BuildVerticalHalfSliceRegion(Vector2 textureSize, float sliceT)
	{
		float width = Mathf.Max(1f, textureSize.X);
		float fullHeight = Mathf.Max(1f, textureSize.Y);
		float halfHeight = Mathf.Max(1f, fullHeight * 0.5f);
		float y = Mathf.Clamp(sliceT, 0f, 1f) * halfHeight;
		y = Mathf.Clamp(y, 0f, Mathf.Max(0f, fullHeight - halfHeight));
		return new Rect2(0f, y, width, halfHeight);
	}

	private async Task PlayBootBackgroundScrollToMainAsync()
	{
		TextureRect background = GetNodeOrNull<TextureRect>(BootBackgroundPath);
		if (background == null)
		{
			await PlayBootDimmerFadeToMainAsync(0f, 0f, animate: false);
			return;
		}

		if (StopBootBackgroundSwayBeforeScroll)
			StopBootBackgroundSwayFx(resetVisual: true);

		float target = Mathf.Clamp(BootBackgroundMainSliceT, 0f, 1f);
		float delay = Mathf.Max(0f, BootBackgroundScrollDelaySeconds);
		float duration = Mathf.Max(0.05f, BootBackgroundScrollDurationSeconds);
		bool animateBackgroundScroll = EnableBootBackgroundScrollToMainFx && UseBootBackgroundHalfSlice;
		Task dimmerFadeTask = PlayBootDimmerFadeToMainAsync(
			delaySeconds: animateBackgroundScroll ? delay : 0f,
			durationSeconds: duration,
			animate: animateBackgroundScroll);
		if (!animateBackgroundScroll)
		{
			ApplyBootBackgroundHalfSlice(target);
			await dimmerFadeTask;
			return;
		}

		if (_bootBackgroundScrollTween != null && _bootBackgroundScrollTween.IsValid())
			_bootBackgroundScrollTween.Kill();

		ApplyBootBackgroundHalfSlice(_bootBackgroundSliceT);
		_bootBackgroundScrollTween = CreateTween();
		_bootBackgroundScrollTween.SetPauseMode(Tween.TweenPauseMode.Process);
		if (delay > 0f)
			_bootBackgroundScrollTween.TweenInterval(delay);
		_bootBackgroundScrollTween.TweenMethod(Callable.From<float>(SetBootBackgroundSliceT), _bootBackgroundSliceT, target, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
		await ToSignal(_bootBackgroundScrollTween, Tween.SignalName.Finished);
		_bootBackgroundScrollTween = null;
		await dimmerFadeTask;
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

		float duration = Mathf.Max(0.05f, BootLetterboxCloseDurationSeconds);
		_bootLetterboxCloseTween = CreateTween();
		_bootLetterboxCloseTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_bootLetterboxCloseTween.SetTrans(Tween.TransitionType.Sine);
		_bootLetterboxCloseTween.SetEase(Tween.EaseType.InOut);
		_bootLetterboxCloseTween.Parallel().TweenProperty(topBar, "offset_bottom", 0f, duration);
		_bootLetterboxCloseTween.Parallel().TweenProperty(bottomBar, "offset_top", 0f, duration);
		await ToSignal(_bootLetterboxCloseTween, Tween.SignalName.Finished);
		topBar.Visible = false;
		bottomBar.Visible = false;
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
		ResetBootTitleContentExitFxVisual();
	}

	private void StopBootPromptTweens()
	{
		if (_bootPromptIdleTween != null && _bootPromptIdleTween.IsValid())
			_bootPromptIdleTween.Kill();
		_bootPromptIdleTween = null;

		if (_bootPromptConfirmTween != null && _bootPromptConfirmTween.IsValid())
			_bootPromptConfirmTween.Kill();
		_bootPromptConfirmTween = null;

		if (_bootTitleContentExitTween != null && _bootTitleContentExitTween.IsValid())
			_bootTitleContentExitTween.Kill();
		_bootTitleContentExitTween = null;
	}

	private void StopBootTransitionTweens()
	{
		if (_bootOpeningMaskTween != null && _bootOpeningMaskTween.IsValid())
			_bootOpeningMaskTween.Kill();
		_bootOpeningMaskTween = null;

		if (_bootLetterboxCloseTween != null && _bootLetterboxCloseTween.IsValid())
			_bootLetterboxCloseTween.Kill();
		_bootLetterboxCloseTween = null;

		if (_bootBackgroundScrollTween != null && _bootBackgroundScrollTween.IsValid())
			_bootBackgroundScrollTween.Kill();
		_bootBackgroundScrollTween = null;

		if (_bootDimmerFadeTween != null && _bootDimmerFadeTween.IsValid())
			_bootDimmerFadeTween.Kill();
		_bootDimmerFadeTween = null;
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

	private void CacheBootTitleContentBase(Control content)
	{
		if (content == null || _bootTitleContentBaseCached)
			return;

		_bootTitleContentBaseScale = content.Scale;
		_bootTitleContentBaseModulate = content.Modulate;
		_bootTitleContentBaseCached = true;
	}

	private void CacheBootDimmerBaseColor(ColorRect dimmer)
	{
		if (dimmer == null || _bootDimmerBaseColorCached)
			return;

		_bootDimmerBaseColor = dimmer.Color;
		_bootDimmerBaseColorCached = true;
	}
}
