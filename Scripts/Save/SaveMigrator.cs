using System;
using System.Collections.Generic;

public static class SaveMigrator
{
	public static MetaSaveDto MigrateToCurrent(MetaSaveDto dto)
	{
		dto ??= new MetaSaveDto();

		if (dto.Version <= 0)
			dto.Version = 1;

		// Future migrations should be applied step-by-step by version.
		if (dto.Version < MetaSaveDto.CurrentVersion)
			dto.Version = MetaSaveDto.CurrentVersion;

		dto.CurrencyWallet = Math.Max(0, dto.CurrencyWallet);
		dto.CurrencyEarnedTotal = Math.Max(0, dto.CurrencyEarnedTotal);
		dto.CurrencySpentTotal = Math.Max(0, dto.CurrencySpentTotal);
		dto.UnlockedCharacterIds ??= new List<string>();
		dto.CharacterProgressById ??= new Dictionary<string, CharacterProgressDto>(StringComparer.Ordinal);
		dto.MetaFlags ??= new List<string>();
		dto.SettledRunIds ??= new List<string>();
		dto.PerfectClearRecords ??= new List<PerfectClearRecord>();
		for (int i = dto.PerfectClearRecords.Count - 1; i >= 0; i--)
		{
			PerfectClearRecord record = dto.PerfectClearRecords[i];
			if (record == null)
			{
				dto.PerfectClearRecords.RemoveAt(i);
				continue;
			}
			record.Normalize();
		}

		var normalized = new Dictionary<string, CharacterProgressDto>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, CharacterProgressDto> pair in dto.CharacterProgressById)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;

			CharacterProgressDto progress = pair.Value ?? new CharacterProgressDto();
			progress.Level = Math.Max(1, progress.Level);
			progress.UnlockedAbilityNodes ??= new List<string>();
			normalized[pair.Key] = progress;
		}
		dto.CharacterProgressById = normalized;

		return dto;
	}

	public static MetaProgressionState ToDomain(MetaSaveDto dto)
	{
		dto = MigrateToCurrent(dto);
		var state = new MetaProgressionState();
		state.ReplaceCurrencySnapshot(dto.CurrencyWallet, dto.CurrencyEarnedTotal, dto.CurrencySpentTotal);

		foreach (string id in dto.UnlockedCharacterIds)
			state.UnlockCharacter(id);

		foreach (KeyValuePair<string, CharacterProgressDto> pair in dto.CharacterProgressById)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;

			CharacterProgress progress = state.EnsureCharacterProgress(pair.Key);
			if (progress == null)
				continue;
			progress.SetLevel(pair.Value?.Level ?? 1);

			if (pair.Value?.UnlockedAbilityNodes == null)
				continue;
			foreach (string nodeId in pair.Value.UnlockedAbilityNodes)
				progress.UnlockAbilityNode(nodeId);
		}

		state.Flags.ReplaceAll(dto.MetaFlags);
		foreach (string runId in dto.SettledRunIds)
			state.MarkRunSettled(runId);
		foreach (PerfectClearRecord record in dto.PerfectClearRecords)
		{
			if (record == null)
				continue;
			state.AddPerfectClearRecord(record.Score, record.CharacterName, record.UnixTime);
		}

		return state;
	}

	public static MetaSaveDto ToDto(MetaProgressionState state)
	{
		state ??= new MetaProgressionState();
		var dto = new MetaSaveDto
		{
			Version = MetaSaveDto.CurrentVersion,
			CurrencyWallet = state.CurrencyWallet,
			CurrencyEarnedTotal = state.CurrencyEarnedTotal,
			CurrencySpentTotal = state.CurrencySpentTotal
		};

		dto.UnlockedCharacterIds.AddRange(state.UnlockedCharacterIds);
		dto.MetaFlags.AddRange(state.Flags.Values);
		dto.SettledRunIds.AddRange(state.SettledRunIds);
		dto.PerfectClearRecords.AddRange(state.PerfectClearRecords);

		foreach (KeyValuePair<string, CharacterProgress> pair in state.CharacterProgressById)
		{
			if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value == null)
				continue;

			var progressDto = new CharacterProgressDto
			{
				Level = Math.Max(1, pair.Value.Level)
			};
			progressDto.UnlockedAbilityNodes.AddRange(pair.Value.UnlockedAbilityNodes);
			dto.CharacterProgressById[pair.Key] = progressDto;
		}

		return dto;
	}
}
