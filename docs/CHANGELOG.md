# Changelog

## 2026-03-03

### UI Flow
- Added title-first boot flow and preserved existing transition into start menu.
- Kept `GameFlowUI` as flow router/state owner for page switching and run entry.

### Start Menu Structure
- Split start menu into independent page scenes under `Scenes/UI/Panels/`.
- Added page controllers for:
  - Main
  - Settings
  - Cards
  - Character Select
- Bound each page controller directly in its page scene for Inspector-friendly editing.

### Shared UI State
- Added `Scripts/UI/Models/GameFlowUiSharedState.cs` to centralize:
  - settings snapshot (audio/window/language),
  - selected character,
  - event loadout draft mirror state.

### Documentation
- Updated:
  - `docs/ARCHITECTURE.md`
  - `docs/SYSTEM_FLOW.md`
  - `docs/SCENE_SPLIT_NOTES.md`
  - `docs/SCRIPT_REFACTOR_PLAN.md`

### Validation
- `dotnet build Oriluneia.sln` passed.
- UTF-8 BOM check passed for repository text assets (excluding `.godot` generated metadata).
