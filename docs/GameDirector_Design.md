# Game Director And Progression Design

This document defines pacing logic for spawn orchestration and upgrade timing.

## Intent
- Keep early game readable.
- Escalate threat via spawn tempo and enemy composition.
- Guarantee upgrade cadence without punishing strong play.
- Keep unlock logic intuitive: milestone unlocks are tied to `upgrade_count`.
- Lock run duration to 15 minutes and shape pacing by four stability phases.

## Match Timeline Contract (15:00)
- `00:00 - 03:45` Stage 1 (Ramp-In)
  - Low baseline threat.
  - Tail-end threat peak.
  - Stage boss: `MiniBossHex_Stage1` at `03:45`.
- `03:45 - 07:30` Stage 2 (First Stress Cycle)
  - Threat resets lower than peak, then ramps again.
  - Stage boss: `MiniBossHex_Stage2` at `07:30`.
- `07:30 - 11:15` Stage 3 (Build Check)
  - Faster spawn tempo and denser packs near tail.
  - Stage boss: `MiniBossHex_Stage3` at `11:15`.
- `11:15 - 15:00` Stage 4 (Final Climb)
  - Highest sustained threat with final tail peak.
  - Stage boss: `MiniBossHex_Stage4` near run tail (`14:30~15:00` window).

Special universe events are removed in this model. Stage-tail miniboss is the only phase-special spike marker.

## Runtime Model
- `ProgressionSystem` owns `UpgradeProgress` (EXP meter), requirement curve, and queued level-up charges.
- `SpawnSystem` pacing is selected by current `StabilitySystem` phase and tier CSV rows.
- `UpgradeSystem` applies selected upgrade effects and increments `AppliedUpgradeCount`.

## Upgrade Trigger Rule (Survivor-Style)
1. Player kills enemies to generate `ExperiencePickup` drops.
2. Player collects pickup to gain EXP immediately.
3. When EXP reaches requirement, one level-up charge is queued.
4. Upgrade menu opens and consumes one queued charge.
5. EXP overflow is preserved; multiple charges can queue for chain level-up.

System notes:
- EXP overflow is preserved.
- Time-based passive EXP drip is not used in current pickup-driven runtime.
- Upgrade menu consume rule remains one queued charge per open.

## HUD Contract (Run-Time)
- HP UI is hidden in menu/title and only shown after `StartRun()`.
- XP bar is shown at top of screen during active run and reads from `ProgressionSystem`:
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
1. Read phase tier from `StabilitySystem.CurrentPhase`.
2. Apply tier runtime settings from `PressureTierRules.csv`.
3. Roll wave budget and split into packed group spawns.
4. Pick enemies by weighted roll from `TierEnemyWeights.csv` under budget/cost constraints.
5. Resolve `enemy_id` to scene path via `EnemyDefinitions.csv`.
6. Spawn around player with tier radius plus pack scatter.

Unlock milestone rule:
- Stability phase + tier controls pacing.
- Stage-tail miniboss schedule controls boss pacing (4 fixed spawns per run).
- Optional elite injection can remain upgrade-count gated.

Fallback behavior:
- If CSV or mapping is incomplete, fallback to `EnemyScene` export.

## Data Tables
All under `Data/Director/`:
- `EnemyDefinitions.csv`
- `PressureTierRules.csv`
- `TierEnemyWeights.csv`
- `_planned/PackTemplates.csv` (planned usage)
- `_planned/BossSchedule.csv` (planned/partial usage)

## Tier Rules Contract
Used fields now include:
- `pressure_min`, `pressure_max`
- `spawn_interval_min`, `spawn_interval_max`
- `budget_min`, `budget_max`
- `max_alive`
- `spawn_radius_min`, `spawn_radius_max`

## Contributor Guardrails
- Do not access progression state directly in enemy behavior scripts.
- Do not hard-code tier logic outside director systems.
- Tune balance in CSV first, then patch code only when needed.
- Keep stage-tail boss pacing time-based, not random-event based.
