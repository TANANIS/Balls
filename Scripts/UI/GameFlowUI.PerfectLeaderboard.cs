using Godot;
using System;
using System.Collections.Generic;

public partial class GameFlowUI
{
	private const string PerfectLeaderboardPath = "user://perfect_1500_leaderboard.cfg";
	private const string PerfectLeaderboardSection = "perfect_1500";
	private const int PerfectLeaderboardMaxEntries = 10;
	private const int PerfectLeaderboardDisplayCount = 5;

	private readonly struct PerfectLeaderboardEntry
	{
		public readonly int Score;
		public readonly long UnixTime;
		public readonly string DateText;
		public readonly string CharacterName;

		public PerfectLeaderboardEntry(int score, long unixTime, string dateText, string characterName)
		{
			Score = Mathf.Max(0, score);
			UnixTime = Math.Max(0L, unixTime);
			DateText = string.IsNullOrWhiteSpace(dateText) ? "-" : dateText;
			CharacterName = string.IsNullOrWhiteSpace(characterName) ? "Unknown" : characterName.Trim();
		}
	}

	private void RecordPerfectClear(int score, string characterName)
	{
		var entries = LoadPerfectLeaderboard();
		DateTime now = DateTime.Now;
		long unixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
		entries.Add(new PerfectLeaderboardEntry(score, unixTime, now.ToString("yyyy-MM-dd HH:mm"), characterName));
		entries.Sort(ComparePerfectEntries);
		if (entries.Count > PerfectLeaderboardMaxEntries)
			entries.RemoveRange(PerfectLeaderboardMaxEntries, entries.Count - PerfectLeaderboardMaxEntries);

		SavePerfectLeaderboard(entries);
		RefreshPerfectLeaderboardUi();
	}

	private void RefreshPerfectLeaderboardUi()
	{
		if (_startPerfectLeaderboardLabel == null)
			return;

		var entries = LoadPerfectLeaderboard();
		if (entries.Count == 0)
		{
			_startPerfectLeaderboardLabel.Text = "No perfect clears yet.\nSurvive the full 15:00 to enter local leaderboard.";
			return;
		}

		int count = Mathf.Min(PerfectLeaderboardDisplayCount, entries.Count);
		string text = "";
		for (int i = 0; i < count; i++)
		{
			PerfectLeaderboardEntry e = entries[i];
			text += $"{i + 1}. {e.CharacterName}  |  Score {e.Score}  |  {e.DateText}";
			if (i < count - 1)
				text += "\n";
		}

		_startPerfectLeaderboardLabel.Text = text;
	}

	private List<PerfectLeaderboardEntry> LoadPerfectLeaderboard()
	{
		var entries = new List<PerfectLeaderboardEntry>();
		var cfg = new ConfigFile();
		if (cfg.Load(PerfectLeaderboardPath) != Error.Ok)
			return entries;

		int count = VariantToInt(cfg.GetValue(PerfectLeaderboardSection, "count", 0));
		for (int i = 0; i < count; i++)
		{
			int score = VariantToInt(cfg.GetValue(PerfectLeaderboardSection, $"score_{i}", 0));
			long unix = VariantToLong(cfg.GetValue(PerfectLeaderboardSection, $"time_{i}", 0L));
			string date = VariantToString(cfg.GetValue(PerfectLeaderboardSection, $"date_{i}", string.Empty));
			string character = VariantToString(cfg.GetValue(PerfectLeaderboardSection, $"character_{i}", "Unknown"));
			if (string.IsNullOrWhiteSpace(date) && unix > 0)
				date = DateTimeOffset.FromUnixTimeSeconds(unix).LocalDateTime.ToString("yyyy-MM-dd HH:mm");
			entries.Add(new PerfectLeaderboardEntry(score, unix, date, character));
		}

		entries.Sort(ComparePerfectEntries);
		return entries;
	}

	private void SavePerfectLeaderboard(List<PerfectLeaderboardEntry> entries)
	{
		var cfg = new ConfigFile();
		cfg.SetValue(PerfectLeaderboardSection, "count", entries.Count);
		for (int i = 0; i < entries.Count; i++)
		{
			PerfectLeaderboardEntry entry = entries[i];
			cfg.SetValue(PerfectLeaderboardSection, $"score_{i}", entry.Score);
			cfg.SetValue(PerfectLeaderboardSection, $"time_{i}", entry.UnixTime);
			cfg.SetValue(PerfectLeaderboardSection, $"date_{i}", entry.DateText);
			cfg.SetValue(PerfectLeaderboardSection, $"character_{i}", entry.CharacterName);
		}

		cfg.Save(PerfectLeaderboardPath);
	}

	private void ClearPerfectLeaderboard()
	{
		if (!FileAccess.FileExists(PerfectLeaderboardPath))
			return;

		Error removeError = DirAccess.RemoveAbsolute(PerfectLeaderboardPath);
		if (removeError != Error.Ok)
		{
			// Fallback to writing an empty board if file delete fails for any reason.
			SavePerfectLeaderboard(new List<PerfectLeaderboardEntry>());
			DebugSystem.Warn($"[GameFlowUI] Failed to delete leaderboard file ({removeError}), wrote empty leaderboard instead.");
		}
	}

	private static int ComparePerfectEntries(PerfectLeaderboardEntry a, PerfectLeaderboardEntry b)
	{
		int byScore = b.Score.CompareTo(a.Score);
		if (byScore != 0)
			return byScore;
		return b.UnixTime.CompareTo(a.UnixTime);
	}

	private static int VariantToInt(Variant value)
	{
		return value.VariantType switch
		{
			Variant.Type.Int => (int)(long)value,
			Variant.Type.Float => Mathf.RoundToInt((float)(double)value),
			Variant.Type.String => int.TryParse((string)value, out int parsed) ? parsed : 0,
			_ => 0
		};
	}

	private static long VariantToLong(Variant value)
	{
		return value.VariantType switch
		{
			Variant.Type.Int => (long)value,
			Variant.Type.Float => (long)(double)value,
			Variant.Type.String => long.TryParse((string)value, out long parsed) ? parsed : 0L,
			_ => 0L
		};
	}

	private static string VariantToString(Variant value)
	{
		return value.VariantType == Variant.Type.String ? (string)value : string.Empty;
	}
}
