# Dev Log (Codex Internal)

## Session Snapshot (2026-02-20)
- Project: Godot C# top-down survival prototype.
- Run target: fixed 15-minute match.
- User pacing intent:
  - Red line = game pressure (stage-tail spikes then drop).
  - Green line = player growth (step-ups at upgrade moments).
- Core direction: survivor-like progression, not random event-heavy flow.

## Implemented In This Iteration
- Character architecture:
  - Role split completed (Ranged / Melee / TankBurst).
  - Melee nerfed (HP down, melee cooldown up, dash cooldown up, dash iframe down).
  - TankBurst buffed for anti-chase (ranged damage up + tank bullet bonus knockback/damage in CombatSystem).
- Progression:
  - Reworked to pickup-driven EXP.
  - EXP full -> queue level-up -> open upgrade menu.
  - Overflow and chained level-ups supported.
- Combat feel:
  - Enemy hit white flash + punch + small knockback.
- UI:
  - HP UI shows only during run.
  - Top EXP bar with ready state.
  - Top-right 15:00 countdown.
  - Dedicated Perfect Clear end-state (separate from failure).
  - Start menu local leaderboard for perfect clears (score/date/character).
- Director:
  - 15-minute, 4-phase pacing with phase-tail MiniBossHex schedule.
  - Universe special events disabled in current model.

## Files Worth Monitoring
- `Scripts/Systems/Director/PressureSystem.cs` (legacy removed on 2026-02-21; kept for historical context)
- `Scripts/Systems/Core/CombatSystem.cs`
- `Scripts/UI/GameFlowUI.cs`
- `Scripts/UI/GameFlowUI.State.cs`
- `Scripts/UI/GameFlowUI.Visuals.cs`
- `Scripts/UI/GameFlowUI.PerfectLeaderboard.cs`
- `Scenes/UI/Panels/StartPanel.tscn`
- `Scenes/UI/Panels/RestartPanel.tscn`
- `Scenes/UI/HudOverlay.tscn`

## Known Follow-Ups
- Balance pass:
  - Melee risk tuning pass #2.
  - Ranged role feel compensation.
  - Tank bonus tuning to avoid over-dominance.
- UX polish:
  - Leaderboard reset/clear action.
  - Small-window robustness (consider scroll container in start menu text area).
- Director:
  - Per-minute micro pacing within each phase.
  - Stage 1/2/3/4 random event-boss framework (future milestone).

## Session Update (2026-02-21)
- Director and pacing:
  - Moved legacy planning CSVs to `Data/Director/_planned/`.
  - Rebalanced stage pressure and enemy mix to bring special enemies online earlier and more often in phases 2/3/4.
- Boss and run flow:
  - MiniBossHex mobility/combat pass (less knockback abuse, faster engage behavior, dash attack support).
  - Added boss arena limiter (visible shrinking circle + player boundary while boss alive).
  - 15:00 clear flow updated: if final boss is alive at timer end, game holds at `00:00` until boss is defeated.
- Player/testing utilities:
  - Added invincibility toggle on `I` key for test runs.
  - Added XP auto-collect radius behavior for nearby pickups.
  - Enemy XP rewards now vary by enemy tier/type instead of all being level-1 value.
- Character and visuals:
  - Bulwark move speed increased by ~1/3 (`160 -> 213.3`).
  - Added per-character sprite override pipeline via `CharacterDefinition.CoreSprite`.
  - Added per-character sprite scale multiplier support.
  - Wired new art:
    - `BladeCore.png` and `BulwarkCore.png` moved to `Assets/Sprites/Player/`.
    - Melee/Tank definitions updated to use new textures.
  - Blade visual scaling fixed to keep original aspect ratio while matching intended effective short-edge target.
- UI/leaderboard:
  - Added start-menu leaderboard clear feature with confirmation dialog.
  - Local perfect-clear leaderboard can now be explicitly reset from title UI.
- Docs and handoff readiness:
  - Unified markdown docs to current canonical progression flow:
    - `ExperiencePickup -> ProgressionSystem EXP -> queued level-up -> UpgradeMenu`.
    - Clarified pressure is pacing signal; EXP is default leveling trigger.
  - Added "Skill Layer - Next Focus" checklist in `docs/TODO.md` for upcoming implementation phase.
- Pressure retirement pass:
  - Added `ProgressionSystem` and moved XP progress/level-up queue ownership out of `PressureSystem`.
  - Updated `ExperiencePickup`, `GameFlowUI`, and `UpgradeSystem` to consume `ProgressionSystem` directly.
  - Refactored `SpawnSystem` tier selection to use stability phase mapping only.
  - Removed `PressureSystem` scripts and node from `SystemsRoot`.

## Session Update (2026-02-21, Skill VFX Standardization)
- Standardized skill effect sprite path contract:
  - `Assets/Sprites/Skills/<SkillName>/`
- Added docs for current runtime practice and fallback binding:
  - `docs/CARDS.md` (card + skill VFX authoring note)
  - `docs/ARCHITECTURE.md` (system-level asset contract)
  - `Assets/Sprites/Skills/README.md` (asset folder rules)
- Current live reference remains shield cooldown skill:
  - runtime: `Scripts/Player/PlayerHealth.cs`
  - fallback asset: `res://Assets/Sprites/Skills/Shield/shield.png`
  - scene anchor: `Scenes/Player.tscn` -> `SkillVfxRoot`
  - accessor standard: `Player.GetSkillVfxRoot()` for future player-side skill VFX

## Session Update (2026-02-21, UI + Character Select + Shield Visibility)
- Start menu UI layout pass:
  - Redesigned to two-column composition.
  - Left side focuses on game text/leaderboard.
  - Right-side action rail hosts primary buttons for cleaner navigation.
- Main menu secondary cards panel:
  - Added "Cards" button and secondary panel for in-run card compendium preview.
  - Card list is populated dynamically from active upgrade catalog.
- Character select presentation upgrade:
  - Added localized character copy (`en` / `zh_TW`) in character definitions.
  - Character panel now shows structured gameplay summary:
    - attack profile (single/melee/burst),
    - mobility (dash/base),
    - survival baseline (HP/regen).
- Damage feedback adjustment:
  - Removed camera zoom-in on player hit.
  - Kept player sprite white-flash feedback only.
