public partial class GameFlowUI
{
	private void RefreshPauseBuildSummary()
	{
		if (_pauseBuildSummaryLabel == null)
			return;

		if (_upgradeSystem == null || _upgradeSystem.AppliedUpgradeCount <= 0)
		{
			_pauseBuildSummaryLabel.Text = $"{Tr("UI.BUILD.CURRENT")}\n{Tr("UI.BUILD.NONE_YET")}";
			return;
		}

		_pauseBuildSummaryLabel.Text =
			Tr("UI.BUILD.CURRENT") + "\n" +
			_upgradeSystem.GetCategoryShareSummary() + "\n" +
			_upgradeSystem.GetKeyUpgradeSummary(6);
	}

	private void RefreshFinalBuildSummary()
	{
		if (_finalBuildSummaryLabel == null)
			return;

		if (_upgradeSystem == null || _upgradeSystem.AppliedUpgradeCount <= 0)
		{
			_finalBuildSummaryLabel.Text = $"{Tr("UI.BUILD.SUMMARY")}\n{Tr("UI.BUILD.NONE_IN_RUN")}";
			return;
		}

		_finalBuildSummaryLabel.Text =
			Tr("UI.BUILD.SUMMARY") + "\n" +
			_upgradeSystem.GetCategoryShareSummary() + "\n" +
			_upgradeSystem.GetKeyUpgradeSummary(8);
	}

	private void ResetBuildSummaryLabels()
	{
		if (_pauseBuildSummaryLabel != null)
			_pauseBuildSummaryLabel.Text = $"{Tr("UI.BUILD.CURRENT")}\n{Tr("UI.BUILD.NONE_YET")}";
		if (_finalBuildSummaryLabel != null)
			_finalBuildSummaryLabel.Text = Tr("UI.BUILD.SUMMARY");
	}
}
