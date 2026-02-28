using Godot;
using System;
using System.Collections.Generic;

public partial class EventDirector : Node
{
	[Export] public float Slot1TriggerSeconds = 60f;
	[Export] public float Slot2TriggerSeconds = 300f;
	[Export] public float Slot3TriggerSeconds = 540f;
	[Export] public float Slot4TriggerSeconds = 780f;
	[Export] public float Tier0ActiveWindowSeconds = 35f;
	[Export] public float Tier1ActiveWindowSeconds = 42f;
	[Export] public float Tier2ActiveWindowSeconds = 48f;
	[Export] public float Tier3ActiveWindowSeconds = 55f;

	private StabilitySystem _stabilitySystem;
	private EventRunner _eventRunner;
	private RewardService _rewardService;
	private EventLoadoutPlan _runPlan;
	private EventLoadoutSlot _activeSlot;
	private int _nextSlotToTrigger;
	private float _lastElapsedSeconds = -1f;
	private float _activeRemainingSeconds;
	private bool _activeEventInProgress;

	public string ActiveEventBannerText { get; private set; } = string.Empty;
	public bool IsEventActive => _activeEventInProgress;

	public event Action<string> EventActivated;
	public event Action<string> EventEnded;

	public override void _EnterTree()
	{
		AddToGroup(RuntimeGroups.EventDirector);
	}

	public override void _Ready()
	{
		_eventRunner = new EventRunner(this);
		_rewardService = new RewardService();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GetTree().Paused)
			return;

		EnsureStabilitySystem();
		if (_stabilitySystem == null)
			return;

		float elapsed = _stabilitySystem.ElapsedSeconds;
		HandleRunResetByElapsed(elapsed);
		EnsureRunPlanLoaded();

		if (_runPlan != null && _runPlan.Slots.Count > 0)
			TryTriggerDueSlots(elapsed);

