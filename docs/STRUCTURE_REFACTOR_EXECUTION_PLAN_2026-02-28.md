# Structure Refactor Execution Plan (2026-02-28)
Last Synced: 2026-02-28

## Purpose
- Execute a full cleanup program for code structure, folders, scene paths, and governance.
- Keep runtime behavior stable while reducing maintenance cost.
- Enforce AGENTS architecture contracts during every phase.

## Scope
- In scope:
  - `Scripts/*` responsibility split and size control.
  - `Scenes/*`, `Enemies/*`, `Prefabs/*` path unification.
  - structural guard scripts in `Tools/Quality/*`.
  - docs synchronization for architecture and flow contracts.
- Out of scope for this pass:
  - gameplay rebalance.
  - visual asset redraw/replacement.
  - design-system level UI rewrite.

## Non-Negotiable Constraints
- UTF-8 without BOM for all text/code/resource text files.
- Combat pipeline rule remains: only `CombatSystem` finalizes damage.
- UI remains presentation/input only.
- Data-driven balance first (`Data/Director/*.csv`, upgrade data), code second.
- `MainScence.tscn` remains composition root unless migration is explicitly gated and validated.

## Execution Strategy
- Batch size: one domain at a time.
- Safety order: refactor internals first, move file paths second.
- Gate after each batch:
  - `dotnet build Oriluneia.sln` must pass.
  - BOM scan must return no files.
  - scene and runtime smoke flow check on the touched area.

## Phase Plan
## Phase 0 - Baseline Freeze
Status: `completed`

Deliverables:
- baseline report file under `docs/reports/`.
- reproducible quality script under `Tools/Quality/`.
- current metrics snapshot (line-size risk, mixed-path references, build/BOM state).

Exit criteria:
- baseline snapshot committed.
- health-check command can be executed by any contributor.

## Phase 1 - Script Size and Responsibility Split
Status: `completed`

Priority order:
1. `Scripts/World/ObstacleFieldGenerator.cs`
2. `Scripts/Player/PlayerWeapon.cs`
3. `Scripts/Player/PlayerHealth.Vfx.cs`
4. `Scripts/Projectiles/Bullet.cs`
5. `Scripts/World/ProceduralTerrainBackground.*`

Rules:
- Split by concern (`Lifecycle`, `Placement`, `Biome`, `Variants`, `Factory`, etc.).
- Keep public API stable where possible.
- No gameplay behavior change intended.

Exit criteria:
- no file above 300 lines in the phase-1 target list.
- no regression in obstacle spawn, projectile hit flow, player combat flow.

## Phase 2 - Scene/Folder Path Unification
Status: `completed`

Target end-state:
- `Scenes/Actors/Enemies/*`
- `Scenes/Projectiles/*`
- `Scenes/Props/Obstacles/*`
- keep `Scenes/UI/*`, `Scenes/World/*`, `Scenes/Systems/*`.

Migration order:
1. projectile scenes.
2. obstacle/prop scenes.
3. enemy scenes.
4. legacy wrappers/aliases cleanup if still needed.

Exit criteria:
- no runtime references to deprecated roots (`res://Prefabs/`, `res://Enemies/`) except approved compatibility shims.
- all references resolve and open in Godot without missing resource errors.

## Phase 3 - Naming and Boundary Consistency
Status: `completed`

Tasks:
- align domain naming between `Scripts/*` and `Scenes/*`.
- centralize group-name strings/constants for service discovery.
- reduce typo-risk for `GetNodesInGroup` usage.

Exit criteria:
- consistent domain naming map documented.
- compile-time constants replace scattered group-name literals in touched modules.

## Phase 4 - Asset/Data Governance
Status: `completed`

Tasks:
- clarify asset placement conventions in docs.
- add data ownership notes for `Data/Director`, `Data/Upgrades`, `Data/Localization`.
- add drift checks for mixed placement hotspots.

Exit criteria:
- contributor can place new asset/data file without ambiguity.
- docs and runtime paths remain aligned.

## Phase 5 - Documentation and Guardrails
Status: `completed`

Tasks:
- sync architecture/system-flow/refactor docs to final state.
- update `docs/TODO.md` with remaining structural debt.
- ensure quality scripts cover recurring regressions.

Exit criteria:
- docs describe actual runtime and folder state.
- pre-commit checklist fully reproducible from repository scripts.

## Phase 6 - Slimming Pass #1 (Post-Refactor Consolidation)
Status: `completed`

Tasks:
- merge low-risk over-split partial families while keeping file-size guardrails.
- reduce script count without changing gameplay flow or ownership boundaries.
- keep each consolidated file under `<= 300` lines.

Completed consolidations:
- `UpgradeMenu`: `3 -> 1` (`UpgradeMenu.UI`, `UpgradeMenu.Options` merged into `UpgradeMenu.cs`).
- `PlayerWeapon`: `4 -> 2` (`Configuration` merged into core, `ElementalBurst` merged into attack).
- `DebugCheatSystem`: `5 -> 3` (`Pause`, `Localization` merged into core).
- `Bullet`: `6 -> 5` (`Bullet.Homing` merged into `Bullet.Collision`).
- `PlayerHealth`: `6 -> 5` (`PlayerHealth.Shield` merged into `PlayerHealth.Core`).
- `AudioManager`: `3 -> 2` (`AudioManager.Playback` merged into `AudioManager.cs`).

Exit criteria:
- no compile/runtime regression from consolidation.
- `Check-StructureHealth.ps1` and `Check-SceneResourcePaths.ps1` remain pass.

## Phase 7 - Slimming Pass #2 (UI + Director Consolidation)
Status: `completed`

Tasks:
- consolidate over-fragmented UI partials under `GameFlowUI` while keeping boundary clarity.
- consolidate low-risk `SpawnSystem` helper partials without altering pacing/spawn logic.
- keep each touched file under `<= 300` lines.

Completed consolidations:
- `GameFlowUI`:
  - merged `BuildSummary` + `PerfectLeaderboard` into `EndStateController`.
  - merged `SettingsPresenter` + `SettingsPersistence` into `SettingsUI`.
- `SpawnSystem`:
  - merged `Types` + `Lifecycle` into `SpawnSystem.cs`.
  - merged `Debug` into `SpawnSystem.Runtime.cs`.

Exit criteria:
- no compile/runtime regression from consolidation.
- `Check-StructureHealth.ps1` and `Check-SceneResourcePaths.ps1` remain pass.

## Runbook Commands
```powershell
# Build gate
dotnet build Oriluneia.sln

# Structure health (build + line risk + path drift + BOM)
powershell -ExecutionPolicy Bypass -File Tools/Quality/Check-StructureHealth.ps1

# Scene/resource path integrity
powershell -ExecutionPolicy Bypass -File Tools/Quality/Check-SceneResourcePaths.ps1
```

## Progress Snapshot (Current)
- Script volume:
  - total scripts: `141`
- Script-size risk:
  - files `> 300` lines: `0`
  - files `>= 220` lines: `15`
- Path drift:
  - `res://Prefabs/` references in `.tscn/.tres`: `0`
  - `res://Enemies/` references in `.tscn/.tres`: `0`
- Validation:
  - `dotnet build Oriluneia.sln`: pass
  - BOM scan: pass
  - `Check-SceneResourcePaths.ps1`: pass

## Current Risks
- Existing local uncommitted changes are present; migration edits must not overwrite unrelated user work.
- Scene path migration can create hidden missing-reference regressions if done in large batches.
- Large partial-class families can hide coupling; each split must run smoke behavior checks.
