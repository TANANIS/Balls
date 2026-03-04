using Godot;
using System;

public partial class StartSettingsPageController : Control
{
	[ExportGroup("Node Paths")]
	[Export] private NodePath BackdropDimPath = "BackdropDim";
	[Export] private NodePath TitlePath = "SettingsScroll/VBox/Title";
	[Export] private NodePath BgmLabelPath = "SettingsScroll/VBox/Rows/BgmLabel";
	[Export] private NodePath BgmSliderPath = "SettingsScroll/VBox/Rows/BgmSlider";
	[Export] private NodePath SfxLabelPath = "SettingsScroll/VBox/Rows/SfxLabel";
	[Export] private NodePath SfxSliderPath = "SettingsScroll/VBox/Rows/SfxSlider";
	[Export] private NodePath WindowModeLabelPath = "SettingsScroll/VBox/Rows/WindowModeLabel";
	[Export] private NodePath WindowModeOptionPath = "SettingsScroll/VBox/Rows/WindowModeChoices/WindowModeOption";
	[Export] private NodePath WindowModeWindowedButtonPath = "SettingsScroll/VBox/Rows/WindowModeChoices/WindowModeWindowedButton";
	[Export] private NodePath WindowModeFullscreenButtonPath = "SettingsScroll/VBox/Rows/WindowModeChoices/WindowModeFullscreenButton";
	[Export] private NodePath WindowSizeLabelPath = "SettingsScroll/VBox/Rows/WindowSizeLabel";
	[Export] private NodePath WindowSizeOptionPath = "SettingsScroll/VBox/Rows/WindowSizeChoices/WindowSizeOption";
	[Export] private NodePath WindowSize720ButtonPath = "SettingsScroll/VBox/Rows/WindowSizeChoices/WindowSize720Button";
	[Export] private NodePath WindowSize900ButtonPath = "SettingsScroll/VBox/Rows/WindowSizeChoices/WindowSize900Button";
	[Export] private NodePath WindowSize1080ButtonPath = "SettingsScroll/VBox/Rows/WindowSizeChoices/WindowSize1080Button";
	[Export] private NodePath LanguageLabelPath = "SettingsScroll/VBox/Rows/LanguageLabel";
	[Export] private NodePath LanguageOptionPath = "SettingsScroll/VBox/Rows/LanguageChoices/LanguageOption";
	[Export] private NodePath LanguageEnglishButtonPath = "SettingsScroll/VBox/Rows/LanguageChoices/LanguageEnglishButton";
	[Export] private NodePath LanguageChineseButtonPath = "SettingsScroll/VBox/Rows/LanguageChoices/LanguageChineseButton";
	[Export] private NodePath ControlsButtonPath = "SettingsScroll/VBox/ActionButtons/ControlsButton";
	[Export] private NodePath DeleteSaveButtonPath = "SettingsScroll/VBox/ActionButtons/DeleteSaveButton";
	[Export] private NodePath BackButtonPath = "SettingsScroll/VBox/BackButton";

	[ExportGroup("Backdrop Dim FX")]
	[Export] private bool EnableBackdropDimFx = true;
	[Export(PropertyHint.Range, "0,1,0.01")] private float BackdropDimAlpha = 0.46f;
	[Export(PropertyHint.Range, "0.05,1.0,0.01")] private float BackdropDimFadeInSeconds = 0.20f;

	public event Action BackPressed;
	public event Action ControlsPressed;
	public event Action DeleteSavePressed;
	public event Action<double> BgmChanged;
	public event Action<double> SfxChanged;
	public event Action<long> WindowModeSelected;
	public event Action<long> WindowSizeSelected;
	public event Action<long> LanguageSelected;

	public Button BackButton => _backButton;
	public Button ControlsButton => _controlsButton;
	public Button DeleteSaveButton => _deleteSaveButton;
	public HSlider BgmSlider => _bgmSlider;
	public HSlider SfxSlider => _sfxSlider;
	public OptionButton WindowModeOption => _windowModeOption;
	public OptionButton WindowSizeOption => _windowSizeOption;
	public OptionButton LanguageOption => _languageOption;

