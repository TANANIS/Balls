using System;
using System.Collections.Generic;
using System.Linq;

public sealed class MetaProgressionState
{
	private const int PerfectLeaderboardMaxEntries = 10;

	public int CurrencyWallet { get; private set; }
	public int CurrencyEarnedTotal { get; private set; }
	public int CurrencySpentTotal { get; private set; }

	public HashSet<string> UnlockedCharacterIds { get; } = new();
	public Dictionary<string, CharacterProgress> CharacterProgressById { get; } = new(StringComparer.Ordinal);
	public MetaFlags Flags { get; } = new();
	public HashSet<string> SettledRunIds { get; } = new(StringComparer.Ordinal);
	public List<PerfectClearRecord> PerfectClearRecords { get; } = new();

	public void ReplaceCurrencySnapshot(int wallet, int earnedTotal, int spentTotal)
	{
		CurrencyWallet = Math.Max(0, wallet);
		CurrencyEarnedTotal = Math.Max(0, earnedTotal);
		CurrencySpentTotal = Math.Max(0, spentTotal);
	}

	public void AddCurrency(int amount)
	{
		if (amount <= 0)
			return;

		CurrencyWallet += amount;
		CurrencyEarnedTotal += amount;
	}

	public bool TrySpendCurrency(int amount)
	{
		if (amount <= 0)
			return false;
		if (CurrencyWallet < amount)
			return false;

		CurrencyWallet -= amount;
		CurrencySpentTotal += amount;
		return true;
	}

	public bool UnlockCharacter(string characterId)
	{
		if (string.IsNullOrWhiteSpace(characterId))
			return false;

		return UnlockedCharacterIds.Add(characterId);
	}

	public CharacterProgress EnsureCharacterProgress(string characterId)
	{
		if (string.IsNullOrWhiteSpace(characterId))
			return null;

		if (!CharacterProgressById.TryGetValue(characterId, out CharacterProgress progress))
		{
			progress = new CharacterProgress();
			CharacterProgressById[characterId] = progress;
		}

		return progress;
	}

	public bool TryGetCharacterProgress(string characterId, out CharacterProgress progress)
	{
		if (string.IsNullOrWhiteSpace(characterId))
		{
			progress = null;
			return false;
		}

		return CharacterProgressById.TryGetValue(characterId, out progress) && progress != null;
	}

	public bool MarkRunSettled(string runId)
	{
		if (string.IsNullOrWhiteSpace(runId))
			return false;

		return SettledRunIds.Add(runId);
	}

	public void AddPerfectClearRecord(int score, string characterName, long unixTime)
	{
		var record = new PerfectClearRecord
		{
			Score = score,
			CharacterName = characterName,
			UnixTime = unixTime
		};
		record.Normalize();
		PerfectClearRecords.Add(record);
		SortAndTrimPerfectRecords();
	}

	public IReadOnlyList<PerfectClearRecord> GetPerfectClearRecords(int maxCount)
	{
		if (maxCount <= 0)
			return Array.Empty<PerfectClearRecord>();

		SortAndTrimPerfectRecords();
		return PerfectClearRecords.Take(Math.Min(maxCount, PerfectClearRecords.Count)).ToList();
	}

	private void SortAndTrimPerfectRecords()
	{
		PerfectClearRecords.Sort(ComparePerfectRecords);
		if (PerfectClearRecords.Count > PerfectLeaderboardMaxEntries)
			PerfectClearRecords.RemoveRange(PerfectLeaderboardMaxEntries, PerfectClearRecords.Count - PerfectLeaderboardMaxEntries);
	}

	private static int ComparePerfectRecords(PerfectClearRecord a, PerfectClearRecord b)
	{
		if (ReferenceEquals(a, b))
			return 0;
		if (a == null)
			return 1;
		if (b == null)
			return -1;

		int byScore = b.Score.CompareTo(a.Score);
		if (byScore != 0)
			return byScore;
		return b.UnixTime.CompareTo(a.UnixTime);
	}
}
