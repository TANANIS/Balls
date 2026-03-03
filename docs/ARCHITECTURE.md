# Oriluneia Architecture
Last Synced: 2026-03-03


## First Principles
- `ProgressionSystem` owns EXP/upgrade progress and level-up queue timing.
- `UpgradeSystem` increments `upgrade_count` and unlocks content.
- `EventDirector` owns in-run slot activation timestamps.
- `RunPlanBuilder` owns pre-run slot validation and per-slot metadata resolution.
- `Director/SpawnSystem` composes encounters from:
  - current `Tier` (pace/density),
  - unlocked content pool,
  - pack rules.

In short:
- Stability phase controls **pacing and threat shape**.
- Event loadout controls **scheduled risk profile**.
- EXP progression controls **when upgrades happen**.
- Upgrades only control **what content is unlocked**.
- Tier only controls **battlefield pacing**.
- "Pressure" is treated as **time intensity**, not a separate abstract stat.

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
|  |  |- SpawnSystem
|  |  `- EventDirector
|  `- Progression
|     |- ProgressionSystem
|     `- UpgradeSystem
`- CanvasLayer/UI
   |- GameFlowUIRoot
   |  |- BootTitleScreen (Press Any Button)
   |  |- StartPanel
   |  |- PausePanel
   |  `- RestartPanel
   `- UpgradeMenu
