using Godot;

public partial class GameFlowUI
{
	private const string SettingsPath = "user://settings.cfg";

	private void HandlePauseInput()
	{
		if (!_started && _startSettingsOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnStartSettingsBackPressed();
			return;
		}
		if (!_started && _startCharacterSelectOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnCharacterSelectBackPressed();
			return;
		}
		if (!_started && _startCardsOpen && Input.IsActionJustPressed("ui_cancel"))
		{
			OnStartCardsBackPressed();
			return;
		}

		if (!_started || _ending)
			return;
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (_upgradeMenu != null && _upgradeMenu.IsOpen)
				return;
			if (_pauseMenuOpen && _settingsOpen)
			{
				OnPauseSettingsBackPressed();
				return;
			}
			if (_pauseMenuOpen)
				ClosePauseMenu();
			else
				OpenPauseMenu();
		}
	}

	private void OpenPauseMenu()
	{
		_pauseMenuOpen = true;
		_settingsOpen = false;
		RefreshPauseBuildSummary();
		if (_pausePanel != null)
			_pausePanel.Visible = true;
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = true;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = false;
		GetTree().Paused = true;
		_pauseResumeButton?.GrabFocus();
	}

	private void ClosePauseMenu()
	{
		_pauseMenuOpen = false;
		_settingsOpen = false;
		if (_pausePanel != null)
			_pausePanel.Visible = false;
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = true;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = false;
		GetTree().Paused = false;
	}

	private void OnPauseResumePressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		ClosePauseMenu();
	}

	private void OnPauseSettingsPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_settingsOpen = true;
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = false;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = true;
		_settingsBackButton?.GrabFocus();
	}

	private void OnPauseSettingsBackPressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		_settingsOpen = false;
		if (_pauseMainVBox != null)
			_pauseMainVBox.Visible = true;
		if (_pauseSettingsPanel != null)
			_pauseSettingsPanel.Visible = false;
		_pauseSettingsButton?.GrabFocus();
	}

	private void OnPauseRestartPressed()
	{
		AudioManager.Instance?.PlaySfxUiButton();
		_pauseMenuOpen = false;
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}

	private void OnPauseToTitlePressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		ClosePauseMenu();
		ShowStartPanel();
		AudioManager.Instance?.PlayBgmMenu();
	}

	private void OnQuitGamePressed()
	{
		AudioManager.Instance?.PlaySfxUiExit();
		GetTree().Quit();
	}

	private void InitializeSettingsUi()
	{
		_suppressSettingsSignal = true;

		if (_settingsBgmSlider != null)
		{
			_settingsBgmSlider.MinValue = 0;
			_settingsBgmSlider.MaxValue = 100;
			_settingsBgmSlider.Step = 1;
			float bgm = AudioManager.Instance?.GetBgmVolumeLinear() ?? 1f;
			_settingsBgmSlider.Value = Mathf.RoundToInt(bgm * 100f);
		}
		if (_startSettingsBgmSlider != null)
		{
			_startSettingsBgmSlider.MinValue = 0;
			_startSettingsBgmSlider.MaxValue = 100;
			_startSettingsBgmSlider.Step = 1;
			float bgm = AudioManager.Instance?.GetBgmVolumeLinear() ?? 1f;
			_startSettingsBgmSlider.Value = Mathf.RoundToInt(bgm * 100f);
		}

		if (_settingsSfxSlider != null)
		{
			_settingsSfxSlider.MinValue = 0;
			_settingsSfxSlider.MaxValue = 100;
			_settingsSfxSlider.Step = 1;
			float sfx = AudioManager.Instance?.GetSfxVolumeLinear() ?? 1f;
			_settingsSfxSlider.Value = Mathf.RoundToInt(sfx * 100f);
		}
		if (_startSettingsSfxSlider != null)
		{
			_startSettingsSfxSlider.MinValue = 0;
			_startSettingsSfxSlider.MaxValue = 100;
			_startSettingsSfxSlider.Step = 1;
			float sfx = AudioManager.Instance?.GetSfxVolumeLinear() ?? 1f;
			_startSettingsSfxSlider.Value = Mathf.RoundToInt(sfx * 100f);
		}

		if (_settingsWindowSizeOption != null)
		{
			_settingsWindowSizeOption.Clear();
			_settingsWindowSizeOption.AddItem("1280x720");
			_settingsWindowSizeOption.AddItem("1600x900");
			_settingsWindowSizeOption.AddItem("1920x1080");
		}
		if (_startSettingsWindowSizeOption != null)
		{
			_startSettingsWindowSizeOption.Clear();
			_startSettingsWindowSizeOption.AddItem("1280x720");
			_startSettingsWindowSizeOption.AddItem("1600x900");
			_startSettingsWindowSizeOption.AddItem("1920x1080");
		}

		if (_settingsWindowModeOption != null)
		{
			_settingsWindowModeOption.Clear();
			_settingsWindowModeOption.AddItem(Tr("UI.SETTINGS.OPTION_WINDOWED"));
			_settingsWindowModeOption.AddItem(Tr("UI.SETTINGS.OPTION_FULLSCREEN"));
			var mode = DisplayServer.WindowGetMode();
			_settingsWindowModeOption.Select(mode == DisplayServer.WindowMode.Fullscreen ? 1 : 0);
		}
		if (_startSettingsWindowModeOption != null)
		{
			_startSettingsWindowModeOption.Clear();
			_startSettingsWindowModeOption.AddItem(Tr("UI.SETTINGS.OPTION_WINDOWED"));
			_startSettingsWindowModeOption.AddItem(Tr("UI.SETTINGS.OPTION_FULLSCREEN"));
			var mode = DisplayServer.WindowGetMode();
			_startSettingsWindowModeOption.Select(mode == DisplayServer.WindowMode.Fullscreen ? 1 : 0);
		}

		if (_settingsLanguageOption != null)
		{
			_settingsLanguageOption.Clear();
			_settingsLanguageOption.AddItem("English");
			_settingsLanguageOption.AddItem("繁體中文");
			_settingsLanguageOption.Select(GetLanguageIndexFromLocale(TranslationServer.GetLocale()));
		}
		if (_startSettingsLanguageOption != null)
		{
			_startSettingsLanguageOption.Clear();
			_startSettingsLanguageOption.AddItem("English");
			_startSettingsLanguageOption.AddItem("繁體中文");
			_startSettingsLanguageOption.Select(GetLanguageIndexFromLocale(TranslationServer.GetLocale()));
		}

		SyncWindowSizeOptionWithCurrent();
		_suppressSettingsSignal = false;
	}

	private void OnSettingsBgmChanged(double value)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		if (_settingsBgmSlider != null && !Mathf.IsEqualApprox((float)_settingsBgmSlider.Value, (float)value))
			_settingsBgmSlider.Value = value;
		if (_startSettingsBgmSlider != null && !Mathf.IsEqualApprox((float)_startSettingsBgmSlider.Value, (float)value))
			_startSettingsBgmSlider.Value = value;
		_suppressSettingsSignal = false;
		AudioManager.Instance?.SetBgmVolumeLinear((float)value / 100f);
		SaveSettingsToDisk();
	}

	private void OnSettingsSfxChanged(double value)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		if (_settingsSfxSlider != null && !Mathf.IsEqualApprox((float)_settingsSfxSlider.Value, (float)value))
			_settingsSfxSlider.Value = value;
		if (_startSettingsSfxSlider != null && !Mathf.IsEqualApprox((float)_startSettingsSfxSlider.Value, (float)value))
			_startSettingsSfxSlider.Value = value;
		_suppressSettingsSignal = false;
		AudioManager.Instance?.SetSfxVolumeLinear((float)value / 100f);
		SaveSettingsToDisk();
	}

	private void OnSettingsWindowModeSelected(long index)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		if (_settingsWindowModeOption != null && _settingsWindowModeOption.Selected != (int)index)
			_settingsWindowModeOption.Select((int)index);
		if (_startSettingsWindowModeOption != null && _startSettingsWindowModeOption.Selected != (int)index)
			_startSettingsWindowModeOption.Select((int)index);
		_suppressSettingsSignal = false;
		DisplayServer.WindowSetMode(index == 1 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
		SaveSettingsToDisk();
	}

	private void OnSettingsWindowSizeSelected(long index)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		if (_settingsWindowSizeOption != null && _settingsWindowSizeOption.Selected != (int)index)
			_settingsWindowSizeOption.Select((int)index);
		if (_startSettingsWindowSizeOption != null && _startSettingsWindowSizeOption.Selected != (int)index)
			_startSettingsWindowSizeOption.Select((int)index);
		_suppressSettingsSignal = false;
		ApplyWindowSizeByIndex((int)index);
		SaveSettingsToDisk();
	}

	private void OnSettingsLanguageSelected(long index)
	{
		if (_suppressSettingsSignal)
			return;
		_suppressSettingsSignal = true;
		if (_settingsLanguageOption != null && _settingsLanguageOption.Selected != (int)index)
			_settingsLanguageOption.Select((int)index);
		if (_startSettingsLanguageOption != null && _startSettingsLanguageOption.Selected != (int)index)
			_startSettingsLanguageOption.Select((int)index);
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
		if (_settingsWindowModeOption != null)
			_settingsWindowModeOption.Select(0);
		DisplayServer.WindowSetSize(size);
	}

	private void SyncWindowSizeOptionWithCurrent()
	{
		if (_settingsWindowSizeOption == null)
			return;
		Vector2I current = DisplayServer.WindowGetSize();
		int idx = 0;
		if (current.X >= 1900)
			idx = 2;
		else if (current.X >= 1500)
			idx = 1;
		_settingsWindowSizeOption.Select(idx);
		if (_startSettingsWindowSizeOption != null)
			_startSettingsWindowSizeOption.Select(idx);
	}

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
