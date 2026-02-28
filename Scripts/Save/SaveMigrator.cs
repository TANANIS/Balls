using System;
using System.Collections.Generic;
using System.Linq;

public static class SaveMigrator
{
	private const string LegacyMeleeCharacterId = "melee";
	private const string LegacyTypoSowrdmanCharacterId = "sowrdman";
	private const string SwordsmanCharacterId = "swordsman";
	private const string FirstClearCharacterFlagPrefix = "meta.first_clear.character.";

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
		dto.DomainShardWalletByDomain ??= new Dictionary<string, int>(StringComparer.Ordinal);
		dto.ConsumableWalletById ??= new Dictionary<string, int>(StringComparer.Ordinal);
		dto.EventChargesByEventId ??= new Dictionary<string, int>(StringComparer.Ordinal);
		dto.UnlockedCharacterIds ??= new List<string>();
		dto.UnlockedEventIds ??= new List<string>();
		dto.UnlockedHybridVariantIds ??= new List<string>();
		dto.CharacterProgressById ??= new Dictionary<string, CharacterProgressDto>(StringComparer.Ordinal);
		dto.MetaFlags ??= new List<string>();
		dto.SettledRunIds ??= new List<string>();
		dto.PerfectClearRecords ??= new List<PerfectClearRecord>();
		for (int i = 0; i < dto.UnlockedCharacterIds.Count; i++)
			dto.UnlockedCharacterIds[i] = NormalizeCharacterId(dto.UnlockedCharacterIds[i]);
		dto.UnlockedCharacterIds = dto.UnlockedCharacterIds
			.Where(id => !string.IsNullOrWhiteSpace(id))
			.Distinct(StringComparer.Ordinal)
			.ToList();
		dto.UnlockedEventIds = dto.UnlockedEventIds
			.Where(id => !string.IsNullOrWhiteSpace(id))
			.Distinct(StringComparer.Ordinal)
			.ToList();
		dto.UnlockedHybridVariantIds = dto.UnlockedHybridVariantIds
			.Where(id => !string.IsNullOrWhiteSpace(id))
			.Distinct(StringComparer.Ordinal)
			.ToList();
		for (int i = 0; i < dto.MetaFlags.Count; i++)
			dto.MetaFlags[i] = NormalizeMetaFlag(dto.MetaFlags[i]);
		dto.MetaFlags = dto.MetaFlags
			.Where(flag => !string.IsNullOrWhiteSpace(flag))
			.Distinct(StringComparer.Ordinal)
			.ToList();
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

