using Godot;
using System;

public partial class StartSettingsPageController : Control
{
	[ExportGroup("Node Paths")]
	[Export] private NodePath TitlePath = "SettingsScroll/VBox/Title";
	[Export] private NodePath BgmLabelPath = "SettingsScroll/VBox/BgmLabel";
	[Export] private NodePath BgmSliderPath = "SettingsScroll/VBox/BgmSlider";
	[Export] private NodePath SfxLabelPath = "SettingsScroll/VBox/SfxLabel";
	[Export] private NodePath SfxSliderPath = "SettingsScroll/VBox/SfxSlider";
	[Export] private NodePath WindowModeLabelPath = "SettingsScroll/VBox/WindowModeLabel";
	[Export] private NodePath WindowModeOptionPath = "SettingsScroll/VBox/WindowModeOption";
	[Export] private NodePath WindowSizeLabelPath = "SettingsScroll/VBox/WindowSizeLabel";
	[Export] private NodePath WindowSizeOptionPath = "SettingsScroll/VBox/WindowSizeOption";
	[Export] private NodePath LanguageLabelPath = "SettingsScroll/VBox/LanguageLabel";
	[Export] private NodePath LanguageOptionPath = "SettingsScroll/VBox/LanguageOption";
	[Export] private NodePath DeleteSaveButtonPath = "SettingsScroll/VBox/DeleteSaveButton";
	[Export] private NodePath BackButtonPath = "SettingsScroll/VBox/BackButton";

	public event Action BackPressed;
	public event Action DeleteSavePressed;
	public event Action<double> BgmChanged;
	public event Action<double> SfxChanged;
	public event Action<long> WindowModeSelected;
	public event Action<long> WindowSizeSelected;
	public event Action<long> LanguageSelected;

	public Button BackButton => _backButton;
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
	private Label _windowSizeLabel;
	private OptionButton _windowSizeOption;
	private Label _languageLabel;
	private OptionButton _languageOption;
	private Button _deleteSaveButton;
	private Button _backButton;
	private bool _suppressSignals;

	public override void _Ready()
	{
		ResolveNodeReferences();
		BindSignals();
	}

	public void SetSuppressSignals(bool suppress)
	{
		_suppressSignals = suppress;
	}

	public void FocusBackButton()
	{
		_backButton?.GrabFocus();
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
	}

	public void SetLanguageSelection(int index)
	{
		SetSuppressSignals(true);
		_languageOption?.Select(Mathf.Clamp(index, 0, 1));
		SetSuppressSignals(false);
	}

	public void PopulateWindowSizeOptions()
	{
		if (_windowSizeOption == null)
			return;

		_windowSizeOption.Clear();
		_windowSizeOption.AddItem("1280x720");
		_windowSizeOption.AddItem("1600x900");
		_windowSizeOption.AddItem("1920x1080");
	}

	public void PopulateWindowModeOptions(string windowedText, string fullscreenText, int selectedIndex)
	{
		if (_windowModeOption == null)
			return;

		_windowModeOption.Clear();
		_windowModeOption.AddItem(windowedText);
		_windowModeOption.AddItem(fullscreenText);
		_windowModeOption.Select(Mathf.Clamp(selectedIndex, 0, 1));
	}

	public void PopulateLanguageOptions(int selectedIndex)
	{
		if (_languageOption == null)
			return;

		_languageOption.Clear();
		_languageOption.AddItem("English");
		_languageOption.AddItem("\u7e41\u9ad4\u4e2d\u6587");
		_languageOption.Select(Mathf.Clamp(selectedIndex, 0, 1));
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
		if (_deleteSaveButton != null)
			_deleteSaveButton.Text = Tr("UI.META.DELETE_SAVE_BUTTON");
		if (_backButton != null)
			_backButton.Text = Tr("UI.COMMON.BACK");
	}

	private static void ConfigurePercentSlider(HSlider slider)
	{
		if (slider == null)
			return;

		slider.MinValue = 0;
		slider.MaxValue = 100;
		slider.Step = 1;
	}

	private void ResolveNodeReferences()
	{
		_titleLabel = GetNodeOrNull<Label>(TitlePath);
		_bgmLabel = GetNodeOrNull<Label>(BgmLabelPath);
		_bgmSlider = GetNodeOrNull<HSlider>(BgmSliderPath);
		_sfxLabel = GetNodeOrNull<Label>(SfxLabelPath);
		_sfxSlider = GetNodeOrNull<HSlider>(SfxSliderPath);
		_windowModeLabel = GetNodeOrNull<Label>(WindowModeLabelPath);
		_windowModeOption = GetNodeOrNull<OptionButton>(WindowModeOptionPath);
		_windowSizeLabel = GetNodeOrNull<Label>(WindowSizeLabelPath);
		_windowSizeOption = GetNodeOrNull<OptionButton>(WindowSizeOptionPath);
		_languageLabel = GetNodeOrNull<Label>(LanguageLabelPath);
		_languageOption = GetNodeOrNull<OptionButton>(LanguageOptionPath);
		_deleteSaveButton = GetNodeOrNull<Button>(DeleteSaveButtonPath);
		_backButton = GetNodeOrNull<Button>(BackButtonPath);
	}

	private void BindSignals()
	{
		if (_backButton != null)
			_backButton.Pressed += () => BackPressed?.Invoke();
		if (_deleteSaveButton != null)
			_deleteSaveButton.Pressed += () => DeleteSavePressed?.Invoke();
		if (_bgmSlider != null)
			_bgmSlider.ValueChanged += value => { if (!_suppressSignals) BgmChanged?.Invoke(value); };
		if (_sfxSlider != null)
			_sfxSlider.ValueChanged += value => { if (!_suppressSignals) SfxChanged?.Invoke(value); };
		if (_windowModeOption != null)
			_windowModeOption.ItemSelected += index => { if (!_suppressSignals) WindowModeSelected?.Invoke(index); };
		if (_windowSizeOption != null)
			_windowSizeOption.ItemSelected += index => { if (!_suppressSignals) WindowSizeSelected?.Invoke(index); };
		if (_languageOption != null)
			_languageOption.ItemSelected += index => { if (!_suppressSignals) LanguageSelected?.Invoke(index); };
	}
}