- Shield visibility reliability pass:
  - Introduced dedicated `SkillVfxRoot` under player scene.
  - Shield visuals now resolve to `Player.GetSkillVfxRoot()` first.
  - Added fallback ring + scaling/transparency tuning to ensure readable visibility.
  - Reduced default shield size and opacity to lower screen obstruction.

## Git Sync Note (2026-02-21)
- Synced latest gameplay/UI polish and VFX-node architecture updates.
- Commit includes:
  - Start menu structure + localization path updates.
  - Character select bilingual presentation refresh.
  - Player skill VFX root architecture and shield behavior/display fixes.
  - Cards/architecture documentation updates for new conventions.

## Session Update (2026-02-21, Encoding Recovery)
- Fixed Traditional Chinese text corruption in character resources:
  - `Data/Characters/RangedCharacter.tres`
  - `Data/Characters/MeleeCharacter.tres`
  - `Data/Characters/TankBurstCharacter.tres`
- Fixed Chinese labels in character presentation UI builder:
  - `Scripts/UI/GameFlowUI.State.cs`
- Added persistent encoding rule to architecture docs:
  - `docs/ARCHITECTURE.md` -> "Text Encoding Rule (Bilingual UI)"
  - Rule: localization source files should be stored in UTF-8 to prevent repeated mojibake.

## Session Update (2026-02-21, Runtime Balance + Stability UX)
- Spawn system data wiring:
  - `EnemyDefinitions.csv` now applies `hp/speed/contact_damage` at spawn time (not scene-only defaults).
  - Spawn queue now carries enemy definition metadata to support per-entry runtime stat override.
- Opening pacing adjustment:
  - Reduced opening dead-air and long travel delay by tightening spawn margin/ring and softening opening ramp values.
- Character baseline rebalance:
  - Blade Core: `MeleeDamage -> 4`, `MeleeCooldown -> 0.68`, `MaxHp -> 2`.
  - Bulwark Core: primary fire pattern changed from 3-round burst to 2-round burst.
  - Ranger Core: `RangedDamage -> 2`, `RangedCooldown -> 0.64`.
  - Added `PrimaryFirePattern.Burst2` support in weapon runtime and character-role presentation labels.
- Localization + reliability:
  - Fixed character-select display corruption and zh-TW text fields in character resources.
  - Added defensive fallback loading path for character definitions in `GameFlowUI` so UI no longer collapses when resource load fails.
- Card balance:
  - `SURV_LIFESTEAL_CLOSE_KILL` adjusted from `25%` to `12%` chance to heal `1 HP` on kill.
  - Synced runtime value + card catalog text + localization text + card docs.
- Stability camera UX:
  - Phase zoom-in changed from persistent phase-wide zoom to temporary phase-entry warning zoom with timed smooth recovery to normal.
  - New tunables added in `StabilitySystem` for hold/recover duration per phase.

## Session Update (2026-02-21, High-Frequency Encoding Incident Note)
- Documented recurring Godot resource parse failure pattern:
  - Error signature: `Parse Error: Expected '['` at line 1 for `.tres` files.
  - Confirmed root cause: UTF-8 BOM (`EF BB BF`) at file head.
  - Fix: convert affected `.tres` files to UTF-8 without BOM.
- Added this as an explicit rule and troubleshooting note in `docs/ARCHITECTURE.md`.

## Session Update (2026-02-21, Structural Refactor + Docs Sync)
- Orphan cleanup:
- GameFlowUI refactor pass:
  - Added `GameFlowUI.References.cs`, `GameFlowUI.CharacterSelect.cs`, `GameFlowUI.EndState.cs`,
    `GameFlowUI.SettingsUI.cs`, `GameFlowUI.SettingsPersistence.cs`.
  - Slimmed `GameFlowUI.cs`, `GameFlowUI.State.cs`, `GameFlowUI.PauseSettings.cs`.
- PlayerHealth refactor pass:
  - Split into `PlayerHealth.Core.cs`, `PlayerHealth.Shield.cs`, `PlayerHealth.Vfx.cs`.
  - `PlayerHealth.cs` now holds exported config, shared state, and public status properties.
- SpawnSystem refactor pass:
  - Added `SpawnSystem.Pacing.cs` and `SpawnSystem.MiniBossSchedule.cs`.
  - Slimmed `SpawnSystem.Runtime.cs` to runtime orchestration concerns.
- Documentation sync:
  - Updated `docs/SCRIPT_REFACTOR_PLAN.md`.
  - Updated `docs/CODE_STRUCTURE_AUDIT_2026-02-21.md`.
  - Updated `docs/TODO.md` completed status for EXP value differentiation.
  - Refreshed `README.md` to current architecture (removed legacy `PressureSystem` wording and encoding artifacts).
  - Refreshed `docs/SCENE_SPLIT_NOTES.md` to current scene composition.

## Session Update (2026-02-22, Repo Hygiene + Naming Migration)
- README and repo docs:
  - Reworked `README.md` into a maintenance-oriented index:
    - architecture summary,
    - runtime flow,
    - folder purpose map,
    - docs usage/location index (including `log.md`),
    - git/export artifact guardrails.
- Git history cleanup for accidental export uploads:
  - Purged exported artifacts from commit history and remote:
    - `Project Genesis.pck`
    - `Project Genesis.exe`
    - `Project Genesis.console.exe`
    - `data_20260120_windows_x86_64/*`
    - `data_20260120_windows_x86_64/Project Genesis.console.console/*`
  - Added/expanded ignore rules in `.gitignore`:
    - `*.exe`, `*.pck`
    - `data_*_windows_*/`, `data_*_linuxbsd_*/`, `data_*_macos_*/`
    - `Project Genesis.console*`
    - `**/Project Genesis.console.console/`
  - Completed force-push sync after history rewrite.
- Project naming migration (`20260120` -> `ProjectGenesis`):
  - Renamed:
    - `20260120.csproj` -> `ProjectGenesis.csproj`
    - `20260120.sln` -> `ProjectGenesis.sln`
  - Updated references:
    - `ProjectGenesis.sln` project display name/path
    - `project.godot`:
      - `config/name="ProjectGenesis"`
      - `project/assembly_name="ProjectGenesis"`
    - `ProjectGenesis.csproj` root namespace -> `ProjectGenesis`
    - `docs/CODE_STRUCTURE_AUDIT_2026-02-21.md` build command reference
  - Validation:
    - `dotnet build ProjectGenesis.csproj` succeeded (0 errors; only NU1900 network-source warnings).

