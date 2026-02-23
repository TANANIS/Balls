using System;
using System.Collections.Generic;

public sealed class MetaProgressionState
{
	public int CurrencyWallet { get; private set; }
	public int CurrencyEarnedTotal { get; private set; }
	public int CurrencySpentTotal { get; private set; }

	public HashSet<string> UnlockedCharacterIds { get; } = new();
	public Dictionary<string, CharacterProgress> CharacterProgressById { get; } = new(StringComparer.Ordinal);
	public MetaFlags Flags { get; } = new();
	public HashSet<string> SettledRunIds { get; } = new(StringComparer.Ordinal);

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
}
