using System;
using System.Collections.Generic;
using System.Linq;

public sealed partial class MetaProgressionService
{
	private const int DomainPowerBundleAmount = 3;

	public int GetDomainPowerCount(string domainId)
	{
		string itemId = GetDomainPowerItemId(domainId);
		return _state.GetConsumableCount(itemId);
	}

	public bool HasDomainPower(string domainId, int amount = 1)
	{
		if (amount <= 0)
			return true;
		return GetDomainPowerCount(domainId) >= amount;
	}

	public bool CanPurchaseDomainPower(string domainId, out int domainShardCost, out int bundleAmount)
	{
		domainShardCost = 0;
		bundleAmount = DomainPowerBundleAmount;

		string normalized = NormalizeDomainId(domainId);
		if (string.IsNullOrWhiteSpace(normalized))
			return false;

		domainShardCost = GetDomainPowerPurchaseCost(normalized);
		return _state.GetDomainShardBalance(normalized) >= domainShardCost;
	}

	public bool TryPurchaseDomainPower(string domainId, int purchaseCount = 1)
	{
		if (purchaseCount <= 0)
			return false;

		if (!CanPurchaseDomainPower(domainId, out int domainShardCost, out int bundleAmount))
			return false;

		string normalized = NormalizeDomainId(domainId);
		string itemId = GetDomainPowerItemId(normalized);
		int totalCost = domainShardCost * purchaseCount;
		if (totalCost > 0 && !_state.TrySpendDomainShards(normalized, totalCost))
			return false;

		_state.AddConsumable(itemId, bundleAmount * purchaseCount);
		_saveStore.SaveState(_state);
		return true;
	}

	public bool TryConsumeDomainPower(string domainId, int amount = 1)
	{
		if (amount <= 0)
			return true;

		string itemId = GetDomainPowerItemId(domainId);
		if (!_state.TrySpendConsumable(itemId, amount))
			return false;

		_saveStore.SaveState(_state);
		return true;
	}

	public bool TryConsumeDomainPowerBatch(Dictionary<string, int> amountByDomain)
	{
		if (amountByDomain == null || amountByDomain.Count == 0)
			return true;

		var normalized = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, int> pair in amountByDomain)
		{
			string domainId = NormalizeDomainId(pair.Key);
			int amount = Math.Max(0, pair.Value);
			if (string.IsNullOrWhiteSpace(domainId) || amount <= 0)
				continue;
			if (normalized.TryGetValue(domainId, out int existing))
				normalized[domainId] = existing + amount;
			else
				normalized[domainId] = amount;
		}

		if (normalized.Count == 0)
			return true;

		foreach (KeyValuePair<string, int> pair in normalized)
		{
			if (!HasDomainPower(pair.Key, pair.Value))
				return false;
		}

		foreach (KeyValuePair<string, int> pair in normalized)
		{
			string itemId = GetDomainPowerItemId(pair.Key);
			if (!_state.TrySpendConsumable(itemId, pair.Value))
				return false;
		}

