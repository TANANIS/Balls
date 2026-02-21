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

	[Export] public float StartStability = 100f;
	[Export] public float BaseDecayPerSecond = 0.35f;
	[Export] public float EnergyAnomalyDecayMultiplier = 1.25f;
	[Export] public float StructuralFractureDecayMultiplier = 1.60f;
	[Export] public float CollapseCriticalDecayMultiplier = 2.10f;
	[Export] public bool UseTimelinePhaseModel = true;
	[Export] public float StablePhaseEndSeconds = 225f;
	[Export] public float EnergyAnomalyPhaseEndSeconds = 450f;
	[Export] public float StructuralFracturePhaseEndSeconds = 675f;
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

	private float _stability;
	private StabilityPhase _phase = StabilityPhase.Stable;
	private bool _collapsed;
	private float _elapsedSeconds;
	private bool _timeLimitReached;
	private float _upgradeDecayMultiplier = 1f;
	private float _playerPowerBonus = 0f;

	public float CurrentStability => _stability;
	public StabilityPhase CurrentPhase => _phase;
	public bool IsCollapsed => _collapsed;
	public float ElapsedSeconds => _elapsedSeconds;
	public bool IsMovementInversionActive => false;
	public float InputDirectionSign => 1f;

	public event Action<StabilityPhase> PhaseChanged;
	public event Action MatchDurationReached;
	public event Action Collapsed;

	public override void _EnterTree()
	{
		AddToGroup("StabilitySystem");
	}

	public override void _Ready()
	{
		_stability = Mathf.Clamp(StartStability, 0f, 100f);
		_phase = UseTimelinePhaseModel ? ResolvePhaseByTimeline(0f) : ResolvePhase(_stability);
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

		UpdatePhaseAndSignals();
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
		float decay = Mathf.Max(0f, BaseDecayPerSecond) * GetPhaseDecayMultiplier(_phase) * _upgradeDecayMultiplier;
		ApplyDelta(-decay * dt);
	}

	private void UpdatePhaseAndSignals()
	{
		StabilityPhase next = UseTimelinePhaseModel
			? ResolvePhaseByTimeline(_elapsedSeconds)
			: ResolvePhase(_stability);
		if (next != _phase)
		{
			_phase = next;
			PhaseChanged?.Invoke(_phase);
			if (VerboseLog)
				DebugSystem.Log($"[StabilitySystem] Phase -> {_phase}");
		}

		if (!_collapsed && _stability <= 0f && !UseTimelinePhaseModel)
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

	private StabilityPhase ResolvePhaseByTimeline(float elapsed)
	{
		float stableEnd = Mathf.Max(1f, StablePhaseEndSeconds);
		float anomalyEnd = Mathf.Max(stableEnd + 1f, EnergyAnomalyPhaseEndSeconds);
		float fractureEnd = Mathf.Max(anomalyEnd + 1f, StructuralFracturePhaseEndSeconds);

		if (elapsed < stableEnd) return StabilityPhase.Stable;
		if (elapsed < anomalyEnd) return StabilityPhase.EnergyAnomaly;
		if (elapsed < fractureEnd) return StabilityPhase.StructuralFracture;
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

		baseMult *= (1f + _playerPowerBonus);
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

	public void MultiplyDecayRate(float factor)
	{
		_upgradeDecayMultiplier = Mathf.Clamp(_upgradeDecayMultiplier * factor, 0.2f, 3f);
	}

	public void MultiplyEventDuration(float factor)
	{
		// Universe event system is removed; keep this API as no-op for upgrade compatibility.
	}

	public void AddPlayerPowerBonus(float amount)
	{
		_playerPowerBonus = Mathf.Clamp(_playerPowerBonus + amount, 0f, 0.8f);
	}
}