## Session Update (2026-02-23, Duplication Refactor Pass #1)
- Cross-module service lookup consolidation:
  - Added shared utility: `Scripts/Shared/GroupServiceResolver.cs`.
  - Standardized `StabilitySystem` resolution via `GroupServiceResolver.ResolveFirstInGroup(...)` in:
    - `Scripts/Player/PlayerWeapon.cs`
    - `Scripts/Player/PlayerMelee.cs`
    - `Scripts/Player/PlayerDash.cs`
    - `Scripts/Player/PlayerMovement.cs`
    - `Scripts/Player/Player.Composition.cs`
    - `Scripts/Enemy/Enemy.Resolve.cs`
    - `Scripts/Systems/Director/SpawnSystem.Runtime.cs`
    - `Scripts/World/ObstacleFieldGenerator.cs`
- GameFlowUI duplication reduction:
  - Added centralized panel state helpers in `Scripts/UI/GameFlowUI.State.cs`:
    - `SetStartSubPanels(...)`
    - `SetPausePanels(...)`
  - Replaced repeated start/pause/end panel `Visible` toggles with helper calls in:
    - `Scripts/UI/GameFlowUI.State.cs`
    - `Scripts/UI/GameFlowUI.PauseSettings.cs`
    - `Scripts/UI/GameFlowUI.CharacterSelect.cs`
    - `Scripts/UI/GameFlowUI.EndState.cs`
    - `Scripts/UI/GameFlowUI.References.cs`
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - Warning note: NU1900 source-access warning remained due to `https://api.nuget.org/v3/index.json` connectivity, not logic regression.

## Session Update (2026-02-23, Duplication Refactor Pass #2 - Player Ability Base)
- Player ability module consolidation:
  - Added shared base class: `Scripts/Player/PlayerAbilityModule.cs`.
  - Centralized common behavior for player ability modules:
    - player/stability references
    - enabled-state setup and toggling
    - stability resolve/refresh
    - cooldown ticking helper
    - power multiplier helper
    - input-action fallback resolver with standardized warning/error flow
- Applied inheritance migration:
  - `Scripts/Player/PlayerDash.cs`
  - `Scripts/Player/PlayerMelee.cs`
  - `Scripts/Player/PlayerWeapon.cs`
- Removed duplicated local implementations in above modules:
  - repeated `ResolveStabilitySystem()`
  - repeated setup boilerplate (`_player`, `_isEnabled`, stability resolve)
  - repeated cooldown decrement pattern
  - duplicated melee/weapon input fallback resolution branches
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - Warning note: NU1900 source-access warning remained due to `https://api.nuget.org/v3/index.json` connectivity, not logic regression.

## Session Update (2026-02-23, Docs Hygiene + Risk Surfacing)
- Corrected stale path in `README.md`:
  - `Scripts/Systems/UpgradeSystem.cs` -> `Scripts/Systems/Progression/UpgradeSystem.cs`.
- Added explicit status labeling to historical/snapshot docs:
  - `docs/CODE_STRUCTURE_AUDIT_2026-02-21.md` marked as archived point-in-time audit.
  - `docs/SCRIPT_REFACTOR_PLAN.md` marked as archived snapshot.
  - `docs/SCENE_SPLIT_NOTES.md` clarified as maintained guidance with re-validation trigger.
- Added architecture risk checklist in `docs/ARCHITECTURE.md`:
  - group-name string discovery fragility,
  - UI multi-boolean state drift risk,
  - partial-class cross-file coupling risk,
  - data schema/version validation risk,
  - low automated gameplay regression coverage risk.
- Log cleanup:
  - Marked `PressureSystem` monitor entry as legacy removed (2026-02-21) to avoid confusion.

## Session Update (2026-02-23, Meta Progression Architecture Spec)
- Added new architecture spec:
  - `docs/META_PROGRESSION_ARCHITECTURE.md`
- Spec scope:
  - full out-of-run progression design (not MVP-only),
  - lean module layout with clear responsibilities,
  - soft-cap reward curve (`score/100` baseline + smooth diminishing returns + bonus + first-clear bonus),
  - unlock targets A/B/C:
    - new characters,
    - character levels,
    - character-specific ability tree.
- Integration points documented:
  - settlement trigger in end-state flow,
  - score source from `ScoreSystem`,
  - unlock gating in character-select and `RunContext`.
- Documentation index synced:
  - `README.md` now includes `docs/META_PROGRESSION_ARCHITECTURE.md`.
- Planning checklist synced:
  - `docs/TODO.md` now includes a dedicated "Meta Progression - Out-Of-Run" section.

## Session Update (2026-02-23, Meta Progression Phase Plan)
- Added phase-based implementation plan:
  - `docs/META_PROGRESSION_IMPLEMENTATION_PLAN.md`
- Plan structure:
  - Phase 1: domain + persistence foundation
  - Phase 2: soft-cap economy + settlement
  - Phase 3: transaction service + character unlock gate
  - Phase 4: character level + character-specific ability tree
- Documentation index synced:
  - `README.md` now includes `docs/META_PROGRESSION_IMPLEMENTATION_PLAN.md`.
- Task tracker synced:
  - `docs/TODO.md` updated to phase checklist format for Meta progression delivery.

## Session Update (2026-02-23, Meta Progression Phase 1 Foundation)
- Implemented Phase 1 domain + persistence foundation.
- Added domain model files:
  - `Scripts/Meta/MetaProgressionState.cs`
  - `Scripts/Meta/CharacterProgress.cs`
  - `Scripts/Meta/MetaFlags.cs`
- Added save pipeline files:
  - `Scripts/Save/MetaSaveDto.cs`
  - `Scripts/Save/SaveMigrator.cs`
  - `Scripts/Save/JsonSaveStore.cs`
- Key behaviors:
  - runtime state and DTO are separated,
  - save versioning + migration hook established (`Version`, `CurrentVersion`),
  - default-safe load path on missing/corrupt save,
  - currency/unlock/character-progress/settled-run-id containers are persistable.
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - NU1900 warning remains external (NuGet index connectivity), not logic regression.
- Planning sync:
  - `docs/TODO.md` marks "Phase 1: Domain + persistence foundation" as completed.

