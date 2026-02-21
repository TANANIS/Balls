# Project Genesis Architecture

## First Principles
- `PressureSystem` decides upgrade timing (`armed` and trigger moment).
- `UpgradeSystem` increments `upgrade_count` and unlocks content.
- `Director/SpawnSystem` composes encounters from:
  - current `Tier` (pace/density),
  - unlocked content pool,
  - pack rules.

In short:
- Pressure only controls **when upgrades happen**.
- Upgrades only control **what content is unlocked**.
- Tier only controls **battlefield pacing**.

## Core Rules
- Only `CombatSystem` can finalize damage.
- Sensors (`Hitbox`, `Hurtbox`, `Bullet`) only submit `DamageRequest`; they never deduct HP directly.
- Data flow is one-way: `Emitter -> Request -> Resolve -> Apply`.

## Runtime Layout
```text
Game
|- World
|- Player
|- Enemies
|- Projectiles
|- Systems
|  |- Core
|  |  |- CombatSystem
|  |  `- DebugSystem
|  |- Director
|  |  |- SpawnSystem
|  |  `- PressureSystem
|  `- Progression
|     `- UpgradeSystem
`- CanvasLayer/UI
   `- UpgradeMenu
```

## System Boundaries
- `Core/*`: universal runtime services.
- `Director/*`: pacing and encounter orchestration.
- `Progression/*`: upgrade application and progression effects.
- `UI/*`: presentation and input only. UI may call systems; systems do not depend on UI.

## Pressure + Upgrade Model
- `CurrentPressure`: volatile world tension (enemy count, low HP, survival time).
- `UpgradeProgress`: upgrade meter, primarily driven by kills.
- Kill gain formula: `gain = KillProgressBase * (1 + pressureNorm * KillPressureBonusFactor)`.
- Time drip: `TimeProgressPerSecond` avoids progression stalls.
- `AppliedUpgradeCount`: content unlock milestone counter (from `UpgradeSystem`).

Trigger flow:
1. Meter reaches threshold -> system is `armed`.
2. Next player kill opens `UpgradeMenu`.
3. Boss flow may bypass kill gate via `ForceOpenForBoss()`.

## Director Data-Driven Tables
Location: `Data/Director/`
- `PressureTierRules.csv`
- `EnemyDefinitions.csv`
- `TierEnemyWeights.csv`
- `_planned/PackTemplates.csv` (planned, not used by runtime)
- `_planned/BossSchedule.csv` (planned, not used by runtime)

Current runtime usage:
- `PressureSystem` reads `PressureTierRules.csv` and applies progression parameters by pressure tier.
- `SpawnSystem` reads:
  - `PressureTierRules.csv` for spawn pace and limits,
  - `EnemyDefinitions.csv` for enemy scene mapping,
  - `TierEnemyWeights.csv` for weighted enemy selection per tier.
  - unlock logic from `AppliedUpgradeCount`:
    - `upgrade_count >= 4`: low-frequency elite injection (10%~15% replace chance),
    - `upgrade_count == 6`: schedule one-time miniboss spawn with 2s spawn freeze.

## Contributor Guardrails
- Do not read pressure directly in enemy scripts.
- Do not hard-code tier logic outside director systems.
- Tune balancing via data tables first, code second.
- Pressure controls pacing. Unlock milestones use `upgrade_count`.
