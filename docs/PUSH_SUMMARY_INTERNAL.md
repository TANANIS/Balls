# Internal Push Summary (2026-02-18)

This document summarizes the two most recent pushes to `origin/main`.

## Push #1
- Push range: `4631b9a -> 584da75`
- Commit: `584da75`
- Title: `Refactor systems into partial modules and extract Player scene`
- Scope: Structural refactor (no intended gameplay rule change), compile stabilization, scene modularization groundwork.

### Objectives
- Reduce script complexity by splitting large files into responsibility-based partial files.
- Improve maintainability and readability with explicit module boundaries and inline memory-aid comments.
- Prepare scene-level modularization by extracting `Player` subtree into its own scene.

### Key code changes by domain
1. Director systems
- `SpawnSystem` split into:
  - `Scripts/Systems/Director/SpawnSystem.cs`
  - `Scripts/Systems/Director/SpawnSystem.Runtime.cs`
  - `Scripts/Systems/Director/SpawnSystem.Selection.cs`
  - `Scripts/Systems/Director/SpawnSystem.Csv.cs`
  - `Scripts/Systems/Director/SpawnSystem.Types.cs`
- `PressureSystem` split into:
  - `Scripts/Systems/Director/PressureSystem.cs`
  - `Scripts/Systems/Director/PressureSystem.TiersCsv.cs`

2. Progression / Core / UI systems
- `UpgradeSystem` split into lifecycle/options/types partials.
- `DebugSystem` split into runtime API and overlay UI partials.
- `GameFlowUI` split into setup/state/visual partials.
- `UpgradeMenu` split into lifecycle/UI/options partials.
- `AudioManager` split into API/setup/playback partials.

3. Player / Enemy / Projectile
- `Player`, `PlayerDash`, `PlayerMelee`, `Enemy`, `Bullet` split into cohesive partial modules.
- Replaced broken/garbled comments in legacy files with readable internal intent comments.

4. Scene and docs
- Extracted `Player` subtree from `MainScence.tscn` into `Scenes/Player.tscn`.
- Main scene now instances `Player` scene instead of embedding all player nodes inline.
- Added refactor documentation:
  - `docs/SCRIPT_REFACTOR_PLAN.md`
  - `docs/SCENE_SPLIT_NOTES.md`

### Behavior impact notes
- Intended gameplay behavior remained equivalent during this push.
- Build fixes included minor compile correctness updates (e.g. type cast correction in perf probe path during refactor cycle).

### Risk profile
- Medium structural risk due to file movement and split count.
- Runtime risk reduced by preserving method signatures and scene node paths.

### Verification done
- `dotnet build 20260120.csproj` passed.

---

## Push #2
- Push range: `584da75 -> dd7e105`
- Commit: `dd7e105`
- Title: `Rework map presentation and dynamic obstacle generation`
- Scope: Gameplay/map presentation update + obstacle generation model rewrite.

### Objectives
- Remove legacy animated background assets and script dependencies.
- Normalize map presentation during menu/start flow.
- Introduce environment obstacle visual asset pipeline and dynamic obstacle spawning behavior.
- Remove player map boundary restriction.

### Asset/content changes
1. Removed background animation package
- Deleted `Assets/Sprites/Background/*` (OGV, frame sheet, PNG frame sequence, imports).
- Deleted `Scripts/UI/BackgroundAnimatedSprite.cs` and its `.uid`.

2. Added obstacle asset location
- Added `Assets/Sprites/Environment/ObstacleRock.png` (+ import file moved/renamed by git).

### Runtime system changes
1. Obstacle generation system added
- New: `Scripts/World/ObstacleFieldGenerator.cs`
- New behavior:
  - Spawns `StaticBody2D` obstacles with visual sprite + collision.
  - Uses randomized placement/scale/rotation.
  - Later iteration adjusted to dynamic spawning outside current view bounds.
  - Obstacles are retained (no runtime purge), capped by `MaxObstacleCount`.

2. Main scene obstacle wiring
- `MainScence.tscn` now attaches obstacle generator script on `World/Obstacles`.
- Scene exports configure generation rate, ring distance, spacing, scale, collider factors.

3. Menu/background and gameplay visibility flow
- `Scripts/UI/GameFlowUI.cs`
- `Scripts/UI/GameFlowUI.State.cs`
- `Scripts/UI/GameFlowUI.Visuals.cs`
- Changes include:
  - Explicit hiding of gameplay-world objects during start UI phase.
  - Explicit menu background/dimmer visibility transitions on Start.
  - Reworked menu background fit/alignment logic to follow viewport/camera center behavior.

4. Player movement bounds default
- `Scripts/Player/Player.cs`
- `UseMovementBounds` default changed to `false` (no map clamp by default).

5. Camera availability
- `Scenes/Player.tscn` includes active `Camera2D` so player-centered view is guaranteed.

### Behavior impact notes
- Menu now avoids showing active gameplay objects pre-start.
- Obstacle spawning changed from static hardcoded block nodes to procedural generation.
- Player movement can now exceed previous hard clamp rectangle unless re-enabled manually.
- Bullet-world interactions continue through `World` grouping for static environment blocking behavior.

### Risk profile
- Medium gameplay tuning risk:
  - Obstacle density and spacing are parameter-sensitive.
  - Camera-aligned menu visuals depend on viewport/camera timing and may need final art polish.
- Low architectural risk (changes are isolated and script-driven).

### Verification done
- `dotnet build 20260120.csproj` passed after each major iteration.

---

## Operational notes for next phase
1. Recommended immediate tuning checkpoints
- Obstacle density (`SpawnIntervalSeconds`, `SpawnPerTick*`, spacing multiplier).
- Spawn ring distance relative to camera zoom.
- Start menu world-hide correctness across restart path.

2. Baseline for upcoming Universe Stability redesign
- Director systems are now modular enough to add `StabilitySystem` without redoing core refactors.
- Current obstacle and map flow changes should be treated as pre-stability baseline.
