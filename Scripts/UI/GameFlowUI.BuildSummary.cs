public partial class GameFlowUI
{
	private void RefreshPauseBuildSummary()
	{
		if (_pauseBuildSummaryLabel == null)
			return;

		if (_upgradeSystem == null || _upgradeSystem.AppliedUpgradeCount <= 0)
		{
			_pauseBuildSummaryLabel.Text = "Current Build\nNo upgrades selected yet.";
			return;
		}

		_pauseBuildSummaryLabel.Text =
			"Current Build\n" +
			_upgradeSystem.GetCategoryShareSummary() + "\n" +
			_upgradeSystem.GetKeyUpgradeSummary(6);
	}

	private void RefreshFinalBuildSummary()
	{
		if (_finalBuildSummaryLabel == null)
			return;

		if (_upgradeSystem == null || _upgradeSystem.AppliedUpgradeCount <= 0)
		{
			_finalBuildSummaryLabel.Text = "Build Summary\nNo upgrades selected in this run.";
			return;
		}

		_finalBuildSummaryLabel.Text =
			"Build Summary\n" +
			_upgradeSystem.GetCategoryShareSummary() + "\n" +
			_upgradeSystem.GetKeyUpgradeSummary(8);
	}

	private void ResetBuildSummaryLabels()
	{
		if (_pauseBuildSummaryLabel != null)
			_pauseBuildSummaryLabel.Text = "Current Build\nNo upgrades selected yet.";
		if (_finalBuildSummaryLabel != null)
			_finalBuildSummaryLabel.Text = "Build Summary";
	}
}
