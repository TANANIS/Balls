# Game Director And Pressure Design

This document defines pacing logic for spawn orchestration and upgrade timing.

## Intent
- Keep early game readable.
- Escalate pressure via spawn tempo and enemy composition.
- Guarantee upgrade cadence without punishing strong play.
- Keep unlock logic intuitive: milestone unlocks are tied to `upgrade_count`, not pressure.
- Lock run duration to 15 minutes and shape pacing by four pressure phases.

## Match Timeline Contract (15:00)
- `00:00 - 03:45` Stage 1 (Ramp-In)
  - Low baseline pressure.
  - Tail-end pressure peak.
  - Stage boss: `MiniBossHex_Stage1` at `03:45`.
- `03:45 - 07:30` Stage 2 (First Stress Cycle)
  - Pressure resets lower than peak, then ramps again.
  - Stage boss: `MiniBossHex_Stage2` at `07:30`.
- `07:30 - 11:15` Stage 3 (Build Check)
  - Faster spawn tempo and denser packs near tail.
  - Stage boss: `MiniBossHex_Stage3` at `11:15`.
- `11:15 - 15:00` Stage 4 (Final Climb)
  - Highest sustained pressure with final tail peak.
  - Stage boss: `MiniBossHex_Stage4` near run tail (`14:30~15:00` window).

Special universe events are removed in this model. Stage-tail miniboss is the only phase-special spike marker.

## Dual-Meter Model
- `CurrentPressure` (volatile): reflects immediate danger from enemy density, low HP, and elapsed time.
- `UpgradeProgress` (progress meter): mainly increased by kills, with pressure bonus and small time drip.

Why this model:
- Pressure-only triggers can be delayed forever by highly efficient clearing.
- Kill-led progression preserves player agency while pressure still influences speed.

## Upgrade Trigger Rule (Survivor-Style)
1. Player kills enemies to generate `ExperiencePickup` drops.
2. Player collects pickup to gain EXP immediately.
3. When EXP reaches requirement, one level-up charge is queued.
4. Upgrade menu opens and consumes one queued charge.
5. EXP overflow is preserved; multiple charges can queue for chain level-up.

System notes:
- Pressure no longer auto-drops when opening upgrade from EXP.
- Time-based passive EXP drip is disabled in EXP-pickup mode.
- Pressure curve remains a director signal for spawning, not player leveling.

## HUD Contract (Run-Time)
- HP UI is hidden in menu/title and only shown after `StartRun()`.
- XP bar is shown at top of screen during active run and reads from `PressureSystem`:
  - Value = `CurrentUpgradeProgress`
  - Max = `GetCurrentUpgradeRequirement()`
  - Ready state = `IsUpgradeReady`
- Match countdown (`15:00 -> 00:00`) is shown on top-right during active run.
- When run ends (death/clear), HP UI and XP bar are hidden.

## Character Balance Notes (Current)
- Melee role has first-pass nerf applied:
  - lower max HP
  - higher melee cooldown
  - higher dash cooldown
  - shorter dash iframe
- Tank role has anti-chase compensation:
  - stronger base ranged damage
  - tank bullet applies extra knockback and bonus damage on hit
- Ranged compensation pass is pending and should be tuned with playtest data.

## End-State And Local Record
- Death path uses failure end-state UI (`SYSTEM FAILURE`).
- Reaching full `15:00` uses dedicated perfect-clear end-state UI (`PERFECT CLEAR`).
- Perfect clear writes local leaderboard record (score, datetime, character display name).

## Spawn Director Rule
`SpawnSystem` is tier-driven and data-driven:
1. Read pressure tier from `PressureSystem.CurrentPressure`.
2. Apply tier runtime settings from `PressureTierRules.csv`.
3. Roll wave budget and split into packed group spawns.
4. Pick enemies by weighted roll from `TierEnemyWeights.csv` under budget/cost constraints.
5. Resolve `enemy_id` to scene path via `EnemyDefinitions.csv`.
6. Spawn around player with tier radius plus pack scatter.

Unlock milestone rule:
- Pressure/tier controls pacing.
- Stage-tail miniboss schedule controls boss pacing (4 fixed spawns per run).
- Optional elite injection can remain upgrade-count gated.

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
- Keep stage-tail boss pacing time-based, not random-event based.
