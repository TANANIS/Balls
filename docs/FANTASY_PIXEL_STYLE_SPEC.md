# Fantasy Pixel Style Spec (v1)

## Status
- Date: 2026-02-23
- Scope: visual spec + size baseline for art production.
- Reference direction: *Little Witch in the Woods* style mood (cozy fantasy pixel), adapted to this project's combat readability needs.

## Style Intent
- Tone:
  - Warm, handcrafted fantasy village/ruin atmosphere.
  - Soft edges and rich color transitions inside pixel clusters.
- Combat readability first:
  - Player silhouette must be readable in < 0.2s.
  - Enemy role must be identifiable by shape + one accent color.
  - Danger cues should rely on value contrast (light/dark), not hue only.
- Originality rule:
  - Use reference for mood and rendering language.
  - Do not copy specific props, character outfits, or tile layouts 1:1.

## Color System (Production Tokens)
- World base:
  - `PX_WORLD_GRASS_1 = #8fbf7d`
  - `PX_WORLD_GRASS_2 = #6fa46b`
  - `PX_WORLD_SOIL = #7c6a52`
  - `PX_WORLD_STONE = #7f8578`
- Warm highlights:
  - `PX_WARM_LIGHT = #e9d38f`
  - `PX_WARM_GOLD = #caa45b`
- Player and ally:
  - `PX_PLAYER_CYAN = #75c6c2`
  - `PX_PLAYER_DEEP = #2f6e73`
- Enemy and threat:
  - `PX_ENEMY_RED = #c96a67`
  - `PX_ENEMY_PURPLE = #7b5f86`
  - `PX_THREAT_BRIGHT = #f2b2a6`
- UI neutrals:
  - `PX_UI_DARK = #2b3130`
  - `PX_UI_MID = #4b5957`
  - `PX_UI_LIGHT = #c7c9b7`

Use rule:
- A single gameplay frame should keep accent groups to 3 max:
  - player accent, enemy accent, reward accent.

## Asset Canvas Baseline
- `480 x 270` is the **map/background source art size baseline** (art asset spec).
- `32 x 32` is the **player source sprite size baseline**.
- This section defines asset dimensions, not project window/viewport settings.
- All gameplay-facing sprite dimensions should be multiples of 8 whenever possible.
- Avoid fractional visual scaling for final assets whenever feasible.

## Godot Import Profile (Pixel Art)
Apply to pixel sprites under `Assets/Sprites/`:
- Filter: `Nearest` (disable linear filtering).
- Mipmaps: `Off`.
- Repeat: `Disabled` (unless explicitly tiling textures).
- Compression: lossless/default suitable for pixel textures.
- Reimport after preset changes and verify in-motion sharpness.

## Size Baseline (Gameplay-First)
The table below defines production canvas targets for replacement assets.
These are source-art canvas sizes (not collision sizes).

| Asset Class | Target Canvas | Notes |
|---|---:|---|
| Player core (`Wizard`, `Knight`, `Priest`) | 32x32 | Keep silhouette compact; center pivot. |
| Basic enemy (`Slime`, `Orc`) | 64x64 | Distinguish by outline/readable motion shape. |
| Heavy enemy (`EliteOrc`, `EliteSlime`) | 80x80 | Preserve "heavier" visual mass. |
| Mini boss (`Lancer`) | 96x96 | Distinctly larger than heavy class. |
| Projectile VFX (`WizardProjectile`, `PriestProjectile`) | 16~64 per frame | Multi-frame sequence preferred; keep front edge readable under motion. |
| Exp pickup | 24x24 | Reward object visible but not confused with bullets. |
| Melee slash (`Melee`) | 192x192 | Arc VFX texture sheet/canvas target. |
| Shield (`Skills/Shield/shield.png`) | 96x96 | Matches current visual radius intent (~44 world units). |
| Obstacle set (`obstacle_big_rock`, `obstacle_small_tree`) | 16~32 source px | Use prefab-level scale + collider tuning; avoid runtime random scale/rotation. |

## Migration Rule From Current Placeholder Assets
- Current runtime uses mixed large placeholders (many `512x512` with downscale).
- During migration:
  - Keep collision/hitbox values unchanged.
  - Replace texture first.
  - Only tune scene `Sprite2D.scale` if visual size drifts from gameplay expectation.
- If size correction is needed, adjust by one source of truth:
  - Prefer fixing source canvas size first.
  - Use scene scale tweak as secondary adjustment only.

## Character Visual Identity Guardrails
- Ranged:
  - Cleaner silhouette, brighter cool accents, lower edge noise.
- Melee:
  - Sharper contour, warm accent, directional slash readability.
- TankBurst:
  - Heavier body mass and darker value block, gold/bronze defensive cues.

## UI Skin Direction
- Use fantasy parchment/wood/stone motifs, but keep text contrast high.
- Keep dense UI surfaces desaturated so combat effects remain visually dominant.
- Bilingual safety:
  - Keep label widths tolerant for zh_TW.
  - Maintain UTF-8 workflow from architecture doc.

## Phase 1 Definition Of Done (Spec + Sizing Foundation)
- Color tokens are fixed and shared with all asset contributors.
- Import profile is documented and tested on at least:
  - player core,
  - one enemy,
  - one pickup.
- At least one full replacement set is produced against this size table:
  - `Wizard`, `Slime`, `WizardProjectile/PriestProjectile`, `ExpPickup`, `Shield`.
- In-run smoke test confirms:
  - no blur artifacts,
  - no missing textures,
  - class readability maintained.


