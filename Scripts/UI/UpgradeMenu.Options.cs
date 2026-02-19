using Godot;

public partial class UpgradeMenu
{
	private bool PickOptions()
	{
		if (_upgradeSystem == null)
			return false;

		if (!_upgradeSystem.TryPickOptions(_rng, 3, out var picks) || picks.Count < 3)
		{
			DebugSystem.Error("[UpgradeMenu] Could not pick upgrade options.");
			return false;
		}

		_leftOption = picks[0];
		_middleOption = picks[1];
		_rightOption = picks[2];
		return true;
	}

	private void ApplyOption(UpgradeSystem.UpgradeOptionData option)
	{
		AudioManager.Instance?.PlaySfxUiUpgradeSelect();
		if (_upgradeSystem != null && !_upgradeSystem.ApplyUpgrade(option.Id))
			return;

		AudioManager.Instance?.PlaySfxPlayerUpgrade();
		CloseMenu();
	}

	private void ApplyRandomCurrentOption()
	{
		int roll = _rng.RandiRange(0, 2);
		if (roll == 0)
			ApplyOption(_leftOption);
		else if (roll == 1)
			ApplyOption(_middleOption);
		else
			ApplyOption(_rightOption);
	}
}
