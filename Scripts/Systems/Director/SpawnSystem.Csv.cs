using Godot;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public partial class SpawnSystem
{
	private void LoadTierRulesFromCsv()
	{
		_tierRules.Clear();

		if (!TryReadCsvLines(PressureTierRulesCsvPath, out List<string> lines))
			return;

		foreach (string line in lines)
		{
			List<string> cols = ParseCsvLine(line);
			if (cols.Count < 10)
				continue;

			_tierRules.Add(new TierRule
			{
				Tier = ParseInt(cols[0], 0),
				PressureMin = ParseFloat(cols[1], 0f),
				PressureMax = ParseFloat(cols[2], 100f),
				SpawnIntervalMin = ParseFloat(cols[3], SpawnInterval),
				SpawnIntervalMax = ParseFloat(cols[4], SpawnInterval),
				BudgetMin = ParseInt(cols[5], 1),
				BudgetMax = ParseInt(cols[6], 1),
				MaxAlive = ParseInt(cols[7], MaxAliveEnemies),
				SpawnRadiusMin = ParseFloat(cols[8], SpawnRadiusMin),
				SpawnRadiusMax = ParseFloat(cols[9], SpawnRadiusMax)
			});
		}

		_tierRules.Sort((a, b) => a.PressureMin.CompareTo(b.PressureMin));
		DebugSystem.Log($"[SpawnSystem] Loaded {_tierRules.Count} rows from {PressureTierRulesCsvPath}.");
	}

	private void LoadEnemyDefinitionsFromCsv()
	{
		_enemyDefinitions.Clear();

		if (!TryReadCsvLines(EnemyDefinitionsCsvPath, out List<string> lines))
			return;

		foreach (string line in lines)
		{
			List<string> cols = ParseCsvLine(line);
			if (cols.Count < 5)
				continue;

			string id = cols[0];
			string scenePath = cols[1];
			int cost = ParseInt(cols[3], 1);
			int minTier = ParseInt(cols[4], 0);
			int hpOverride = cols.Count > 6 ? ParseInt(cols[6], 0) : 0;
			float speedOverride = cols.Count > 7 ? ParseFloat(cols[7], 0f) : 0f;
			int contactDamageOverride = cols.Count > 8 ? ParseInt(cols[8], 0) : 0;

			if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(scenePath))
				continue;

			PackedScene scene = GD.Load<PackedScene>(scenePath);
			if (scene == null)
			{
				DebugSystem.Warn($"[SpawnSystem] Missing enemy scene for '{id}': {scenePath}");
				continue;
			}

			_enemyDefinitions[id] = new EnemyDefinition
			{
				Id = id,
				ScenePath = scenePath,
				Cost = Mathf.Max(1, cost),
				MinTier = minTier,
				HpOverride = Mathf.Max(0, hpOverride),
				SpeedOverride = Mathf.Max(0f, speedOverride),
				ContactDamageOverride = Mathf.Max(0, contactDamageOverride),
				Scene = scene
			};
		}

		DebugSystem.Log($"[SpawnSystem] Loaded {_enemyDefinitions.Count} enemy definitions from {EnemyDefinitionsCsvPath}.");
	}

	private void LoadTierWeightsFromCsv()
	{
		_tierWeights.Clear();

		if (!TryReadCsvLines(TierEnemyWeightsCsvPath, out List<string> lines))
			return;

		foreach (string line in lines)
		{
			List<string> cols = ParseCsvLine(line);
			if (cols.Count < 3)
				continue;

			int tier = ParseInt(cols[0], -1);
			string enemyId = cols[1];
			float weight = ParseFloat(cols[2], 0f);

			if (tier < 0 || string.IsNullOrWhiteSpace(enemyId) || weight <= 0f)
				continue;

			if (!_tierWeights.TryGetValue(tier, out List<WeightedEnemy> list))
			{
				list = new List<WeightedEnemy>();
				_tierWeights[tier] = list;
			}

			list.Add(new WeightedEnemy { EnemyId = enemyId, Weight = weight });
		}

		DebugSystem.Log($"[SpawnSystem] Loaded weights for {_tierWeights.Count} tiers from {TierEnemyWeightsCsvPath}.");
	}

	private static bool TryReadCsvLines(string path, out List<string> lines)
	{
		lines = new List<string>();

		if (string.IsNullOrWhiteSpace(path))
		{
			DebugSystem.Warn("[SpawnSystem] CSV path is empty.");
			return false;
		}

		if (!FileAccess.FileExists(path))
		{
			DebugSystem.Warn($"[SpawnSystem] CSV not found: {path}");
			return false;
		}

		using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			DebugSystem.Warn($"[SpawnSystem] Failed to open CSV: {path}");
			return false;
		}

		while (!file.EofReached())
		{
			string line = file.GetLine().Trim();
			if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
				continue;

			lines.Add(line);
		}

		return true;
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
}
