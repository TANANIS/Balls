# Scene Split Notes (MainScence.tscn)
Last Synced: 2026-03-03


> Status: Maintained guidance. Re-validate when introducing additional world themes or UI root-level wrappers.

Current composition is already split and in use:

1. `Scenes/Player.tscn`
2. `Scenes/Systems/SystemsRoot.tscn`
3. `Scenes/UI/GameFlowUIRoot.tscn` (instanced under `CanvasLayer/UI`)
4. `Scenes/World/WorldRoot.tscn`

`MainScence.tscn` now acts as composition root and wires these runtime roots together.

## UI Root Update (2026-03-03)
- `Scenes/UI/GameFlowUIRoot.tscn` now includes a boot entry panel:
  - `Scenes/UI/Panels/TitleScreen.tscn`
- Boot flow:
  - startup enters `TitleScreen`,
  - pressing any key/mouse/gamepad button advances to existing `StartPanel` meta flow.

## Start Menu Page Split (2026-03-03)
- `Scenes/UI/Panels/StartPanel.tscn` now acts as a host container and instances page scenes:
  - `StartMainScroll.tscn`
  - `StartCharacterSelectPage.tscn`
  - `StartEventUnlockPage.tscn`
  - `StartEventLoadoutPage.tscn`
  - `StartSettingsPage.tscn`
  - `StartCardsPage.tscn`
- Goal: keep each start-menu page independently editable in Godot Inspector while preserving existing `GameFlowUI` NodePath contracts.
- Runtime behavior is unchanged: page visibility/state continues to be controlled by `GameFlowUI` (`SetStartSubPanels` and existing panel controllers).

### Page Controller Attachment Log (2026-03-03)
- `StartMainScroll.tscn`
  - script: `Scripts/UI/Pages/StartMainPageController.cs`
- `StartSettingsPage.tscn`
  - script: `Scripts/UI/Pages/StartSettingsPageController.cs`
- `StartCardsPage.tscn`
  - script: `Scripts/UI/Pages/StartCardsPageController.cs`
- `StartCharacterSelectPage.tscn`
  - script: `Scripts/UI/Pages/StartCharacterSelectPageController.cs`
- Controller role:
  - each page resolves its own child nodes and emits page-level actions to `GameFlowUI`.

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
