# Game Director & Pressure Design

This document defines pacing logic for spawn orchestration and upgrade timing.

## Intent
- Keep early game readable.
- Escalate pressure through composition and spawn tempo.
- Guarantee upgrade cadence without punishing strong play.

## Dual-Meter Model
- `CurrentPressure` (volatile, up/down):
  - Reflects current danger from enemy density, low HP, and elapsed time.
- `UpgradeProgress` (progress meter):
  - Primarily increased by kills.
  - Receives pressure-based bonus.
  - Small time drip avoids dead pacing.

Why:
- If upgrades depend only on pressure, skilled clearing can delay upgrades forever.
- Kill-led progression keeps agency while pressure still matters.

## Trigger Rule
1. `UpgradeProgress` reaches threshold (`FirstTriggerThreshold` for first trigger, then `TriggerThreshold`).
2. System becomes `armed`.
3. Next player kill opens `UpgradeMenu`.
4. On trigger:
   - apply cooldown,
   - reduce pressure and progress meters.

Boss exception:
- `ForceOpenForBoss()` may open menu immediately.

## Data Tables
Director tables live in `Data/Director/`:
- `EnemyDefinitions.csv`
- `PressureTierRules.csv`
- `TierEnemyWeights.csv`
- `PackTemplates.csv`
- `BossSchedule.csv`

Current runtime behavior:
- `PressureSystem` reads `PressureTierRules.csv` at startup.
- Tier-dependent fields are applied live as pressure crosses ranges.

### PressureTierRules Contract
`PressureTierRules.csv` includes pacing plus progression tuning:
- `kill_progress_base`
- `kill_pressure_bonus_factor`
- `time_progress_per_sec`
- `upgrade_threshold`
- `first_upgrade_threshold`

Booleans:
- `templates_enabled / allow_elites / allow_pincer`: `1` true, `0` false.

## Contributor Guardrails
- Do not read pressure directly in enemy scripts.
- Do not hard-code tier logic outside director systems.
- Tune pacing in data tables first; code changes second.
