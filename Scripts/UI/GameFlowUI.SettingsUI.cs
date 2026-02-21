using Godot;

public partial class GameFlowUI
{
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
			_settingsLanguageOption.AddItem("蝜?銝剜?");
			_settingsLanguageOption.Select(GetLanguageIndexFromLocale(TranslationServer.GetLocale()));
		}
		if (_startSettingsLanguageOption != null)
		{
			_startSettingsLanguageOption.Clear();
			_startSettingsLanguageOption.AddItem("English");
			_startSettingsLanguageOption.AddItem("蝜?銝剜?");
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
}
