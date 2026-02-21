using Godot;

public partial class GameFlowUI
{
	private const string SettingsPath = "user://settings.cfg";

	private void SaveSettingsToDisk()
	{
		var cfg = new ConfigFile();
		cfg.SetValue("audio", "bgm", _settingsBgmSlider != null ? _settingsBgmSlider.Value : 100.0);
		cfg.SetValue("audio", "sfx", _settingsSfxSlider != null ? _settingsSfxSlider.Value : 100.0);
		cfg.SetValue("window", "mode", _settingsWindowModeOption != null ? _settingsWindowModeOption.Selected : 0);
		cfg.SetValue("window", "size", _settingsWindowSizeOption != null ? _settingsWindowSizeOption.Selected : 0);
		cfg.SetValue("locale", "language", _settingsLanguageOption != null ? _settingsLanguageOption.Selected : 0);
		cfg.Save(SettingsPath);
	}

	private void LoadSettingsFromDisk()
	{
		var cfg = new ConfigFile();
		if (cfg.Load(SettingsPath) != Error.Ok)
		{
			_suppressSettingsSignal = true;
			if (_settingsLanguageOption != null)
				_settingsLanguageOption.Select(0);
			if (_startSettingsLanguageOption != null)
				_startSettingsLanguageOption.Select(0);
			_suppressSettingsSignal = false;
			ApplyLocale(LocaleEnglish);
			return;
		}

		_suppressSettingsSignal = true;

		float bgm = Mathf.Clamp((float)(double)cfg.GetValue("audio", "bgm", 100.0), 0f, 100f);
		float sfx = Mathf.Clamp((float)(double)cfg.GetValue("audio", "sfx", 100.0), 0f, 100f);
		int mode = (int)(long)cfg.GetValue("window", "mode", 0L);
		int size = (int)(long)cfg.GetValue("window", "size", 0L);
		int language = (int)(long)cfg.GetValue("locale", "language", 0L);

		if (_settingsBgmSlider != null)
			_settingsBgmSlider.Value = bgm;
		if (_startSettingsBgmSlider != null)
			_startSettingsBgmSlider.Value = bgm;
		if (_settingsSfxSlider != null)
			_settingsSfxSlider.Value = sfx;
		if (_startSettingsSfxSlider != null)
			_startSettingsSfxSlider.Value = sfx;
		AudioManager.Instance?.SetBgmVolumeLinear(bgm / 100f);
		AudioManager.Instance?.SetSfxVolumeLinear(sfx / 100f);

		if (_settingsWindowModeOption != null)
			_settingsWindowModeOption.Select(Mathf.Clamp(mode, 0, 1));
		if (_startSettingsWindowModeOption != null)
			_startSettingsWindowModeOption.Select(Mathf.Clamp(mode, 0, 1));
		DisplayServer.WindowSetMode(mode == 1 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);

		if (_settingsWindowSizeOption != null)
			_settingsWindowSizeOption.Select(Mathf.Clamp(size, 0, 2));
		if (_startSettingsWindowSizeOption != null)
			_startSettingsWindowSizeOption.Select(Mathf.Clamp(size, 0, 2));
		if (mode == 0)
			ApplyWindowSizeByIndex(size);

		int clampedLanguage = Mathf.Clamp(language, 0, 1);
		if (_settingsLanguageOption != null)
			_settingsLanguageOption.Select(clampedLanguage);
		if (_startSettingsLanguageOption != null)
			_startSettingsLanguageOption.Select(clampedLanguage);
		ApplyLocale(GetLocaleByIndex(clampedLanguage));

		_suppressSettingsSignal = false;
	}
}
