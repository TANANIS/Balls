# Dev Log (Codex Internal, Trimmed)

## Purpose
- This file is a short working memory for Codex.
- Detailed historical context should be read from git history and docs.
- Keep only currently actionable information.

## Current Runtime Baseline (2026-02-25)
- Project: Godot C# top-down survival run (`15:00` target).
- Upgrade pipeline:
  - runtime source is `UpgradeSystem` + `UpgradeCatalog`.
  - card pool currently uses Batch 01 (`13` active cards).
  - dual-axis contract is active:
    - `Layer` for phase pool routing,
    - `Category` for decay/statistics.
- Pool routing:
  - phase pools: Early / Mid / Late.
  - Early pool ratio aligned to 100%.
- Safety fuses:
  - stack caps and mutual exclusion validation are active.
  - category weight decay is active.
- Debug panel (F3):
  - bilingual panel text.
  - opening F3 pauses game; closing restores prior pause state.
  - direct apply upgrade uses debug path (`DebugApplyUpgrade`) and bypasses gate/exclusive/prerequisite checks while still respecting stack cap and character compatibility.

## High-Signal Recent Changes

### 2026-03-01 - Start Menu Copy Refresh + Boss Kill Bonus
- Updated start menu description copy (`UI.START.DESC`) for current pre-run flow:
  - hero select -> event purchases -> arrange calamities,
  - goal and boss reward now explicitly stated.
- Added miniboss kill progression bonus (temporary tuning):
  - each miniboss defeat now grants immediate `+1` level and `+10` EXP.
- Runtime hook:
  - `ExperienceDropSystem` detects miniboss kill and calls `ProgressionSystem.GrantBossKillBonus(...)`.
- Validation:
  - `dotnet build Oriluneia.sln` passed.

### 2026-02-28 - Unused Sprite Cleanup (Conservative)
- Added cleanup script:
  - `Tools/Quality/Delete-UnusedImages.ps1`
- Added cleanup report:
  - `Tools/Quality/deleted_unused_images_2026-02-28.txt`
- Deletion policy used:
  - remove only exact-path-unreferenced images under `Assets/Sprites`,
  - preserve dynamic-load domains to avoid false-positive deletions:
    - `Assets/Sprites/Player/*`
    - `Assets/Sprites/Projectiles/ElementalBurst/*`
    - `Assets/Sprites/World/Terrain/Canonical/*`
- Removed files:
  - 39 unused sprite images + their `.import` sidecars.
- Verification:
  - `dotnet build Oriluneia.sln` passed.
  - `Tools/Quality/Check-SceneResourcePaths.ps1` passed.
  - `Tools/Quality/Check-StructureHealth.ps1 -SkipBuild` passed.

### 2026-02-28 - Structure Cleanup Program (Phase 0-4)
- Added execution and baseline docs:
  - `docs/STRUCTURE_REFACTOR_EXECUTION_PLAN_2026-02-28.md`
  - `docs/reports/STRUCTURE_BASELINE_2026-02-28.md`
- Added quality scripts:
  - `Tools/Quality/Check-StructureHealth.ps1`
  - `Tools/Quality/Check-SceneResourcePaths.ps1`
- Completed high-risk script decomposition:
  - `ObstacleFieldGenerator` split into `Spawn/Environment/Variants/Runtime` partials.
  - `PlayerWeapon` split into `Attack/Configuration/ElementalBurst` partials.
  - `PlayerHealth` VFX split into `Shield/Damage/Priest` partials.
  - `Bullet` homing logic extracted to `Bullet.Homing.cs`.
  - `ProceduralTerrainBackground` split expanded (`Mask.Noise`, `Tiling.Caps`, `Tiling.Resolve`).
- Phase 1 result:
  - files `> 300` lines reduced to `0`.
- Completed scene path unification:
  - runtime scene ownership moved to `Scenes/*` domains (`Actors/Enemies`, `Projectiles`, `Props/Obstacles`, `Gameplay`, `VFX`).
  - `Data/Director/EnemyDefinitions.csv` scene paths migrated to `Scenes/Actors/Enemies/*`.
  - runtime references to deprecated roots now:
    - `res://Prefabs/` => `0`
    - `res://Enemies/` => `0`
