# Scene Split Notes (MainScence.tscn)

`MainScence.tscn` is now around 565 lines with many node sections. It is still workable, but should be split by ownership to reduce editor cognitive load.

## Recommended split order
1. Extract `Player` subtree into `Scenes/Player.tscn`. (Done)
2. Extract `Systems` subtree into `Scenes/SystemsRoot.tscn`.
3. Extract `CanvasLayer` subtree into `Scenes/HudAndMenus.tscn`.
4. Keep `Game` as composition root that instances the three scenes above.

## Why this split
- Faster navigation in scene tree and fewer accidental edits.
- Per-feature scene ownership (player/system/ui) aligns with current script boundaries.
- Lower merge conflicts when gameplay and UI are edited in parallel.

## Do not split yet
- `World` background nodes can stay in main scene unless multiple maps/themes are planned.
