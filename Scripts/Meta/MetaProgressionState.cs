using System;
using System.Collections.Generic;
using System.Linq;

public sealed class MetaProgressionState
{
	private const int PerfectLeaderboardMaxEntries = 10;

	public int CurrencyWallet { get; private set; }
	public int CurrencyEarnedTotal { get; private set; }
	public int CurrencySpentTotal { get; private set; }
	public Dictionary<string, int> DomainShardWalletByDomain { get; } = new(StringComparer.Ordinal);
	public Dictionary<string, int> ConsumableWalletById { get; } = new(StringComparer.Ordinal);
	public Dictionary<string, int> EventChargesByEventId { get; } = new(StringComparer.Ordinal);

	public HashSet<string> UnlockedCharacterIds { get; } = new();
	public HashSet<string> UnlockedHybridVariantIds { get; } = new(StringComparer.Ordinal);
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

	public int GetDomainShardBalance(string domainId)
	{
		if (string.IsNullOrWhiteSpace(domainId))
			return 0;
		return DomainShardWalletByDomain.TryGetValue(domainId, out int count) ? Math.Max(0, count) : 0;
	}

	public void SetDomainShardBalance(string domainId, int amount)
	{
		if (string.IsNullOrWhiteSpace(domainId))
			return;
		int clamped = Math.Max(0, amount);
		if (clamped <= 0)
		{
			DomainShardWalletByDomain.Remove(domainId);
			return;
		}

		DomainShardWalletByDomain[domainId] = clamped;
	}

	public void AddDomainShards(string domainId, int amount)
	{
		if (string.IsNullOrWhiteSpace(domainId) || amount <= 0)
			return;

		int current = GetDomainShardBalance(domainId);
		DomainShardWalletByDomain[domainId] = current + amount;
	}

	public bool TrySpendDomainShards(string domainId, int amount)
	{
		if (string.IsNullOrWhiteSpace(domainId) || amount <= 0)
			return false;

		int current = GetDomainShardBalance(domainId);
		if (current < amount)
			return false;

		int next = current - amount;
		if (next <= 0)
			DomainShardWalletByDomain.Remove(domainId);
		else
			DomainShardWalletByDomain[domainId] = next;
		return true;
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

	public int GetConsumableCount(string itemId)
	{
		if (string.IsNullOrWhiteSpace(itemId))
			return 0;
		return ConsumableWalletById.TryGetValue(itemId, out int count) ? Math.Max(0, count) : 0;
	}

	public void SetConsumableCount(string itemId, int amount)
	{
		if (string.IsNullOrWhiteSpace(itemId))
			return;
		int clamped = Math.Max(0, amount);
		if (clamped <= 0)
		{
			ConsumableWalletById.Remove(itemId);
			return;
		}

		ConsumableWalletById[itemId] = clamped;
	}

	public void AddConsumable(string itemId, int amount)
	{
		if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
			return;

		int current = GetConsumableCount(itemId);
		ConsumableWalletById[itemId] = current + amount;
	}

	public bool TrySpendConsumable(string itemId, int amount)
	{
		if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
			return false;

		int current = GetConsumableCount(itemId);
		if (current < amount)
			return false;

		int next = current - amount;
		if (next <= 0)
			ConsumableWalletById.Remove(itemId);
		else
			ConsumableWalletById[itemId] = next;
		return true;
	}

	public int GetEventChargeCount(string eventId)
	{
		if (string.IsNullOrWhiteSpace(eventId))
			return 0;
		return EventChargesByEventId.TryGetValue(eventId, out int count) ? Math.Max(0, count) : 0;
	}

	public void SetEventChargeCount(string eventId, int amount)
	{
		if (string.IsNullOrWhiteSpace(eventId))
			return;
		int clamped = Math.Max(0, amount);
		if (clamped <= 0)
		{
			EventChargesByEventId.Remove(eventId);
			return;
		}

		EventChargesByEventId[eventId] = clamped;
	}

	public void AddEventCharges(string eventId, int amount)
	{
		if (string.IsNullOrWhiteSpace(eventId) || amount <= 0)
			return;

		int current = GetEventChargeCount(eventId);
		EventChargesByEventId[eventId] = current + amount;
	}

	public bool TrySpendEventCharges(string eventId, int amount)
	{
		if (string.IsNullOrWhiteSpace(eventId) || amount <= 0)
			return false;

		int current = GetEventChargeCount(eventId);
		if (current < amount)
			return false;

		int next = current - amount;
		if (next <= 0)
			EventChargesByEventId.Remove(eventId);
		else
			EventChargesByEventId[eventId] = next;
		return true;
	}

	public bool UnlockCharacter(string characterId)
	{
		if (string.IsNullOrWhiteSpace(characterId))
			return false;

		return UnlockedCharacterIds.Add(characterId);
	}

	public bool UnlockEvent(string eventId)
	{
		if (string.IsNullOrWhiteSpace(eventId))
			return false;
		if (GetEventChargeCount(eventId) > 0)
			return false;
		AddEventCharges(eventId, 1);
		return true;
	}

	public bool UnlockHybridVariant(string variantId)
	{
		if (string.IsNullOrWhiteSpace(variantId))
			return false;
		return UnlockedHybridVariantIds.Add(variantId);
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
