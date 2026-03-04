using Godot;
using System;
using System.Collections.Generic;

public partial class StartMainPageController : Control
{
	[ExportGroup("Node Paths")]
	[Export] private NodePath StartButtonPath = "VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/StartButton";
	[Export] private NodePath CardsButtonPath = "VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/CardsButton";
	[Export] private NodePath LeaderboardButtonPath = "VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/LeaderboardButton";
	[Export] private NodePath SettingsButtonPath = "VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/SettingsButton";
	[Export] private NodePath QuitButtonPath = "VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox/QuitButton";
	[Export] private NodePath PerfectLeaderboardPath = "VBox/MainBody/LeftColumn/PerfectLeaderboard";
	[Export] private NodePath SummonBoardPath = "VBox/MainBody/LeftColumn/LeftMargin/BoardLayer/SummonBoard";
	[Export] private NodePath SummonBoardShadowPath = "VBox/MainBody/LeftColumn/LeftMargin/BoardLayer/SummonBoardShadow";
	[Export] private NodePath MainBackgroundPath = "Background";
	[Export] private NodePath MainEnterRevealMaskPath = "EnterRevealMask";
	[Export] private NodePath SummonBoardLayerPath = "VBox/MainBody/LeftColumn/LeftMargin/BoardLayer";
	[Export] private NodePath ButtonsVBoxPath = "VBox/MainBody/RightColumnPanel/Margin/ButtonsVBox";
	[Export] private NodePath ButtonHoverIndicatorPath = "VBox/MainBody/RightColumnPanel/ButtonHoverIndicator";
	[Export] private NodePath LeaderboardPopupPath = "VBox/MainBody/LeftColumn/LeftMargin/LeaderboardDrawer";
	[Export] private NodePath LeaderboardPopupTitlePath = "VBox/MainBody/LeftColumn/LeftMargin/LeaderboardDrawer/Margin/VBox/Title";
	[Export] private NodePath LeaderboardPopupContentPath = "VBox/MainBody/LeftColumn/LeftMargin/LeaderboardDrawer/Margin/VBox/Body/Content";

	[ExportGroup("Main Background FX")]
	[Export] private bool EnableMainBackgroundSway = false;
	[Export] private bool UseMainBackgroundHalfSlice = true;
	[Export(PropertyHint.Range, "0,1,0.001")] private float MainBackgroundHalfSliceT = 1f;
	[Export(PropertyHint.Range, "1,1.2,0.001")] private float MainBackgroundSwayScale = 1.015f;
	[Export(PropertyHint.Range, "0,40,0.1")] private float MainBackgroundSwayOffsetX = 6f;
	[Export(PropertyHint.Range, "0,40,0.1")] private float MainBackgroundSwayOffsetY = 3f;
	[Export(PropertyHint.Range, "0,3,0.01")] private float MainBackgroundSwayRotationDegrees = 0.06f;
	[Export(PropertyHint.Range, "1,30,0.1")] private float MainBackgroundSwayCycleSeconds = 16f;

	[ExportGroup("Main Enter Reveal FX")]
	[Export] private bool EnableMainEnterRevealFx = false;
	[Export(PropertyHint.Range, "0.05,3,0.01")] private float MainEnterRevealDurationSeconds = 0.95f;
	[Export(PropertyHint.Range, "0,1,0.01")] private float MainEnterRevealDelaySeconds = 0.03f;
	[Export] private Color MainEnterRevealColor = new Color(0f, 0f, 0f, 1f);
	[Export] private bool EnableMainEnterRevealEdgeFade = true;
	[Export(PropertyHint.Range, "0,0.5,0.005")] private float MainEnterRevealEdgeFadeRatio = 0.10f;

	[ExportGroup("Summon Board FX")]
	[Export] private bool EnableSummonBoardIdleFx = true;
	[Export(PropertyHint.Range, "0,40,0.1")] private float SummonBoardFloatAmplitude = 10f;
	[Export(PropertyHint.Range, "1,24,0.1")] private float SummonBoardFloatCycleSeconds = 7.2f;
	[Export(PropertyHint.Range, "0,8,0.1")] private float SummonBoardTiltDegrees = 0.9f;
	[Export(PropertyHint.Range, "0,1,0.01")] private float SummonBoardShadowFollowRatio = 0.32f;
	[Export(PropertyHint.Range, "0,0.6,0.01")] private float SummonBoardShadowAlpha = 0.24f;
	[Export(PropertyHint.Range, "0,0.2,0.01")] private float SummonBoardShadowPulseAlpha = 0.06f;
	[Export(PropertyHint.Range, "0.8,1.2,0.001")] private float SummonBoardShadowPulseScale = 0.028f;

	[ExportGroup("Main Content Enter FX")]
	[Export] private bool EnableMainContentEnterFx = true;
	[Export(PropertyHint.Range, "0.05,1.5,0.01")] private float MainContentEnterDurationSeconds = 0.22f;
	[Export(PropertyHint.Range, "0.05,2.5,0.01")] private float SummonBoardEnterDurationSeconds = 0.42f;
	[Export(PropertyHint.Range, "0,0.5,0.01")] private float MainContentEnterStaggerSeconds = 0.04f;
	[Export(PropertyHint.Range, "0,1,0.01")] private float MainContentEnterFromAlpha = 0f;
	[Export(PropertyHint.Range, "0.8,1.0,0.005")] private float MainContentEnterFromScale = 0.98f;

	[ExportGroup("Menu Hover FX")]
	[Export(PropertyHint.Range, "-180,120,0.1")] private float ButtonHoverIndicatorGap = -0f;
	[Export(PropertyHint.Range, "-40,40,0.1")] private float ButtonHoverIndicatorOffsetY = -1f;
	[Export(PropertyHint.Range, "0.03,0.5,0.01")] private float ButtonHoverMoveDurationSeconds = 0.11f;
	[Export(PropertyHint.Range, "0,0.2,0.005")] private float ButtonHoverPulseScale = 0.04f;
	[Export(PropertyHint.Range, "0,0.5,0.01")] private float ButtonHoverPulseAlpha = 0.15f;
	[Export(PropertyHint.Range, "0.2,4,0.01")] private float ButtonHoverPulseCycleSeconds = 1.1f;
	[Export] private Color ButtonTextIdleColor = new Color(0.72f, 0.83f, 0.95f, 0.95f);
	[Export] private Color ButtonTextActiveColor = new Color(0.92f, 0.96f, 1f, 0.98f);

