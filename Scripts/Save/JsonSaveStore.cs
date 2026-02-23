using Godot;
using System;
using System.IO;
using System.Text.Json;

public sealed class JsonSaveStore
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true
	};

	private string _savePath;
	private string _profileId;

	public string SavePath => _savePath;
	public string ProfileId => _profileId;

	public JsonSaveStore(string profileId = "default")
	{
		SetProfile(profileId);
	}

	public void SetProfile(string profileId)
	{
		_profileId = NormalizeProfileId(profileId);
		_savePath = BuildSavePath(_profileId);
	}

	public bool DeleteSaveFile()
	{
		try
		{
			string globalPath = ProjectSettings.GlobalizePath(_savePath);
			if (!System.IO.File.Exists(globalPath))
				return true;
			System.IO.File.Delete(globalPath);
			return true;
		}
		catch (Exception ex)
		{
			GD.PushError($"[JsonSaveStore] Failed to delete meta save at '{_savePath}'. {ex.Message}");
			return false;
		}
	}

	public MetaProgressionState LoadState()
	{
		MetaSaveDto dto = LoadDto();
		return SaveMigrator.ToDomain(dto);
	}

	public void SaveState(MetaProgressionState state)
	{
		MetaSaveDto dto = SaveMigrator.ToDto(state);
		SaveDto(dto);
	}

	public MetaSaveDto LoadDto()
	{
		if (!Godot.FileAccess.FileExists(_savePath))
			return new MetaSaveDto();

		try
		{
			string json = Godot.FileAccess.GetFileAsString(_savePath);
			if (string.IsNullOrWhiteSpace(json))
				return new MetaSaveDto();

			MetaSaveDto dto = JsonSerializer.Deserialize<MetaSaveDto>(json, JsonOptions);
			return SaveMigrator.MigrateToCurrent(dto);
		}
		catch (Exception ex)
		{
			GD.PushWarning($"[JsonSaveStore] Failed to load meta save at '{_savePath}'. Using defaults. {ex.Message}");
			return new MetaSaveDto();
		}
	}

	public void SaveDto(MetaSaveDto dto)
	{
		dto = SaveMigrator.MigrateToCurrent(dto);

		try
		{
			EnsureParentDirectoryExists(_savePath);
			string json = JsonSerializer.Serialize(dto, JsonOptions);
			using Godot.FileAccess file = Godot.FileAccess.Open(_savePath, Godot.FileAccess.ModeFlags.Write);
			if (file == null)
				throw new InvalidOperationException($"Cannot open '{_savePath}' for writing.");
			file.StoreString(json);
		}
		catch (Exception ex)
		{
			GD.PushError($"[JsonSaveStore] Failed to save meta progression to '{_savePath}'. {ex.Message}");
		}
	}

	private static void EnsureParentDirectoryExists(string savePath)
	{
		string globalPath = ProjectSettings.GlobalizePath(savePath);
		string parentDir = System.IO.Path.GetDirectoryName(globalPath);
		if (string.IsNullOrWhiteSpace(parentDir))
			return;

		DirAccess.MakeDirRecursiveAbsolute(parentDir);
	}

	private static string BuildSavePath(string profileId)
	{
		return $"user://saves/{profileId}/meta_progression.json";
	}

	private static string NormalizeProfileId(string profileId)
	{
		if (string.IsNullOrWhiteSpace(profileId))
			return "default";

		var chars = profileId.Trim().ToCharArray();
		for (int i = 0; i < chars.Length; i++)
		{
			char c = chars[i];
			bool valid = char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.';
			if (!valid)
				chars[i] = '_';
		}

		string normalized = new string(chars);
		return string.IsNullOrWhiteSpace(normalized) ? "default" : normalized;
	}
}