- Naming/boundary hardening:
  - group literals centralized with `Scripts/Shared/RuntimeGroups.cs`.
- Validation:
  - `dotnet build Oriluneia.sln` passed.
  - BOM scan passed.
  - scene resource path check passed.

### 2026-02-27 - Slime Speed Follow-up
- `Data/Director/EnemyDefinitions.csv`
  - `slime` speed override: `105 -> 96`
- Intent:
  - soften baseline chase pressure from filler enemies while keeping count-based map pressure.

### 2026-02-27 - Character Numeric CSV Overlay
- Added character numeric table:
  - `Data/Characters/CharacterStats.csv`
- Added runtime loader:
  - `Scripts/Characters/CharacterStatsCsvService.cs`
- Character numeric fields now overridable via CSV:
  - `MaxHp`
  - `MoveMaxSpeed`
  - `RangedCooldown`
  - `MeleeCooldown`
  - `DashCooldown`
- Applied at three runtime entry points:
  - `RunContext` (default/select resolution),
  - `GameFlowUI` character definition load path,
  - `Player.ApplyCharacter` pre-apply normalization.
- Translation loading baseline kept stable (`project.godot` uses `*.translation` registration).

### 2026-02-26 - Director Pressure Relief + Enemy Contact Recalibration
- Player survivability and mobility baseline updates:
  - all four characters `MoveMaxSpeed` increased by `+15%`.
  - all four characters `MaxHp` increased by `+1`.
- Spawn pressure relief (early/mid):
  - `SpawnSystem` catch-up tuning:
    - `HordeTargetAliveRatio: 0.90 -> 0.82`
    - `HordeCatchUpBudgetFactor: 0.40 -> 0.22`
  - `PressureTierRules.csv` Tier1 tuning:
    - `spawn_interval_min: 1.55 -> 1.70`
    - `spawn_interval_max: 2.20 -> 2.35`
    - `max_alive: 28 -> 24`
- Elite orc dash/animation follow-up:
  - chain dash duration restored to second-pass target:
    - `ChainDashDurationMultiplier: 1.77 -> 2.26`
  - attack animation binding adjusted for dash readability:
    - `PlayAttackAnimationInWindup = true`
    - `attack_02` speed increased to `20`
    - `attack_03` speed adjusted to `13` (slower second segment)
- Orc dash threat toned down:
  - `DashSpeedMultiplier: 3.0 -> 2.55` (`-15%`)
- Enemy contact hitbox recalibration pass (reduced oversized contact rings):
  - Orc `33 -> 27`
  - Slime `33 -> 30`
  - SkeletonArcher `33 -> 30`
  - Skeleton `38 -> 33`
  - Werebear `38 -> 33`
  - Werewolf `46 -> 35`
  - EliteOrc `42 -> 38`
  - Boss Lancer `56 -> 50`
  - Boss GreatswordSkeleton `56 -> 50`
- Validation:
  - `dotnet test Tools/BalanceTests/BalanceTests.csproj` passed.
  - `dotnet test ... --filter SpawnDifficultyCurveTests` passed.
  - `dotnet build Oriluneia.sln` passed after each pass.

### 2026-02-26 - UI Palette Pass (Sci-Fi -> Rustic/Fantasy)
- Applied a rustic palette across primary runtime UI:
  - `StartPanel`, `PausePanel`, `RestartPanel`, `HudOverlay`, `PlayerHealthBarDemo`, cursor ring.
- Palette direction:
  - dark wood/brown panel base,
  - warm bronze borders,
  - parchment-like text colors,
  - warmer hover/pressed button states.
- Upgrade draw UI (`UpgradeMenu`) synchronized to the same palette:
  - panel/button style colors replaced from cyan-neon to warm earthy tones.
  - title and option text now match rustic color identity.
- Rarity readability pass for upgrade options:
  - `COMMON`: parchment ivory,
  - `RARE`: amber gold,
  - `EPIC`: copper-gold,
  - plus dedicated hover/pressed/disabled text color overrides.