	[ExportGroup("Leaderboard Drawer")]
	[Export] private bool EnableLeaderboardDrawer = true;
	[Export(PropertyHint.Range, "-200,200,0.1")] private float LeaderboardDrawerOffsetX = 0f;
	[Export(PropertyHint.Range, "-200,200,0.1")] private float LeaderboardDrawerOffsetY = 0f;
	[Export(PropertyHint.Range, "0.05,1.2,0.01")] private float LeaderboardDrawerSwapDurationSeconds = 0.36f;
	[Export(PropertyHint.Range, "20,900,1")] private float LeaderboardDrawerHiddenOffsetY = 240f;
	[Export(PropertyHint.Range, "20,900,1")] private float LeaderboardDrawerBoardExitOffsetY = 360f;

	public event Action StartPressed;
	public event Action CardsPressed;
	public event Action LeaderboardPressed;
	public event Action SettingsPressed;
	public event Action QuitPressed;

	public Button StartButton => _startButton;
	public Button CardsButton => _cardsButton;
	public Button LeaderboardButton => _leaderboardButton;
	public Button SettingsButton => _settingsButton;
	public Button QuitButton => _quitButton;
	public Label PerfectLeaderboardLabel => _perfectLeaderboardLabel;
	public TextureRect MainBackground => _mainBackground;

	private Button _startButton;
	private Button _cardsButton;
	private Button _leaderboardButton;
	private Button _settingsButton;
	private Button _quitButton;
	private Label _perfectLeaderboardLabel;
	private TextureRect _summonBoard;
	private TextureRect _summonBoardShadow;
	private TextureRect _mainBackground;
	private TextureRect _buttonHoverIndicator;
	private Control _leaderboardPopup;
	private Label _leaderboardPopupTitleLabel;
	private Label _leaderboardPopupContentLabel;
	private Control _summonBoardLayer;
	private Control _buttonsVBox;
	private ColorRect _mainEnterRevealMask;
	private Tween _mainEnterRevealTween;
	private Tween _mainContentEnterTween;
	private Tween _buttonHoverMoveTween;
	private Tween _leaderboardSwapTween;
	private Vector2 _summonBoardBasePosition;
	private Vector2 _summonBoardShadowBasePosition;
	private Vector2 _summonBoardShadowBaseScale = Vector2.One;
	private Vector2 _buttonHoverBaseScale = Vector2.One;
	private Vector2 _summonBoardLayerBasePosition;
	private float _summonBoardBaseRotationDegrees;
	private float _summonBoardLayerBaseAlpha = 1f;
	private float _summonBoardFxTime;
	private float _buttonHoverFxTime;
	private bool _summonBoardFxBaseCached;
	private bool _summonBoardLayerBaseCached;
	private bool _mainBackgroundBaseCached;
	private bool _buttonHoverBaseCached;
	private bool _leaderboardPopupBaseCached;
	private bool _leaderboardDrawerAnimating;
	private Vector2 _mainBackgroundBasePosition;
	private Vector2 _mainBackgroundBaseScale = Vector2.One;
	private Vector2 _leaderboardPopupBasePosition;
	private float _mainBackgroundBaseRotationDegrees;
	private float _mainBackgroundSwayElapsed;
	private float _mainBackgroundSwayPhaseX;
	private float _mainBackgroundSwayPhaseY;
	private float _mainBackgroundSwayPhaseRot;
	private float _mainBackgroundSwayPhaseScale;
	private Texture2D _mainBackgroundSourceTexture;
	private AtlasTexture _mainBackgroundSliceTexture;
	private bool _pendingMainContentEnterFx;
	private bool _suppressMainBackground;
	private bool _mainContentEnterAnimating;
	private bool _leaderboardPopupOpen;
	private Button _activeMenuButton;
	private string _leaderboardPopupCachedText = string.Empty;
	private StyleBoxEmpty _menuButtonStyleEmpty;
	private ShaderMaterial _mainEnterRevealMaskMaterial;
	private readonly Dictionary<Control, Vector2> _mainContentEnterBaseScales = new();

	private static readonly Shader MainEnterRevealEdgeFadeShader = new()
	{
		Code = @"
shader_type canvas_item;
uniform float feather_ratio : hint_range(0.0, 0.5) = 0.10;
void fragment() {
	vec4 c = COLOR;
	float fade = smoothstep(0.0, max(0.0001, feather_ratio), UV.y);
	COLOR = vec4(c.rgb, c.a * fade);
}
"
	};

	public override void _Ready()
	{
		ResolveNodeReferences();
		BindSignals();
		if (EnableMainBackgroundSway)
			StartMainBackgroundSwayFx();
		else
			StopMainBackgroundSwayFx(resetVisual: true);

		CacheSummonBoardFxBase();
		UpdateSummonBoardIdleFx(0f, forceReset: !EnableSummonBoardIdleFx);
		if (IsVisibleInTree())
		{
			RequestMainContentEnterFx();
			TryPlayPendingMainContentEnterFx();
		}
		else
		{
			ResetMainContentEnterFxVisuals();
		}
	}

	public override void _Process(double delta)
	{
		if (EnableMainBackgroundSway)
			UpdateMainBackgroundSwayFx((float)delta);
		UpdateSummonBoardIdleFx((float)delta, forceReset: !EnableSummonBoardIdleFx || _leaderboardPopupOpen || _leaderboardDrawerAnimating);
		UpdateButtonHoverIndicatorFx((float)delta);
	}

	public override void _Notification(int what)
	{
		if (what == NotificationVisibilityChanged && IsVisibleInTree())
			TryPlayPendingMainContentEnterFx();
		else if (what == NotificationVisibilityChanged && !IsVisibleInTree())
		{
			_mainContentEnterAnimating = false;
			if (_buttonHoverIndicator != null)
				_buttonHoverIndicator.Visible = false;
			HideLeaderboardDrawer(animate: false);
		}
	}

