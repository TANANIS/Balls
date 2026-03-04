# Project Docs Quick Guide

Last updated: 2026-03-04

## Where to read first
- Architecture and runtime boundaries: `docs/ARCHITECTURE.md`
- System runtime flow: `docs/SYSTEM_FLOW.md`
- Upgrade/card design and constraints: `docs/CARDS.md`
- Recent structured release notes: `docs/CHANGELOG.md`
- Working session log (engineering diary): `log.md`

## Current UI structure (snapshot)
- `GameFlowUI` is the flow router/state switch owner only.
- Each page has its own controller for node binding, button wiring, and page-local refresh.
- Shared state is placed in `Scripts/UI/Models/*` and consumed by pages/services.
- Input stack:
  - `InputDeviceService`: active device family detection.
  - `InputGlyphService`: keyboard/Xbox/PlayStation glyph resolution.
  - `InputRebindService`: runtime action rebinding + persistence.

## Current title/start behavior (snapshot)
- Title and start pages are scene-based and editable in Inspector.
- Control settings include an `Auto Lock` toggle:
  - Keyboard/Mouse mode: user editable.
  - Gamepad mode: forced enabled and read-only.
- Cursor mode split:
  - UI/pause/overlay: custom pointer (`Assets/mouse pointer.png`).
  - Active gameplay: aim ring/lock marker behavior.

## Notes
- Contract and implementation details are maintained in `docs/*.md`.
- `log.md` records practical session-level work logs and verification outcomes.