- Validation:
  - `dotnet build Oriluneia.sln` passed after UI palette update.

### 2026-02-26 - Priest Sustain + HUD Readability + Projectile Continuation Tuning
- Priest sustain logic:
  - `TankBurstCharacter` regen interval updated to `30s`.
  - regen timer is no longer reset by incoming damage.
  - when regen interval completes at full HP, Priest heal VFX still triggers (visual timing signal only, no HP gain).
  - Priest heal VFX check now uses case-insensitive character id compare.
- Priest combat lock confirmation:
  - heal VFX path continues to call `player.LockAttacks(duration, interruptCurrentAttack: true)`; Priest cannot attack during heal animation.
- HUD simplification:
  - removed segmented HP blocks from `PlayerHealthBarDemo`; HUD now displays numeric `HP x/y` only.
- Projectile readability/balance:
  - Priest projectile visual scale reduced.
  - continuation falloff strengthened for ranged multi-hit mechanics:
    - pierce and ricochet now apply per-step damage multipliers.
  - split children inherit homing tuning when parent has Arcane Tracking.
- World pacing pressure:
  - far-enemy recycle parameters were tightened to reduce safe-zone abuse and off-screen enemy accumulation.
- Validation:
  - `dotnet build Oriluneia.sln` passed after changes.

### 2026-02-26 - Swordsman Naming + Asset Unification
- Character naming normalized from mixed `Melee/Knight` to `Swordsman`:
  - character id: `swordsman`,
  - display text: `Swordsman / 劍士`,
  - resource: `Data/Characters/SwordsmanCharacter.tres`.
- Runtime/UI binding aligned:
  - `GameFlowUI` character select button/key paths and fallback definition now use `Swordsman`.
  - animation profile now resolves swordsman atlas from `Assets/Sprites/Player/Swordsman/Swordsman*`.
- Save compatibility hardening:
  - legacy ids `melee` and typo `sowrdman` migrate/normalize to `swordsman`.
- Tooling alignment:
  - `CardDrawSim` and `BalanceTests` now read/use `swordsman` id (legacy aliases still accepted in simulator CLI).
  - repository ignore rules now exclude test/sim artifacts (`Tools/**/bin`, `Tools/**/obj`, `Tools/CardDrawSim/reports`, `Tools/**/TestResults`).

### 2026-02-26 - Balance + Progression + Character Pass
- Progression pacing:
  - added late XP slowdown fuse in `ProgressionSystem`:
    - start level `5`,
    - ramp to `x2.0` requirement within `2` levels.
- Character unlock/mobility:
  - Archer default unlock switched to locked (`UnlockCost = 70`).
  - Archer move speed tuned upward (`208 -> 216`).
  - Knight visual scale normalized to default (`CoreSpriteScaleMultiplier = Vector2.One`).
- Enemy durability tuning:
  - Orc HP reduced by about one-third:
    - director definition `10 -> 7`,
    - scene health aligned to `7`.
- Card/system tuning:
  - removed crit chance card from active pool (`ATK_CRIT_CHANCE_UP_10`).
  - damage card retuned:
    - display `Damage +20% -> Damage +10%`,
    - runtime curve `x1.10 -> x1.08 -> x1.06`.
  - projectile+ card renamed and widened:
    - `+1 Arc Bolt -> Skill Split`,
    - same-axis spacing increased (`3/7 -> 8/14` degrees).
  - homing/ricochet target filtering hardened:
    - already-hit targets are no longer valid retargets,
    - if no valid retarget exists, projectile continues forward until lifetime ends.
- Damage authoring flexibility:
  - ranged/melee attack base stats now support float-oriented runtime accumulation
    before final hit resolution.

### 2026-02-25 - Card Framework Alignment
- Removed cooldown card (`ATK_COOLDOWN_DOWN_10`) from runtime/catalog/localization/apply path.
- Added timing gates for strong survival cards via card definition fields:
  - `MinUpgradeCount`, `MinPhase`, optional `MaxPhase` gate.
