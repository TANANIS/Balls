using System;
using System.Collections.Generic;

public sealed partial class MetaProgressionService
{
	public int GetEventChargeCount(string eventId)
	{
		if (string.IsNullOrWhiteSpace(eventId))
			return 0;
		return _state.GetEventChargeCount(eventId);
	}

	public bool HasEventCharges(string eventId, int amount = 1)
	{
		if (amount <= 0)
			return true;
		return GetEventChargeCount(eventId) >= amount;
	}

	public bool CanPurchaseEventCharges(string eventId, out string domainId, out int domainShardCost, out int chargesPerPurchase)
	{
		domainId = string.Empty;
		domainShardCost = 0;
		chargesPerPurchase = 0;
		if (!ProgressionDefs.TryGetEvent(eventId, out EventUnlockDef def) || def == null)
			return false;

		domainId = NormalizeDomainId(def.DomainId);
		if (string.IsNullOrWhiteSpace(domainId))
			return false;

		domainShardCost = Math.Max(0, def.DomainShardCost);
		chargesPerPurchase = Math.Max(1, def.ChargeBundleAmount);
		return _state.GetDomainShardBalance(domainId) >= domainShardCost;
	}

	public bool TryPurchaseEventCharges(string eventId, int purchaseCount = 1)
	{
		if (purchaseCount <= 0)
			return false;
		if (!CanPurchaseEventCharges(eventId, out string domainId, out int domainShardCost, out int chargesPerPurchase))
			return false;

		int totalCost = domainShardCost * purchaseCount;
		if (totalCost > 0 && !_state.TrySpendDomainShards(domainId, totalCost))
			return false;

		_state.AddEventCharges(eventId, chargesPerPurchase * purchaseCount);
		_saveStore.SaveState(_state);
		return true;
	}

	public bool TryConsumeEventCharge(string eventId, int amount = 1)
	{
		if (amount <= 0)
			return true;
		if (!_state.TrySpendEventCharges(eventId, amount))
			return false;

		_saveStore.SaveState(_state);
		return true;
	}

	public bool TryConsumeEventChargesBatch(Dictionary<string, int> chargesByEventId)
	{
		Dictionary<string, int> normalized = NormalizeEventChargeMap(chargesByEventId);
		if (normalized.Count == 0)
			return true;

		foreach (KeyValuePair<string, int> pair in normalized)
		{
			if (_state.GetEventChargeCount(pair.Key) < pair.Value)
				return false;
		}

		foreach (KeyValuePair<string, int> pair in normalized)
		{
			if (!_state.TrySpendEventCharges(pair.Key, pair.Value))
				return false;
		}

		_saveStore.SaveState(_state);
		return true;
	}

	private static Dictionary<string, int> NormalizeEventChargeMap(Dictionary<string, int> raw)
	{
		var normalized = new Dictionary<string, int>(StringComparer.Ordinal);
		if (raw == null || raw.Count == 0)
			return normalized;

		foreach (KeyValuePair<string, int> pair in raw)
		{
			if (string.IsNullOrWhiteSpace(pair.Key))
				continue;
			if (!ProgressionDefs.TryGetEvent(pair.Key, out _))
				continue;
			int amount = Math.Max(0, pair.Value);
			if (amount <= 0)
				continue;
			if (normalized.TryGetValue(pair.Key, out int existing))
				normalized[pair.Key] = existing + amount;
			else
				normalized[pair.Key] = amount;
		}

		return normalized;
	}

	// Legacy compatibility wrappers (bool unlock path).
	public bool IsEventUnlocked(string eventId)
	{
		return HasEventCharges(eventId, 1);
	}

	public bool CanUnlockEvent(string eventId, out string domainId, out int domainShardCost)
	{
		bool canPurchase = CanPurchaseEventCharges(eventId, out domainId, out domainShardCost, out _);
		return canPurchase;
	}

	public bool TryUnlockEvent(string eventId)
	{
		return TryPurchaseEventCharges(eventId, 1);
	}

	public bool IsHybridVariantUnlocked(string variantId)
	{
		return !string.IsNullOrWhiteSpace(variantId) && _state.UnlockedHybridVariantIds.Contains(variantId);
	}

	public bool CanUnlockHybridVariant(string variantId, out string domainId, out int domainShardCost)
	{
		domainId = string.Empty;
		domainShardCost = 0;
		if (!ProgressionDefs.TryGetHybridVariant(variantId, out HybridVariantDef def) || def == null)
			return false;
		if (IsHybridVariantUnlocked(def.VariantId))
			return false;

		domainId = NormalizeDomainId(def.DomainId);
		if (string.IsNullOrWhiteSpace(domainId))
			return false;

		domainShardCost = Math.Max(0, def.DomainShardCost);
		return _state.GetDomainShardBalance(domainId) >= domainShardCost;
	}

	public bool TryUnlockHybridVariant(string variantId)
	{
		if (!CanUnlockHybridVariant(variantId, out string domainId, out int domainShardCost))
			return false;

		if (domainShardCost > 0 && !_state.TrySpendDomainShards(domainId, domainShardCost))
			return false;
		if (!_state.UnlockHybridVariant(variantId))
			return false;

		_saveStore.SaveState(_state);
		return true;
	}
}
