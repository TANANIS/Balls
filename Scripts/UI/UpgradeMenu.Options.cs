using Godot;

public partial class UpgradeMenu
{
	private bool PickOptions()
	{
		if (_upgradeSystem == null)
			return false;

		if (!_upgradeSystem.TryPickOptions(_rng, 3, out var picks) || picks.Count <= 0)
		{
			return false;
		}

		_leftOption = picks[0];
		_middleOption = picks.Count > 1 ? picks[1] : default;
		_rightOption = picks.Count > 2 ? picks[2] : default;
		_availableOptionCount = picks.Count;
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
		if (_availableOptionCount <= 0)
			return;

		int roll = _rng.RandiRange(0, _availableOptionCount - 1);
		ApplyOptionByIndex(roll);
	}

	private void ApplyOptionByIndex(int index)
	{
		if (index == 0 && _availableOptionCount >= 1)
			ApplyOption(_leftOption);
		else if (index == 1 && _availableOptionCount >= 2)
			ApplyOption(_middleOption);
		else if (index == 2 && _availableOptionCount >= 3)
			ApplyOption(_rightOption);
	}
}
