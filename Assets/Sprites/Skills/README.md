# Skills Sprite Layout

This folder contains runtime visual assets for in-run skill effects.

## Required Structure
- `Assets/Sprites/Skills/<SkillName>/`
- Keep each skill's art isolated in its own folder.

## Naming Rules
- Use lowercase file names.
- Use explicit names for purpose (`shield.png`, `impact.png`, `aura_loop.png`).
- Keep source image and generated `.import` side-by-side.

## Runtime Binding Rule
- Runtime scripts should expose a `Texture2D` export field first.
- If no texture is assigned, load fallback from:
  - `res://Assets/Sprites/Skills/<SkillName>/<file>.png`

## Current Example
- `Shield/`
  - `shield.png`
- Used by:
  - `Scripts/Player/PlayerHealth.cs` (`ShieldTexture` fallback)