- Updated docs and changelog to reflect dual-axis contract and gating rules.

### 2026-02-25 - Projectile Identity Split
- `ATK_PROJECTILE_PLUS_1`:
  - same-axis tight-spread volley (single-target pressure), `MaxStack = 2`.
- `ATK_SPLIT_SHOT`:
  - on-hit split from enemy position,
  - split count scaling: `3 -> 4 -> 5 -> 6` (360 deg spread),
  - non-chain split children,
  - split child damage path uses fractional accumulation baseline (`0.5`).
- Mutual exclusion enforced between `ATK_PROJECTILE_PLUS_1` and `ATK_SPLIT_SHOT`.

### 2026-02-25 - Shared Script + Variant Prefab Projectile Convention
- Shared runtime script remains `Scripts/Projectiles/Bullet.cs`.
- Split child spawn now prefers exported prefab field:
  - `SplitChildProjectileScene`.
- Fallback order:
  - `SplitChildProjectileScene -> source projectile scene -> current scene file`.
- Dedicated split prefab:
  - `Prefabs/SplitProjectile.tscn`.
- Current split visual baseline:
  - `Assets/Sprites/Projectiles/Split/Frames/split_bullet_01..07.png`,
  - flight `0..5`, impact `6`,
  - scale tuned to about `2/3` of primary,
  - collider radius `9`.

### 2026-02-25 - New Modifier Card `MOD_ELEMENTAL_BURST`
- Availability and weight:
  - `MinPhase = Early`, `Weight = 8`, `MaxStack = 1`.
- Runtime behavior:
  - charge every `5s`,
  - charged state is retained if player does not fire,
  - next shot becomes explosive,
  - detonate on first hit or max travel distance.
- Current tuned values:
  - radius `130`,
  - damage scale `1.20x`,
  - max distance `280`,
  - max targets `5`.
- VFX:
  - projectile frames: `Assets/Sprites/Projectiles/ElementalBurst/Projectile/elemental_burst_charge_01..08.png`,
  - explosion animation: `Assets/Sprites/Projectiles/ElementalBurst/Explosion/elemental_burst_explosion_01..05.png` (normal playback timing, no final-frame hold extension),
  - fallback uses first explosion frame.
- Audio:
  - `Assets/Sound/Player/sfx_player_elemental_burst.wav`,
  - loaded by `AudioManager.Setup`,
  - played on detonation in `Bullet`.

### 2026-02-25 - Spawn Stall Mitigation
- Added far-enemy recycle/leash handling to reduce situations where enemy count cap blocks new spawns while old enemies remain ineffective/off-screen.

### 2026-02-25 - Fantasy Copy Refresh (UI + Character + Card Text)
- Start/pause/end and upgrade-related UI strings were rewritten to remove sci-fi leftovers and align with fantasy wording.
- Character presentation text cleanup:
  - `Mage / 法師` naming now replaces old `Ranger Core / 遊俠核心`.
  - Melee/Tank zh_TW placeholders were replaced with finalized descriptions.
- Card display copy refresh (mechanics unchanged, ids unchanged):
  - `Split Shot -> Shatter Shot`
  - `+1 Projectile -> +1 Arc Bolt`
  - `EXP Gain -> Essence Gain`
  - `Shield -> Ward Shield`
  - `Kill-Chance Lifesteal -> Blood Rite`
- Fallback-path consistency:
  - `Data/Characters/*.tres` + `GameFlowUI` fallback builders now use the same revised wording.
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).

### 2026-02-25 - README Sync (Current Mainline Snapshot)
- Updated `README.md` to reflect current mainline status:
  - fantasy presentation direction,
  - live card/progression snapshot,
  - projectile shared-script + variant-prefab convention,
  - F3 debug panel behavior summary,
  - branch note (`legacy/old-scifi-main` archive).
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).

### 2026-02-25 - Sprite Asset Folder Cleanup + Rebinding
- Reorganized projectile visuals to deterministic folders:
  - `Assets/Sprites/Projectiles/Common`
  - `Assets/Sprites/Projectiles/Wizard`
  - `Assets/Sprites/Projectiles/Split/Frames`
  - `Assets/Sprites/Projectiles/ElementalBurst/{Projectile,Explosion}`
