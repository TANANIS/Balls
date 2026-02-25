using Godot;

public partial class GameFlowUI
{
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
		option.AddItem("繁體中文");
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
}
