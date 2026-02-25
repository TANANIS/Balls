using Godot;

public partial class GameFlowUI
{
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
}
