using Godot;

public partial class GameFlowUI
{
	private const string SettingsPath = "user://settings.cfg";
	private const float DefaultBgmPercent = 50f;
	private const float DefaultSfxPercent = 80f;

	private void InitializeSettingsUi()
	{
		_suppressSettingsSignal = true;

		float bgm = AudioManager.Instance?.GetBgmVolumeLinear() ?? 1f;
		float sfx = AudioManager.Instance?.GetSfxVolumeLinear() ?? 1f;
		ConfigurePercentSlider(_settingsBgmSlider, bgm);
		ConfigurePercentSlider(_startSettingsBgmSlider, bgm);
		ConfigurePercentSlider(_settingsSfxSlider, sfx);
		ConfigurePercentSlider(_startSettingsSfxSlider, sfx);

		PopulateWindowSizeOptions(_settingsWindowSizeOption);
		PopulateWindowSizeOptions(_startSettingsWindowSizeOption);

		PopulateWindowModeOptions(_settingsWindowModeOption);
		PopulateWindowModeOptions(_startSettingsWindowModeOption);

		PopulateLanguageOptions(_settingsLanguageOption);
		PopulateLanguageOptions(_startSettingsLanguageOption);

		SyncWindowSizeOptionWithCurrent();
		_suppressSettingsSignal = false;
	}

	private void OnSettingsBgmChanged(double value)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		SyncBgmSliderValues(value);
		_suppressSettingsSignal = false;
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
		double bgm = _settingsBgmSlider?.Value ?? _startSettingsBgmSlider?.Value ?? DefaultBgmPercent;
		double sfx = _settingsSfxSlider?.Value ?? _startSettingsSfxSlider?.Value ?? DefaultSfxPercent;
		int mode = _settingsWindowModeOption?.Selected
			?? _startSettingsWindowModeOption?.Selected
			?? (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? 1 : 0);
		int size = _settingsWindowSizeOption?.Selected ?? _startSettingsWindowSizeOption?.Selected ?? 0;
		int language = _settingsLanguageOption?.Selected
			?? _startSettingsLanguageOption?.Selected
			?? GetLanguageIndexFromLocale(TranslationServer.GetLocale());

		cfg.SetValue("audio", "bgm", bgm);
		cfg.SetValue("audio", "sfx", sfx);
		cfg.SetValue("window", "mode", mode);
		cfg.SetValue("window", "size", size);
		cfg.SetValue("locale", "language", language);
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
			AudioManager.Instance?.SetBgmVolumeLinear(DefaultBgmPercent / 100f);
			AudioManager.Instance?.SetSfxVolumeLinear(DefaultSfxPercent / 100f);
			ApplyLocale(LocaleEnglish);
			return;
		}

		_suppressSettingsSignal = true;

		float bgm = Mathf.Clamp((float)(double)cfg.GetValue("audio", "bgm", (double)DefaultBgmPercent), 0f, 100f);
		float sfx = Mathf.Clamp((float)(double)cfg.GetValue("audio", "sfx", (double)DefaultSfxPercent), 0f, 100f);
		int mode = (int)(long)cfg.GetValue("window", "mode", 0L);
		int size = (int)(long)cfg.GetValue("window", "size", 0L);
		int language = (int)(long)cfg.GetValue("locale", "language", 0L);

		SyncBgmSliderValues(bgm);
		SyncSfxSliderValues(sfx);
		AudioManager.Instance?.SetBgmVolumeLinear(bgm / 100f);
		AudioManager.Instance?.SetSfxVolumeLinear(sfx / 100f);

		SyncWindowModeSelection(mode);
		DisplayServer.WindowSetMode(mode == 1 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);

		SyncWindowSizeSelection(size);
		if (mode == 0)
			ApplyWindowSizeByIndex(size);

		int clampedLanguage = Mathf.Clamp(language, 0, 1);
		SyncLanguageSelection(clampedLanguage);
		ApplyLocale(GetLocaleByIndex(clampedLanguage));

		_suppressSettingsSignal = false;
	}
}
