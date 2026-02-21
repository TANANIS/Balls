# Project Genesis Architecture

## First Principles
- `ProgressionSystem` owns EXP/upgrade progress and level-up queue timing.
- `UpgradeSystem` increments `upgrade_count` and unlocks content.
- `Director/SpawnSystem` composes encounters from:
  - current `Tier` (pace/density),
  - unlocked content pool,
  - pack rules.

In short:
- Stability phase controls **pacing and threat shape**.
- EXP progression controls **when upgrades happen**.
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
|  |  `- SpawnSystem
|  `- Progression
|     |- ProgressionSystem
|     `- UpgradeSystem
`- CanvasLayer/UI
   `- UpgradeMenu
```

## System Boundaries
- `Core/*`: universal runtime services.
- `Director/*`: pacing and encounter orchestration.
- `Progression/*`: upgrade application and progression effects.
- `UI/*`: presentation and input only. UI may call systems; systems do not depend on UI.

## Progression + Upgrade Model
- `UpgradeProgress`: XP/upgrade meter.
- EXP requirement curve: `base + linear * level`, then scaled by `growth_factor^level`.
- Overflow is preserved, and multiple level-up charges can queue.
- `AppliedUpgradeCount`: content unlock milestone counter (from `UpgradeSystem`).

Trigger flow:
1. Enemy dies -> `ExperienceDropSystem` spawns `ExperiencePickup`.
2. Player collects pickup -> `ProgressionSystem.AddExperienceFromPickup()`.
3. XP reaches requirement -> queue one level-up charge.
4. `UpgradeMenu` opens and consumes one queued charge.
5. Boss flow can still force open via `ProgressionSystem.ForceOpenForBoss()`.

## Director Data-Driven Tables
Location: `Data/Director/`
- `PressureTierRules.csv`
- `EnemyDefinitions.csv`
- `TierEnemyWeights.csv`
- `_planned/PackTemplates.csv` (planned, not used by runtime)
- `_planned/BossSchedule.csv` (planned, not used by runtime)

Current runtime usage:
- `SpawnSystem` reads:
  - `PressureTierRules.csv` for spawn pace and limits,
  - `EnemyDefinitions.csv` for enemy scene mapping and spawn-time stat overrides (`hp`, `speed`, `contact_damage`),
  - `TierEnemyWeights.csv` for weighted enemy selection per tier.
  - unlock logic from `AppliedUpgradeCount`:
    - `upgrade_count >= 4`: low-frequency elite injection (10%~15% replace chance),
    - `upgrade_count == 6`: schedule one-time miniboss spawn with 2s spawn freeze.

## Contributor Guardrails
- Do not read progression state directly in enemy scripts.
- Do not hard-code tier logic outside director systems.
- Tune balancing via data tables first, code second.
- Stability phase + tier data control pacing. Unlock milestones use `upgrade_count`.

## Text Encoding Rule (Bilingual UI)
- All localization text files must be saved as UTF-8.
- When editing `.tres`/`.tscn` with Traditional Chinese content, avoid tools that may write legacy code pages.
- If garbled text appears in UI:
  - first fix source strings in `Data/Characters/*.tres` and UI composition strings,
  - then re-save as UTF-8 and rebuild to validate.

## Skill VFX Asset Contract
- Skill visual assets are standardized under:
  - `Assets/Sprites/Skills/<SkillName>/`
- Runtime scene anchor for player skill visuals:
  - `Player/SkillVfxRoot` (`Node2D`, high z-index)
  - all player-attached skill VFX should be added under this node (not directly under gameplay logic nodes).
- Runtime access contract:
  - use `Player.GetSkillVfxRoot()` as the first-choice accessor.
  - only fallback to local `NodePath` lookup when `Player` facade is unavailable.
- Gameplay systems that render skill effects should:
  - expose an export slot for texture override,
  - provide a stable fallback path in the same skill folder,
  - keep visual state transitions in the owning runtime component.
- Current reference implementation:
  - `SURV_SHIELD_COOLDOWN` in `Scripts/Player/PlayerHealth.cs`
  - fallback sprite path: `res://Assets/Sprites/Skills/Shield/shield.png`
