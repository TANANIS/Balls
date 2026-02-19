# Game Director And Pressure Design

This document defines pacing logic for spawn orchestration and upgrade timing.

## Intent
- Keep early game readable.
- Escalate pressure via spawn tempo and enemy composition.
- Guarantee upgrade cadence without punishing strong play.
- Keep unlock logic intuitive: milestone unlocks are tied to `upgrade_count`, not pressure.
- Lock run duration to 15 minutes and shape pacing by four environment-state phases.

## Match Timeline Contract (15:00)
- `00:00 - 03:00` Stable:
  - Base enemies only.
  - Pressure has natural decay room.
  - No universe anomaly effect.
  - Goal: establish baseline build and first attack-modifier power spike.
- `03:00 - 07:00` Energy Anomaly:
  - First anomaly cycle starts.
  - First hard pressure point.
- `07:00 - 11:00` Structural Fracture:
  - Enemy composition upgrades sharply.
  - Spawn waves accelerate.
  - Universe events are stronger and/or more frequent.
  - Pressure natural decay is reduced.
  - Goal: build validation window.
- `11:00 - 15:00` Collapse Critical:
  - Layered anomalies.
  - High-density horde generation.
  - Pressure decay nearly stalls.
  - End-phase elite presence.

## Universe Event Cadence Contract
- Target cadence: every 3 minutes.
- Target timestamps in one 15-minute run:
  - `03:00`
  - `06:00`
  - `09:00`
  - `12:00`
- Note: current project implementation is not fully aligned yet; this is the target logic to sync toward.

## Dual-Meter Model
- `CurrentPressure` (volatile): reflects immediate danger from enemy density, low HP, and elapsed time.
- `UpgradeProgress` (progress meter): mainly increased by kills, with pressure bonus and small time drip.

Why this model:
- Pressure-only triggers can be delayed forever by highly efficient clearing.
- Kill-led progression preserves player agency while pressure still influences speed.

## Upgrade Trigger Rule
1. `UpgradeProgress` reaches threshold (`FirstTriggerThreshold` for first time, then `TriggerThreshold`).
2. System becomes `armed`.
3. Next player kill opens `UpgradeMenu`.
4. On trigger: apply cooldown and reduce pressure/progress by configured amounts.

Boss exception:
- `ForceOpenForBoss()` can open the menu immediately for event pacing.

## Spawn Director Rule
`SpawnSystem` is tier-driven and data-driven:
1. Read pressure tier from `PressureSystem.CurrentPressure`.
2. Apply tier runtime settings from `PressureTierRules.csv`.
3. Roll wave budget and split into packed group spawns.
4. Pick enemies by weighted roll from `TierEnemyWeights.csv` under budget/cost constraints.
5. Resolve `enemy_id` to scene path via `EnemyDefinitions.csv`.
6. Spawn around player with tier radius plus pack scatter.

Unlock milestone rule:
- Pressure/tier only controls pacing.
- Content unlock uses `UpgradeSystem.AppliedUpgradeCount`.
- `upgrade_count >= 4`: allow elite injection (10%~15% chance per spawn decision).
- `upgrade_count == 6`: schedule one miniboss event and freeze regular spawns for 2 seconds.

Fallback behavior:
- If CSV or mapping is incomplete, fallback to `EnemyScene` export.

## Data Tables
All under `Data/Director/`:
- `EnemyDefinitions.csv`
- `PressureTierRules.csv`
- `TierEnemyWeights.csv`
- `PackTemplates.csv` (planned usage)
- `BossSchedule.csv` (planned/partial usage)

## PressureTierRules Contract
Used fields now include:
- `pressure_min`, `pressure_max`
- `spawn_interval_min`, `spawn_interval_max`
- `budget_min`, `budget_max`
- `max_alive`
- `spawn_radius_min`, `spawn_radius_max`
- `kill_progress_base`
- `kill_pressure_bonus_factor`
- `time_progress_per_sec`
- `upgrade_threshold`
- `first_upgrade_threshold`

## Contributor Guardrails
- Do not access pressure directly in enemy behavior scripts.
- Do not hard-code tier logic outside director systems.
- Tune balance in CSV first, then patch code only when needed.
- Do not use pressure as content unlock gate for elite/miniboss milestones.