```

## Scene Path Contract (2026-02-28)
- Canonical runtime scene roots:
  - `Scenes/Actors/Enemies/*`
  - `Scenes/Projectiles/*`
  - `Scenes/Props/Obstacles/*`
  - `Scenes/Gameplay/*`
  - `Scenes/VFX/*`
  - existing roots kept: `Scenes/UI/*`, `Scenes/World/*`, `Scenes/Systems/*`
- Deprecated for runtime ownership:
  - `Enemies/*`
  - `Prefabs/*`
- Guardrail:
  - runtime `.tscn/.tres` references to `res://Prefabs/` and `res://Enemies/` should stay at `0`.
  - validate with `Tools/Quality/Check-StructureHealth.ps1` and `Tools/Quality/Check-SceneResourcePaths.ps1`.

## System Boundaries
- `Core/*`: universal runtime services.
- `Director/*`: pacing and encounter orchestration.
- `Director/Event*`: slot-based event scheduling and activation.
- `Progression/*`: upgrade application and progression effects.
- `UI/*`: presentation and input only. UI may call systems; systems do not depend on UI.
- `Audio/*`: centralized BGM/SFX routing and runtime playback policy.

## UI Binding Contract (2026-03-03)
- `GameFlowUI` node references are now `Export NodePath` fields (Inspector-overridable) instead of hard-coded path constants.
- Default exported paths still target current `Scenes/UI/GameFlowUIRoot.tscn` hierarchy; runtime behavior is unchanged if paths are not edited.
- Localization label bindings in `GameFlowUI.Localization*` also follow exported NodePath configuration.
- Runtime UI layout tuning values (dialog sizes, responsive breakpoints, roll-flash offsets/sizes, event-unlock table dimensions) are exposed under `UI Layout/*` exported fields on `GameFlowUI`.
- Boot sequence now enters `BootTitleScreen` first and transitions to menu flow on any key/mouse/gamepad button input.

## UI Routing + Page Controller Contract (2026-03-03)
- `GameFlowUI` is now the start-flow router/state owner:
  - page visibility switching (`SetStartSubPanels`),
  - flow forward/back transitions,
  - run-state entry/exit.
- Start pages own their local node binding, button events, and page-local text refresh:
  - `Scripts/UI/Pages/StartMainPageController.cs`
  - `Scripts/UI/Pages/StartSettingsPageController.cs`
  - `Scripts/UI/Pages/StartCardsPageController.cs`
  - `Scripts/UI/Pages/StartCharacterSelectPageController.cs`
- Shared pre-run UI data is centralized in model/service-like state objects:
  - `Scripts/UI/Models/GameFlowUiSharedState.cs`
  - includes settings state, selected character state, and event-loadout draft snapshot.
- Current transition stage:
  - Event unlock/loadout pages still execute logic in `GameFlowUI.*` partial controllers,
  - but draft selection is mirrored into shared state for future full separation.

## Event Scheduling Contract (V0.3)
- Canonical spec: `docs/EVENT_SCHEDULING_META_CONTAINMENT_V0_3.md`
- Run plan is authored pre-run with exactly 4 slots.
- Distortion and affinity evaluate adjacent slots only.
- Max same-domain consecutive slot count is `2`.
- Event difficulty tuning is time-driven:
  - slot timestamp,
  - duration/window,
  - tier profile,
  - distortion/affinity multipliers.
- Do not introduce `basePressureValue` as a balancing axis.
- Runtime `EventRunner` applies active slot rules directly to gameplay loops:
  - `PlayerMovement` / `Enemy` movement multipliers and external pull vectors.
  - `Bullet` / `EnemyProjectile` projectile speed-range compression in Event Horizon.
  - `PlayerMelee` range compression in Event Horizon.
  - `SpawnSystem.EventSpawnDirectionalRush(...)` hook for Blood Tide directional enemy influx.
- `EventDirector` resolves resonance hybrid tags at slot activation (30% deterministic roll, gated by hybrid unlock state) and forwards selected hybrid variant id to `EventRunner`.
- Temporary runtime verification layer:
  - `EventTelegraphOverlay` draws active event zones/markers in world space (ice zones, pulse rings, horizon/well radius, war direction/marks).
  - HUD exposes `EventHint` + short `HybridToast` text for event behavior validation.
- Runtime `RewardService` (director-owned) records completed slot rewards into run domain-shard ledger and exposes snapshot for run-end settlement.
- Pre-run loadout availability is gated by domain power inventory (`MetaProgressionService.GetDomainPowerCount(domainId) > 0`).
- Entering pre-run loadout auto-generates one random event per slot from available domain power; slot domain forcing is the only reroll trigger.
- Out-of-run event economy flow:
  - `GameFlowUI` provides a dedicated meta `Event Purchase/Hybrid Unlock` panel between character select and loadout.
  - Domain power purchase actions call `MetaProgressionService.TryPurchaseDomainPower(..., purchaseCount=1)` (`+3` power per purchase).
  - Hybrid actions call `MetaProgressionService.TryUnlockHybridVariant(...)` directly.
  - Talisman progression branch is out-of-run only and owns event/class upgrade tracks.
  - Successful transactions persist immediately via `MetaProgressionService` single-writer rule.
- Migration note:
  - current runtime still uses legacy bool event-unlock path in some modules.
  - target contract is charge-count gating and per-run consume.
- Run-end settlement UI shows per-domain shard gain (`Ice/Spacetime/War`) from `RewardBreakdown.DomainShardGainsByDomain` in addition to total currency.

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
- Miniboss progression bonus (temporary tuning):
  - defeating a miniboss grants immediate bonus `+1` level and `+10` EXP.
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
6. Miniboss kill bonus is applied through `ExperienceDropSystem -> ProgressionSystem.GrantBossKillBonus(...)`.

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

## Character Numeric Data Source
- Runtime character stat source is now dual-layer:
  - base definition from `Data/Characters/*.tres`,
  - numeric override from `Data/Characters/CharacterStats.csv`.
- Current CSV-overridden fields:
  - `MaxHp`
  - `MoveMaxSpeed`
  - `RangedCooldown`
  - `MeleeCooldown`
  - `DashCooldown`
- Application points:
  - `RunContext` load/select path,
  - `GameFlowUI` character definition load fallback,
  - `Player.ApplyCharacter` final pre-apply normalization.

## Contributor Guardrails
- Do not read progression state directly in enemy scripts.
- Do not hard-code tier logic outside director systems.
- Tune balancing via data tables first, code second.
- Stability phase + tier data control pacing. Unlock milestones use `upgrade_count`.
- Do not add random mid-run event selection for V0.3.
- Keep event-domain interaction strictly adjacent-slot scoped.

## Known Architecture Risks (Current)
- Group-name service discovery is string-based (`GetNodesInGroup`), so typo/rename regressions are compile-time invisible.
- `GameFlowUI` state currently combines multiple booleans (`_started`, `_ending`, `_pauseMenuOpen`, `_settingsOpen`, `_start*Open`), which can drift into invalid combinations as flows grow.
- Large partial-class families (`SpawnSystem`, `GameFlowUI`, `Player*`) are easier to read now but can hide cross-file coupling; behavior changes should always be traced across sibling partial files.
- Runtime depends on CSV/resource schema consistency (`Data/Director/*.csv`, character `.tres`); missing schema/version checks can fail late at runtime.
- Automated gameplay regression tests are minimal; most validation is still build + manual run, which increases risk for flow-level regressions.

Update note (2026-02-28):
- runtime group literals are centralized under `Scripts/Shared/RuntimeGroups.cs` and adopted across core systems.

Update note (2026-03-03):
- `GameFlowUI` boot title entry and inspector-driven node-path/layout configuration were added to reduce UI refactor break risk.

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
