using Godot;

public partial class GameFlowUI
{
	private const string SettingsPath = "user://settings.cfg";
	private const float DefaultBgmPercent = 50f;
	private const float DefaultSfxPercent = 80f;

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
