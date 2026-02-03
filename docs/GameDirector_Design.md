# Game Director And Pressure Design

This document defines pacing logic for spawn orchestration and upgrade timing.

## Intent
- Keep early game readable.
- Escalate pressure via spawn tempo and enemy composition.
- Guarantee upgrade cadence without punishing strong play.

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
3. Pick enemy by weighted roll from `TierEnemyWeights.csv`.
4. Resolve `enemy_id` to scene path via `EnemyDefinitions.csv`.
5. Spawn around player using tier radius range.

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