		_saveStore.SaveState(_state);
		return true;
	}

	public int GetOrderSigilCountForTier(int tierIndex)
	{
		string itemId = GetOrderSigilItemIdForTier(tierIndex);
		return _state.GetConsumableCount(itemId);
	}

	public bool CanSpendOrderSigilForTier(int tierIndex, int amount = 1)
	{
		if (amount <= 0)
			return true;
		string itemId = GetOrderSigilItemIdForTier(tierIndex);
		return _state.GetConsumableCount(itemId) >= amount;
	}

	public bool TrySpendOrderSigilForTier(int tierIndex, int amount = 1)
	{
		if (amount <= 0)
			return true;
		string itemId = GetOrderSigilItemIdForTier(tierIndex);
		if (!_state.TrySpendConsumable(itemId, amount))
			return false;
		_saveStore.SaveState(_state);
		return true;
	}

	public void DebugGrantOrderSigilForTier(int tierIndex, int amount, bool saveNow = true)
	{
		if (amount <= 0)
			return;
		string itemId = GetOrderSigilItemIdForTier(tierIndex);
		_state.AddConsumable(itemId, amount);
		if (saveNow)
			_saveStore.SaveState(_state);
	}

	public void DebugSetCurrencyWallet(int wallet, bool saveNow = true)
	{
		int nextWallet = Math.Max(0, wallet);
		int currentWallet = _state.CurrencyWallet;
		int earned = _state.CurrencyEarnedTotal;
		int spent = _state.CurrencySpentTotal;

		if (nextWallet > currentWallet)
			earned += nextWallet - currentWallet;
		else if (nextWallet < currentWallet)
			spent += currentWallet - nextWallet;

		_state.ReplaceCurrencySnapshot(nextWallet, Math.Max(nextWallet, earned), Math.Max(0, spent));
		if (saveNow)
			_saveStore.SaveState(_state);
	}

	public void DebugSetDomainShardBalance(string domainId, int amount, bool saveNow = true)
	{
		string normalized = NormalizeDomainId(domainId);
		if (string.IsNullOrWhiteSpace(normalized))
			return;

		_state.SetDomainShardBalance(normalized, Math.Max(0, amount));
		if (saveNow)
			_saveStore.SaveState(_state);
	}

	public void DebugSetDomainPowerCount(string domainId, int amount, bool saveNow = true)
	{
		string itemId = GetDomainPowerItemId(domainId);
		if (string.IsNullOrWhiteSpace(itemId))
			return;

		_state.SetConsumableCount(itemId, Math.Max(0, amount));
		if (saveNow)
			_saveStore.SaveState(_state);
	}

	public void RecordPerfectClear(int score, string characterName)
	{
		long unixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
		_state.AddPerfectClearRecord(score, characterName, unixTime);
		_saveStore.SaveState(_state);
	}

	public IReadOnlyList<PerfectClearRecord> GetPerfectLeaderboard(int maxCount)
	{
		if (maxCount <= 0)
			return Array.Empty<PerfectClearRecord>();
		return _state.GetPerfectClearRecords(maxCount).ToList();
	}

	private bool EnsureBaselineUnlocks()
	{
		bool changed = false;
		foreach (CharacterDef def in ProgressionDefs.GetAllCharacters())
		{
			if (def != null && def.IsDefaultUnlocked)
				changed |= _state.UnlockCharacter(def.CharacterId);
		}
		foreach (EventUnlockDef def in ProgressionDefs.GetAllEvents())
		{
			if (def == null)
				continue;
			int defaultCharges = Math.Max(0, def.DefaultChargeCount);
			if (defaultCharges <= 0 && def.IsDefaultUnlocked)
				defaultCharges = Math.Max(1, def.ChargeBundleAmount);
			if (defaultCharges <= 0)
				continue;
			int current = _state.GetEventChargeCount(def.EventId);
			if (current >= defaultCharges)
				continue;
			_state.SetEventChargeCount(def.EventId, defaultCharges);
			changed = true;
		}
		foreach (HybridVariantDef def in ProgressionDefs.GetAllHybridVariants())
		{
			if (def != null && def.IsDefaultUnlocked)
				changed |= _state.UnlockHybridVariant(def.VariantId);
		}
		return changed;
	}

	private bool EnsureBaselineEventConsumables()
	{
		bool changed = false;
		changed |= EnsureMinimumConsumable(OrderSigilTier0ItemId, 3);
		changed |= EnsureMinimumConsumable(OrderSigilTier1ItemId, 3);
		changed |= EnsureMinimumConsumable(OrderSigilTier2ItemId, 3);
		changed |= EnsureMinimumConsumable(OrderSigilTier3ItemId, 3);
		changed |= EnsureDomainPowerBaselineAndMigration();
		return changed;
	}

	private bool EnsureDomainPowerBaselineAndMigration()
	{
		bool changed = false;
		if (!_state.Flags.Has(DomainPowerMigrationFlag))
		{
			changed |= TryMigrateLegacyEventChargesToDomainPower();
			_state.Flags.Add(DomainPowerMigrationFlag);
			changed = true;
		}

		changed |= EnsureMinimumConsumable(DomainPowerIceItemId, 3);
		changed |= EnsureMinimumConsumable(DomainPowerSpacetimeItemId, 3);
		changed |= EnsureMinimumConsumable(DomainPowerWarItemId, 3);
		return changed;
	}

	private bool TryMigrateLegacyEventChargesToDomainPower()
	{
		bool changed = false;
		var domainLegacyTotals = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (EventUnlockDef def in ProgressionDefs.GetAllEvents())
		{
			if (def == null || string.IsNullOrWhiteSpace(def.EventId))
				continue;
			string domainId = NormalizeDomainId(def.DomainId);
			if (string.IsNullOrWhiteSpace(domainId))
				continue;

			int legacyCharges = _state.GetEventChargeCount(def.EventId);
			if (legacyCharges <= 0)
				continue;

			if (domainLegacyTotals.TryGetValue(domainId, out int existing))
				domainLegacyTotals[domainId] = existing + legacyCharges;
			else
				domainLegacyTotals[domainId] = legacyCharges;
		}

		foreach (KeyValuePair<string, int> pair in domainLegacyTotals)
		{
			string itemId = GetDomainPowerItemId(pair.Key);
			if (string.IsNullOrWhiteSpace(itemId))
				continue;

			int current = _state.GetConsumableCount(itemId);
			if (current >= pair.Value)
				continue;
			_state.SetConsumableCount(itemId, pair.Value);
			changed = true;
		}

		return changed;
	}

	private bool IsFirstCharacterClear(string characterId)
	{
		string normalized = NormalizeCharacterId(characterId);
		if (string.IsNullOrWhiteSpace(normalized))
			return false;
		return !_state.Flags.Has($"{FirstClearCharacterFlagPrefix}{normalized}");
	}

	private static string NormalizeCharacterId(string characterId)
	{
		return ProgressionDefs.NormalizeCharacterId(characterId);
	}

	private bool EnsureMinimumConsumable(string itemId, int minimum)
	{
		if (string.IsNullOrWhiteSpace(itemId))
			return false;
		int target = Math.Max(0, minimum);
		if (_state.GetConsumableCount(itemId) >= target)
			return false;
		_state.SetConsumableCount(itemId, target);
		return true;
	}

	private static string GetOrderSigilItemIdForTier(int tierIndex)
	{
		return tierIndex switch
		{
			0 => OrderSigilTier0ItemId,
			1 => OrderSigilTier1ItemId,
			2 => OrderSigilTier2ItemId,
			3 => OrderSigilTier3ItemId,
			_ => OrderSigilTier0ItemId
		};
	}

	private static string GetDomainPowerItemId(string domainId)
	{
		string normalized = NormalizeDomainId(domainId);
		return normalized switch
		{
			"Ice" => DomainPowerIceItemId,
			"Spacetime" => DomainPowerSpacetimeItemId,
			"War" => DomainPowerWarItemId,
			_ => string.Empty
		};
	}

	private static int GetDomainPowerPurchaseCost(string domainId)
	{
		string normalized = NormalizeDomainId(domainId);
		return normalized switch
		{
			"Ice" => 30,
			"Spacetime" => 30,
			"War" => 30,
			_ => 30
		};
	}

	private Dictionary<string, int> NormalizeRunDomainShardPayload(Dictionary<string, int> raw)
	{
		var normalized = new Dictionary<string, int>(StringComparer.Ordinal);
		if (raw == null || raw.Count == 0)
			return normalized;

		foreach (KeyValuePair<string, int> pair in raw)
		{
			string domainId = NormalizeDomainId(pair.Key);
			if (string.IsNullOrWhiteSpace(domainId))
				continue;
			int amount = Math.Max(0, pair.Value);
			if (amount <= 0)
				continue;
			normalized[domainId] = amount;
		}

		return normalized;
	}

	private Dictionary<string, int> ApplyRunDomainShardRewards(Dictionary<string, int> rewardsByDomain)
	{
		var applied = new Dictionary<string, int>(StringComparer.Ordinal);
		if (rewardsByDomain == null || rewardsByDomain.Count == 0)
			return applied;

		foreach (KeyValuePair<string, int> pair in rewardsByDomain)
		{
			string domainId = NormalizeDomainId(pair.Key);
			if (string.IsNullOrWhiteSpace(domainId))
				continue;
			int amount = Math.Max(0, pair.Value);
			if (amount <= 0)
				continue;
			_state.AddDomainShards(domainId, amount);
			applied[domainId] = amount;
		}

		return applied;
	}

	private static int SumDomainShards(Dictionary<string, int> rewardsByDomain)
	{
		if (rewardsByDomain == null || rewardsByDomain.Count == 0)
			return 0;

		int total = 0;
		foreach (KeyValuePair<string, int> pair in rewardsByDomain)
			total += Math.Max(0, pair.Value);
		return total;
	}

	private static string NormalizeDomainId(string domainId)
	{
		return ProgressionDefs.NormalizeDomainId(domainId);
	}

	private static bool HasPrerequisiteNodes(CharacterProgress progress, AbilityNodeDef nodeDef)
	{
		if (progress == null || nodeDef?.PrerequisiteNodeIds == null || nodeDef.PrerequisiteNodeIds.Count == 0)
			return true;

		foreach (string prereq in nodeDef.PrerequisiteNodeIds)
		{
			if (string.IsNullOrWhiteSpace(prereq))
				continue;
			if (!progress.UnlockedAbilityNodes.Contains(prereq))
				return false;
		}

		return true;
	}
}
