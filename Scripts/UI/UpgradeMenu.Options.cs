using Godot;

public partial class UpgradeMenu
{
	private bool PickOptions()
	{
		if (_upgradeSystem == null)
			return false;

		if (!_upgradeSystem.TryPickTwo(_rng, out _leftOption, out _rightOption))
		{
			DebugSystem.Error("[UpgradeMenu] Could not pick upgrade options.");
			return false;
		}

		return true;
	}

	private void ApplyOption(UpgradeSystem.UpgradeOptionData option)
	{
		AudioManager.Instance?.PlaySfxUiUpgradeSelect();
		_upgradeSystem?.ApplyUpgrade(option.Id);
		AudioManager.Instance?.PlaySfxPlayerUpgrade();
		CloseMenu();
	}

	private void ApplyRandomCurrentOption()
	{
		if (_rng.RandiRange(0, 1) == 0)
			ApplyOption(_leftOption);
		else
			ApplyOption(_rightOption);
	}
}
