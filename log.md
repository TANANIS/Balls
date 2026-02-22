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
- `Scripts/Systems/Director/PressureSystem.cs`
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
  - Removed unused `Scripts/Enemy/EnemyDebugEventModule.cs` (+ `.uid`).
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
