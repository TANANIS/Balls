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
