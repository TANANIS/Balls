using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;

public static class CharacterStatsCsvService
{
	private const string CsvPath = "res://Data/Characters/CharacterStats.csv";
	private static readonly Dictionary<string, CharacterStatsRow> Rows = new(StringComparer.OrdinalIgnoreCase);
	private static bool _loaded;

	private readonly struct CharacterStatsRow
	{
		public readonly int MaxHp;
		public readonly float MoveMaxSpeed;
		public readonly float RangedCooldown;
		public readonly float MeleeCooldown;
		public readonly float DashCooldown;

		public CharacterStatsRow(int maxHp, float moveMaxSpeed, float rangedCooldown, float meleeCooldown, float dashCooldown)
		{
			MaxHp = maxHp;
			MoveMaxSpeed = moveMaxSpeed;
			RangedCooldown = rangedCooldown;
			MeleeCooldown = meleeCooldown;
			DashCooldown = dashCooldown;
		}
	}

	public static void ApplyTo(CharacterDefinition definition)
	{
		if (definition == null || string.IsNullOrWhiteSpace(definition.CharacterId))
			return;

		EnsureLoaded();

		if (!Rows.TryGetValue(definition.CharacterId.Trim(), out CharacterStatsRow row))
			return;

		definition.MaxHp = Mathf.Max(1, row.MaxHp);
		definition.MoveMaxSpeed = Mathf.Max(0f, row.MoveMaxSpeed);
		definition.RangedCooldown = Mathf.Max(0.01f, row.RangedCooldown);
		definition.MeleeCooldown = Mathf.Max(0.01f, row.MeleeCooldown);
		definition.DashCooldown = Mathf.Max(0.01f, row.DashCooldown);
	}

	private static void EnsureLoaded()
	{
		if (_loaded)
			return;

		_loaded = true;
		Rows.Clear();

		if (!FileAccess.FileExists(CsvPath))
		{
			GD.PushWarning($"[CharacterStatsCsvService] Missing CSV: {CsvPath}");
			return;
		}

		using FileAccess file = FileAccess.Open(CsvPath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PushWarning($"[CharacterStatsCsvService] Failed opening CSV: {CsvPath}");
			return;
		}

		bool skippedHeader = false;
		while (!file.EofReached())
		{
			string raw = file.GetLine();
			if (string.IsNullOrWhiteSpace(raw))
				continue;

			string line = raw.Trim();
			if (line.StartsWith("#"))
				continue;

			if (!skippedHeader)
			{
				skippedHeader = true;
				continue;
			}

			string[] cols = line.Split(',');
			if (cols.Length < 6)
				continue;

			string id = cols[0].Trim();
			if (string.IsNullOrWhiteSpace(id))
				continue;

			if (!TryParseInt(cols[1], out int maxHp))
				continue;
			if (!TryParseFloat(cols[2], out float moveMaxSpeed))
				continue;
			if (!TryParseFloat(cols[3], out float rangedCooldown))
				continue;
			if (!TryParseFloat(cols[4], out float meleeCooldown))
				continue;
			if (!TryParseFloat(cols[5], out float dashCooldown))
				continue;

			Rows[id] = new CharacterStatsRow(maxHp, moveMaxSpeed, rangedCooldown, meleeCooldown, dashCooldown);
		}

		GD.Print($"[CharacterStatsCsvService] Loaded {Rows.Count} rows.");
	}

	private static bool TryParseInt(string token, out int value)
	{
		return int.TryParse(token.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
	}

	private static bool TryParseFloat(string token, out float value)
	{
		return float.TryParse(token.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
	}
}
