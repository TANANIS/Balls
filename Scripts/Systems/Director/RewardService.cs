using System;
using System.Collections.Generic;
using Godot;

public sealed class RewardService
{
	private readonly Dictionary<string, int> _domainShardBuffer = new(StringComparer.Ordinal);
	private readonly HashSet<string> _completedEventKeys = new(StringComparer.Ordinal);

	public void ResetForRun()
	{
		_domainShardBuffer.Clear();
		_completedEventKeys.Clear();
	}

	public int RecordCompletedEvent(EventLoadoutSlot slot, float purity = 1f)
	{
		if (slot == null || string.IsNullOrWhiteSpace(slot.EventId))
			return 0;

		string key = $"{slot.SlotIndex}:{slot.EventId}";
		if (!_completedEventKeys.Add(key))
			return 0;

		string domain = NormalizeDomainId(slot.DomainId);
		if (string.IsNullOrWhiteSpace(domain))
			return 0;

		float baseShard = GetBaseShardReward(slot.EventId);
		float distortion = string.Equals(slot.DistortionLevel, "D1", StringComparison.OrdinalIgnoreCase) ? 1.50f : 1.00f;
		float affinity = GetAffinityRewardMultiplier(slot.AffinityWithPrevious);
		float clampedPurity = Mathf.Clamp(purity, 0.8f, 1.2f);
		int payout = Mathf.Max(0, Mathf.RoundToInt(baseShard * distortion * affinity * clampedPurity));
		if (payout <= 0)
			return 0;

		if (!_domainShardBuffer.TryGetValue(domain, out int current))
			current = 0;
		_domainShardBuffer[domain] = current + payout;
		return payout;
	}

	public int GetRunDomainShardTotal()
	{
		int total = 0;
		foreach (KeyValuePair<string, int> pair in _domainShardBuffer)
			total += Math.Max(0, pair.Value);
		return total;
	}

	public Dictionary<string, int> GetRunDomainShardRewardsSnapshot()
	{
		var snapshot = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, int> pair in _domainShardBuffer)
		{
			if (pair.Value <= 0 || string.IsNullOrWhiteSpace(pair.Key))
				continue;
			snapshot[pair.Key] = pair.Value;
		}

		return snapshot;
	}

	private static float GetAffinityRewardMultiplier(string affinity)
	{
		return affinity switch
		{
			"Resonance" => 1.30f,
			"Dissonance" => 0.80f,
			_ => 1.00f
		};
	}

	private static float GetBaseShardReward(string eventId)
	{
		return eventId switch
		{
			"EVT_ICE_ICESTORM" => 18f,
			"EVT_ICE_FROZEN_PULSE" => 22f,
			"EVT_WAR_BLOOD_TIDE" => 20f,
			"EVT_WAR_BERSERK_MARK" => 24f,
			"EVT_SPACE_EVENT_HORIZON" => 26f,
			"EVT_SPACE_GRAVITY_WELL" => 21f,
			_ => 0f
		};
	}

	private static string NormalizeDomainId(string domainId)
	{
		return domainId switch
		{
			"Ice" => "Ice",
			"War" => "War",
			"Spacetime" => "Spacetime",
			_ => string.Empty
		};
	}
}
