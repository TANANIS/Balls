using Godot;

public partial class GameFlowUI
{
	private void OnScoreChanged(int score)
	{
		if (_scoreLabel != null)
			_scoreLabel.Text = $"{Tr("UI.HUD.SCORE")}: {score}";
	}

	private void UpdateUpgradeProgressUi()
	{
		if (_experienceBarRoot == null || _experienceBar == null || _experienceLabel == null)
			return;

		if (!_started || _ending)
		{
			_experienceBarRoot.Visible = false;
			return;
		}

		_experienceBarRoot.Visible = true;

		if (!IsInstanceValid(_progressionSystem))
		{
			var progressionList = GetTree().GetNodesInGroup(RuntimeGroups.ProgressionSystem);
			if (progressionList.Count > 0)
				_progressionSystem = progressionList[0] as ProgressionSystem;
		}

		if (!IsInstanceValid(_progressionSystem))
		{
			_experienceBar.MaxValue = 1f;
			_experienceBar.Value = 0f;
			_experienceLabel.Text = $"{Tr("UI.HUD.XP")} --/--";
			return;
		}

		float required = Mathf.Max(1f, _progressionSystem.GetCurrentUpgradeRequirement());
		float progress = Mathf.Clamp(_progressionSystem.CurrentUpgradeProgress, 0f, required);

		_experienceBar.MaxValue = required;
		_experienceBar.Value = progress;
		_experienceLabel.Text = _progressionSystem.IsUpgradeReady
			? $"{Tr("UI.HUD.LEVEL")} {_progressionSystem.CurrentUpgradeLevel}  {Tr("UI.HUD.READY")} x{Mathf.Max(1, _progressionSystem.PendingUpgradeCount)}"
			: $"{Tr("UI.HUD.LEVEL")} {_progressionSystem.CurrentUpgradeLevel}  {Tr("UI.HUD.XP")} {Mathf.FloorToInt(progress)}/{Mathf.CeilToInt(required)}";
	}

	private void UpdateMatchCountdownUi()
	{
		if (_matchCountdownLabel == null)
			return;

		if (!_started || _ending || _stabilitySystem == null)
		{
			_matchCountdownLabel.Visible = false;
			return;
		}

		_matchCountdownLabel.Visible = true;
		float limit = Mathf.Max(1f, _stabilitySystem.MatchDurationLimitSeconds);
		float remain = Mathf.Max(0f, limit - _stabilitySystem.ElapsedSeconds);
		int total = Mathf.CeilToInt(remain);
		int mm = total / 60;
		int ss = total % 60;
		_matchCountdownLabel.Text = $"{mm:D2}:{ss:D2}";
	}

	private void UpdateEventBannerUi()
	{
		if (_eventBannerLabel == null)
			return;

		if (!_started || _ending)
		{
			_eventBannerLabel.Visible = false;
			return;
		}

		if (!IsInstanceValid(_eventDirector))
		{
			var eventDirectorList = GetTree().GetNodesInGroup(RuntimeGroups.EventDirector);
			if (eventDirectorList.Count > 0)
				_eventDirector = eventDirectorList[0] as EventDirector;
		}

		if (!IsInstanceValid(_eventDirector) || !_eventDirector.IsEventActive)
		{
			_eventBannerLabel.Visible = false;
			return;
		}

		_eventBannerLabel.Visible = true;
		_eventBannerLabel.Text = _eventDirector.ActiveEventBannerText;
	}

	private void UpdateEventHintUi()
	{
		if (_eventHintLabel == null)
			return;

		if (!_started || _ending)
		{
			_eventHintLabel.Visible = false;
			return;
		}

		if (!IsInstanceValid(_eventDirector))
		{
			var eventDirectorList = GetTree().GetNodesInGroup(RuntimeGroups.EventDirector);
			if (eventDirectorList.Count > 0)
				_eventDirector = eventDirectorList[0] as EventDirector;
		}

		if (!IsInstanceValid(_eventDirector) || !_eventDirector.IsEventActive)
		{
			_eventHintLabel.Visible = false;
			return;
		}

		string text = _eventDirector.ActiveEventHintText ?? string.Empty;
		if (string.IsNullOrWhiteSpace(text))
		{
			_eventHintLabel.Visible = false;
			return;
		}

		_eventHintLabel.Visible = true;
		_eventHintLabel.Text = text;
	}

	private void UpdateHybridToastUi()
	{
		if (_hybridToastLabel == null)
			return;

		if (!_started || _ending)
		{
			_hybridToastLabel.Visible = false;
			return;
		}

		if (!IsInstanceValid(_eventDirector))
		{
			var eventDirectorList = GetTree().GetNodesInGroup(RuntimeGroups.EventDirector);
			if (eventDirectorList.Count > 0)
				_eventDirector = eventDirectorList[0] as EventDirector;
		}

		if (!IsInstanceValid(_eventDirector))
		{
			_hybridToastLabel.Visible = false;
			return;
		}

		string text = _eventDirector.ActiveHybridToastText ?? string.Empty;
		if (string.IsNullOrWhiteSpace(text))
		{
			_hybridToastLabel.Visible = false;
			return;
		}

		_hybridToastLabel.Visible = true;
		_hybridToastLabel.Text = text;
	}

	private void OnViewportSizeChanged()
	{
	}
}
