using System;
using System.Collections.Generic;
using Godot;

public partial class GameFlowUI
{
	private const int PerfectLeaderboardDisplayCount = 5;

	private void RecordPerfectClear(int score, string characterName)
	{
		MetaProgressionService.Instance.RecordPerfectClear(score, characterName);
		RefreshPerfectLeaderboardUi();
	}

	private void RefreshPerfectLeaderboardUi()
	{
		if (_startPerfectLeaderboardLabel == null)
			return;

		IReadOnlyList<PerfectClearRecord> entries = MetaProgressionService.Instance.GetPerfectLeaderboard(PerfectLeaderboardDisplayCount);
		if (entries.Count == 0)
		{
			_startPerfectLeaderboardLabel.Text = Tr("UI.START.PERFECT_BOARD_EMPTY");
			return;
		}

		string text = string.Empty;
		for (int i = 0; i < entries.Count; i++)
		{
			PerfectClearRecord record = entries[i];
			string dateText = record.UnixTime > 0
				? DateTimeOffset.FromUnixTimeSeconds(record.UnixTime).LocalDateTime.ToString("yyyy-MM-dd HH:mm")
				: "-";
			text += $"{i + 1}. {record.CharacterName}  |  {Tr("UI.HUD.SCORE")} {record.Score}  |  {dateText}";
			if (i < entries.Count - 1)
				text += "\n";
		}

		_startPerfectLeaderboardLabel.Text = text;
	}
}
