using Godot;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public partial class PressureSystem
{
	private void LoadTierRulesFromCsv()
	{
		_tierRules.Clear();

		if (string.IsNullOrWhiteSpace(PressureTierRulesCsvPath))
		{
			DebugSystem.Warn("[PressureSystem] PressureTierRulesCsvPath is empty. Using inspector values.");
			return;
		}

		if (!FileAccess.FileExists(PressureTierRulesCsvPath))
		{
			DebugSystem.Warn($"[PressureSystem] CSV not found: {PressureTierRulesCsvPath}. Using inspector values.");
			return;
		}

		using var file = FileAccess.Open(PressureTierRulesCsvPath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			DebugSystem.Warn($"[PressureSystem] Failed to open CSV: {PressureTierRulesCsvPath}. Using inspector values.");
			return;
		}

		while (!file.EofReached())
		{
			string line = file.GetLine().Trim();
			if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
				continue;

			var cols = ParseCsvLine(line);
			if (cols.Count < 18)
				continue;

			var rule = new TierRule
			{
				Tier = ParseInt(cols[0], 0),
				PressureMin = ParseFloat(cols[1], 0f),
				PressureMax = ParseFloat(cols[2], 100f),
				MaxAlive = ParseInt(cols[7], EnemyCountForMaxPressure),
				KillProgressBase = ParseFloat(cols[13], KillProgressBase),
				KillPressureBonusFactor = ParseFloat(cols[14], KillPressureBonusFactor),
				TimeProgressPerSecond = ParseFloat(cols[15], TimeProgressPerSecond),
				UpgradeThreshold = ParseFloat(cols[16], TriggerThreshold),
				FirstUpgradeThreshold = ParseFloat(cols[17], FirstTriggerThreshold)
			};

			_tierRules.Add(rule);
		}

		_tierRules.Sort((a, b) => a.PressureMin.CompareTo(b.PressureMin));
		DebugSystem.Log($"[PressureSystem] Loaded {_tierRules.Count} tier rule rows from CSV.");
	}

	private void UpdateTierRuntimeSettings()
	{
		if (!UseTierRulesCsv || _tierRules.Count == 0)
			return;

		int idx = -1;
		for (int i = 0; i < _tierRules.Count; i++)
		{
			var rule = _tierRules[i];
			if (_pressure >= rule.PressureMin && _pressure < rule.PressureMax)
			{
				idx = i;
				break;
			}
		}
		if (idx < 0)
			idx = _tierRules.Count - 1;

		if (idx == _activeTierIndex)
			return;

		_activeTierIndex = idx;
		var active = _tierRules[idx];

		EnemyCountForMaxPressure = Mathf.Max(1, active.MaxAlive);
		KillProgressBase = Mathf.Max(0f, active.KillProgressBase);
		KillPressureBonusFactor = Mathf.Max(0f, active.KillPressureBonusFactor);
		TimeProgressPerSecond = Mathf.Max(0f, active.TimeProgressPerSecond);
		TriggerThreshold = Mathf.Clamp(active.UpgradeThreshold, 1f, MaxUpgradeProgress);
		FirstTriggerThreshold = Mathf.Clamp(active.FirstUpgradeThreshold, 1f, MaxUpgradeProgress);

		DebugSystem.Log($"[PressureSystem] Tier {active.Tier} active: killBase={KillProgressBase:F1}, bonus={KillPressureBonusFactor:F2}, drip={TimeProgressPerSecond:F2}, threshold={TriggerThreshold:F1}");
	}

	private static List<string> ParseCsvLine(string line)
	{
		var result = new List<string>();
		var sb = new StringBuilder();
		bool inQuotes = false;

		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (c == '"')
			{
				inQuotes = !inQuotes;
				continue;
			}

			if (c == ',' && !inQuotes)
			{
				result.Add(sb.ToString().Trim());
				sb.Clear();
				continue;
			}

			sb.Append(c);
		}

		result.Add(sb.ToString().Trim());
		return result;
	}

	private static float ParseFloat(string s, float fallback)
	{
		if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
			return v;
		return fallback;
	}

	private static int ParseInt(string s, int fallback)
	{
		if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
			return v;
		return fallback;
	}

	private struct TierRule
	{
		public int Tier;
		public float PressureMin;
		public float PressureMax;
		public int MaxAlive;
		public float KillProgressBase;
		public float KillPressureBonusFactor;
		public float TimeProgressPerSecond;
		public float UpgradeThreshold;
		public float FirstUpgradeThreshold;
	}
}
