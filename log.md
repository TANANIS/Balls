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
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).

### 2026-02-25 - README Sync (Current Mainline Snapshot)
- Updated `README.md` to reflect current mainline status:
  - fantasy presentation direction,
  - live card/progression snapshot,
  - projectile shared-script + variant-prefab convention,
  - F3 debug panel behavior summary,
  - branch note (`legacy/old-scifi-main` archive).
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).

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
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).

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
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).
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
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).
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
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).

## Session Update (2026-02-25, Phase 5 Complete - SpawnSystem Base Cleanup)
- Refactor completed for spawn base-file cleanup:
  - `Scripts/Systems/Director/SpawnSystem.cs` reduced to config/state fields.
  - added `Scripts/Systems/Director/SpawnSystem.Lifecycle.cs` (`_EnterTree`, `_Ready`, `_PhysicsProcess`).
  - added `Scripts/Systems/Director/SpawnSystem.SpawnFactory.cs` (wave scheduling, pending queue, instantiate path).
  - added `Scripts/Systems/Director/SpawnSystem.BoundsAndPlacement.cs` (spawn ring/pack center/spacing placement).
  - added `Scripts/Systems/Director/SpawnSystem.Debug.cs` (debug id list/spawn + anchor helpers).
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).

## Session Update (2026-02-25, Phase 4 Partial - GameFlowUI Binding Boundary)
- Performed low-risk boundary extraction in UI cluster:
  - `ResolveNodeReferences` + `BindSignals` moved out of `GameFlowUI.References.cs`.
  - added `Scripts/UI/GameFlowUI.Binding.cs`.
  - `Scripts/UI/GameFlowUI.References.cs` now focuses on path constants + cached references/state fields.
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).

## Session Update (2026-02-25, Phase 4 Complete - GameFlowUI Controller Boundary Alignment)
- Completed controller-oriented naming and file boundary alignment in UI flow:
  - renamed `Scripts/UI/GameFlowUI.State.cs` -> `Scripts/UI/GameFlowUI.UIStateController.cs`.
  - renamed `Scripts/UI/GameFlowUI.CharacterSelect.cs` -> `Scripts/UI/GameFlowUI.MetaProgressionPanelController.cs`.
  - renamed `Scripts/UI/GameFlowUI.EndState.cs` -> `Scripts/UI/GameFlowUI.EndStateController.cs`.
  - kept presenter responsibilities in rendering/localization-related partials (`Visuals`, `Localization`, `BuildSummary`) with `Binding` isolated.
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).

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
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).

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
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).