	private Label _titleLabel;
	private Label _bgmLabel;
	private HSlider _bgmSlider;
	private Label _sfxLabel;
	private HSlider _sfxSlider;
	private Label _windowModeLabel;
	private OptionButton _windowModeOption;
	private Button _windowModeWindowedButton;
	private Button _windowModeFullscreenButton;
	private Label _windowSizeLabel;
	private OptionButton _windowSizeOption;
	private Button _windowSize720Button;
	private Button _windowSize900Button;
	private Button _windowSize1080Button;
	private Label _languageLabel;
	private OptionButton _languageOption;
	private Button _languageEnglishButton;
	private Button _languageChineseButton;
	private Button _controlsButton;
	private Button _deleteSaveButton;
	private Button _backButton;
	private ColorRect _backdropDim;
	private Tween _backdropDimTween;
	private bool _backdropDimBaseColorCached;
	private Color _backdropDimBaseColor = new Color(0f, 0f, 0f, 1f);
	private bool _suppressSignals;
	private StyleBoxFlat _choiceNormalStyle;
	private StyleBoxFlat _choiceActiveStyle;

	public override void _Ready()
	{
		ResolveNodeReferences();
		InitializeBackdropDimFx();
		EnsureChoiceButtonStyles();
		BindSignals();
		RefreshChoiceRowsVisuals();
	}

	public override void _Notification(int what)
	{
		if (what != NotificationVisibilityChanged)
			return;

		if (IsVisibleInTree())
			PlayBackdropDimFx(expand: true, immediate: false);
		else
			PlayBackdropDimFx(expand: false, immediate: true);
	}

	public void SetSuppressSignals(bool suppress)
	{
		_suppressSignals = suppress;
	}

	public void FocusBackButton()
	{
		_backButton?.GrabFocus();
	}

	public void FocusControlsButton()
	{
		_controlsButton?.GrabFocus();
	}

	public void SetSliderValues(float bgmPercent, float sfxPercent)
	{
		SetSuppressSignals(true);
		if (_bgmSlider != null)
			_bgmSlider.Value = bgmPercent;
		if (_sfxSlider != null)
			_sfxSlider.Value = sfxPercent;
		SetSuppressSignals(false);
	}

	public void SetWindowSelections(int modeIndex, int sizeIndex)
	{
		SetSuppressSignals(true);
		_windowModeOption?.Select(Mathf.Clamp(modeIndex, 0, 1));
		_windowSizeOption?.Select(Mathf.Clamp(sizeIndex, 0, 2));
		SetSuppressSignals(false);
		RefreshChoiceRowsVisuals();
	}

	public void SetLanguageSelection(int index)
	{
		SetSuppressSignals(true);
		_languageOption?.Select(Mathf.Clamp(index, 0, 1));
		SetSuppressSignals(false);
		RefreshChoiceRowsVisuals();
	}

	public void PopulateWindowSizeOptions()
	{
		if (_windowSizeOption == null)
			return;

		_windowSizeOption.Clear();
		_windowSizeOption.AddItem("1280x720");
		_windowSizeOption.AddItem("1600x900");
		_windowSizeOption.AddItem("1920x1080");
		RefreshWindowSizeChoiceTexts();
		RefreshChoiceRowsVisuals();
	}

	public void PopulateWindowModeOptions(string windowedText, string fullscreenText, int selectedIndex)
	{
		if (_windowModeOption == null)
			return;

		_windowModeOption.Clear();
		_windowModeOption.AddItem(windowedText);
		_windowModeOption.AddItem(fullscreenText);
		_windowModeOption.Select(Mathf.Clamp(selectedIndex, 0, 1));
		RefreshWindowModeChoiceTexts();
		RefreshChoiceRowsVisuals();
	}

	public void PopulateLanguageOptions(int selectedIndex)
	{
		if (_languageOption == null)
			return;

		_languageOption.Clear();
		_languageOption.AddItem("English");
		_languageOption.AddItem("\u7e41\u9ad4\u4e2d\u6587");
		_languageOption.Select(Mathf.Clamp(selectedIndex, 0, 1));
		RefreshLanguageChoiceTexts();
		RefreshChoiceRowsVisuals();
	}

	public void ConfigureSliders()
	{
		ConfigurePercentSlider(_bgmSlider);
		ConfigurePercentSlider(_sfxSlider);
	}

