# Game Director And Progression Design
Last Synced: 2026-02-28


This document defines in-run pacing, pre-run event scheduling, and upgrade timing.

## Intent
- Keep early game readable and late game intense.
- Replace random event reaction with deterministic pre-run event commitment.
- Keep risk/reward understandable via slot previews.
- Keep run duration fixed at `15:00` with four stage phases.

## Match Timeline Contract (15:00)
- `00:00 - 03:45`: Stage 1 (`Ramp-In`)
- `03:45 - 07:30`: Stage 2 (`First Stress Cycle`)
- `07:30 - 11:15`: Stage 3 (`Build Check`)
- `11:15 - 15:00`: Stage 4 (`Final Climb`)

## Event Slot Contract

| Slot | Tier | Phase |
|---|---|---|
| Slot1 | Tier0 | Early |
| Slot2 | Tier1 | Mid |
| Slot3 | Tier2 | Late |
| Slot4 | Tier3 | Final |

Rules:
- Slot timestamps are owned by `EventDirector`.
- Event chain is preselected before run start.
- No random mid-run event selection.

## Time-Driven Intensity Rule
- "Pressure" is defined as game-time intensity.
- Do not use abstract `basePressureValue` for balancing.
- Event tuning must be authored with time profile controls:
  - activation timestamp,
  - event duration/window,
  - tier time profile,
  - optional domain interaction multipliers.

## Pre-Run Planning Flow
1. Load event pool with positive remaining charges from meta state.
2. On entering loadout UI, system auto-rolls 4 slot events from current available pool.
3. Player may optionally force a slot's domain (Tier is fixed by slot index) by consuming the corresponding tier `Order Sigil`.
4. Forced slots reroll from that domain pool only; manual event reroll buttons are disabled.
5. Unlocked advanced events with remaining charges are included in the same random pool.
6. `RunPlanBuilder` validates sequence:
  - max two same-domain consecutive slots.
7. `DistortionResolver` computes per-slot distortion.
8. `AffinityResolver` computes adjacent-slot relation.
9. Save `RunPlan` for runtime activation.

## Distortion Rule (Same-Domain Consecutive)
- Valid: `A A B C`
- Invalid: `A A A B`

| Distortion | Time Intensity | Reward |
|---|---:|---:|
| D0 | x1.00 | x1.00 |
| D1 | x1.30 | x1.50 |

## Affinity Rule (Adjacent Cross-Domain)
Current matrix:
- `Ice` + `Spacetime` -> `Resonance`
- `Ice` + `War` -> `Dissonance`
- `Spacetime` + `War` -> `Resonance`

| Relation | Time Intensity | Reward | Hybrid Variant |
|---|---:|---:|---|
| Resonance | x1.20 | x1.30 | 30% chance |
| Neutral | x1.00 | x1.00 | No |
| Dissonance | x0.85 | x0.80 | No |

## Runtime Event Activation Order
1. `EventDirector` reaches slot timestamp.
2. Load slot `EventDefinition`.
3. Apply tier time profile.
4. Apply slot distortion multiplier.
5. Resolve and apply affinity multiplier with previous slot.
6. If resonance, roll hybrid variant tag.
7. Execute final rule set through `EventRunner`.

## Upgrade Trigger Rule (Survivor-Style)
1. Enemy death drops `ExperiencePickup`.
2. Player collects pickup and gains EXP.
3. EXP reaches requirement and queues one level-up charge.
4. Upgrade menu opens and consumes one queued charge.
5. Overflow EXP is preserved.

## HUD Contract (Run-Time)
- HP UI is visible only during active run.
- XP bar reads `ProgressionSystem` runtime values.
- Countdown (`15:00 -> 00:00`) remains primary global objective signal.
- Event status banner should be short and phase-readable.

## Spawn Director Rule
`SpawnSystem` remains tier-driven and data-driven:
1. Read current phase tier from `StabilitySystem`.
2. Apply tier runtime settings from `PressureTierRules.csv`.
3. Spawn enemies from `TierEnemyWeights.csv` and `EnemyDefinitions.csv`.
4. Apply event runner modifications as temporary overlays.

Guardrail:
- Event systems can modulate runtime timing/intensity, but core spawn table ownership stays in director CSV.

## End-State And Record
- Death: failure end-state panel.
- Survive to `15:00`: perfect-clear panel.
- Settlement writes score and event rewards into meta save profile.

## Data Tables
Current active director tables under `Data/Director/`:
- `EnemyDefinitions.csv`
- `PressureTierRules.csv`
- `TierEnemyWeights.csv`

Event table contract:
- event definition storage is owned by the event scheduling system spec (`docs/EVENT_SCHEDULING_META_CONTAINMENT_V0_3.md`).

## Contributor Guardrails
- Do not hard-code tier logic outside director systems.
- Do not implement mid-run random event selection for V0.3.
- Keep adjacent-only evaluation for distortion/affinity.
- Tune event intensity with timeline parameters first, scalar math second.