- Removed unused legacy sprite files and stale import metadata from old root/legacy folders.
- Rebound runtime references:
  - `Scripts/Projectiles/Bullet.cs`
  - `Prefabs/WizardProjectile.tscn`
  - `Prefabs/PriestProjectile.tscn`
  - `Prefabs/SplitProjectile.tscn`
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).

### 2026-02-25 - Combat Feel Alignment (Knight + Facing)
- Melee animation timeline now aligns with ranged animation-speed model:
  - cooldown multipliers also scale melee attack animation speed.
- Knight base melee tempo adjusted:
  - `MeleeCooldown: 0.68 -> 1.36`.
- Player visual facing rule updated:
  - face mouse aim position by default,
  - fallback to movement direction only inside a tiny center deadzone.

## Canonical Specs and Docs
- Card spec: `docs/CARDS.md`
- Card change history: `docs/CARDS_CHANGELOG.md`
- Active backlog: `docs/TODO.md`

## Open Work (Current)
- Implement deterministic draw simulator (editor/console):
  - fixed seed,
  - 10,000 runs,
  - N picks per run,
  - output card rates, pair/triple frequency, ceiling-combo probability,
  - verify phase survival ratio error <= 2%.
- Run in-game smoke pass for the latest card/runtime tuning.
- Keep localization rows aligned when adding/removing cards.

## Session Update (2026-02-25, God File Risk Blueprint)
- Completed a fresh size-and-responsibility audit for current scripts.
- Added refactor blueprint:
  - `docs/GOD_FILE_REFACTOR_BLUEPRINT.md`
  - includes risk baseline, phase plan, split targets, acceptance criteria, and guardrails.
- Updated TODO execution tracking:
  - `docs/TODO.md`
  - marked audit complete and added phase-by-phase execution checklist.

## Session Update (2026-02-25, Phase 1 Complete - Bullet Decomposition)
- Refactor completed with no behavior-intended delta:
  - `Scripts/Projectiles/Bullet.cs` now hosts core lifecycle/runtime state.
  - `Scripts/Projectiles/Bullet.Collision.cs` remains hit-filter and damage bridge.
  - added `Scripts/Projectiles/Bullet.Vfx.cs` (projectile/effect frame pipeline).
  - added `Scripts/Projectiles/Bullet.SplitShot.cs` (split spawn/count/anti-chain logic).
  - added `Scripts/Projectiles/Bullet.ElementalBurst.cs` (burst detonation/AoE/VFX callback path).
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).
- TODO sync:
  - marked `Execute Phase 1 (Bullet decomposition)` as complete in `docs/TODO.md`.

## Session Update (2026-02-25, Phase 2 Complete - DebugCheatSystem Decomposition)
- Refactor completed for debug menu runtime boundary split:
  - `Scripts/Systems/Core/DebugCheatSystem.cs` keeps lifecycle + refs.
  - added `Scripts/Systems/Core/DebugCheatSystem.UI.cs`.
  - added `Scripts/Systems/Core/DebugCheatSystem.Actions.cs`.
  - added `Scripts/Systems/Core/DebugCheatSystem.Localization.cs`.
  - added `Scripts/Systems/Core/DebugCheatSystem.Pause.cs`.
- Behavior contract preserved:
  - F3 open/close still toggles menu and pause ownership.
  - existing actions still route through same runtime services (`ProgressionSystem`, `UpgradeSystem`, `SpawnSystem`, `PlayerHealth`, `MetaProgressionService`).
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).
- TODO sync:
  - marked `Execute Phase 2 (DebugCheatSystem decomposition)` as complete in `docs/TODO.md`.