	public void RequestMainContentEnterFx()
	{
		HideLeaderboardDrawer(animate: false);
		if (!EnableMainContentEnterFx)
		{
			_pendingMainContentEnterFx = false;
			_mainContentEnterAnimating = false;
			ResetMainContentEnterFxVisuals();
			return;
		}

		if (_mainContentEnterTween != null && _mainContentEnterTween.IsValid())
			_mainContentEnterTween.Kill();

		_mainContentEnterAnimating = true;
		PrepareMainContentEnterFxStartPose();
		_pendingMainContentEnterFx = true;
	}

	public void TryPlayPendingMainContentEnterFx()
	{
		if (!_pendingMainContentEnterFx || !IsVisibleInTree())
			return;

		_pendingMainContentEnterFx = false;
		PlayMainEnterRevealFx();
		PlayMainContentEnterFx();
	}

	public void FocusDefault()
	{
		_startButton?.GrabFocus();
		SelectMenuButton(_startButton, animateIndicator: true);
			HideLeaderboardDrawer(animate: false);
	}

	public void SetPerfectLeaderboardText(string text)
	{
		_leaderboardPopupCachedText = text ?? string.Empty;
		if (_perfectLeaderboardLabel != null)
			_perfectLeaderboardLabel.Text = _leaderboardPopupCachedText;
		SyncLeaderboardPopupText();
	}

	public bool HasPerfectLeaderboard()
	{
		return _perfectLeaderboardLabel != null || _leaderboardPopupContentLabel != null;
	}

	public void ToggleLeaderboardDrawer()
	{
		if (_leaderboardDrawerAnimating)
			return;
		if (_leaderboardPopupOpen)
			HideLeaderboardDrawer(animate: true);
		else
			ShowLeaderboardDrawer(animate: true);
	}

	public void SetMainBackgroundSuppressed(bool suppressed)
	{
		_suppressMainBackground = suppressed;
		ApplyMainBackgroundVisibility();
	}

	public void ApplyLocalizedTexts()
	{
		if (_startButton != null)
			_startButton.Text = Tr("UI.START.BUTTON_START");
		if (_settingsButton != null)
			_settingsButton.Text = Tr("UI.COMMON.SETTINGS");
		if (_cardsButton != null)
			_cardsButton.Text = Tr("UI.START.BUTTON_CARDS");
		if (_leaderboardButton != null)
			_leaderboardButton.Text = TrOrFallback("UI.START.BUTTON_LEADERBOARD", "Leaderboard");
		if (_quitButton != null)
			_quitButton.Text = Tr("UI.COMMON.QUIT");
		if (_leaderboardPopupTitleLabel != null)
			_leaderboardPopupTitleLabel.Text = Tr("UI.START.PERFECT_BOARD_TITLE");
		SyncLeaderboardPopupText();
	}

	private void ResolveNodeReferences()
	{
		_startButton = GetNodeOrNull<Button>(StartButtonPath);
		_cardsButton = GetNodeOrNull<Button>(CardsButtonPath);
		_leaderboardButton = GetNodeOrNull<Button>(LeaderboardButtonPath);
		_settingsButton = GetNodeOrNull<Button>(SettingsButtonPath);
		_quitButton = GetNodeOrNull<Button>(QuitButtonPath);
		_perfectLeaderboardLabel = GetNodeOrNull<Label>(PerfectLeaderboardPath);
		_summonBoard = GetNodeOrNull<TextureRect>(SummonBoardPath);
		_summonBoardShadow = GetNodeOrNull<TextureRect>(SummonBoardShadowPath);
		_mainBackground = GetNodeOrNull<TextureRect>(MainBackgroundPath);
		_buttonHoverIndicator = GetNodeOrNull<TextureRect>(ButtonHoverIndicatorPath);
		_leaderboardPopup = GetNodeOrNull<Control>(LeaderboardPopupPath);
		_leaderboardPopupTitleLabel = GetNodeOrNull<Label>(LeaderboardPopupTitlePath);
		_leaderboardPopupContentLabel = GetNodeOrNull<Label>(LeaderboardPopupContentPath);
		_summonBoardLayer = GetNodeOrNull<Control>(SummonBoardLayerPath);
		_buttonsVBox = GetNodeOrNull<Control>(ButtonsVBoxPath);
		ApplyMainBackgroundHalfSlice();
		ApplyMainBackgroundVisibility();
		InitializeButtonHoverIndicator();
		InitializeLeaderboardPopup();
		_mainEnterRevealMask = GetNodeOrNull<ColorRect>(MainEnterRevealMaskPath);
		if (_mainEnterRevealMask != null)
		{
			_mainEnterRevealMask.Color = MainEnterRevealColor;
			_mainEnterRevealMask.MouseFilter = MouseFilterEnum.Ignore;
			_mainEnterRevealMask.Visible = false;
			ApplyMainEnterRevealEdgeFadeMaterial();
		}
	}

	private void BindSignals()
	{
		if (_startButton != null)
		{
			_startButton.Pressed += () => StartPressed?.Invoke();
			BindMenuButtonStateSignals(_startButton);
		}
		if (_cardsButton != null)
		{
			_cardsButton.Pressed += () => CardsPressed?.Invoke();
			BindMenuButtonStateSignals(_cardsButton);
		}
		if (_leaderboardButton != null)
		{
			_leaderboardButton.Pressed += () => LeaderboardPressed?.Invoke();
			BindMenuButtonStateSignals(_leaderboardButton);
		}
		if (_settingsButton != null)
		{
			_settingsButton.Pressed += () => SettingsPressed?.Invoke();
			BindMenuButtonStateSignals(_settingsButton);
		}
		if (_quitButton != null)
		{
			_quitButton.Pressed += () => QuitPressed?.Invoke();
			BindMenuButtonStateSignals(_quitButton);
		}

		SelectMenuButton(_startButton, animateIndicator: false);
	}

