using Godot;

public partial class GameFlowUI
{
	private const string SettingsPath = "user://settings.cfg";
	private const float DefaultBgmPercent = 50f;
	private const float DefaultSfxPercent = 80f;
	private const bool DefaultAutoAimEnabled = true;

	private void InitializeSettingsUi()
	{
		_suppressSettingsSignal = true;
		_startSettingsPageController?.SetSuppressSignals(true);

		float bgmPercent = Mathf.Clamp(_sharedState.Settings.BgmPercent, 0f, 100f);
		float sfxPercent = Mathf.Clamp(_sharedState.Settings.SfxPercent, 0f, 100f);
		ConfigurePercentSlider(_settingsBgmSlider, bgmPercent / 100f);
		ConfigurePercentSlider(_startSettingsBgmSlider, bgmPercent / 100f);
		ConfigurePercentSlider(_settingsSfxSlider, sfxPercent / 100f);
		ConfigurePercentSlider(_startSettingsSfxSlider, sfxPercent / 100f);
		_startSettingsPageController?.ConfigureSliders();

		PopulateWindowSizeOptions(_settingsWindowSizeOption);
		if (_startSettingsPageController != null)
			_startSettingsPageController.PopulateWindowSizeOptions();
		else
			PopulateWindowSizeOptions(_startSettingsWindowSizeOption);

		PopulateWindowModeOptions(_settingsWindowModeOption);
		if (_startSettingsPageController != null)
		{
			int currentModeIndex = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? 1 : 0;
			_startSettingsPageController.PopulateWindowModeOptions(
				Tr("UI.SETTINGS.OPTION_WINDOWED"),
				Tr("UI.SETTINGS.OPTION_FULLSCREEN"),
				currentModeIndex);
		}
		else
		{
			PopulateWindowModeOptions(_startSettingsWindowModeOption);
		}

		PopulateLanguageOptions(_settingsLanguageOption);
		if (_startSettingsPageController != null)
			_startSettingsPageController.PopulateLanguageOptions(GetLanguageIndexFromLocale(TranslationServer.GetLocale()));
		else
			PopulateLanguageOptions(_startSettingsLanguageOption);

		SyncWindowSizeOptionWithCurrent();
		SyncBgmSliderValues(bgmPercent);
		SyncSfxSliderValues(sfxPercent);
		_sharedState.Settings.BgmPercent = bgmPercent;
		_sharedState.Settings.SfxPercent = sfxPercent;
		_sharedState.Settings.WindowModeIndex = _settingsWindowModeOption?.Selected
			?? _startSettingsWindowModeOption?.Selected
			?? 0;
		_sharedState.Settings.WindowSizeIndex = _settingsWindowSizeOption?.Selected
			?? _startSettingsWindowSizeOption?.Selected
			?? 0;
		_sharedState.Settings.LanguageIndex = _settingsLanguageOption?.Selected
			?? _startSettingsLanguageOption?.Selected
			?? GetLanguageIndexFromLocale(TranslationServer.GetLocale());
		_sharedState.Settings.Locale = GetLocaleByIndex(_sharedState.Settings.LanguageIndex);
		bool autoAimEnabled = _sharedState.Settings.AutoAimEnabled;
		_player?.SetAutoAimEnabled(autoAimEnabled);
		_startControlsPageController?.SetAutoAimToggle(autoAimEnabled);
		_startSettingsPageController?.SetSuppressSignals(false);
		_suppressSettingsSignal = false;
	}