## Session Update (2026-02-23, Meta Progression Phase 2 Economy)
- Implemented Phase 2 economy and settlement calculation building blocks.
- Added files:
  - `Scripts/Meta/RunResult.cs`
  - `Scripts/Meta/RewardBreakdown.cs`
  - `Scripts/Meta/EconomyTuning.cs`
  - `Scripts/Meta/PowerCurve.cs`
  - `Scripts/Meta/RewardCalculator.cs`
- Reward model implemented:
  - baseline `base = floor(score / scoreDivisor)`,
  - smooth soft-cap curve with optional linear tail,
  - bonus composition:
    - flat bonus,
    - perfect clear bonus,
    - first-clear bonuses (character/global),
  - breakdown output with duplicate-run detection flag.
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - NU1900 warning remains external (NuGet source connectivity).
- Planning sync:
  - `docs/TODO.md` marks "Phase 2: Economy + settlement (soft cap curve)" as completed.

## Session Update (2026-02-23, Meta Progression Phase 3 Service + Gate)
- Implemented transaction entry service:
  - `Scripts/Meta/MetaProgressionService.cs`
  - singleton access + baseline unlock (`ranged`) bootstrap
  - settlement transaction with duplicate-run protection
  - character unlock / character level / ability-node transaction stubs with spending persistence
- Integrated run settlement trigger:
  - `Scripts/UI/GameFlowUI.EndState.cs`
  - end-state now submits `RunResult` (runId, score, characterId, perfect flag) and logs reward breakdown.
- Integrated run id lifecycle:
  - `Scripts/UI/GameFlowUI.State.cs` sets a new per-run id on `StartRun()`.
  - `Scripts/UI/GameFlowUI.References.cs` stores `_currentRunId`.
- Integrated unlock gate:
  - `Scripts/UI/GameFlowUI.CharacterSelect.cs`
    - locked characters are shown as `[Locked]`,
    - locked selection and confirm are blocked,
    - confirm button disabled when selection is locked.
  - `Scripts/Runtime/RunContext.cs`
    - guards selected character assignment through unlock validation.
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - NU1900 warning remains external (NuGet source connectivity).
- Planning sync:
  - `docs/TODO.md` marks "Phase 3: Transaction service + character unlock gate" as completed.

## Session Update (2026-02-23, Meta Progression Phase 4 Framework)
- Implemented Phase 4 framework (without gameplay effect binding).
- Added progression definition layer:
  - `Scripts/Defs/ProgressionDefs.cs`
  - `Scripts/Defs/Models/CharacterDef.cs`
  - `Scripts/Defs/Models/AbilityNodeDef.cs`
- Added level/tree model support:
  - `Scripts/Meta/MetaProgressionState.cs` adds `TryGetCharacterProgress(...)` for side-effect free queries.
- Upgraded service transaction logic to def-driven rules:
  - `Scripts/Meta/MetaProgressionService.cs`
  - character unlock cost now resolved from defs (`TryUnlockCharacter(string)`),
  - character level-up uses per-character growth cost model (`Can/TryUpgradeCharacterLevel`),
  - ability tree unlock checks:
    - node existence,
    - unlock state,
    - min character level,
    - prerequisite node chain,
    - currency affordability.
- Intentional placeholder scope:
  - character ability trees are structurally ready, but node lists are currently empty placeholders pending design content.
  - no runtime stat/effect hooks are applied yet for ability-node unlocks.
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - NU1900 warning remains external (NuGet source connectivity).
- Planning sync:
  - `docs/TODO.md` marks Phase 4 framework complete and leaves content/effect binding as next task.

## Session Update (2026-02-23, Flux UX + Meta Panel Polish)
- Naming and UX detail update:
  - Out-of-run currency is now presented as `Flux` in run-end and meta-selection UI.
- End-state reward visibility:
  - `Scripts/UI/GameFlowUI.EndState.cs`
  - both failure and perfect-clear now display:
    - run-earned Flux (`+X`),
    - current Flux wallet total.
- Character select upgraded toward meta-progression panel:
  - `Scripts/UI/GameFlowUI.CharacterSelect.cs`
  - panel now presents:
    - current Flux,
    - selected character level,
    - character lock status and unlock cost,
    - ability-tree framework status (node content pending design).
  - locked characters remain blocked and labeled as locked.
- Start panel visual/style pass:
  - `Scenes/UI/Panels/StartPanel.tscn`
  - `CharacterSelectPanel` title changed to `META PROGRESSION`.
  - `RightColumnPanel` now uses same-color flat background with no edge glow/border.
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - NU1900 warning remains external (NuGet source connectivity).

## Session Update (2026-02-23, Flux Feature Localization Sync)
- Synced newly added Flux/meta-progression UI to bilingual flow (`en` / `zh_TW`).
- Updated dynamic text to use localization fallback pattern (`TrOrDefault`) in:
  - `Scripts/UI/GameFlowUI.CharacterSelect.cs`
  - `Scripts/UI/GameFlowUI.EndState.cs`
  - including lock labels, Flux labels, unlock cost, character level, and ability-tree framework text.
- Updated static title localization hookup:
  - `Scripts/UI/GameFlowUI.Localization.cs`
  - `CharacterSelectPanel` title now resolved through localization key `UI.META.TITLE` with bilingual fallback.
- Added localization keys for meta-progression section:
  - `Data/Localization/UI.csv`
  - keys include `UI.META.*` (title, Flux labels, lock states, level labels, ability-tree status, etc.).
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - NU1900 warning remains external (NuGet source connectivity).

## Session Update (2026-02-23, Chinese Mojibake Root-Cause Fix)
- Root cause identified:
  - newly added `UI.META.*` zh entries in `Data/Localization/UI.csv` were written with wrong encoding path and became mojibake.
  - several zh fallback literals in `GameFlowUI` scripts were also mojibake.
  - because `Tr(...)` returned corrupted translated strings, UI displayed unreadable Chinese.
- Fix applied:
  - removed corrupted `UI.META.*` lines from `Data/Localization/UI.csv` to force clean fallback path.
  - rewrote `Scripts/UI/GameFlowUI.CharacterSelect.cs` with clean UTF-8 content and Unicode-escape zh fallback literals.
  - fixed `Scripts/UI/GameFlowUI.Localization.cs` character-select title localization line and switched to Unicode-escape zh fallback.
  - preserved bilingual behavior through `TrOrDefault(key, en, zh)` without relying on corrupted entries.
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - NU1900 warning remains external (NuGet source connectivity).

