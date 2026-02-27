# Project Genesis Architecture
Last Synced: 2026-02-27


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
- `Audio/*`: centralized BGM/SFX routing and runtime playback policy.

## Audio Runtime Contract
- `Scripts/Audio/AudioManager*.cs` is the single runtime entry for audio playback APIs.
- BGM is playlist-based, state-driven:
  - `Menu`: title/out-of-run.
  - `Gameplay`: in-run combat.
  - `Result`: run settlement screen (success/failure).
- Playlist behavior:
  - each track is non-loop,
  - when a track finishes, the manager picks another random track from the active playlist,
  - immediate repeat is avoided when possible.
- SFX routing remains event-based (`UI`, `Player`, `Enemies`) through explicit `PlaySfx*()` methods; gameplay systems should not hardcode asset paths.
- Default audio levels (when no user settings file exists):
  - BGM: `50%`
  - SFX: `80%`

## Progression + Upgrade Model
- `UpgradeProgress`: XP/upgrade meter.
- EXP requirement curve: `base + linear * level`, then scaled by `growth_factor^level`.
- Late-run slowdown fuse:
  - starts at upgrade level `5`,
  - ramps to `x2.0` requirement within `2` levels,
  - keeps early/mid progression responsive while reducing late upgrade flooding.
- Overflow is preserved, and multiple level-up charges can queue.
- `AppliedUpgradeCount`: content unlock milestone counter (from `UpgradeSystem`).

Damage model note:
- Character attack stats use float-friendly accumulation in runtime modules (`PlayerWeapon`, `PlayerMelee`).
- Damage requests are still finalized through `CombatSystem` and integer-resolved at hit application boundaries.

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

## Known Architecture Risks (Current)
- Group-name service discovery is string-based (`GetNodesInGroup`), so typo/rename regressions are compile-time invisible.
- `GameFlowUI` state currently combines multiple booleans (`_started`, `_ending`, `_pauseMenuOpen`, `_settingsOpen`, `_start*Open`), which can drift into invalid combinations as flows grow.
- Large partial-class families (`SpawnSystem`, `GameFlowUI`, `Player*`) are easier to read now but can hide cross-file coupling; behavior changes should always be traced across sibling partial files.
- Runtime depends on CSV/resource schema consistency (`Data/Director/*.csv`, character `.tres`); missing schema/version checks can fail late at runtime.
- Automated gameplay regression tests are minimal; most validation is still build + manual run, which increases risk for flow-level regressions.

## Text Encoding Rule (Bilingual UI)
- All localization text files must be saved as UTF-8.
- When editing `.tres`/`.tscn` with Traditional Chinese content, avoid tools that may write legacy code pages.
- If garbled text appears in UI:
  - first fix source strings in `Data/Characters/*.tres` and UI composition strings,
  - then re-save as UTF-8 and rebuild to validate.
- For Godot text resources (`.tres` / `.tscn`), use UTF-8 **without BOM**.
  - Symptom: parser error at line 1, e.g. `Parse Error: Expected '['`.
  - Root cause: file starts with BOM bytes `EF BB BF`, so first token is not recognized as `[` by parser.
  - Quick fix: re-save as UTF-8 no BOM (or strip BOM bytes) and reload/export again.

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

## Runtime Sync Notes
- Character baseline sync:
  - all `Data/Characters/*.tres` roles currently use:
    - `MoveMaxSpeed` increased by `+15%` from pre-pass baseline.
    - `MaxHp` increased by `+1`.
- Priest sustain flow:
  - `RegenIntervalSeconds` is now tuned to `30s` for `tank_burst`.
  - regen timer no longer resets on taking damage.
  - when Priest is full HP and regen interval completes, heal VFX still plays (no HP gain) and starts next interval.
  - Priest heal VFX applies temporary attack lock via `Player.LockAttacks(...)`.
- HUD HP display:
  - `PlayerHealthBarDemo` now uses numeric text only (`HP current/max`).
  - segmented HP blocks are removed from active HUD layout.
- Projectile readability/budget updates:
  - Priest projectile visual scale reduced for combat readability.
  - projectile lifetime/off-screen cleanup path is active to prevent long-tail stray projectiles.