	private void OnControlsAutoAimToggled(bool enabled)
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_sharedState.Settings.AutoAimEnabled = enabled;
		_player?.SetAutoAimEnabled(enabled);
		SaveSettingsToDisk();
	}

	private void OnSettingsBgmChanged(double value)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		SyncBgmSliderValues(value);
		_suppressSettingsSignal = false;
		_sharedState.Settings.BgmPercent = (float)value;
		AudioManager.Instance?.SetBgmVolumeLinear((float)value / 100f);
		SaveSettingsToDisk();
	}

	private void OnSettingsSfxChanged(double value)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		SyncSfxSliderValues(value);
		_suppressSettingsSignal = false;
		_sharedState.Settings.SfxPercent = (float)value;
		AudioManager.Instance?.SetSfxVolumeLinear((float)value / 100f);
		SaveSettingsToDisk();
	}

	private void OnSettingsWindowModeSelected(long index)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		SyncWindowModeSelection((int)index);
		_suppressSettingsSignal = false;
		_sharedState.Settings.WindowModeIndex = Mathf.Clamp((int)index, 0, 1);
		DisplayServer.WindowSetMode(index == 1 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
		SaveSettingsToDisk();
	}

	private void OnSettingsWindowSizeSelected(long index)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		SyncWindowSizeSelection((int)index);
		_suppressSettingsSignal = false;
		_sharedState.Settings.WindowSizeIndex = Mathf.Clamp((int)index, 0, 2);
		ApplyWindowSizeByIndex((int)index);
		SaveSettingsToDisk();
	}

	private void OnSettingsLanguageSelected(long index)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		SyncLanguageSelection((int)index);
		_suppressSettingsSignal = false;
		_sharedState.Settings.LanguageIndex = Mathf.Clamp((int)index, 0, 1);
		ApplyLocale(GetLocaleByIndex((int)index));
		InitializeSettingsUi();
		SaveSettingsToDisk();
	}

	private void ApplyWindowSizeByIndex(int index)
	{
		Vector2I size = index switch
		{
			0 => new Vector2I(1280, 720),
			1 => new Vector2I(1600, 900),
			_ => new Vector2I(1920, 1080)
		};
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		SyncWindowModeSelection(0);
		DisplayServer.WindowSetSize(size);
	}

	private void SyncWindowSizeOptionWithCurrent()
	{
		Vector2I current = DisplayServer.WindowGetSize();
		int idx = 0;
		if (current.X >= 1900)
			idx = 2;
		else if (current.X >= 1500)
			idx = 1;
		SyncWindowSizeSelection(idx);
	}

	private static void ConfigurePercentSlider(HSlider slider, float linearValue)
	{
		if (slider == null)
			return;

		slider.MinValue = 0;
		slider.MaxValue = 100;
		slider.Step = 1;
		slider.Value = Mathf.RoundToInt(Mathf.Clamp(linearValue, 0f, 1f) * 100f);
	}

	private static void PopulateWindowSizeOptions(OptionButton option)
	{
		if (option == null)
			return;

		option.Clear();
		option.AddItem("1280x720");
		option.AddItem("1600x900");
		option.AddItem("1920x1080");
	}

	private void PopulateWindowModeOptions(OptionButton option)
	{
		if (option == null)
			return;

		option.Clear();
		option.AddItem(Tr("UI.SETTINGS.OPTION_WINDOWED"));
		option.AddItem(Tr("UI.SETTINGS.OPTION_FULLSCREEN"));
		DisplayServer.WindowMode mode = DisplayServer.WindowGetMode();
		option.Select(mode == DisplayServer.WindowMode.Fullscreen ? 1 : 0);
	}

	private void PopulateLanguageOptions(OptionButton option)
	{
		if (option == null)
			return;

		option.Clear();
		option.AddItem("English");
		// Keep explicit script literal to avoid mojibake from non-UTF8 editor writes.
		option.AddItem("\u7e41\u9ad4\u4e2d\u6587");
		option.Select(GetLanguageIndexFromLocale(TranslationServer.GetLocale()));
	}

	private void SyncBgmSliderValues(double value)
	{
		if (_settingsBgmSlider != null && !Mathf.IsEqualApprox((float)_settingsBgmSlider.Value, (float)value))
			_settingsBgmSlider.Value = value;
		if (_startSettingsBgmSlider != null && !Mathf.IsEqualApprox((float)_startSettingsBgmSlider.Value, (float)value))
			_startSettingsBgmSlider.Value = value;
	}

	private void SyncSfxSliderValues(double value)
	{
		if (_settingsSfxSlider != null && !Mathf.IsEqualApprox((float)_settingsSfxSlider.Value, (float)value))
			_settingsSfxSlider.Value = value;
		if (_startSettingsSfxSlider != null && !Mathf.IsEqualApprox((float)_startSettingsSfxSlider.Value, (float)value))
			_startSettingsSfxSlider.Value = value;
	}

	private void SyncWindowModeSelection(int index)
	{
		int clamped = Mathf.Clamp(index, 0, 1);
		if (_settingsWindowModeOption != null && _settingsWindowModeOption.Selected != clamped)
			_settingsWindowModeOption.Select(clamped);
		if (_startSettingsWindowModeOption != null && _startSettingsWindowModeOption.Selected != clamped)
			_startSettingsWindowModeOption.Select(clamped);
	}

	private void SyncWindowSizeSelection(int index)
	{
		int clamped = Mathf.Clamp(index, 0, 2);
		if (_settingsWindowSizeOption != null && _settingsWindowSizeOption.Selected != clamped)
			_settingsWindowSizeOption.Select(clamped);
		if (_startSettingsWindowSizeOption != null && _startSettingsWindowSizeOption.Selected != clamped)
			_startSettingsWindowSizeOption.Select(clamped);
	}

	private void SyncLanguageSelection(int index)
	{
		int clamped = Mathf.Clamp(index, 0, 1);
		if (_settingsLanguageOption != null && _settingsLanguageOption.Selected != clamped)
			_settingsLanguageOption.Select(clamped);
		if (_startSettingsLanguageOption != null && _startSettingsLanguageOption.Selected != clamped)
			_startSettingsLanguageOption.Select(clamped);
	}

	private void SaveSettingsToDisk()
	{
		var cfg = new ConfigFile();
		GameFlowUiSettingsModel settings = _sharedState.Settings;
		cfg.SetValue("audio", "bgm", (double)settings.BgmPercent);
		cfg.SetValue("audio", "sfx", (double)settings.SfxPercent);
		cfg.SetValue("window", "mode", settings.WindowModeIndex);
		cfg.SetValue("window", "size", settings.WindowSizeIndex);
		cfg.SetValue("locale", "language", settings.LanguageIndex);
		cfg.SetValue("controls", "auto_aim", settings.AutoAimEnabled);
		cfg.Save(SettingsPath);
	}

	private void LoadSettingsFromDisk()
	{
		var cfg = new ConfigFile();
		if (cfg.Load(SettingsPath) != Error.Ok)
		{
			_suppressSettingsSignal = true;
			SyncBgmSliderValues(DefaultBgmPercent);
			SyncSfxSliderValues(DefaultSfxPercent);
			SyncLanguageSelection(0);
			_suppressSettingsSignal = false;
			_sharedState.Settings.BgmPercent = DefaultBgmPercent;
			_sharedState.Settings.SfxPercent = DefaultSfxPercent;
			_sharedState.Settings.WindowModeIndex = 0;
			_sharedState.Settings.WindowSizeIndex = 0;
			_sharedState.Settings.LanguageIndex = 0;
			_sharedState.Settings.Locale = LocaleEnglish;
			_sharedState.Settings.AutoAimEnabled = DefaultAutoAimEnabled;
			AudioManager.Instance?.SetBgmVolumeLinear(DefaultBgmPercent / 100f);
			AudioManager.Instance?.SetSfxVolumeLinear(DefaultSfxPercent / 100f);
			_player?.SetAutoAimEnabled(DefaultAutoAimEnabled);
			_startControlsPageController?.SetAutoAimToggle(DefaultAutoAimEnabled);
			ApplyLocale(LocaleEnglish);
			return;
		}

		_suppressSettingsSignal = true;

		float bgm = Mathf.Clamp((float)(double)cfg.GetValue("audio", "bgm", (double)DefaultBgmPercent), 0f, 100f);
		float sfx = Mathf.Clamp((float)(double)cfg.GetValue("audio", "sfx", (double)DefaultSfxPercent), 0f, 100f);
		int mode = (int)(long)cfg.GetValue("window", "mode", 0L);
		int size = (int)(long)cfg.GetValue("window", "size", 0L);
		int language = (int)(long)cfg.GetValue("locale", "language", 0L);
		bool autoAim = (bool)cfg.GetValue("controls", "auto_aim", DefaultAutoAimEnabled);
		int clampedMode = Mathf.Clamp(mode, 0, 1);
		int clampedSize = Mathf.Clamp(size, 0, 2);
		int clampedLanguage = Mathf.Clamp(language, 0, 1);
		_sharedState.Settings.BgmPercent = bgm;
		_sharedState.Settings.SfxPercent = sfx;
		_sharedState.Settings.WindowModeIndex = clampedMode;
		_sharedState.Settings.WindowSizeIndex = clampedSize;
		_sharedState.Settings.LanguageIndex = clampedLanguage;
		_sharedState.Settings.Locale = GetLocaleByIndex(clampedLanguage);
		_sharedState.Settings.AutoAimEnabled = autoAim;

		SyncBgmSliderValues(bgm);
		SyncSfxSliderValues(sfx);
		AudioManager.Instance?.SetBgmVolumeLinear(bgm / 100f);
		AudioManager.Instance?.SetSfxVolumeLinear(sfx / 100f);

		SyncWindowModeSelection(clampedMode);
		DisplayServer.WindowSetMode(clampedMode == 1 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);

		SyncWindowSizeSelection(clampedSize);
		if (clampedMode == 0)
			ApplyWindowSizeByIndex(clampedSize);

		SyncLanguageSelection(clampedLanguage);
		ApplyLocale(GetLocaleByIndex(clampedLanguage));
		_player?.SetAutoAimEnabled(autoAim);
		_startControlsPageController?.SetAutoAimToggle(autoAim);

		_suppressSettingsSignal = false;
	}
}