## Session Update (2026-02-23, Save Isolation + Delete Save Feature)
  - `Scripts/Save/JsonSaveStore.cs`
  - save path now uses profile partition:
    - `user://saves/<profile>/meta_progression.json`
  - added profile API:
    - `SetProfile(profileId)`
    - `ProfileId`
    - `SavePath`
  - added delete API:
    - `DeleteSaveFile()`
- Meta progression service controls:
  - `Scripts/Meta/MetaProgressionService.cs`
  - added:
    - `CurrentProfileId`
    - `CurrentSavePath`
    - `SetProfile(profileId)` (reloads state with baseline unlock checks)
    - `DeleteCurrentProfileSave()` (deletes file + resets in-memory state)
- UI delete-save entry (with confirmation):
  - `Scenes/UI/Panels/StartPanel.tscn`
    - added `DeleteSaveButton`
    - added `DeleteSaveConfirmDialog`
  - `Scripts/UI/GameFlowUI.References.cs`
    - node paths, node refs, signal binding for delete-save flow
  - `Scripts/UI/GameFlowUI.State.cs`
    - `OnStartDeleteSavePressed()`
    - `OnStartDeleteSaveConfirmed()`
    - after deletion: refresh selection and meta panel data
  - `Scripts/UI/GameFlowUI.Localization.cs`
    - bilingual fallback text for delete-save button/dialog
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - NU1900 warning remains external (NuGet source connectivity).

## Session Update (2026-02-23, Start Menu Action Relocation)
- UX structure adjustment:
  - moved both actions from start-menu first layer into Start Settings panel:
    - `Clear Leaderboard Record`
    - `Delete Save Data`
- Scene updates:
  - `Scenes/UI/Panels/StartPanel.tscn`
  - removed the two buttons from `RightColumnPanel/Margin/ButtonsVBox`
  - re-added them under `Panel/SettingsPanel/VBox` (before settings back button)
- Script path binding updates:
  - `Scripts/UI/GameFlowUI.References.cs`
  - updated node paths:
    - `StartClearLeaderboardButtonPath`
    - `StartDeleteSaveButtonPath`
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors).
  - NU1900 warning remains external (NuGet source connectivity).

## Session Update (2026-02-23, Meta Progression UI Layout Pass)
- Reworked start-menu meta progression panel layout to match requested reference composition:
  - top header row (`META PROGRESSION` + right-side Flux value),
  - middle split area (left character card grid + right ability-tree visual panel),
  - lower detail panel with character/meta description,
  - bottom dual action buttons (`Back`, `Start Run`).
- Scene changes:
  - `Scenes/UI/Panels/StartPanel.tscn`
  - resized `CharacterSelectPanel` to use near-full menu canvas height and rebuilt child layout containers.
- Script wiring updates:
  - `Scripts/UI/GameFlowUI.References.cs`
    - updated character-select node paths after layout refactor,
    - added `FluxValue` node reference.
  - `Scripts/UI/GameFlowUI.CharacterSelect.cs`
    - `RefreshCharacterSelectUi()` now refreshes top-right Flux wallet text.
  - `Scripts/UI/GameFlowUI.Localization.cs`
  - updated meta title path after header-row move,
  - added localized label update for `Flux:`.

## Session Update (2026-02-23, Character Unlock Cost + Meta Unlock Flow)
- Character unlock pricing update:
  - `Scripts/Defs/ProgressionDefs.cs`
  - unified all character unlock costs to `70 Flux` (`ranged`, `melee`, `tank_burst`).
- Meta panel unlock interaction:
  - `Scripts/UI/GameFlowUI.CharacterSelect.cs`
  - character cards are now selectable even when locked (kept lock label in button text),
  - confirm button behavior is now state-driven:
    - unlocked character => `Start Run`,
    - locked character => `Unlock (70 Flux)` (dynamic by def cost),
  - pressing confirm on a locked character now attempts `MetaProgressionService.TryUnlockCharacter(...)`,
  - on successful unlock, panel refreshes in-place with updated Flux and button state.
- Localization interaction fix:
  - `Scripts/UI/GameFlowUI.Localization.cs`
  - removed fixed overwrite of character-confirm button text,
  - refreshes character-select UI at end of localization pass so dynamic confirm text stays correct per state.

## Session Update (2026-02-23, Meta UI Overflow + Expectation Guard)
- Fixed layout overflow risk in meta progression panel:
  - `Scenes/UI/Panels/StartPanel.tscn`
  - reduced heavy vertical min-heights (`ContentRow`, `BottomRow`),
  - reduced character card font sizes and enabled `clip_text` to prevent long `[Locked]` labels overflowing,
  - moved detail description into `ScrollContainer` to prevent bottom button overlap/cutoff on dense text.
- Updated node path after scroll-container insertion:
  - `Scripts/UI/GameFlowUI.References.cs`
  - `StartCharacterDescriptionPath` now points to `.../DescScroll/SelectedCharacterDesc`.
- Marked not-yet-implemented features as explicitly unavailable:
  - `Scenes/UI/Panels/StartPanel.tscn`
    - placeholder 4th character card text changed to `Coming Soon`.
    - ability-tree graphic placeholder text changed to `Coming Soon`.
  - `Scripts/UI/GameFlowUI.CharacterSelect.cs`
    - ability-tree fallback text now uses `UI.META.NOT_AVAILABLE` fallback (`Coming Soon` / `撠?`) instead of "framework pending design" wording.
  - `Scripts/UI/GameFlowUI.Localization.cs`
    - localized both "Coming Soon" placeholders for character slot and ability-tree area.

## Session Update (2026-02-23, Description Visibility Fix + End-State UI Polish)
- Fixed meta character description disappearing:
  - `Scenes/UI/Panels/StartPanel.tscn`
  - increased `SelectedCharacterDesc` minimum width inside `DescScroll` to prevent zero-width collapse in `ScrollContainer`.
