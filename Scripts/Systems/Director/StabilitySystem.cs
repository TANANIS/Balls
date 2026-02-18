using Godot;
using System;

public partial class StabilitySystem : Node
{
	public enum StabilityPhase
	{
		Stable,
		EnergyAnomaly,
		StructuralFracture,
		CollapseCritical
	}

	public enum UniverseEventType
	{
		None,
		EnergySurge,
		GravityInversion
	}

	[Export] public float StartStability = 100f;
	[Export] public float BaseDecayPerSecond = 0.35f;
	[Export] public float EnergyAnomalyDecayMultiplier = 1.25f;
	[Export] public float StructuralFractureDecayMultiplier = 1.60f;
	[Export] public float CollapseCriticalDecayMultiplier = 2.10f;
	[Export] public float EventIntervalSeconds = 180f;
	[Export] public float EventDurationMinSeconds = 30f;
	[Export] public float EventDurationMaxSeconds = 60f;
	[Export] public float EventIncomingLeadSeconds = 12f;
	[Export] public float GravityInversionDecayPauseSeconds = 5f;
	[Export] public float MatchDurationLimitSeconds = 900f;
	[Export] public float EnergyAnomalyEnemySpeedMultiplier = 1.18f;
	[Export] public float StructuralFractureEnemySpeedMultiplier = 1.35f;
	[Export] public float CollapseCriticalEnemySpeedMultiplier = 1.65f;
	[Export] public float EnergyAnomalyPlayerPowerMultiplier = 1.18f;
	[Export] public float StructuralFracturePlayerPowerMultiplier = 1.08f;
	[Export] public float CollapseCriticalPlayerPowerMultiplier = 1.00f;
	[Export] public float StructuralFractureInertiaMultiplier = 0.74f;
	[Export] public float CollapseCriticalInertiaMultiplier = 0.58f;
	[Export] public float StructuralFractureCameraZoomMultiplier = 1.16f;
	[Export] public float CollapseCriticalCameraZoomMultiplier = 1.32f;
	[Export] public float StableObstacleSpawnMultiplier = 0.35f;
	[Export] public float EnergyAnomalyObstacleSpawnMultiplier = 0.55f;
	[Export] public float StructuralFractureObstacleSpawnMultiplier = 1.30f;
	[Export] public float CollapseCriticalObstacleSpawnMultiplier = 1.55f;
	[Export] public float EnergyAnomalyPressureFluctuationAmplitude = 0.09f;
	[Export] public float StructuralFracturePressureFluctuationAmplitude = 0.15f;
	[Export] public float CollapseCriticalPressureFluctuationAmplitude = 0.24f;
	[Export] public bool VerboseLog = false;

	private readonly RandomNumberGenerator _rng = new();
	private float _stability;
	private StabilityPhase _phase = StabilityPhase.Stable;
	private bool _collapsed;
	private float _elapsedSeconds;
	private float _nextEventAtSeconds;
	private float _activeEventRemainingSeconds;
	private float _decayPauseRemainingSeconds;
	private float _inputFlipTimer;
	private bool _incomingAnnounced;
	private bool _timeLimitReached;
	private bool _inputSignNegative;
	private UniverseEventType _activeEvent = UniverseEventType.None;
	private UniverseEventType _pendingEvent = UniverseEventType.EnergySurge;

	public float CurrentStability => _stability;
	public StabilityPhase CurrentPhase => _phase;
	public bool IsCollapsed => _collapsed;
	public UniverseEventType ActiveEvent => _activeEvent;
	public UniverseEventType PendingEvent => _pendingEvent;
	public bool IsUniverseEventActive => _activeEvent != UniverseEventType.None;
	public float ActiveEventRemainingSeconds => Mathf.Max(0f, _activeEventRemainingSeconds);
	public float SecondsUntilNextEvent => Mathf.Max(0f, _nextEventAtSeconds - _elapsedSeconds);
	public float ElapsedSeconds => _elapsedSeconds;
	public bool IsMovementInversionActive => _activeEvent == UniverseEventType.GravityInversion || _phase == StabilityPhase.CollapseCritical;
	public float InputDirectionSign => IsMovementInversionActive && _inputSignNegative ? -1f : 1f;

	public event Action<StabilityPhase> PhaseChanged;
	public event Action<float, UniverseEventType> EventIncoming;
	public event Action<UniverseEventType, float> EventStarted;
	public event Action<UniverseEventType> EventEnded;
	public event Action MatchDurationReached;
	public event Action Collapsed;

	public override void _EnterTree()
	{
		AddToGroup("StabilitySystem");
	}

	public override void _Ready()
	{
		_rng.Randomize();
		_stability = Mathf.Clamp(StartStability, 0f, 100f);
		_phase = ResolvePhase(_stability);
		_pendingEvent = RollUniverseEvent();
		_nextEventAtSeconds = Mathf.Max(1f, EventIntervalSeconds);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_collapsed)
			return;
		if (GetTree().Paused)
			return;