	public void ApplyLocalizedTexts()
	{
		if (_titleLabel != null)
			_titleLabel.Text = Tr("UI.COMMON.SETTINGS");
		if (_bgmLabel != null)
			_bgmLabel.Text = Tr("UI.SETTINGS.BGM");
		if (_sfxLabel != null)
			_sfxLabel.Text = Tr("UI.SETTINGS.SFX");
		if (_windowModeLabel != null)
			_windowModeLabel.Text = Tr("UI.SETTINGS.WINDOW_MODE");
		if (_windowSizeLabel != null)
			_windowSizeLabel.Text = Tr("UI.SETTINGS.WINDOW_SIZE");
		if (_languageLabel != null)
			_languageLabel.Text = Tr("UI.SETTINGS.LANGUAGE");
		if (_controlsButton != null)
			_controlsButton.Text = ResolveTrOrDefault("UI.SETTINGS.CONTROLS", "Controls", "\u64cd\u4f5c\u8a2d\u5b9a");
		if (_deleteSaveButton != null)
			_deleteSaveButton.Text = Tr("UI.META.DELETE_SAVE_BUTTON");
		if (_backButton != null)
			_backButton.Text = Tr("UI.COMMON.BACK");
		RefreshWindowModeChoiceTexts();
		RefreshLanguageChoiceTexts();
		RefreshChoiceRowsVisuals();
	}

	public void RefreshChoiceRowsVisuals()
	{
		EnsureChoiceButtonStyles();
		ApplyChoiceButtonState(_windowModeWindowedButton, _windowModeOption?.Selected == 0);
		ApplyChoiceButtonState(_windowModeFullscreenButton, _windowModeOption?.Selected == 1);
		ApplyChoiceButtonState(_windowSize720Button, _windowSizeOption?.Selected == 0);
		ApplyChoiceButtonState(_windowSize900Button, _windowSizeOption?.Selected == 1);
		ApplyChoiceButtonState(_windowSize1080Button, _windowSizeOption?.Selected == 2);
		ApplyChoiceButtonState(_languageEnglishButton, _languageOption?.Selected == 0);
		ApplyChoiceButtonState(_languageChineseButton, _languageOption?.Selected == 1);
	}

	private static void ConfigurePercentSlider(HSlider slider)
	{
		if (slider == null)
			return;

		slider.MinValue = 0;
		slider.MaxValue = 100;
		slider.Step = 1;
	}

	private string ResolveTrOrDefault(string key, string fallbackEn, string fallbackZhTw)
	{
		string translated = Tr(key);
		if (!string.IsNullOrWhiteSpace(translated) && translated != key)
			return translated;
		return TranslationServer.GetLocale().StartsWith("zh") ? fallbackZhTw : fallbackEn;
	}

	private void ResolveNodeReferences()
	{
		_backdropDim = GetNodeOrNull<ColorRect>(BackdropDimPath);
		_titleLabel = GetNodeOrNull<Label>(TitlePath);
		_bgmLabel = GetNodeOrNull<Label>(BgmLabelPath);
		_bgmSlider = GetNodeOrNull<HSlider>(BgmSliderPath);
		_sfxLabel = GetNodeOrNull<Label>(SfxLabelPath);
		_sfxSlider = GetNodeOrNull<HSlider>(SfxSliderPath);
		_windowModeLabel = GetNodeOrNull<Label>(WindowModeLabelPath);
		_windowModeOption = GetNodeOrNull<OptionButton>(WindowModeOptionPath);
		_windowModeWindowedButton = GetNodeOrNull<Button>(WindowModeWindowedButtonPath);
		_windowModeFullscreenButton = GetNodeOrNull<Button>(WindowModeFullscreenButtonPath);
		_windowSizeLabel = GetNodeOrNull<Label>(WindowSizeLabelPath);
		_windowSizeOption = GetNodeOrNull<OptionButton>(WindowSizeOptionPath);
		_windowSize720Button = GetNodeOrNull<Button>(WindowSize720ButtonPath);
		_windowSize900Button = GetNodeOrNull<Button>(WindowSize900ButtonPath);
		_windowSize1080Button = GetNodeOrNull<Button>(WindowSize1080ButtonPath);
		_languageLabel = GetNodeOrNull<Label>(LanguageLabelPath);
		_languageOption = GetNodeOrNull<OptionButton>(LanguageOptionPath);
		_languageEnglishButton = GetNodeOrNull<Button>(LanguageEnglishButtonPath);
		_languageChineseButton = GetNodeOrNull<Button>(LanguageChineseButtonPath);
		_controlsButton = GetNodeOrNull<Button>(ControlsButtonPath);
		_deleteSaveButton = GetNodeOrNull<Button>(DeleteSaveButtonPath);
		_backButton = GetNodeOrNull<Button>(BackButtonPath);
	}

