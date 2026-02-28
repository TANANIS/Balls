# Asset And Data Placement Contract
Last Synced: 2026-02-28

## Purpose
- Keep scene/resource placement deterministic and discoverable.
- Prevent re-growth of mixed roots (`Prefabs/`, `Enemies/`) for runtime scenes.
- Define ownership for data files used by runtime systems.

## Scene Placement (Canonical)
- Runtime scene roots:
  - `Scenes/Actors/Enemies/*`
  - `Scenes/Projectiles/*`
  - `Scenes/Props/Obstacles/*`
  - `Scenes/Gameplay/*`
  - `Scenes/VFX/*`
  - `Scenes/UI/*`
  - `Scenes/World/*`
  - `Scenes/Systems/*`
- Composition root remains:
  - `MainScence.tscn`

## Deprecated Scene Roots
- `Enemies/` and `Prefabs/` are deprecated for runtime scene ownership.
- New runtime `.tscn` files must not be added under these roots.
- Legacy references to `res://Prefabs/*` and `res://Enemies/*` are treated as structural regressions.

## Script Placement
- Runtime logic under `Scripts/*` by domain:
  - `Scripts/Systems/*`
  - `Scripts/Player/*`
  - `Scripts/Enemy/*`
  - `Scripts/Projectiles/*`
  - `Scripts/UI/*`
  - `Scripts/World/*`
  - shared helpers under `Scripts/Shared/*`.

## Asset Placement
- Sprite/audio source assets remain under `Assets/*`.
- Scene files under `Scenes/*` should reference `Assets/*` directly for art/audio resources.
- Do not hardcode random asset paths in gameplay code; keep deterministic fallback path policy.

## Data Ownership
- `Data/Director/*.csv`: pacing, definitions, tier weights (director ownership).
- `Data/Upgrades/*`: upgrade catalog and card metadata ownership.
- `Data/Characters/*`: character base definitions and numeric overlays.
- `Data/Localization/*`: user-facing text and card/UI localization.

## Validation Gates
- Run:
  - `dotnet build ProjectGenesis.sln`
  - `powershell -ExecutionPolicy Bypass -File Tools/Quality/Check-StructureHealth.ps1`
  - `powershell -ExecutionPolicy Bypass -File Tools/Quality/Check-SceneResourcePaths.ps1`