## Session Update (2026-02-25, Phase 3 Complete - UpgradeSystem Policy/Apply Split)
- Refactor completed for upgrade runtime boundaries:
  - `Scripts/Systems/Progression/UpgradeSystem.cs` now keeps lifecycle/state/index utilities.
  - added `Scripts/Systems/Progression/UpgradeSystem.Apply.cs` (id -> gameplay mutations).
  - added `Scripts/Systems/Progression/UpgradeSystem.Eligibility.cs` (compatibility/gates/exclusive/prereq checks).
  - `Scripts/Systems/Progression/UpgradeSystem.Options.cs` now carries pool policy and weighting flow (phase routing, pity, category decay).
- Behavior contract preserved:
  - apply path and stack behavior unchanged,
  - option draw policy unchanged,
  - debug apply bypass behavior unchanged.
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).

## Session Update (2026-02-25, Phase 5 Complete - SpawnSystem Base Cleanup)
- Refactor completed for spawn base-file cleanup:
  - `Scripts/Systems/Director/SpawnSystem.cs` reduced to config/state fields.
  - added `Scripts/Systems/Director/SpawnSystem.Lifecycle.cs` (`_EnterTree`, `_Ready`, `_PhysicsProcess`).
  - added `Scripts/Systems/Director/SpawnSystem.SpawnFactory.cs` (wave scheduling, pending queue, instantiate path).
  - added `Scripts/Systems/Director/SpawnSystem.BoundsAndPlacement.cs` (spawn ring/pack center/spacing placement).
  - added `Scripts/Systems/Director/SpawnSystem.Debug.cs` (debug id list/spawn + anchor helpers).
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).

## Session Update (2026-02-25, Phase 4 Partial - GameFlowUI Binding Boundary)
- Performed low-risk boundary extraction in UI cluster:
  - `ResolveNodeReferences` + `BindSignals` moved out of `GameFlowUI.References.cs`.
  - added `Scripts/UI/GameFlowUI.Binding.cs`.
  - `Scripts/UI/GameFlowUI.References.cs` now focuses on path constants + cached references/state fields.
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).

## Session Update (2026-02-25, Phase 4 Complete - GameFlowUI Controller Boundary Alignment)
- Completed controller-oriented naming and file boundary alignment in UI flow:
  - renamed `Scripts/UI/GameFlowUI.State.cs` -> `Scripts/UI/GameFlowUI.UIStateController.cs`.
  - renamed `Scripts/UI/GameFlowUI.CharacterSelect.cs` -> `Scripts/UI/GameFlowUI.MetaProgressionPanelController.cs`.
  - renamed `Scripts/UI/GameFlowUI.EndState.cs` -> `Scripts/UI/GameFlowUI.EndStateController.cs`.
  - kept presenter responsibilities in rendering/localization-related partials (`Visuals`, `Localization`, `BuildSummary`) with `Binding` isolated.
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).

## Session Update (2026-02-25, Refactor Tail - SettingsPresenter + Guardrails)
- Consolidated duplicated settings-panel wiring into shared presenter helpers:
  - added `Scripts/UI/GameFlowUI.SettingsPresenter.cs`.
  - `SettingsUI` / `SettingsPersistence` now use shared sync/populate helpers for start/pause panels.
- Persistence robustness improvement:
  - `SaveSettingsToDisk` now reads from either pause-settings controls or start-settings controls before falling back to defaults.
- Upgrade draw correctness fix:
  - removed title-string hard filter in `UpgradeSystem.Options` pool build so entries that rely on localization keys are not silently excluded.
- Added guardrail doc:
  - `docs/SCRIPT_SIZE_GUARDRAILS.md` (line thresholds + split rules + audit command).
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).

## Session Update (2026-02-25, Post-Refactor Risk Audit)
- Added ranked logic risk audit document:
  - `docs/REFACTOR_LOGIC_RISK_REVIEW_2026-02-25.md`
  - includes High -> Low findings with file/line evidence and mitigation suggestions.

