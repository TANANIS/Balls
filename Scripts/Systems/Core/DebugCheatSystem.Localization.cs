using Godot;
using System.Collections.Generic;
using System.Text;

public partial class DebugCheatSystem
{
	private static string Bi(string en, string zh)
	{
		return $"{en} / {zh}";
	}

	private string ResolveZhByKey(string key, string fallback)
	{
		if (string.IsNullOrWhiteSpace(key))
			return fallback;

		EnsureCardZhLookupLoaded();
		if (_cardZhLookup.TryGetValue(key, out string zh) && !string.IsNullOrWhiteSpace(zh))
			return zh;

		return fallback;
	}

	private void EnsureCardZhLookupLoaded()
	{
		if (_cardZhLookupLoaded)
			return;

		_cardZhLookupLoaded = true;
		_cardZhLookup.Clear();

		if (!FileAccess.FileExists(CardLocalizationCsvPath))
			return;

		using FileAccess file = FileAccess.Open(CardLocalizationCsvPath, FileAccess.ModeFlags.Read);
		if (file == null)
			return;

		while (!file.EofReached())
		{
			string line = file.GetLine();
			if (string.IsNullOrWhiteSpace(line))
				continue;
			if (line.StartsWith("keys,"))
				continue;
			if (!TrySplitCsv3(line, out string key, out _, out string zh))
				continue;
			if (string.IsNullOrWhiteSpace(key))
				continue;

			_cardZhLookup[key] = zh.Trim();
		}
	}

	private static bool TrySplitCsv3(string line, out string col0, out string col1, out string col2)
	{
		col0 = string.Empty;
		col1 = string.Empty;
		col2 = string.Empty;
		if (string.IsNullOrEmpty(line))
			return false;

		List<string> cols = SplitCsvLine(line);
		if (cols.Count < 3)
			return false;

		col0 = cols[0].Trim();
		col1 = cols[1].Trim();
		col2 = cols[2].Trim();
		return true;
	}

	private static List<string> SplitCsvLine(string line)
	{
		var cols = new List<string>();
		if (line == null)
			return cols;

		var sb = new StringBuilder(line.Length);
		bool inQuotes = false;
		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (c == '"')
			{
				// Escaped quote inside quoted field: ""
				if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
				{
					sb.Append('"');
					i++;
					continue;
				}

				inQuotes = !inQuotes;
				continue;
			}

			if (c == ',' && !inQuotes)
			{
				cols.Add(sb.ToString());
				sb.Clear();
				continue;
			}

			sb.Append(c);
		}

		cols.Add(sb.ToString());
		return cols;
	}
}