		float dt = (float)delta;
		_eventRunner?.Tick(dt);
		TickRuntimeUiTimers(dt);
		UpdateActiveEventTimer(dt);
		_lastElapsedSeconds = elapsed;
	}

	public void ResetForNewRun()
	{
		AbortActiveEvent();
		_rewardService?.ResetForRun();
		_runPlan = RunContext.Instance?.GetEventLoadoutPlan();
		_activeSlot = null;
		_nextSlotToTrigger = 0;
		_activeRemainingSeconds = 0f;
		_activeEventInProgress = false;
		ActiveEventBannerText = string.Empty;
		ClearRuntimeUiState();
		_lastElapsedSeconds = 0f;
	}

	public int GetRunDomainShardTotal()
	{
		return _rewardService?.GetRunDomainShardTotal() ?? 0;
	}

	public Dictionary<string, int> GetRunDomainShardRewardsSnapshot()
	{
		return _rewardService?.GetRunDomainShardRewardsSnapshot()
			?? new Dictionary<string, int>(StringComparer.Ordinal);
	}

	public float GetMoveMultiplierAt(Vector2 worldPos, ulong actorInstanceId, bool isPlayer)
	{
		return _eventRunner?.GetMoveMultiplierAt(worldPos, actorInstanceId, isPlayer) ?? 1f;
	}

	public Vector2 GetExternalVelocityAt(Vector2 worldPos)
	{
		return _eventRunner?.GetExternalVelocityAt(worldPos) ?? Vector2.Zero;
	}

	public float GetProjectileSpeedMultiplierAt(Vector2 worldPos)
	{
		return _eventRunner?.GetProjectileSpeedMultiplierAt(worldPos) ?? 1f;
	}

	public float GetPlayerRangeMultiplierAt(Vector2 worldPos)
	{
		return _eventRunner?.GetPlayerRangeMultiplierAt(worldPos) ?? 1f;
	}

	private void EnsureStabilitySystem()
	{
		_stabilitySystem = GroupServiceResolver.ResolveFirstInGroup(this, RuntimeGroups.StabilitySystem, _stabilitySystem);
	}

	private void HandleRunResetByElapsed(float elapsed)
	{
		// Detect run reset when timeline jumps backwards (restart/title->new run path).
		if (_lastElapsedSeconds >= 0f && elapsed + 0.2f < _lastElapsedSeconds)
			ResetForNewRun();
	}

	private void EnsureRunPlanLoaded()
	{
		if (_runPlan != null && _runPlan.Slots.Count > 0)
			return;

		_runPlan = RunContext.Instance?.GetEventLoadoutPlan();
		if (_runPlan == null || _runPlan.Slots.Count == 0)
			return;

		_nextSlotToTrigger = 0;
	}

	private void TryTriggerDueSlots(float elapsed)
	{
		while (_runPlan != null && _nextSlotToTrigger < _runPlan.Slots.Count)
		{
			EventLoadoutSlot slot = _runPlan.Slots[_nextSlotToTrigger];
			float triggerAt = GetSlotTriggerSeconds(slot.SlotIndex);
			if (elapsed < triggerAt)
				break;

			ActivateSlot(slot);
			_nextSlotToTrigger++;
		}
	}

	private void ActivateSlot(EventLoadoutSlot slot)
	{
		if (_activeEventInProgress)
			CompleteActiveEvent(emitSignal: false);

		EventLoadoutSlot runtimeSlot = ResolveHybridVariantForSlot(slot);
		if (runtimeSlot == null)
			return;

		float distortionTime = runtimeSlot.DistortionLevel == "D1" ? 1.30f : 1.00f;
		float affinityTime = GetAffinityTimeMultiplier(runtimeSlot.AffinityWithPrevious);
		float intensityMultiplier = distortionTime * affinityTime;

		(float domainEnemySpeed, float domainPlayerPower) = GetDomainRuntimeModifiers(runtimeSlot.DomainId);
		float enemySpeed = Mathf.Clamp(1f + ((domainEnemySpeed - 1f) * intensityMultiplier), 0.55f, 1.75f);
		float playerPower = Mathf.Clamp(1f + ((domainPlayerPower - 1f) * intensityMultiplier), 0.55f, 1.75f);
		ApplyEventBaselineMultipliers(runtimeSlot.EventId, ref enemySpeed, ref playerPower);

		_stabilitySystem?.SetEventRuntimeMultipliers(enemySpeed, playerPower);
		_eventRunner?.BeginEvent(runtimeSlot);

		_activeSlot = runtimeSlot.Clone();
		_activeEventInProgress = true;
		_activeRemainingSeconds = GetTierWindowSeconds(runtimeSlot.ResolvedTierIndex);
		string hybridSuffix = runtimeSlot.HybridVariantTriggered
			? $" + {GetHybridShortLabel(runtimeSlot.HybridVariantId)}"
			: string.Empty;
		ActiveEventBannerText = $"{GetTierLabel(runtimeSlot.ResolvedTierIndex)} {runtimeSlot.EventName} [{runtimeSlot.DistortionLevel}]{hybridSuffix}";
		OnEventActivatedRuntimeUi(runtimeSlot);
		EventActivated?.Invoke(ActiveEventBannerText);
	}

	private void UpdateActiveEventTimer(float dt)
	{
		if (!_activeEventInProgress)
			return;

		_activeRemainingSeconds -= Mathf.Max(0f, dt);
		if (_activeRemainingSeconds > 0f)
			return;

		CompleteActiveEvent(emitSignal: true);
	}

	private void CompleteActiveEvent(bool emitSignal)
	{
		string ended = ActiveEventBannerText;
		if (_activeSlot != null)
			_rewardService?.RecordCompletedEvent(_activeSlot, purity: 1f);

		ClearActiveEventModifiers();
		_eventRunner?.EndEvent();
		_activeSlot = null;
		_activeEventInProgress = false;
		_activeRemainingSeconds = 0f;
		ActiveEventBannerText = string.Empty;
		ClearRuntimeUiState();
		if (emitSignal)
			EventEnded?.Invoke(ended);
	}

	private void AbortActiveEvent()
	{
		ClearActiveEventModifiers();
		_eventRunner?.EndEvent();
		_activeSlot = null;
		_activeEventInProgress = false;
		_activeRemainingSeconds = 0f;
		ActiveEventBannerText = string.Empty;
		ClearRuntimeUiState();
	}

	private void ClearActiveEventModifiers()
	{
		_stabilitySystem?.ResetEventRuntimeMultipliers();
	}

	private float GetSlotTriggerSeconds(int slotIndex)
	{
		return slotIndex switch
		{
			0 => Mathf.Max(0f, Slot1TriggerSeconds),
			1 => Mathf.Max(0f, Slot2TriggerSeconds),
			2 => Mathf.Max(0f, Slot3TriggerSeconds),
			_ => Mathf.Max(0f, Slot4TriggerSeconds)
		};
	}

	private float GetTierWindowSeconds(int tierIndex)
	{
		return tierIndex switch
		{
			0 => Mathf.Max(1f, Tier0ActiveWindowSeconds),
			1 => Mathf.Max(1f, Tier1ActiveWindowSeconds),
			2 => Mathf.Max(1f, Tier2ActiveWindowSeconds),
			_ => Mathf.Max(1f, Tier3ActiveWindowSeconds)
		};
	}

	private static float GetAffinityTimeMultiplier(string affinity)
	{
		return affinity switch
		{
			"Resonance" => 1.20f,
			"Dissonance" => 0.85f,
			_ => 1.00f
		};
	}

	private static (float EnemySpeed, float PlayerPower) GetDomainRuntimeModifiers(string domainId)
	{
		return domainId switch
		{
			"Ice" => (0.88f, 0.94f),
			"War" => (1.12f, 1.08f),
			"Spacetime" => (0.92f, 0.90f),
			_ => (1.00f, 1.00f)
		};
	}

	private static void ApplyEventBaselineMultipliers(string eventId, ref float enemySpeed, ref float playerPower)
	{
		switch (eventId)
		{
			case "EVT_WAR_BLOOD_TIDE":
				playerPower = Mathf.Clamp(playerPower * 1.10f, 0.55f, 1.80f);
				break;
			case "EVT_WAR_BERSERK_MARK":
				enemySpeed = Mathf.Clamp(enemySpeed * 1.08f, 0.55f, 1.80f);
				playerPower = Mathf.Clamp(playerPower * 1.12f, 0.55f, 1.80f);
				break;
			case "EVT_SPACE_EVENT_HORIZON":
				playerPower = Mathf.Clamp(playerPower * 0.94f, 0.55f, 1.80f);
				break;
		}
	}

	private static string GetTierLabel(int tierIndex)
	{
		return tierIndex switch
		{
			0 => "TIER0",
			1 => "TIER1",
			2 => "TIER2",
			_ => "TIER3"
		};
	}

	private EventLoadoutSlot ResolveHybridVariantForSlot(EventLoadoutSlot slot)
	{
		if (slot == null)
			return null;

		EventLoadoutSlot resolved = slot.Clone();
		resolved.HybridVariantTriggered = false;
		resolved.HybridVariantId = string.Empty;

		if (!string.Equals(resolved.AffinityWithPrevious, "Resonance", StringComparison.OrdinalIgnoreCase))
			return resolved;
		if (!TryGetPreviousSlotDomain(resolved.SlotIndex, out string previousDomainId))
			return resolved;
		if (!TryResolveHybridVariantId(previousDomainId, resolved.DomainId, out string variantId))
			return resolved;

		MetaProgressionService meta = MetaProgressionService.Instance;
		if (meta == null || !meta.IsHybridVariantUnlocked(variantId))
			return resolved;
		if (!RollHybridVariantTag(resolved, variantId))
			return resolved;

		resolved.HybridVariantTriggered = true;
		resolved.HybridVariantId = variantId;
		return resolved;
	}

	private bool TryGetPreviousSlotDomain(int slotIndex, out string domainId)
	{
		domainId = string.Empty;
		if (_runPlan == null || slotIndex <= 0)
			return false;

		int previousSlotIndex = slotIndex - 1;
		foreach (EventLoadoutSlot slot in _runPlan.Slots)
		{
			if (slot == null || slot.SlotIndex != previousSlotIndex)
				continue;
			if (string.IsNullOrWhiteSpace(slot.DomainId))
				return false;
			domainId = slot.DomainId;
			return true;
		}

		return false;
	}

	private static bool TryResolveHybridVariantId(string leftDomainId, string rightDomainId, out string variantId)
	{
		variantId = string.Empty;
		if (string.IsNullOrWhiteSpace(leftDomainId) || string.IsNullOrWhiteSpace(rightDomainId))
			return false;

		string left = ProgressionDefs.NormalizeDomainId(leftDomainId);
		string right = ProgressionDefs.NormalizeDomainId(rightDomainId);
		if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right) || string.Equals(left, right, StringComparison.Ordinal))
			return false;

		bool isIceSpace =
			(string.Equals(left, "Ice", StringComparison.Ordinal) && string.Equals(right, "Spacetime", StringComparison.Ordinal))
			|| (string.Equals(left, "Spacetime", StringComparison.Ordinal) && string.Equals(right, "Ice", StringComparison.Ordinal));
		if (isIceSpace)
		{
			variantId = "HYB_ICE_SPACE_GLACIAL_HORIZON";
			return true;
		}

		bool isSpaceWar =
			(string.Equals(left, "Spacetime", StringComparison.Ordinal) && string.Equals(right, "War", StringComparison.Ordinal))
			|| (string.Equals(left, "War", StringComparison.Ordinal) && string.Equals(right, "Spacetime", StringComparison.Ordinal));
		if (isSpaceWar)
		{
			variantId = "HYB_SPACE_WAR_WARP_ASSAULT";
			return true;
		}

		return false;
	}

	private static bool RollHybridVariantTag(EventLoadoutSlot slot, string variantId)
	{
		const int triggerChancePercent = 30;
		uint roll = HashToPercent($"{variantId}|{slot.SlotIndex}|{slot.EventId}|{slot.DistortionLevel}|{slot.AffinityWithPrevious}");
		return roll < triggerChancePercent;
	}

	private static uint HashToPercent(string value)
	{
		ulong hash = 1469598103934665603ul;
		if (!string.IsNullOrEmpty(value))
		{
			for (int i = 0; i < value.Length; i++)
			{
				hash ^= value[i];
				hash *= 1099511628211ul;
			}
		}

		return (uint)(hash % 100ul);
	}

	private static string GetHybridShortLabel(string hybridVariantId)
	{
		return hybridVariantId switch
		{
			"HYB_ICE_SPACE_GLACIAL_HORIZON" => "HYB: Glacial Horizon",
			"HYB_SPACE_WAR_WARP_ASSAULT" => "HYB: Warp Assault",
			_ => "HYB"
		};
	}
}
