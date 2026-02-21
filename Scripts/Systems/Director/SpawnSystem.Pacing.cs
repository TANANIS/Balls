using Godot;

public partial class SpawnSystem
{
	private float GetPhaseSpawnRateMultiplier()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => StableSpawnRateMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalySpawnRateMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFractureSpawnRateMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalSpawnRateMultiplier,
			_ => 1f
		};
	}

	private float GetPhaseMaxAliveMultiplier()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => StableMaxAliveMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalyMaxAliveMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFractureMaxAliveMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalMaxAliveMultiplier,
			_ => 1f
		};
	}

	private float GetPhaseSpawnInterval(float baseInterval)
	{
		float mult = Mathf.Max(0.01f, GetPhaseSpawnRateMultiplier());
		float tierTailRamp = Mathf.Max(1f, GetPhaseTierTailRampMultiplier());
		float tierProgress = GetCurrentTierProgress01();
		float interval = baseInterval / mult;
		interval /= Mathf.Lerp(1f, tierTailRamp, tierProgress);
		interval *= GetOpeningSpawnIntervalMultiplier();
		if (GetCurrentPhase() == StabilitySystem.StabilityPhase.CollapseCritical)
		{
			float jitter = Mathf.Clamp(CollapseCriticalSpawnChaosJitter, 0f, 0.95f);
			float chaos = _rng.RandfRange(1f - jitter, 1f + jitter);
			interval *= chaos;
		}
		return Mathf.Max(SpawnIntervalMinClamp, interval);
	}

	private int GetPhaseMaxAlive()
	{
		float mult = Mathf.Max(0.01f, GetPhaseMaxAliveMultiplier());
		float tierTailRamp = Mathf.Max(1f, GetPhaseTierTailRampMultiplier());
		float tierProgress = GetCurrentTierProgress01();
		float openingMult = GetOpeningVolumeMultiplier(OpeningMaxAliveStartMultiplier);
		float phaseTailMult = IsInPhaseTailPrepWindow() ? Mathf.Clamp(PhaseTailMaxAliveMultiplier, 0.35f, 1f) : 1f;
		int max = Mathf.RoundToInt(_baseMaxAlive * mult * Mathf.Lerp(1f, tierTailRamp, tierProgress) * openingMult * phaseTailMult);
		if (MaxAliveCap > 0)
			max = Mathf.Min(max, MaxAliveCap);
		return Mathf.Max(1, max);
	}

	private float GetPhaseBudgetMultiplier()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => StableBudgetMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalyBudgetMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFractureBudgetMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalBudgetMultiplier,
			_ => 1f
		};
	}

	private float GetPhaseBudget(int baseBudget)
	{
		float phaseMult = Mathf.Max(0.05f, GetPhaseBudgetMultiplier());
		float tierTailRamp = Mathf.Max(1f, GetPhaseTierTailRampMultiplier());
		float tierProgress = GetCurrentTierProgress01();
		float rampMult = Mathf.Lerp(1f, tierTailRamp, tierProgress);
		float openingMult = GetOpeningVolumeMultiplier(OpeningBudgetStartMultiplier);
		float phaseTailMult = IsInPhaseTailPrepWindow() ? Mathf.Clamp(PhaseTailBudgetMultiplier, 0.35f, 1f) : 1f;
		return Mathf.Max(1f, baseBudget * phaseMult * rampMult * openingMult * phaseTailMult);
	}

	private float GetOpeningRamp01()
	{
		if (!UseOpeningRamp)
			return 1f;

		float duration = Mathf.Max(0.1f, OpeningRampSeconds);
		float t = Mathf.Clamp(_survivalSeconds / duration, 0f, 1f);
		return t * t * (3f - 2f * t); // smoothstep
	}

	private float GetOpeningSpawnIntervalMultiplier()
	{
		float start = Mathf.Max(1f, OpeningSpawnIntervalStartMultiplier);
		float ramp = GetOpeningRamp01();
		return Mathf.Lerp(start, 1f, ramp);
	}

	private float GetOpeningVolumeMultiplier(float startMultiplier)
	{
		float start = Mathf.Clamp(startMultiplier, 0.05f, 1f);
		float ramp = GetOpeningRamp01();
		return Mathf.Lerp(start, 1f, ramp);
	}

	private float GetPhaseTierTailRampMultiplier()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => StableTierTailRampMultiplier,
			StabilitySystem.StabilityPhase.EnergyAnomaly => EnergyAnomalyTierTailRampMultiplier,
			StabilitySystem.StabilityPhase.StructuralFracture => StructuralFractureTierTailRampMultiplier,
			StabilitySystem.StabilityPhase.CollapseCritical => CollapseCriticalTierTailRampMultiplier,
			_ => 1f
		};
	}

	private float GetCurrentTierProgress01()
	{
		if (!IsInstanceValid(_stabilitySystem))
			return 0f;

		float elapsed = _stabilitySystem.ElapsedSeconds;
		float stableEnd = Mathf.Max(1f, _stabilitySystem.StablePhaseEndSeconds);
		float anomalyEnd = Mathf.Max(stableEnd + 1f, _stabilitySystem.EnergyAnomalyPhaseEndSeconds);
		float fractureEnd = Mathf.Max(anomalyEnd + 1f, _stabilitySystem.StructuralFracturePhaseEndSeconds);
		float matchEnd = Mathf.Max(fractureEnd + 1f, _stabilitySystem.MatchDurationLimitSeconds);

		float phaseStart = 0f;
		float phaseEnd = stableEnd;
		switch (GetCurrentPhase())
		{
			case StabilitySystem.StabilityPhase.Stable:
				phaseStart = 0f;
				phaseEnd = stableEnd;
				break;
			case StabilitySystem.StabilityPhase.EnergyAnomaly:
				phaseStart = stableEnd;
				phaseEnd = anomalyEnd;
				break;
			case StabilitySystem.StabilityPhase.StructuralFracture:
				phaseStart = anomalyEnd;
				phaseEnd = fractureEnd;
				break;
			case StabilitySystem.StabilityPhase.CollapseCritical:
				phaseStart = fractureEnd;
				phaseEnd = matchEnd;
				break;
		}

		float duration = Mathf.Max(1f, phaseEnd - phaseStart);
		return Mathf.Clamp((elapsed - phaseStart) / duration, 0f, 1f);
	}

	private int GetPhasePackCount()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.Stable => Mathf.Max(1, StablePacksPerWave),
			StabilitySystem.StabilityPhase.EnergyAnomaly => Mathf.Max(1, EnergyAnomalyPacksPerWave),
			StabilitySystem.StabilityPhase.StructuralFracture => Mathf.Max(1, StructuralFracturePacksPerWave),
			StabilitySystem.StabilityPhase.CollapseCritical => Mathf.Max(1, CollapseCriticalPacksPerWave),
			_ => 1
		};
	}

	private int GetEliteUnlockReductionForPhase()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.EnergyAnomaly => Mathf.Max(0, EnergyAnomalyEliteUnlockReduction),
			StabilitySystem.StabilityPhase.StructuralFracture => Mathf.Max(0, StructuralFractureEliteUnlockReduction),
			StabilitySystem.StabilityPhase.CollapseCritical => Mathf.Max(0, CollapseCriticalEliteUnlockReduction),
			_ => 0
		};
	}

	private int GetMiniBossUnlockReductionForPhase()
	{
		return GetCurrentPhase() switch
		{
			StabilitySystem.StabilityPhase.StructuralFracture => Mathf.Max(0, StructuralFractureMiniBossUnlockReduction),
			StabilitySystem.StabilityPhase.CollapseCritical => Mathf.Max(0, CollapseCriticalMiniBossUnlockReduction),
			_ => 0
		};
	}
}