	private void InitializeBackdropDimFx()
	{
		if (_backdropDim == null)
			return;

		_backdropDim.MouseFilter = MouseFilterEnum.Ignore;
		_backdropDimBaseColor = _backdropDim.Color;
		_backdropDimBaseColor.A = 1f;
		_backdropDimBaseColorCached = true;
		ApplyBackdropDimAlpha(0f, visible: false);
	}

	private void PlayBackdropDimFx(bool expand, bool immediate)
	{
		if (_backdropDim == null)
			return;

		if (_backdropDimTween != null && _backdropDimTween.IsValid())
			_backdropDimTween.Kill();
		_backdropDimTween = null;

		float targetAlpha = expand && EnableBackdropDimFx
			? Mathf.Clamp(BackdropDimAlpha, 0f, 1f)
			: 0f;

		if (immediate || !IsInsideTree() || !EnableBackdropDimFx)
		{
			ApplyBackdropDimAlpha(targetAlpha, visible: targetAlpha > 0.001f);
			return;
		}

		Color color = _backdropDim.Color;
		float currentAlpha = Mathf.Clamp(color.A, 0f, 1f);
		if (Mathf.IsEqualApprox(currentAlpha, targetAlpha))
		{
			_backdropDim.Visible = targetAlpha > 0.001f;
			return;
		}

		_backdropDim.Visible = targetAlpha > 0.001f || currentAlpha > 0.001f;
		_backdropDimTween = CreateTween();
		_backdropDimTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_backdropDimTween.TweenProperty(
			_backdropDim,
			"color:a",
			targetAlpha,
			Mathf.Max(0.05f, BackdropDimFadeInSeconds))
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(targetAlpha >= currentAlpha ? Tween.EaseType.Out : Tween.EaseType.InOut);
		_backdropDimTween.TweenCallback(Callable.From(() =>
		{
			_backdropDimTween = null;
			if (_backdropDim != null && targetAlpha <= 0.001f)
				_backdropDim.Visible = false;
		}));
	}

	private void ApplyBackdropDimAlpha(float alpha, bool visible)
	{
		if (_backdropDim == null)
			return;

		Color color = _backdropDimBaseColorCached ? _backdropDimBaseColor : _backdropDim.Color;
		color.A = Mathf.Clamp(alpha, 0f, 1f);
		_backdropDim.Color = color;
		_backdropDim.Visible = visible;
	}

	private void BindSignals()
	{
		if (_backButton != null)
			_backButton.Pressed += () => BackPressed?.Invoke();
		if (_controlsButton != null)
			_controlsButton.Pressed += () => ControlsPressed?.Invoke();
		if (_deleteSaveButton != null)
			_deleteSaveButton.Pressed += () => DeleteSavePressed?.Invoke();
		if (_bgmSlider != null)
			_bgmSlider.ValueChanged += value => { if (!_suppressSignals) BgmChanged?.Invoke(value); };
		if (_sfxSlider != null)
			_sfxSlider.ValueChanged += value => { if (!_suppressSignals) SfxChanged?.Invoke(value); };
		if (_windowModeOption != null)
			_windowModeOption.ItemSelected += index =>
			{
				RefreshChoiceRowsVisuals();
				if (!_suppressSignals)
					WindowModeSelected?.Invoke(index);
			};
		if (_windowSizeOption != null)
			_windowSizeOption.ItemSelected += index =>
			{
				RefreshChoiceRowsVisuals();
				if (!_suppressSignals)
					WindowSizeSelected?.Invoke(index);
			};
		if (_languageOption != null)
			_languageOption.ItemSelected += index =>
			{
				RefreshChoiceRowsVisuals();
				if (!_suppressSignals)
					LanguageSelected?.Invoke(index);
			};
		if (_windowModeWindowedButton != null)
			_windowModeWindowedButton.Pressed += () => OnWindowModeChoicePressed(0);
		if (_windowModeFullscreenButton != null)
			_windowModeFullscreenButton.Pressed += () => OnWindowModeChoicePressed(1);
		if (_windowSize720Button != null)
			_windowSize720Button.Pressed += () => OnWindowSizeChoicePressed(0);
		if (_windowSize900Button != null)
			_windowSize900Button.Pressed += () => OnWindowSizeChoicePressed(1);
		if (_windowSize1080Button != null)
			_windowSize1080Button.Pressed += () => OnWindowSizeChoicePressed(2);
		if (_languageEnglishButton != null)
			_languageEnglishButton.Pressed += () => OnLanguageChoicePressed(0);
		if (_languageChineseButton != null)
			_languageChineseButton.Pressed += () => OnLanguageChoicePressed(1);
	}