- End-state (run settlement) UI layout pass:
  - `Scenes/UI/Panels/RestartPanel.tscn` rebuilt into structured sections:
    - header (`Title`, `PerfectBanner`, reason/hint),
    - stats row (`Survival`, `Score`, `Flux gain`, `Flux wallet`),
    - scrollable build-summary section,
    - clear bottom action button.
- End-state data binding updates:
  - `Scripts/UI/GameFlowUI.References.cs`
    - updated restart-panel node paths after layout rebuild,
    - added references for new labels:
      - `_finalSurvivalLabel`
      - `_finalFluxGainLabel`
      - `_finalFluxWalletLabel`
  - `Scripts/UI/GameFlowUI.EndState.cs`
    - rewired output to structured labels instead of single multiline score block,
    - restart hint now displays actual run-end reason,
    - fixed Flux wallet zh fallback string to Unicode-safe literal.

## Session Update (2026-02-23, Player Hit Movement Freeze)
- Added short movement-freeze window when player takes real HP damage to reduce contact-slide artifacts.
- Implementation:
  - `Scripts/Player/PlayerHealth.cs`
    - new tunable: `DamageMoveFreezeSeconds` (default `0.06`).
  - `Scripts/Player/PlayerHealth.Core.cs`
    - on successful damage (after shield check), calls player movement freeze trigger.
  - `Scripts/Player/Player.State.cs`
    - added `ApplyHitMovementFreeze(float duration)` bridge method.
  - `Scripts/Player/PlayerMovement.cs`
    - added internal freeze timer and `ApplyMovementFreeze(float duration)`.
    - while freeze is active: velocity forced to zero for the short window.
- Behavior note:
  - shield-absorbed hits do not trigger movement freeze (only actual HP loss does).

  - `Scripts/UI/GameFlowUI.cs`
    - `EditorOverrideFluxWalletOnReady` (bool)
    - `EditorFluxWallet` (int)
- Added service-side setter for wallet override:
  - `Scripts/Meta/MetaProgressionService.cs`
    - clamps to non-negative,
    - adjusts via add/spend path,
    - persists save immediately.
  - `Scenes/UI/Panels/StartPanel.tscn`

## Session Update (2026-02-23, Settings Layout Stabilization + End-State Actions)
- Settings layout stabilization (menu + in-run):
  - `Scenes/UI/Panels/StartPanel.tscn`
    - Start Settings panel expanded to full content area height.
    - wrapped settings content with `SettingsScroll` to avoid text/control overflow in bilingual strings.
  - `Scenes/UI/Panels/PausePanel.tscn`
    - Pause Settings width/height aligned with pause panel content area.
    - wrapped settings content with `SettingsScroll` for consistent spacing and no clipping.
- Restart/end-state UX action update:
  - `Scenes/UI/Panels/RestartPanel.tscn`
    - bottom action changed to two buttons:
      - `BackToMetaButton`
      - `RestartButton`
- UI path + signal/localization sync:
  - `Scripts/UI/GameFlowUI.References.cs`
    - updated start/pause settings node paths to `.../SettingsScroll/VBox/...`.
    - added restart back-to-meta button reference/binding.
  - `Scripts/UI/GameFlowUI.State.cs`
    - added `OnRestartBackToMetaPressed()` -> returns to out-of-run menu/meta view.
  - `Scripts/UI/GameFlowUI.Localization.cs`
    - updated settings label path lookups to new scroll-based hierarchy.
    - added localized fallback text for end-state back-to-meta button.

## Session Update (2026-02-23, Leaderboard Merged Into Meta Save)
- Integrated perfect-clear leaderboard into meta save domain/persistence:
  - added model: `Scripts/Meta/PerfectClearRecord.cs`
  - `Scripts/Meta/MetaProgressionState.cs`
    - added `PerfectClearRecords` collection and sorted/trimmed record helpers.
  - `Scripts/Save/MetaSaveDto.cs`
    - schema version bumped to `2`,
    - added `PerfectClearRecords` payload.
  - `Scripts/Save/SaveMigrator.cs`
    - migration/default normalization for `PerfectClearRecords`,
    - DTO <-> domain conversion wired.
  - `Scripts/Meta/MetaProgressionService.cs`
    - added `RecordPerfectClear(...)` and `GetPerfectLeaderboard(...)`.
- UI leaderboard source switched from separate local cfg file to meta service:
  - replaced implementation: `Scripts/UI/GameFlowUI.PerfectLeaderboard.cs`
  - removed standalone file persistence (`user://perfect_1500_leaderboard.cfg`) path logic.
- Removed clear-leaderboard UI/action (now merged with save lifecycle):
  - `Scenes/UI/Panels/StartPanel.tscn`
    - removed `ClearLeaderboardButton` and `ClearLeaderboardConfirmDialog`.
  - `Scripts/UI/GameFlowUI.References.cs`
    - removed clear-leaderboard node refs and signal bindings.
  - `Scripts/UI/GameFlowUI.State.cs`
    - removed clear-leaderboard handlers.
  - `Scripts/UI/GameFlowUI.Localization.cs`
    - removed clear-leaderboard button/dialog localization wiring.
- Logic consequence:
  - deleting profile save now clears leaderboard implicitly because leaderboard and meta progression now share the same save file.

## Session Update (2026-02-23, TODO Sync - God File Risk Plan)
- Updated `docs/TODO.md` with dedicated "Refactor - God File Risk" backlog.
- Added actionable decomposition items focused on preventing future script blow-up:
  - `GameFlowUI` boundary split (`UIStateController` / `UIBinding` / `UIPresenter`),
  - extraction targets for `MetaProgressionPanelController` and `EndStateController`,
  - start/pause settings logic consolidation into shared `SettingsPresenter`,
  - lightweight script-size/split guardrail documentation tasks.

## Session Update (2026-02-23, Fantasy Pixel Art Branch Kickoff + World Pass)
- Branching and planning:
  - created art-experiment branch: `feature/art-fantasy-pixel-style`.
  - added art execution plan: `docs/ART_DIFFERENTIATION_FANTASY_PIXEL_PLAN.md`.
  - added style/spec baseline: `docs/FANTASY_PIXEL_STYLE_SPEC.md`.
  - synced `README.md` documentation index to include both new docs.
- Clarified sizing intent after requirement correction:
  - fixed misunderstanding where `480x270` had been applied to viewport.
  - restored runtime viewport in `project.godot` to `1280x720`.
  - kept `480x270` and `32x32` as asset-canvas standards in spec docs.
