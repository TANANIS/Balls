# Scene Split Notes (MainScence.tscn)

Current composition is already split and in use:

1. `Scenes/Player.tscn`
2. `Scenes/Systems/SystemsRoot.tscn`
3. `Scenes/UI/GameFlowUIRoot.tscn` (instanced under `CanvasLayer/UI`)
4. `Scenes/World/WorldRoot.tscn`

`MainScence.tscn` now acts as composition root and wires these runtime roots together.

## Why This Is Good Enough For Now
- Ownership is separated by domain (Player / Systems / UI / World).
- Merge conflicts are lower than a single giant scene layout.
- Runtime script boundaries now align with scene boundaries.

## Optional Future Split (Only If Needed)
- Extract `CanvasLayer` into an additional wrapper scene only when UI root count grows significantly.
- Keep `World` in current root unless multiple map themes or runtime map swaps are introduced.
