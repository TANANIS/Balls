# Project Genesis
2D top-down survival action prototype built with Godot 4 (Mono/C#), targeting a fixed 15:00 run loop.

## Core Architecture
- Combat resolution single entry: `Scripts/Systems/Core/CombatSystem.cs`
- Progression and upgrades: `Scripts/Systems/Progression/ProgressionSystem.cs` and `Scripts/Systems/Progression/UpgradeSystem.cs`
- Spawn pacing and pressure shaping: `Scripts/Systems/Director/SpawnSystem.cs` with `Data/Director/*.csv`
- UI is presentation-only and reads runtime state: `Scripts/UI/*`

## Runtime Flow (Current)
1. Enemy dies, then `ExperienceDropSystem` spawns `ExperiencePickup`.
2. Player collects pickup, then `ProgressionSystem` adds EXP.
3. EXP reaches requirement, then a level-up charge is queued.
4. `UpgradeMenu` opens, then `UpgradeSystem` applies one upgrade.
5. `SpawnSystem` scales pressure by phase and director tables.
6. `AudioManager` switches BGM playlist by game state (`Menu` / `Gameplay` / `Result`) and chains random tracks on finish.

## Folder Map (Purpose + Location)
- `Scenes/`: Scene composition (player, world, systems root, UI).
- `Scripts/`: C# gameplay logic (Player, Enemy, Systems, UI, Audio).
- `Data/`: Runtime data and balancing tables.
- `Assets/`: Art and audio assets (including `.import` metadata).
  - `Assets/Sound/Bgm`: BGM tracks and alternates.
  - `Assets/Sound/UI`: UI SFX.
  - `Assets/Sound/Player`: player/combat/pickup SFX.
  - `Assets/Sound/Enemies`: enemy-specific SFX.
- `Prefabs/`: Reusable scene prefabs (projectile, pickup, menu, etc.).
- `Enemies/`: Enemy scenes.
- `docs/`: Design, architecture, flow, refactor, and planning docs.
- `log.md`: Internal iteration log (mainly for Codex collaboration context).

## Documentation Index (Usage + Location)
- `docs/ARCHITECTURE.md`: System boundaries, data flow, guardrails, encoding rules.
- `docs/SYSTEM_FLOW.md`: Mermaid runtime flow diagram.
- `docs/GAME_CONCEPT.md`: Core game concept and loop.
- `docs/GameDirector_Design.md`: Director and 15:00 pacing design.
- `docs/CARDS.md`: Card system spec and layer model.
- `docs/CARDS_CHANGELOG.md`: Card balancing/spec change history.
- `docs/SCRIPT_REFACTOR_PLAN.md`: Script split/refactor plan.
- `docs/CODE_STRUCTURE_AUDIT_2026-02-21.md`: Structure audit and risks.
- `docs/SCENE_SPLIT_NOTES.md`: Scene split notes.
- `docs/META_PROGRESSION_ARCHITECTURE.md`: Out-of-run progression architecture (currency, soft-cap economy, character progression).
- `docs/META_PROGRESSION_IMPLEMENTATION_PLAN.md`: Phase-based implementation checklist for meta progression.
- `docs/ART_DIFFERENTIATION_FANTASY_PIXEL_PLAN.md`: Fantasy pixel art differentiation scope, sequencing, and acceptance criteria.
- `docs/FANTASY_PIXEL_STYLE_SPEC.md`: Fantasy pixel visual tokens, import profile, and sprite size baseline.
- `docs/PIXEL_SCALE_MASTER_SPEC.md`: Camera/unit/art hard rules and validation checklist for pixel-scale consistency.
- `docs/TODO.md`: Current next-step checklist.
- `Assets/Sprites/Skills/README.md`: Skill VFX asset path/naming contract.
- `log.md`: Ongoing dev log and handoff notes.

## Maintenance Rules
- Keep `docs/` as the source of truth for architecture and behavior contracts.
- Update relevant docs after behavior or structure changes.
- Save text files as UTF-8; for Godot `.tres/.tscn`, use UTF-8 without BOM.

## Git Rules For Build Outputs
- Do not commit export/install artifacts such as `.exe`, `.pck`, or export data folders.
- Keep `.godot/` ignored (cache/intermediate metadata).

## Tech Stack
- Engine: Godot 4.x (Mono)
- Language: C#
- Platform: PC prototype