- Obstacle asset pipeline migration:
  - migrated new obstacle assets to standardized English naming:
    - `Assets/Sprites/Environment/obstacle_big_rock.png`
    - `Assets/Sprites/Environment/obstacle_small_tree.png`
  - removed legacy obstacle runtime assets/references (`ObstacleRock*`, `ForestObstacle_*` usage).
  - added prefab-based obstacle nodes with explicit colliders:
    - `Prefabs/Obstacles/ObstacleBigRock.tscn`
    - `Prefabs/Obstacles/ObstacleSmallTree.tscn`
- Obstacle generation refactor:
  - rewrote `Scripts/World/ObstacleFieldGenerator.cs` from texture-only runtime generation to scene-prefab spawning.
  - removed runtime random rotation/scale behavior.
  - added higher-density configuration and alternating-type control.
  - later upgraded to natural-cluster placement:
    - weighted type selection (tree-biased),
    - cluster center + stickiness + lifetime controls,
    - spacing bias per obstacle type for less mechanical patterns.
- In-run world background architecture update:
  - added new in-run tile asset:
    - `Assets/Sprites/Environment/bg_run_forest_tile.png` (from new game background source).
  - introduced infinite tiled background system:
    - `Scripts/World/InfiniteTiledBackground.cs`
    - supports seamless infinite tiling + deterministic per-tile horizontal flip.
  - removed old temporary starfield/mask system:
    - deleted `Scripts/World/SpaceBackdrop.cs` (+ `.uid`),
    - removed old starfield/dimmer nodes from `Scenes/World/WorldRoot.tscn`.
- Menu background reliability fixes:
  - reworked `GameFlowUI` menu background fitting in `Scripts/UI/GameFlowUI.Visuals.cs`:
    - adaptive cover scaling,
    - center alignment stabilization,
    - overscan guard against edge exposure on different zoom/window conditions.
- Run/start visual flow sync:
  - updated `Scripts/UI/GameFlowUI.State.cs` so run mode shows in-run background while menu mode keeps menu background.
- Camera + readability tuning pass for transition to future `32x32` player art:
  - adjusted `Scenes/Player.tscn` camera zoom baseline.
  - increased in-run tile scale and margins in `Scenes/World/WorldRoot.tscn`.
- Validation:
  - repeated `dotnet build ProjectGenesis.sln` checks passed (0 errors).
  - only external NU1900 warnings remained (NuGet index connectivity).

## Session Update (2026-02-24, SwarmCircle Rename + Slime Pipeline Cleanup)
- Renamed runtime base enemy scene:
  - `Enemies/SwarmCircle.tscn` -> `Enemies/Slime.tscn`
- Director/runtime id migration:
  - `swarm_circle` -> `slime` in:
    - `Data/Director/EnemyDefinitions.csv`
    - `Data/Director/TierEnemyWeights.csv`
    - `Data/Director/_planned/PackTemplates.csv`
    - `Scripts/Systems/Director/SpawnSystem.Selection.cs`
- Audio routing update:
  - death SFX scene-key moved to `res://Enemies/Slime.tscn` in `Scripts/Audio/AudioManager.Setup.cs`.
- Slime art asset normalization (naming/location):
  - renamed to lowercase snake-case under:
    - `Assets/Sprites/Enemies/Slime/`
  - organized shadow assets under:
    - `Assets/Sprites/Enemies/Slime/shadow/`
  - removed stale legacy `.import` entries and obsolete shadow folder artifacts.
- Slime scene node cleanup:
  - visual node renamed to `AnimatedSprite2D`.
  - event module moved under explicit `Events` node for resolver consistency.
  - sprite-frame references updated to normalized slime asset paths.
- Docs sync:
  - updated references from `SwarmCircle` to `Slime` in active art/spec docs.
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, NU1900 external warnings only).

## Session Update (2026-02-24, Major Fantasy Pixel Runtime Migration + Player/Projectile Refactor)
- Enemy runtime migration (old geometry ids/scenes retired):
  - `SwarmCircle -> Slime`
  - `ChargerTriangle -> Orc`
  - `TankSquare -> EliteOrc`
  - `EliteSwarmCircle -> Werebear`
  - `MiniBossHex -> Lancer`
- Spawn/audio/data sync completed for renamed enemy set:
  - director definitions/weights/planned tables updated
  - enemy scene references and death SFX bindings updated
  - old legacy enemy sprites/sfx assets removed from active runtime paths.

- Character identity remap completed for player archetypes:
  - Ranged = `Wizard`
  - Melee = `Knight`
  - TankBurst = `Priest`
- Character resources and start panel naming updated:
  - `Data/Characters/MeleeCharacter.tres` -> Knight
  - `Data/Characters/TankBurstCharacter.tres` -> Priest
  - start/meta UI labels synchronized with new archetype naming.

- Player visual system reworked to be character-id driven:
  - added runtime animation-profile switch path:
    - `Scripts/Player/Player.AnimationProfile.cs`
  - `Player.ApplyCharacter(...)` now applies per-character animation atlases and profile contracts.
  - melee attack now triggers visual attack state (fixes Knight attack animation not playing during melee timeline).

- Player architecture refactor landed (combat/input/visual stability):
  - command pipeline and explicit per-frame command model:
    - `Scripts/Player/Player.CommandPipeline.cs`
  - visual state machine extraction:
    - `Scripts/Player/PlayerStateMachine.cs`
  - ranged/melee attack timelines extracted:
    - `Scripts/Player/PlayerAttackTimeline.cs`
    - `Scripts/Player/PlayerMeleeTimeline.cs`
  - composition/health/damage signaling paths cleaned to reduce coupling regressions.

- Projectile pipeline replaced (old basic bullet path retired):
  - removed legacy base projectile resource path from runtime (`Prefabs/Bullet.tscn` retired).
  - added per-archetype projectile prefabs:
    - `Prefabs/WizardProjectile.tscn`
    - `Prefabs/PriestProjectile.tscn`
  - `PlayerWeapon` now resolves projectile scenes by character id and keeps fallback loading guards.