	private void PlayMainContentEnterFx()
	{
		if (!EnableMainContentEnterFx)
		{
			_mainContentEnterAnimating = false;
			ResetMainContentEnterFxVisuals();
			return;
		}

		if (_mainContentEnterTween != null && _mainContentEnterTween.IsValid())
			_mainContentEnterTween.Kill();
		PrepareMainContentEnterFxStartPose();

		CanvasItem[] sequence = GetMainContentEnterSequence();
		float duration = Mathf.Max(0.05f, MainContentEnterDurationSeconds);
		float stagger = Mathf.Max(0f, MainContentEnterStaggerSeconds);

		_mainContentEnterTween = CreateTween();
		_mainContentEnterTween.SetPauseMode(Tween.TweenPauseMode.Process);

		bool first = true;
		foreach (CanvasItem item in sequence)
		{
			if (item == null)
				continue;
			float itemDuration = item == _summonBoardLayer
				? Mathf.Max(0.05f, SummonBoardEnterDurationSeconds)
				: duration;

			if (!first && stagger > 0f)
				_mainContentEnterTween.TweenInterval(stagger);
			first = false;

			_mainContentEnterTween.TweenProperty(item, "modulate:a", 1f, itemDuration)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.Out);
			if (item is Control control)
			{
				Vector2 baseScale = GetMainContentEnterBaseScale(control);
				_mainContentEnterTween.Parallel().TweenProperty(control, "scale", baseScale, itemDuration)
					.SetTrans(Tween.TransitionType.Sine)
					.SetEase(Tween.EaseType.Out);
			}
		}

