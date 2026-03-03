# UI Structure Log (2026-03-03)

## Scope
- Boot title entry screen added before start menu.
- `GameFlowUI` node lookup paths migrated from hard-coded strings to `Export NodePath` fields.
- Localization label lookups migrated to exported node-path fields.
- UI layout tuning values migrated to inspector-driven exports.

## Changed Runtime Structure
- Added:
  - `Scenes/UI/Panels/TitleScreen.tscn`
  - `Scripts/UI/GameFlowUI.TitleScreen.cs`
  - `Scripts/UI/GameFlowUI.LayoutConfig.cs`
  - `Scripts/UI/GameFlowUI.LocalizationPaths.cs`
- Updated:
  - `Scenes/UI/GameFlowUIRoot.tscn` (TitleScreen instancing)
  - `Scripts/UI/GameFlowUI.References.cs` (global node-path exports)
  - `Scripts/UI/GameFlowUI.EventUnlockPanelController.cs` (event-unlock node-path exports)
  - `Scripts/UI/GameFlowUI.EventLoadoutPanelController.cs` (event-loadout node-path exports)
  - `Scripts/UI/GameFlowUI.Localization.cs` (path-source migration to exported NodePath)
  - `Scripts/UI/GameFlowUI.UIStateController.cs` (dialog size path now config-driven)
  - `Scripts/UI/GameFlowUI.Visuals.cs` (menu background layout params now config-driven)

## Behavior Notes
- Startup flow changed:
  - old: game boot -> start panel
  - new: game boot -> title screen -> any input -> start panel
- Default exported paths are prefilled to current scene hierarchy, so no manual assignment is required unless hierarchy changes.

## Validation
- `dotnet build Oriluneia.sln`: pass
- BOM scan on tracked source/resource text files: pass

## Purpose
- Reduce scene hierarchy coupling risk in UI refactors.
- Make day-to-day UI iteration editor-first (Inspector) instead of code-first.