- Projectile VFX behavior upgraded:
  - `Scripts/Projectiles/Bullet.cs` and `Bullet.Collision.cs` now support:
    - staged animation flow (flight/impact)
    - impact-finish cleanup instead of immediate disappear
    - direction-based facing rotation.
  - Wizard projectile switched to sequence frames:
    - `Wizard-Attack_Effect (1..4)`
    - frames 1-3 flight loop, frame 4 impact.
  - Priest temporarily reuses same 4-frame sequence contract for consistency.
  - projectile scale baseline raised to improve readability under new pixel style.

- Knight dash visual contract finalized:
  - dash animation source moved from attack-sheet slicing to dedicated `Knight-Dash.png`.
  - dash state now plays dedicated `dash` animation (with fallback to `walk` for non-dash-profile characters).
  - prior atlas bleed/tearing issues mitigated by frame slicing updates and `FilterClip` for runtime atlas frames.

- Debug cleanup + replacement direction:
  - legacy debug overlay/system files removed from active runtime path.
  - project now uses newer cheat/debug entry path (`DebugCheatSystem`) instead of old overlay stack.

- Documentation sync completed for this major pass:
  - `docs/FANTASY_PIXEL_STYLE_SPEC.md`
    - core player naming updated (Wizard/Knight/Priest)
    - bullet entry replaced with projectile-VFX entry.
  - `docs/ART_DIFFERENTIATION_FANTASY_PIXEL_PLAN.md`
    - touchpoint list updated from `Prefabs/Bullet.tscn` to character projectile prefabs
    - phase tasks updated to current projectile/effect pipeline.
  - `README.md`
    - prefab description updated from "bullet" wording to "projectile" wording.

- Validation:
  - repeated `dotnet build ProjectGenesis.sln` passes succeeded (0 compile errors).
  - external `NU1900` warnings remain environmental (NuGet index connectivity), not gameplay-logic regressions.

## Session Update (2026-02-24, Lancer Color Fix + EXP Replacement Stabilization)
- Fixed incorrect Lancer in-game color tint:
  - root cause: `Enemies/Lancer.tscn` had a hard-coded sprite `modulate` (`Color(1, 0.65, 0.65, 1)`).
  - action: removed tint override so Lancer now renders with source texture colors.

- Replaced EXP pickup visual with the newly provided art while keeping load reliability:
  - initial direct path swap to `Assets/Sprites/EXP.png` triggered editor load failure.
  - diagnosed PNG metadata incompatibility risk (`iTXt` chunk-heavy export) and normalized file content.
  - final stable integration strategy:
    - overwrite `Assets/Sprites/Items/ExpPickup.png` with the new 24x32 art,
    - keep prefab path on the established import pipeline (`res://Assets/Sprites/Items/ExpPickup.png`).
  - updated runtime visual scale in `Prefabs/ExperiencePickup.tscn` to `Vector2(1, 1)` for the new source size.

- Collision/scene sanity pass notes:
  - verified active projectile/enemy/player collision shapes against new pixel visuals.
  - legacy compatibility prefabs were already migrated previously (`Prefabs/Bullet.tscn` and `Prefabs/enemy.tscn` point to new scenes).

- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 compile errors).
  - only external `NU1900` warnings remain (NuGet service index connectivity).

## Session Update (2026-02-24, Audio Pack Migration + BGM Playlist Upgrade)
- Sound asset pipeline migration to standardized structure:
  - moved runtime audio into categorized folders:
    - `Assets/Sound/Bgm`
    - `Assets/Sound/UI`
    - `Assets/Sound/Player`
    - `Assets/Sound/Enemies`
  - normalized naming to lowercase snake_case for runtime-bound assets.
  - replaced active runtime paths with newly provided sound pack assets.
- Runtime audio bindings updated:
  - `Scripts/Audio/AudioManager.Setup.cs`
    - all stream load paths migrated to new folder structure,
    - enemy death SFX map moved from legacy `EnemiesDies/` to `Enemies/`,
    - added dedicated player streams:
      - priest fire (`sfx_player_fire_priest.wav`)
      - exp pickup (`sfx_player_exp_pickup.wav`)
      - projectile hit enemy (`sfx_player_hit_enemy.wav`)
  - `Scripts/Audio/AudioManager.cs`
    - added playback APIs:
      - `PlaySfxPlayerFirePriest()`
      - `PlaySfxPlayerExpPickup()`
      - `PlaySfxPlayerHitEnemy()`
  - gameplay callsite wiring:
    - `Scripts/Player/PlayerWeapon.cs`:
      - priest projectile now uses priest-specific fire SFX.
    - `Scripts/Systems/Progression/ExperiencePickup.cs`:
      - pickup now plays exp-pickup SFX (instead of upgrade SFX).
    - `Scripts/Projectiles/Bullet.Collision.cs`:
      - successful hit now triggers hit-enemy SFX.
- BGM system upgraded from single-track to playlist mode:
  - `Scripts/Audio/AudioManager.cs` + `AudioManager.Playback.cs` + `AudioManager.Setup.cs`
  - introduced playlist states:
    - `Menu`
    - `Gameplay`
    - `Result`
  - selection behavior:
    - random next-track on `AudioStreamPlayer.Finished`,
    - best-effort non-repeat against previous track.
  - active pools:
    - Menu: `bgm_menu_alt_01..03.mp3`
    - Gameplay: `bgm_gameplay.mp3` + `bgm_gameplay_alt_01..04.mp3`
    - Result: `bgm_result.mp3`
- End-state audio flow:
  - `Scripts/UI/GameFlowUI.EndState.cs`
  - both win/loss settlement now switch to `PlayBgmResult()`.
- Default audio volume policy updated:
  - `Scripts/UI/GameFlowUI.SettingsPersistence.cs`
  - if `user://settings.cfg` does not exist:
    - BGM defaults to `50%`
    - SFX defaults to `80%`
  - existing saved user values remain respected.
- Legacy sound assets:
  - legacy in-repo runtime paths were retired from active use.
  - backup copy moved outside repo for safety:
    - `C:\Users\JSrad\Desktop\Godot\audio_legacy_backup_20260224\_Legacy`
- Docs sync:
  - updated `README.md` runtime flow + sound folder map.
  - updated `docs/ARCHITECTURE.md` with audio runtime contract and defaults.
- Validation:
  - `dotnet build ProjectGenesis.csproj` succeeded (0 compile errors).
  - only external `NU1900` warnings remain (NuGet service index connectivity).