	private void OnWindowModeChoicePressed(int index)
	{
		int clamped = Mathf.Clamp(index, 0, 1);
		if (_windowModeOption != null)
			_windowModeOption.Select(clamped);
		RefreshChoiceRowsVisuals();
		if (!_suppressSignals)
			WindowModeSelected?.Invoke(clamped);
	}

	private void OnWindowSizeChoicePressed(int index)
	{
		int clamped = Mathf.Clamp(index, 0, 2);
		if (_windowSizeOption != null)
			_windowSizeOption.Select(clamped);
		RefreshChoiceRowsVisuals();
		if (!_suppressSignals)
			WindowSizeSelected?.Invoke(clamped);
	}

	private void OnLanguageChoicePressed(int index)
	{
		int clamped = Mathf.Clamp(index, 0, 1);
		if (_languageOption != null)
			_languageOption.Select(clamped);
		RefreshChoiceRowsVisuals();
		if (!_suppressSignals)
			LanguageSelected?.Invoke(clamped);
	}

	private void RefreshWindowModeChoiceTexts()
	{
		if (_windowModeOption == null)
			return;
		if (_windowModeWindowedButton != null && _windowModeOption.ItemCount > 0)
			_windowModeWindowedButton.Text = _windowModeOption.GetItemText(0);
		if (_windowModeFullscreenButton != null && _windowModeOption.ItemCount > 1)
			_windowModeFullscreenButton.Text = _windowModeOption.GetItemText(1);
	}

	private void RefreshWindowSizeChoiceTexts()
	{
		if (_windowSizeOption == null)
			return;
		if (_windowSize720Button != null && _windowSizeOption.ItemCount > 0)
			_windowSize720Button.Text = _windowSizeOption.GetItemText(0);
		if (_windowSize900Button != null && _windowSizeOption.ItemCount > 1)
			_windowSize900Button.Text = _windowSizeOption.GetItemText(1);
		if (_windowSize1080Button != null && _windowSizeOption.ItemCount > 2)
			_windowSize1080Button.Text = _windowSizeOption.GetItemText(2);
	}

	private void RefreshLanguageChoiceTexts()
	{
		if (_languageOption == null)
			return;
		if (_languageEnglishButton != null && _languageOption.ItemCount > 0)
			_languageEnglishButton.Text = _languageOption.GetItemText(0);
		if (_languageChineseButton != null && _languageOption.ItemCount > 1)
			_languageChineseButton.Text = _languageOption.GetItemText(1);
	}

	private void EnsureChoiceButtonStyles()
	{
		if (_choiceNormalStyle == null)
		{
			_choiceNormalStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.13f, 0.12f, 0.13f, 0.55f),
				BorderColor = new Color(0.86f, 0.83f, 0.77f, 0.35f),
				BorderWidthLeft = 1,
				BorderWidthTop = 1,
				BorderWidthRight = 1,
				BorderWidthBottom = 1,
				CornerRadiusTopLeft = 4,
				CornerRadiusTopRight = 4,
				CornerRadiusBottomRight = 4,
				CornerRadiusBottomLeft = 4
			};
		}

		if (_choiceActiveStyle == null)
		{
			_choiceActiveStyle = new StyleBoxFlat
			{
				BgColor = new Color(0.93f, 0.9f, 0.82f, 0.98f),
				BorderColor = new Color(0.95f, 0.94f, 0.9f, 1f),
				BorderWidthLeft = 1,
				BorderWidthTop = 1,
				BorderWidthRight = 1,
				BorderWidthBottom = 1,
				CornerRadiusTopLeft = 4,
				CornerRadiusTopRight = 4,
				CornerRadiusBottomRight = 4,
				CornerRadiusBottomLeft = 4
			};
		}
	}

	private void ApplyChoiceButtonState(Button button, bool active)
	{
		if (button == null)
			return;
		StyleBox style = active ? _choiceActiveStyle : _choiceNormalStyle;
		button.AddThemeStyleboxOverride("normal", style);
		button.AddThemeStyleboxOverride("hover", style);
		button.AddThemeStyleboxOverride("pressed", style);
		button.AddThemeStyleboxOverride("focus", style);
		button.AddThemeColorOverride("font_color", active
			? new Color(0.1f, 0.1f, 0.1f, 1f)
			: new Color(0.93f, 0.91f, 0.86f, 0.98f));
	}
}