		var normalizedDomainShards = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, int> pair in dto.DomainShardWalletByDomain)
		{
			string domainId = NormalizeDomainId(pair.Key);
			if (string.IsNullOrWhiteSpace(domainId))
				continue;
			int amount = Math.Max(0, pair.Value);
			if (amount <= 0)
				continue;
			normalizedDomainShards[domainId] = amount;
		}
		dto.DomainShardWalletByDomain = normalizedDomainShards;

		var normalizedConsumables = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, int> pair in dto.ConsumableWalletById)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;
			int amount = Math.Max(0, pair.Value);
			if (amount <= 0)
				continue;
			normalizedConsumables[pair.Key] = amount;
		}
		dto.ConsumableWalletById = normalizedConsumables;

		var normalizedEventCharges = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, int> pair in dto.EventChargesByEventId)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;
			if (!ProgressionDefs.TryGetEvent(pair.Key, out _))
				continue;
			int amount = Math.Max(0, pair.Value);
			if (amount <= 0)
				continue;
			normalizedEventCharges[pair.Key] = amount;
		}

		// v5 legacy migration: unlocked event ids become one purchase bundle of charges by default.
		foreach (string eventId in dto.UnlockedEventIds)
		{
			if (!ProgressionDefs.TryGetEvent(eventId, out EventUnlockDef def) || def == null)
				continue;
			int bundle = Math.Max(1, def.ChargeBundleAmount);
			if (!normalizedEventCharges.TryGetValue(def.EventId, out int current) || current < bundle)
				normalizedEventCharges[def.EventId] = bundle;
		}
		dto.EventChargesByEventId = normalizedEventCharges;

		var normalized = new Dictionary<string, CharacterProgressDto>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, CharacterProgressDto> pair in dto.CharacterProgressById)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;

			CharacterProgressDto progress = pair.Value ?? new CharacterProgressDto();
			progress.Level = Math.Max(1, progress.Level);
			progress.UnlockedAbilityNodes ??= new List<string>();
			normalized[NormalizeCharacterId(pair.Key)] = progress;
		}
		dto.CharacterProgressById = normalized;

		return dto;
	}

	public static MetaProgressionState ToDomain(MetaSaveDto dto)
	{
		dto = MigrateToCurrent(dto);
		var state = new MetaProgressionState();
		state.ReplaceCurrencySnapshot(dto.CurrencyWallet, dto.CurrencyEarnedTotal, dto.CurrencySpentTotal);
		foreach (KeyValuePair<string, int> pair in dto.DomainShardWalletByDomain)
			state.SetDomainShardBalance(pair.Key, pair.Value);
		foreach (KeyValuePair<string, int> pair in dto.ConsumableWalletById)
			state.SetConsumableCount(pair.Key, pair.Value);
		foreach (KeyValuePair<string, int> pair in dto.EventChargesByEventId)
			state.SetEventChargeCount(pair.Key, pair.Value);

		foreach (string id in dto.UnlockedCharacterIds)
			state.UnlockCharacter(id);
		foreach (string variantId in dto.UnlockedHybridVariantIds)
			state.UnlockHybridVariant(variantId);

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
		foreach (KeyValuePair<string, int> pair in state.DomainShardWalletByDomain)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;
			if (pair.Value <= 0)
				continue;
			dto.DomainShardWalletByDomain[pair.Key] = pair.Value;
		}
		foreach (KeyValuePair<string, int> pair in state.ConsumableWalletById)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;
			if (pair.Value <= 0)
				continue;
			dto.ConsumableWalletById[pair.Key] = pair.Value;
		}
		foreach (KeyValuePair<string, int> pair in state.EventChargesByEventId)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;
			if (pair.Value <= 0)
				continue;
			dto.EventChargesByEventId[pair.Key] = pair.Value;
		}

		dto.UnlockedCharacterIds.AddRange(state.UnlockedCharacterIds);
		foreach (KeyValuePair<string, int> pair in state.EventChargesByEventId)
		{
			if (!string.IsNullOrWhiteSpace(pair.Key) && pair.Value > 0)
				dto.UnlockedEventIds.Add(pair.Key);
		}
		dto.UnlockedHybridVariantIds.AddRange(state.UnlockedHybridVariantIds);
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

	private static string NormalizeMetaFlag(string flag)
	{
		if (string.IsNullOrWhiteSpace(flag))
			return string.Empty;
		string legacy = $"{FirstClearCharacterFlagPrefix}{LegacyMeleeCharacterId}";
		if (string.Equals(flag, legacy, StringComparison.Ordinal))
			return $"{FirstClearCharacterFlagPrefix}{SwordsmanCharacterId}";
		return flag;
	}

	private static string NormalizeCharacterId(string characterId)
	{
		if (string.Equals(characterId, LegacyMeleeCharacterId, StringComparison.Ordinal))
			return SwordsmanCharacterId;
		if (string.Equals(characterId, LegacyTypoSowrdmanCharacterId, StringComparison.Ordinal))
			return SwordsmanCharacterId;
		return characterId;
	}

	private static string NormalizeDomainId(string domainId)
	{
		if (string.Equals(domainId, "Ice", StringComparison.OrdinalIgnoreCase))
			return "Ice";
		if (string.Equals(domainId, "War", StringComparison.OrdinalIgnoreCase))
			return "War";
		if (string.Equals(domainId, "Spacetime", StringComparison.OrdinalIgnoreCase))
			return "Spacetime";
		return string.Empty;
	}
}
