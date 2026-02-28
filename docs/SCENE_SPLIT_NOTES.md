# Scene Split Notes (MainScence.tscn)
Last Synced: 2026-02-28


> Status: Maintained guidance. Re-validate when introducing additional world themes or UI root-level wrappers.

Current composition is already split and in use:

1. `Scenes/Player.tscn`
2. `Scenes/Systems/SystemsRoot.tscn`
3. `Scenes/UI/GameFlowUIRoot.tscn` (instanced under `CanvasLayer/UI`)
4. `Scenes/World/WorldRoot.tscn`

`MainScence.tscn` now acts as composition root and wires these runtime roots together.

## Path Normalization Update (2026-02-28)
- Runtime scene ownership was normalized to `Scenes/*` domain roots:
  - enemies: `Scenes/Actors/Enemies/*`
  - projectiles: `Scenes/Projectiles/*`
  - obstacles: `Scenes/Props/Obstacles/*`
  - gameplay pickups: `Scenes/Gameplay/*`
  - vfx: `Scenes/VFX/*`
- Deprecated roots for runtime scene ownership:
  - `Enemies/*`
  - `Prefabs/*`

## Why This Is Good Enough For Now
- Ownership is separated by domain (Player / Systems / UI / World).
- Merge conflicts are lower than a single giant scene layout.
- Runtime script boundaries now align with scene boundaries.

## Optional Future Split (Only If Needed)
- Extract `CanvasLayer` into an additional wrapper scene only when UI root count grows significantly.
- Keep `World` in current root unless multiple map themes or runtime map swaps are introduced.