		_mainContentEnterTween.TweenCallback(Callable.From(() =>
		{
			_mainContentEnterTween = null;
			_mainContentEnterAnimating = false;
			RestoreMenuSelectionVisualsAfterEnter();
		}));
	}

	private void ResetMainContentEnterFxVisuals()
	{
		_mainContentEnterAnimating = false;
		CanvasItem[] sequence = GetMainContentEnterSequence();
		foreach (CanvasItem item in sequence)
		{
			if (item == null)
				continue;
			SetCanvasItemAlpha(item, 1f);
			if (item is Control control)
				control.Scale = GetMainContentEnterBaseScale(control);
		}
		RefreshMenuButtonTextStates();
		RestoreMenuSelectionVisualsAfterEnter();
	}

	private void PrepareMainContentEnterFxStartPose()
	{
		CanvasItem[] sequence = GetMainContentEnterSequence();
		float fromAlpha = Mathf.Clamp(MainContentEnterFromAlpha, 0f, 1f);
		float fromScale = Mathf.Clamp(MainContentEnterFromScale, 0.8f, 1f);
		if (_buttonHoverIndicator != null)
			_buttonHoverIndicator.Visible = false;

		foreach (CanvasItem item in sequence)
		{
			if (item == null)
				continue;
			SetCanvasItemAlpha(item, fromAlpha);
			if (item is Control control)
			{
				Vector2 baseScale = GetMainContentEnterBaseScale(control);
				control.PivotOffset = control.Size * 0.5f;
				control.Scale = baseScale * fromScale;
			}
		}
	}

	private CanvasItem[] GetMainContentEnterSequence()
	{
		return new CanvasItem[]
		{
			_summonBoardLayer,
			_startButton,
			_cardsButton,
			_leaderboardButton,
			_settingsButton,
			_quitButton
		};
	}

	private static void SetCanvasItemAlpha(CanvasItem item, float alpha)
	{
		Color c = item.Modulate;
		c.A = Mathf.Clamp(alpha, 0f, 1f);
		item.Modulate = c;
	}

	private Vector2 GetMainContentEnterBaseScale(Control control)
	{
		if (control == null)
			return Vector2.One;
		if (_mainContentEnterBaseScales.TryGetValue(control, out Vector2 scale))
			return scale;
		scale = control.Scale;
		_mainContentEnterBaseScales[control] = scale;
		return scale;
	}

	private void ApplyMainBackgroundHalfSlice()
	{
		if (_mainBackground == null)
			return;
		Texture2D sourceTexture = ResolveMainBackgroundSourceTexture();
		if (sourceTexture == null)
			return;

		if (!UseMainBackgroundHalfSlice)
		{
			_mainBackground.Texture = sourceTexture;
			_mainBackground.Material = null;
			return;
		}

		_mainBackgroundSliceTexture ??= new AtlasTexture();
		_mainBackgroundSliceTexture.Atlas = sourceTexture;
		_mainBackgroundSliceTexture.Region = BuildVerticalHalfSliceRegion(sourceTexture.GetSize(), Mathf.Clamp(MainBackgroundHalfSliceT, 0f, 1f));
		_mainBackground.Texture = _mainBackgroundSliceTexture;
		_mainBackground.Material = null;
	}

	private Texture2D ResolveMainBackgroundSourceTexture()
	{
		if (_mainBackground == null)
			return null;

		if (_mainBackgroundSourceTexture != null)
			return _mainBackgroundSourceTexture;

		if (_mainBackground.Texture is AtlasTexture atlasTexture && atlasTexture.Atlas != null)
		{
			_mainBackgroundSourceTexture = atlasTexture.Atlas;
			return _mainBackgroundSourceTexture;
		}

		_mainBackgroundSourceTexture = _mainBackground.Texture;
		return _mainBackgroundSourceTexture;
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

	private void ApplyMainBackgroundVisibility()
	{
		if (_mainBackground == null)
			return;

		_mainBackground.Visible = !_suppressMainBackground;
	}

	private void InitializeButtonHoverIndicator()
	{
		if (_buttonHoverIndicator == null)
			return;

		_buttonHoverIndicator.MouseFilter = MouseFilterEnum.Ignore;
		_buttonHoverIndicator.Visible = false;
		CacheButtonHoverBaseScale();
	}

	private void InitializeLeaderboardPopup()
	{
		if (_leaderboardPopup == null)
			return;

		_leaderboardPopup.MouseFilter = MouseFilterEnum.Ignore;
		CacheLeaderboardPopupBaseScale();
		CacheSummonBoardLayerBase();
		Vector2 openPos = ResolveLeaderboardPopupOpenPosition();
		Vector2 hiddenPos = ResolveLeaderboardPopupClosedPosition(openPos);
		ApplySummonBoardLayerPose(_summonBoardLayerBasePosition, _summonBoardLayerBaseAlpha, visible: true);
		_leaderboardPopup.Visible = false;
		_leaderboardPopup.Position = hiddenPos;
		SetCanvasItemAlpha(_leaderboardPopup, 1f);
		_leaderboardPopupOpen = false;
		_leaderboardDrawerAnimating = false;
		if (_leaderboardPopupTitleLabel != null)
			_leaderboardPopupTitleLabel.Text = Tr("UI.START.PERFECT_BOARD_TITLE");
		SyncLeaderboardPopupText();
	}

	private void BindMenuButtonStateSignals(Button button)
	{
		if (button == null)
			return;

		_menuButtonStyleEmpty ??= new StyleBoxEmpty();
		button.AddThemeStyleboxOverride("normal", _menuButtonStyleEmpty);
		button.AddThemeStyleboxOverride("hover", _menuButtonStyleEmpty);
		button.AddThemeStyleboxOverride("pressed", _menuButtonStyleEmpty);
		button.AddThemeStyleboxOverride("focus", _menuButtonStyleEmpty);
		button.MouseEntered += () => SelectMenuButton(button, animateIndicator: true);
		button.FocusEntered += () => SelectMenuButton(button, animateIndicator: true);
	}

	private void SelectMenuButton(Button button, bool animateIndicator)
	{
		if (button == null)
			return;
		_activeMenuButton = button;
		RefreshMenuButtonTextStates();
		MoveButtonHoverIndicatorTo(button, animateIndicator);
	}

	private void RefreshMenuButtonTextStates()
	{
		foreach (Button button in GetMenuButtons())
		{
			if (button == null)
				continue;
			bool isActive = button == _activeMenuButton;
			button.AddThemeColorOverride("font_color", isActive ? ButtonTextActiveColor : ButtonTextIdleColor);
		}
	}

	private Button[] GetMenuButtons()
	{
		return new[]
		{
			_startButton,
			_cardsButton,
			_leaderboardButton,
			_settingsButton,
			_quitButton
		};
	}

	private void MoveButtonHoverIndicatorTo(Button button, bool animate)
	{
		if (_buttonHoverIndicator == null || button == null || !GodotObject.IsInstanceValid(button))
			return;

		Vector2 target = ResolveButtonHoverIndicatorTarget(button);
		if (_mainContentEnterAnimating)
		{
			_buttonHoverIndicator.Visible = false;
			_buttonHoverIndicator.Position = target;
			return;
		}

		_buttonHoverIndicator.Visible = true;
		if (!animate || !IsInsideTree())
		{
			if (_buttonHoverMoveTween != null && _buttonHoverMoveTween.IsValid())
				_buttonHoverMoveTween.Kill();
			_buttonHoverIndicator.Position = target;
			return;
		}

		if (_buttonHoverMoveTween != null && _buttonHoverMoveTween.IsValid())
			_buttonHoverMoveTween.Kill();
		_buttonHoverMoveTween = CreateTween();
		_buttonHoverMoveTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_buttonHoverMoveTween.TweenProperty(_buttonHoverIndicator, "position", target, Mathf.Max(0.03f, ButtonHoverMoveDurationSeconds))
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_buttonHoverMoveTween.TweenCallback(Callable.From(() => _buttonHoverMoveTween = null));
	}

	private void RestoreMenuSelectionVisualsAfterEnter()
	{
		RefreshMenuButtonTextStates();
		if (_activeMenuButton == null)
			return;
		MoveButtonHoverIndicatorTo(_activeMenuButton, animate: false);
	}

	private void ShowLeaderboardDrawer(bool animate)
	{
		if (!EnableLeaderboardDrawer || _leaderboardPopup == null || _leaderboardDrawerAnimating)
			return;

		CacheLeaderboardPopupBaseScale();
		CacheSummonBoardLayerBase();
		SyncLeaderboardPopupText();
		UpdateLeaderboardDrawerPosition();
		_leaderboardPopupOpen = true;

		if (_leaderboardSwapTween != null && _leaderboardSwapTween.IsValid())
			_leaderboardSwapTween.Kill();

		Vector2 openPos = _leaderboardPopup.Position;
		Vector2 closedPos = ResolveLeaderboardPopupClosedPosition(openPos);
		Vector2 boardClosedPos = ResolveSummonBoardLayerClosedPosition(_summonBoardLayerBasePosition);
		if (!animate || !IsInsideTree())
		{
			ApplySummonBoardLayerPose(boardClosedPos, 0f, visible: false);
			_leaderboardPopup.Visible = true;
			_leaderboardPopup.Position = openPos;
			SetCanvasItemAlpha(_leaderboardPopup, 1f);
			return;
		}

		_leaderboardDrawerAnimating = true;
		ApplySummonBoardLayerPose(_summonBoardLayerBasePosition, _summonBoardLayerBaseAlpha, visible: true);
		_leaderboardPopup.Visible = true;
		_leaderboardPopup.Position = closedPos;
		SetCanvasItemAlpha(_leaderboardPopup, 0f);
		float duration = Mathf.Max(0.05f, LeaderboardDrawerSwapDurationSeconds);
		_leaderboardSwapTween = CreateTween();
		_leaderboardSwapTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_leaderboardSwapTween.TweenProperty(_summonBoardLayer, "position", boardClosedPos, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_leaderboardSwapTween.Parallel().TweenProperty(_summonBoardLayer, "modulate:a", 0f, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_leaderboardSwapTween.Parallel().TweenProperty(_leaderboardPopup, "modulate:a", 1f, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_leaderboardSwapTween.Parallel().TweenProperty(_leaderboardPopup, "position", openPos, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_leaderboardSwapTween.TweenCallback(Callable.From(() =>
		{
			ApplySummonBoardLayerPose(boardClosedPos, 0f, visible: false);
			_leaderboardSwapTween = null;
			_leaderboardDrawerAnimating = false;
		}));
	}

	public void HideLeaderboardDrawer(bool animate)
	{
		if (_leaderboardPopup == null)
			return;
		_leaderboardPopupOpen = false;
		CacheLeaderboardPopupBaseScale();
		CacheSummonBoardLayerBase();

		if (_leaderboardSwapTween != null && _leaderboardSwapTween.IsValid())
			_leaderboardSwapTween.Kill();

		Vector2 openPos = ResolveLeaderboardPopupOpenPosition();
		Vector2 closedPos = ResolveLeaderboardPopupClosedPosition(openPos);
		Vector2 boardClosedPos = ResolveSummonBoardLayerClosedPosition(_summonBoardLayerBasePosition);
		if (!animate || !IsInsideTree() || !_leaderboardPopup.Visible)
		{
			ApplySummonBoardLayerPose(_summonBoardLayerBasePosition, _summonBoardLayerBaseAlpha, visible: true);
			_leaderboardPopup.Visible = false;
			_leaderboardPopup.Position = closedPos;
			SetCanvasItemAlpha(_leaderboardPopup, 1f);
			_leaderboardDrawerAnimating = false;
			return;
		}

		_leaderboardDrawerAnimating = true;
		ApplySummonBoardLayerPose(boardClosedPos, 0f, visible: true);
		_leaderboardPopup.Visible = true;
		_leaderboardPopup.Position = openPos;
		SetCanvasItemAlpha(_leaderboardPopup, 1f);

		float duration = Mathf.Max(0.05f, LeaderboardDrawerSwapDurationSeconds);
		_leaderboardSwapTween = CreateTween();
		_leaderboardSwapTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_leaderboardSwapTween.TweenProperty(_summonBoardLayer, "position", _summonBoardLayerBasePosition, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_leaderboardSwapTween.Parallel().TweenProperty(_summonBoardLayer, "modulate:a", _summonBoardLayerBaseAlpha, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_leaderboardSwapTween.Parallel().TweenProperty(_leaderboardPopup, "position", closedPos, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
		_leaderboardSwapTween.Parallel().TweenProperty(_leaderboardPopup, "modulate:a", 0f, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.In);
		_leaderboardSwapTween.TweenCallback(Callable.From(() =>
		{
			if (_leaderboardPopup != null)
			{
				_leaderboardPopup.Visible = false;
				_leaderboardPopup.Position = closedPos;
				SetCanvasItemAlpha(_leaderboardPopup, 1f);
			}

			ApplySummonBoardLayerPose(_summonBoardLayerBasePosition, _summonBoardLayerBaseAlpha, visible: true);
			_leaderboardSwapTween = null;
			_leaderboardDrawerAnimating = false;
		}));
	}

	private void UpdateLeaderboardDrawerPosition()
	{
		if (_leaderboardPopup == null)
			return;
		_leaderboardPopup.Position = ResolveLeaderboardPopupOpenPosition();
	}

	private void CacheLeaderboardPopupBaseScale()
	{
		if (_leaderboardPopupBaseCached || _leaderboardPopup == null)
			return;

		_leaderboardPopupBasePosition = _leaderboardPopup.Position;
		_leaderboardPopupBaseCached = true;
	}

	private Vector2 ResolveLeaderboardPopupOpenPosition()
	{
		return _leaderboardPopupBasePosition + new Vector2(LeaderboardDrawerOffsetX, LeaderboardDrawerOffsetY);
	}

	private Vector2 ResolveLeaderboardPopupClosedPosition(Vector2 openPosition)
	{
		return openPosition + new Vector2(0f, Mathf.Max(20f, LeaderboardDrawerHiddenOffsetY));
	}

	private Vector2 ResolveSummonBoardLayerClosedPosition(Vector2 openPosition)
	{
		return openPosition + new Vector2(0f, -Mathf.Max(20f, LeaderboardDrawerBoardExitOffsetY));
	}

	private void CacheSummonBoardLayerBase()
	{
		if (_summonBoardLayerBaseCached || _summonBoardLayer == null)
			return;

		_summonBoardLayerBasePosition = _summonBoardLayer.Position;
		_summonBoardLayerBaseAlpha = _summonBoardLayer.Modulate.A;
		_summonBoardLayerBaseCached = true;
	}

	private void ApplySummonBoardLayerPose(Vector2 position, float alpha, bool visible)
	{
		if (_summonBoardLayer == null)
			return;

		_summonBoardLayer.Visible = visible;
		_summonBoardLayer.Position = position;
		Color c = _summonBoardLayer.Modulate;
		c.A = Mathf.Clamp(alpha, 0f, 1f);
		_summonBoardLayer.Modulate = c;
	}

	private void SyncLeaderboardPopupText()
	{
		if (_leaderboardPopupContentLabel == null)
			return;

		if (!string.IsNullOrWhiteSpace(_leaderboardPopupCachedText))
		{
			_leaderboardPopupContentLabel.Text = _leaderboardPopupCachedText;
			return;
		}

		_leaderboardPopupContentLabel.Text = Tr("UI.START.PERFECT_BOARD_EMPTY");
	}

	private string TrOrFallback(string key, string fallback)
	{
		string localized = Tr(key);
		return string.IsNullOrWhiteSpace(localized) || localized == key
			? fallback
			: localized;
	}

	private Vector2 ResolveButtonHoverIndicatorTarget(Button button)
	{
		Control indicatorParent = _buttonHoverIndicator?.GetParent() as Control;
		if (indicatorParent == null || button == null)
			return Vector2.Zero;

		Rect2 buttonRect = button.GetGlobalRect();
		Vector2 indicatorSize = _buttonHoverIndicator.Size;
		if (indicatorSize.X <= 0f || indicatorSize.Y <= 0f)
		{
			Vector2 texSize = _buttonHoverIndicator.Texture?.GetSize() ?? new Vector2(40f, 40f);
			indicatorSize = texSize;
		}

		float textLeftGlobalX = ResolveButtonTextLeftGlobalX(button, buttonRect);
		float x = textLeftGlobalX - indicatorSize.X - ButtonHoverIndicatorGap;
		float y = buttonRect.Position.Y + ((buttonRect.Size.Y - indicatorSize.Y) * 0.5f) + ButtonHoverIndicatorOffsetY;
		Vector2 globalTarget = new Vector2(x, y);
		return indicatorParent.GetGlobalTransformWithCanvas().AffineInverse() * globalTarget;
	}

	private static float ResolveButtonTextLeftGlobalX(Button button, Rect2 buttonRect)
	{
		float contentLeft = 0f;
		float contentRight = buttonRect.Size.X;
		StyleBox styleBox = button.GetThemeStylebox("normal");
		if (styleBox != null)
		{
			contentLeft += styleBox.GetContentMargin(Side.Left);
			contentRight -= styleBox.GetContentMargin(Side.Right);
		}

		float contentWidth = Mathf.Max(1f, contentRight - contentLeft);
		Font font = button.GetThemeFont("font");
		int fontSize = button.GetThemeFontSize("font_size");
		float textWidth = 0f;
		if (font != null && !string.IsNullOrWhiteSpace(button.Text))
		{
			textWidth = font.GetStringSize(button.Text, HorizontalAlignment.Left, -1, fontSize).X;
			textWidth = Mathf.Min(contentWidth, textWidth);
		}

		float textLeftLocal = contentLeft;
		switch (button.Alignment)
		{
			case HorizontalAlignment.Center:
				textLeftLocal = contentLeft + ((contentWidth - textWidth) * 0.5f);
				break;
			case HorizontalAlignment.Right:
				textLeftLocal = contentRight - textWidth;
				break;
			default:
				textLeftLocal = contentLeft;
				break;
		}

		return buttonRect.Position.X + textLeftLocal;
	}

	private void UpdateButtonHoverIndicatorFx(float deltaSeconds)
	{
		if (_buttonHoverIndicator == null)
			return;
		if (_activeMenuButton != null && _buttonHoverMoveTween == null)
		{
			Vector2 target = ResolveButtonHoverIndicatorTarget(_activeMenuButton);
			if (_buttonHoverIndicator.Position.DistanceTo(target) > 0.5f)
				_buttonHoverIndicator.Position = target;
		}
		if (!_buttonHoverIndicator.Visible)
			return;

		CacheButtonHoverBaseScale();
		float cycle = Mathf.Max(0.2f, ButtonHoverPulseCycleSeconds);
		_buttonHoverFxTime += Mathf.Max(0f, deltaSeconds);
		float phase = (_buttonHoverFxTime / cycle) * Mathf.Tau;
		float pulse = (Mathf.Sin(phase) * 0.5f) + 0.5f;
		float scale = 1f + (Mathf.Sin(phase) * Mathf.Max(0f, ButtonHoverPulseScale));
		_buttonHoverIndicator.Scale = _buttonHoverBaseScale * new Vector2(scale, scale);
		Color c = _buttonHoverIndicator.Modulate;
		c.A = Mathf.Clamp((1f - ButtonHoverPulseAlpha) + (pulse * ButtonHoverPulseAlpha), 0f, 1f);
		_buttonHoverIndicator.Modulate = c;
	}

	private void CacheButtonHoverBaseScale()
	{
		if (_buttonHoverBaseCached || _buttonHoverIndicator == null)
			return;

		_buttonHoverBaseScale = _buttonHoverIndicator.Scale;
		_buttonHoverBaseCached = true;
	}

	private void PlayMainEnterRevealFx()
	{
		if (_mainEnterRevealMask == null)
			return;
		if (!EnableMainEnterRevealFx)
		{
			_mainEnterRevealMask.Visible = false;
			return;
		}

		if (_mainEnterRevealTween != null && _mainEnterRevealTween.IsValid())
			_mainEnterRevealTween.Kill();

		ApplyMainEnterRevealEdgeFadeMaterial();
		_mainEnterRevealMask.Color = MainEnterRevealColor;
		_mainEnterRevealMask.Visible = true;
		_mainEnterRevealMask.AnchorLeft = 0f;
		_mainEnterRevealMask.AnchorTop = 0f;
		_mainEnterRevealMask.AnchorRight = 1f;
		_mainEnterRevealMask.AnchorBottom = 1f;
		_mainEnterRevealMask.OffsetLeft = 0f;
		_mainEnterRevealMask.OffsetRight = 0f;
		_mainEnterRevealMask.OffsetBottom = 0f;
		_mainEnterRevealMask.OffsetTop = 0f;

		float duration = Mathf.Max(0.05f, MainEnterRevealDurationSeconds);
		float delay = Mathf.Max(0f, MainEnterRevealDelaySeconds);
		float targetTop = Mathf.Max(1f, GetViewportRect().Size.Y);

		_mainEnterRevealTween = CreateTween();
		_mainEnterRevealTween.SetPauseMode(Tween.TweenPauseMode.Process);
		if (delay > 0f)
			_mainEnterRevealTween.TweenInterval(delay);
		_mainEnterRevealTween.TweenProperty(_mainEnterRevealMask, "offset_top", targetTop, duration)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_mainEnterRevealTween.TweenCallback(Callable.From(() =>
		{
			if (_mainEnterRevealMask != null)
				_mainEnterRevealMask.Visible = false;
			_mainEnterRevealTween = null;
		}));
	}

	private void ApplyMainEnterRevealEdgeFadeMaterial()
	{
		if (_mainEnterRevealMask == null)
			return;

		if (!EnableMainEnterRevealEdgeFade)
		{
			_mainEnterRevealMask.Material = null;
			return;
		}

		if (_mainEnterRevealMaskMaterial == null)
		{
			_mainEnterRevealMaskMaterial = new ShaderMaterial
			{
				Shader = MainEnterRevealEdgeFadeShader
			};
		}

		_mainEnterRevealMaskMaterial.SetShaderParameter("feather_ratio", Mathf.Clamp(MainEnterRevealEdgeFadeRatio, 0f, 0.5f));
		_mainEnterRevealMask.Material = _mainEnterRevealMaskMaterial;
	}

	private void StartMainBackgroundSwayFx()
	{
		CacheMainBackgroundBase();
		StopMainBackgroundSwayFx(resetVisual: true);
		if (!EnableMainBackgroundSway || _mainBackground == null)
			return;

		_mainBackground.PivotOffset = _mainBackground.Size * 0.5f;
		_mainBackgroundSwayElapsed = 0f;
		_mainBackgroundSwayPhaseX = GD.Randf() * Mathf.Tau;
		_mainBackgroundSwayPhaseY = GD.Randf() * Mathf.Tau;
		_mainBackgroundSwayPhaseRot = GD.Randf() * Mathf.Tau;
		_mainBackgroundSwayPhaseScale = GD.Randf() * Mathf.Tau;
		UpdateMainBackgroundSwayFx(0f);
	}

	private void UpdateMainBackgroundSwayFx(float deltaSeconds)
	{
		if (_mainBackground == null)
			return;
		if (!EnableMainBackgroundSway)
		{
			StopMainBackgroundSwayFx(resetVisual: true);
			return;
		}

		CacheMainBackgroundBase();
		_mainBackground.PivotOffset = _mainBackground.Size * 0.5f;

		float cycle = Mathf.Max(1f, MainBackgroundSwayCycleSeconds);
		_mainBackgroundSwayElapsed += Mathf.Max(0f, deltaSeconds);
		float t = _mainBackgroundSwayElapsed / cycle;
		float main = t * Mathf.Tau;

		float swayXPrimary = Mathf.Sin(main + _mainBackgroundSwayPhaseX);
		float swayXSecondary = Mathf.Sin(main * 0.53f + _mainBackgroundSwayPhaseX * 0.37f);
		float swayYPrimary = Mathf.Sin(main * 0.79f + _mainBackgroundSwayPhaseY);
		float swayYSecondary = Mathf.Sin(main * 1.31f + _mainBackgroundSwayPhaseY * 0.41f);
		float offsetX = MainBackgroundSwayOffsetX * (swayXPrimary * 0.65f + swayXSecondary * 0.35f);
		float offsetY = MainBackgroundSwayOffsetY * (swayYPrimary * 0.65f + swayYSecondary * 0.35f);

		float rotPrimary = Mathf.Sin(main * 0.47f + _mainBackgroundSwayPhaseRot);
		float rotSecondary = Mathf.Sin(main * 1.07f + _mainBackgroundSwayPhaseRot * 0.51f);
		float rotation = MainBackgroundSwayRotationDegrees * (rotPrimary * 0.7f + rotSecondary * 0.3f);

		float scalePulse = Mathf.Sin(main * 0.29f + _mainBackgroundSwayPhaseScale) * 0.5f + 0.5f;
		float targetScale = Mathf.Max(1f, MainBackgroundSwayScale);
		float scale = Mathf.Lerp(1f, targetScale, scalePulse);

		_mainBackground.Position = _mainBackgroundBasePosition + new Vector2(offsetX, offsetY);
		_mainBackground.RotationDegrees = _mainBackgroundBaseRotationDegrees + rotation;
		_mainBackground.Scale = _mainBackgroundBaseScale * new Vector2(scale, scale);
	}

	private void StopMainBackgroundSwayFx(bool resetVisual)
	{
		if (!resetVisual || _mainBackground == null)
			return;

		CacheMainBackgroundBase();
		_mainBackground.Position = _mainBackgroundBasePosition;
		_mainBackground.Scale = _mainBackgroundBaseScale;
		_mainBackground.RotationDegrees = _mainBackgroundBaseRotationDegrees;
	}

	private void CacheMainBackgroundBase()
	{
		if (_mainBackgroundBaseCached || _mainBackground == null)
			return;

		_mainBackgroundBasePosition = _mainBackground.Position;
		_mainBackgroundBaseScale = _mainBackground.Scale;
		_mainBackgroundBaseRotationDegrees = _mainBackground.RotationDegrees;
		_mainBackgroundBaseCached = true;
	}

	private void CacheSummonBoardFxBase()
	{
		if (_summonBoardFxBaseCached)
			return;
		if (_summonBoard == null)
			return;

		_summonBoardBasePosition = _summonBoard.Position;
		_summonBoardBaseRotationDegrees = _summonBoard.RotationDegrees;

		if (_summonBoardShadow != null)
		{
			_summonBoardShadowBasePosition = _summonBoardShadow.Position;
			_summonBoardShadowBaseScale = _summonBoardShadow.Scale;
			Color c = _summonBoardShadow.Modulate;
			c.A = Mathf.Clamp(SummonBoardShadowAlpha, 0f, 1f);
			_summonBoardShadow.Modulate = c;
		}

		_summonBoardFxBaseCached = true;
	}

	private void UpdateSummonBoardIdleFx(float deltaSeconds, bool forceReset)
	{
		if (_summonBoard == null)
			return;

		CacheSummonBoardFxBase();

		if (forceReset)
		{
			ResetSummonBoardIdleFxVisuals();
			return;
		}

		float cycle = Mathf.Max(1f, SummonBoardFloatCycleSeconds);
		_summonBoardFxTime += Mathf.Max(0f, deltaSeconds);
		float t = _summonBoardFxTime / cycle;
		float phase = t * Mathf.Tau;

		float bobPrimary = Mathf.Sin(phase);
		float bobSecondary = Mathf.Sin((phase * 0.53f) + 1.12f);
		float bob = (bobPrimary * 0.72f + bobSecondary * 0.28f) * Mathf.Max(0f, SummonBoardFloatAmplitude);

		float tiltPrimary = Mathf.Sin((phase * 0.37f) + 0.83f);
		float tiltSecondary = Mathf.Sin((phase * 0.81f) + 2.17f);
		float tilt = (tiltPrimary * 0.75f + tiltSecondary * 0.25f) * Mathf.Max(0f, SummonBoardTiltDegrees);

		_summonBoard.Position = _summonBoardBasePosition + new Vector2(0f, bob);
		_summonBoard.RotationDegrees = _summonBoardBaseRotationDegrees + tilt;

		if (_summonBoardShadow == null)
			return;

		float follow = Mathf.Clamp(SummonBoardShadowFollowRatio, 0f, 1f);
		_summonBoardShadow.Position = _summonBoardShadowBasePosition + new Vector2(0f, bob * follow);

		float pulse = Mathf.Sin(phase * 0.63f);
		float pulseScale = 1f + (pulse * SummonBoardShadowPulseScale);
		_summonBoardShadow.Scale = _summonBoardShadowBaseScale * new Vector2(pulseScale, pulseScale);

		Color c = _summonBoardShadow.Modulate;
		c.A = Mathf.Clamp(SummonBoardShadowAlpha + ((-bobPrimary) * SummonBoardShadowPulseAlpha), 0f, 1f);
		_summonBoardShadow.Modulate = c;
	}

	private void ResetSummonBoardIdleFxVisuals()
	{
		_summonBoard.Position = _summonBoardBasePosition;
		_summonBoard.RotationDegrees = _summonBoardBaseRotationDegrees;
		if (_summonBoardShadow == null)
			return;

		_summonBoardShadow.Position = _summonBoardShadowBasePosition;
		_summonBoardShadow.Scale = _summonBoardShadowBaseScale;
		Color c = _summonBoardShadow.Modulate;
		c.A = Mathf.Clamp(SummonBoardShadowAlpha, 0f, 1f);
		_summonBoardShadow.Modulate = c;
	}
}