## Session Update (2026-02-25, Risk Hotfix Follow-up)
- Addressed high-priority risk findings directly:
  - fixed garbled fallback zh_TW strings in `Scripts/UI/GameFlowUI.cs`.
  - replaced debug CSV split with quoted-field-aware parser in `Scripts/Systems/Core/DebugCheatSystem.Localization.cs`.
  - added spawn-anchor self-heal in `Scripts/Systems/Director/SpawnSystem.Lifecycle.cs`.
  - added player-reference guard in `Scripts/Systems/Progression/UpgradeSystem.Apply.cs`.
  - added explicit warning when upgrade option pool is empty in `Scripts/Systems/Progression/UpgradeSystem.Options.cs`.
- Updated risk document to mark resolved items and keep only open findings:
  - `docs/REFACTOR_LOGIC_RISK_REVIEW_2026-02-25.md`.
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).

### 2026-02-27 - Terrain/Obstacles/UI/Card Polish Sync
- Terrain pipeline refactor and split completed:
  - ProceduralTerrainBackground split into mask/tiling/features partial files.
  - diagonal + cap edge rules iterated with in-map debug naming/overlay, then debug moved to separate tools.
  - restart now triggers terrain refresh; world generation stability improved.
- World content pipeline updates:
  - terrain assets reorganized under world sprite folders; canonical naming cleaned.
  - obstacle prefabs expanded and grouped under unified obstacle folder.
  - obstacle density/clustering tuned toward denser forests while keeping traversal lanes.
  - edge-safety spawning enforced (avoid spawning directly on grass/dirt border tiles).
- Runtime fixes and tuning:
  - swordsman cooldown rollback restored previous feel.
  - mage/ranger HP set to 4.
  - card damage stack retune applied to +50% -> +100% -> +150%.
  - zh-TW card text normalized (祕法追蹤 -> 秘法追蹤) to reduce missing-glyph risk.
- UI and visual consistency:
  - restart/result panel blue-tinted inner block replaced with warm palette.
  - result score text color aligned to parchment theme.
- Documentation sync:
  - all files in docs/*.md stamped with Last Synced: 2026-02-27.
  - cards spec/changelog updated to reflect latest tuning.

## Session Update (2026-03-04, Title/UI Input + Auto-Lock + Cursor Mode)
- Start/UI flow and menu modularization:
  - continued page-oriented split under `Scenes/UI/Panels/*` with controller-per-page wiring.
  - added independent controls page scene/controller (`ControlsPage.tscn`, `StartControlsPageController.cs`).
- Input stack additions:
  - added `InputDeviceService` (active device family detection + active gamepad id).
  - added `InputRebindService` (managed actions, runtime rebinding, config persistence).
  - added glyph mapping service/bootstrap (`InputGlyphService`, `InputMapBootstrap`).
- Settings and shared state:
  - added shared `AutoAimEnabled` setting in `GameFlowUiSharedState`.
  - controls page now exposes `Auto Lock (Gamepad Auto)` toggle with mode-aware lock behavior.
  - settings persistence now stores/loads `controls/auto_aim` and applies value to player runtime.
- Auto-lock behavior policy:
  - keyboard/mouse mode: player may toggle auto-lock (nearest target to mouse when enabled).
  - gamepad mode: auto-lock forced on; controls toggle is forced checked and non-editable.
  - right-stick directional target selection path integrated for gamepad auto-lock mode.
- Cursor presentation split:
  - introduced gameplay/UI cursor presentation modes in `CursorRing`.
  - UI screens (including pause/upgrade overlays) use custom pointer texture (`Assets/mouse pointer.png`).
  - active in-run gameplay switches back to existing aim ring/marker behavior.
- Validation:
  - `dotnet build Oriluneia.sln` succeeded (0 errors, 0 warnings).

## Session Update (2026-03-04, UI Baseline Rollback + Docs Sync)
- Direction change:
  - paused current heavy paper-stacked UI experiment for menu/meta pages.
  - restored temporary target to mainline minimal UI baseline to keep gameplay/event iteration stable.
- Documentation sync:
  - updated `docs/ARCHITECTURE.md` with `UI Baseline Rollback (2026-03-04)` contract note.
  - updated `docs/TODO.md` with rollback completion + redesign follow-up item.
- Notes:
  - this is a temporary presentation rollback only; runtime event/meta contracts remain unchanged.