		float dt = (float)delta;
		_elapsedSeconds += dt;
		if (!_timeLimitReached && MatchDurationLimitSeconds > 0f && _elapsedSeconds >= MatchDurationLimitSeconds)
		{
			_timeLimitReached = true;
			MatchDurationReached?.Invoke();
		}

		TickUniverseEventRuntime(dt);
		TickDirectionDistortion(dt);
		TickStabilityDecay(dt);
	}

	public bool TryRecover(float amount, string source = "unknown")
	{
		if (_collapsed || amount <= 0f)
			return false;

		float before = _stability;
		float cap = GetRecoveryCap(_phase);
		_stability = Mathf.Clamp(_stability + amount, 0f, cap);
		UpdatePhaseAndSignals();

		if (VerboseLog && _stability > before)
			DebugSystem.Log($"[StabilitySystem] Recover +{_stability - before:F2} ({source}) => {_stability:F2}");
		return _stability > before;
	}

	private void ApplyDelta(float delta)
	{
		float before = _stability;
		_stability = Mathf.Clamp(_stability + delta, 0f, 100f);
		UpdatePhaseAndSignals();

		if (VerboseLog && !Mathf.IsEqualApprox(before, _stability))
			DebugSystem.Log($"[StabilitySystem] Stability {_stability:F2} phase={_phase}");
	}

	private void TickStabilityDecay(float dt)
	{
		if (_decayPauseRemainingSeconds > 0f)
		{
			_decayPauseRemainingSeconds = Mathf.Max(0f, _decayPauseRemainingSeconds - dt);
			return;
		}

		float decay = Mathf.Max(0f, BaseDecayPerSecond) * GetPhaseDecayMultiplier(_phase);
		ApplyDelta(-decay * dt);
	}

	private void TickUniverseEventRuntime(float dt)
	{
		if (!IsUniverseEventActive)
		{
			float incomingLead = Mathf.Clamp(EventIncomingLeadSeconds, 1f, Mathf.Max(1f, EventIntervalSeconds));
			float until = _nextEventAtSeconds - _elapsedSeconds;
			if (!_incomingAnnounced && until <= incomingLead && until > 0f)
			{
				_incomingAnnounced = true;
				EventIncoming?.Invoke(until, _pendingEvent);
			}

			if (_elapsedSeconds >= _nextEventAtSeconds)
				StartUniverseEvent(_pendingEvent);
		}
		else
		{
			_activeEventRemainingSeconds = Mathf.Max(0f, _activeEventRemainingSeconds - dt);
			if (_activeEventRemainingSeconds <= 0f)
				EndUniverseEvent();
		}
	}

	private void StartUniverseEvent(UniverseEventType eventType)
	{
		float minDur = Mathf.Max(5f, EventDurationMinSeconds);
		float maxDur = Mathf.Max(minDur, EventDurationMaxSeconds);
		float duration = _rng.RandfRange(minDur, maxDur);

		_activeEvent = eventType;
		_activeEventRemainingSeconds = duration;
		if (_activeEvent == UniverseEventType.GravityInversion)
			_decayPauseRemainingSeconds = Mathf.Max(_decayPauseRemainingSeconds, GravityInversionDecayPauseSeconds);

		EventStarted?.Invoke(_activeEvent, duration);
		if (VerboseLog)
			DebugSystem.Log($"[StabilitySystem] Event started: {_activeEvent} ({duration:F1}s)");

		_nextEventAtSeconds = _elapsedSeconds + Mathf.Max(1f, EventIntervalSeconds);
		_pendingEvent = RollUniverseEvent();
		_incomingAnnounced = false;
	}

	private void EndUniverseEvent()
	{
		UniverseEventType ended = _activeEvent;
		_activeEvent = UniverseEventType.None;
		_activeEventRemainingSeconds = 0f;
		EventEnded?.Invoke(ended);
		if (VerboseLog)
			DebugSystem.Log($"[StabilitySystem] Event ended: {ended}");
	}

	private UniverseEventType RollUniverseEvent()
	{
		return _rng.Randf() < 0.5f
			? UniverseEventType.EnergySurge
			: UniverseEventType.GravityInversion;
	}

	private void TickDirectionDistortion(float dt)
	{
		if (!IsMovementInversionActive)
		{
			_inputSignNegative = false;
			_inputFlipTimer = 0f;
			return;
		}

		_inputFlipTimer -= dt;
		if (_inputFlipTimer > 0f)
			return;

		float min = _activeEvent == UniverseEventType.GravityInversion ? 0.45f : 0.9f;
		float max = _activeEvent == UniverseEventType.GravityInversion ? 1.10f : 2.10f;
		_inputFlipTimer = _rng.RandfRange(min, max);
		_inputSignNegative = !_inputSignNegative;
	}

	private void UpdatePhaseAndSignals()
	{
		StabilityPhase next = ResolvePhase(_stability);
		if (next != _phase)
		{
			_phase = next;
			PhaseChanged?.Invoke(_phase);
			if (VerboseLog)
				DebugSystem.Log($"[StabilitySystem] Phase -> {_phase}");
		}

		if (!_collapsed && _stability <= 0f)
		{
			_collapsed = true;
			_stability = 0f;
			DebugSystem.Warn("[StabilitySystem] Universe collapse triggered.");
			Collapsed?.Invoke();
		}
	}

	private static StabilityPhase ResolvePhase(float stability)
	{
		if (stability > 70f) return StabilityPhase.Stable;
		if (stability > 40f) return StabilityPhase.EnergyAnomaly;
		if (stability > 15f) return StabilityPhase.StructuralFracture;
		return StabilityPhase.CollapseCritical;
	}

	private float GetPhaseDecayMultiplier(StabilityPhase phase)
	{
		return phase switch
		{
			StabilityPhase.Stable => 1f,
			StabilityPhase.EnergyAnomaly => EnergyAnomalyDecayMultiplier,
			StabilityPhase.StructuralFracture => StructuralFractureDecayMultiplier,
			StabilityPhase.CollapseCritical => CollapseCriticalDecayMultiplier,
			_ => 1f
		};
	}

	private static float GetRecoveryCap(StabilityPhase phase)
	{
		return phase switch
		{
			StabilityPhase.Stable => 95f,
			StabilityPhase.EnergyAnomaly => 69.9f,
			StabilityPhase.StructuralFracture => 39.9f,
			StabilityPhase.CollapseCritical => 14.9f,
			_ => 95f
		};
	}

	public static string GetEventDisplayName(UniverseEventType eventType)
	{
		return eventType switch
		{
			UniverseEventType.EnergySurge => "Energy Surge",
			UniverseEventType.GravityInversion => "Gravity Inversion",
			_ => "None"
		};
	}

	public float GetEnemySpeedMultiplier()
	{
		return _phase switch
		{
			StabilityPhase.EnergyAnomaly => EnergyAnomalyEnemySpeedMultiplier,
			StabilityPhase.StructuralFracture => StructuralFractureEnemySpeedMultiplier,
			StabilityPhase.CollapseCritical => CollapseCriticalEnemySpeedMultiplier,
			_ => 1f
		};
	}

	public float GetPlayerPowerMultiplier()
	{
		float baseMult = _phase switch
		{
			StabilityPhase.EnergyAnomaly => EnergyAnomalyPlayerPowerMultiplier,
			StabilityPhase.StructuralFracture => StructuralFracturePlayerPowerMultiplier,
			StabilityPhase.CollapseCritical => CollapseCriticalPlayerPowerMultiplier,
			_ => 1f
		};

		if (_activeEvent == UniverseEventType.EnergySurge)
			baseMult *= 1.12f;
		return Mathf.Max(0.1f, baseMult);
	}

	public float GetPlayerInertiaMultiplier()
	{
		float mult = _phase switch
		{
			StabilityPhase.StructuralFracture => StructuralFractureInertiaMultiplier,
			StabilityPhase.CollapseCritical => CollapseCriticalInertiaMultiplier,
			_ => 1f
		};
		return Mathf.Max(0.1f, mult);
	}

	public float GetCameraZoomMultiplier()
	{
		return _phase switch
		{
			StabilityPhase.StructuralFracture => StructuralFractureCameraZoomMultiplier,
			StabilityPhase.CollapseCritical => CollapseCriticalCameraZoomMultiplier,
			_ => 1f
		};
	}

	public float GetObstacleSpawnMultiplier()
	{
		float mult = _phase switch
		{
			StabilityPhase.Stable => StableObstacleSpawnMultiplier,
			StabilityPhase.EnergyAnomaly => EnergyAnomalyObstacleSpawnMultiplier,
			StabilityPhase.StructuralFracture => StructuralFractureObstacleSpawnMultiplier,
			StabilityPhase.CollapseCritical => CollapseCriticalObstacleSpawnMultiplier,
			_ => 1f
		};
		return Mathf.Max(0.05f, mult);
	}

	public float GetPressureFluctuationAmplitude()
	{
		return _phase switch
		{
			StabilityPhase.EnergyAnomaly => EnergyAnomalyPressureFluctuationAmplitude,
			StabilityPhase.StructuralFracture => StructuralFracturePressureFluctuationAmplitude,
			StabilityPhase.CollapseCritical => CollapseCriticalPressureFluctuationAmplitude,
			_ => 0f
		};
	}
}
