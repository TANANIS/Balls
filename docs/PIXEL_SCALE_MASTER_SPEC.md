# Pixel Scale Master Spec (v1)
Last Synced: 2026-02-27


## Purpose
- Lock `Camera / Unit / Art` into one deterministic rule set.
- Remove trial-and-error tuning for scene scale.
- Keep future 32x32 player migration stable.

## Scope
- In-run gameplay view only.
- Menu visual layout is a separate layer and follows UI rules.

## Single Source Of Truth
- Gameplay reference canvas: `480 x 270` (art-space baseline, not forced window size).
- Player source sprite target: `32 x 32`.
- Runtime window can stay `1280 x 720` (or other), but gameplay readability is evaluated against the reference canvas.

## Core Contract
1. Player screen occupancy target:
   - Player visual height should occupy `5% ~ 7%` of gameplay screen height.
   - Initial target: `6%`.
2. Camera baseline must be solved from occupancy target, not guessed.
3. Background tiles and obstacle sprites use integer scale steps whenever possible (`1x`, `2x`, `3x`, ...).
4. Collision shapes are gameplay-first and manually authored per prefab.
   - Never auto-scale colliders from random sprite scale at runtime.

## Camera Rule
- Define:
  - `H_ref` = reference gameplay height (`270`)
  - `P_player` = intended on-screen player height in reference pixels
  - `S_player_world` = effective player world height after sprite scale
- Occupancy target:
  - `P_player / H_ref = 0.06` (initial)
- Practical workflow:
  1. Confirm effective player world size from `Scenes/Player.tscn`.
  2. Set camera baseline zoom so player appears near `~16 px` on 270p reference (`270 * 0.06 = 16.2`).
  3. Phase zoom multipliers apply on top of this baseline; baseline itself is fixed.

## Art Scale Bands (Gameplay Objects)
- Player core: `32x32` source.
- Common obstacles: `16~32` source, then prefab integer upscales.
- In-run background tile: `480x270` source for one tile unit.
- Enemy classes stay in current size bands from `docs/FANTASY_PIXEL_STYLE_SPEC.md` unless gameplay readability fails.

## Creature Size Contract (New)
- Rule type:
  - gameplay collision remains authoritative,
  - visual size must be calibrated against collision, not source sprite canvas.
- Visual-to-collision ratio target:
  - `visual_diameter / collision_diameter = 1.05 ~ 1.35`
  - recommended start point: `1.18`
- Why:
  - prevents tiny-looking enemies with normal hitboxes,
  - preserves readability and fair contact expectation.

### Class Targets (Reference)
- Player:
  - collision radius ~`34` => collision diameter `68`
  - visual diameter target: `72 ~ 90`
- Basic swarm/chaser:
  - collision radius `24~28` => collision diameter `48~56`
  - visual diameter target: `54 ~ 72`
- Heavy/elite:
  - collision radius `31~34` => collision diameter `62~68`
  - visual diameter target: `68 ~ 88`
- Mini boss:
  - collision radius ~`48` => collision diameter `96`
  - visual diameter target: `104 ~ 132`

### Practical Calibration Workflow
1. Keep collision shapes unchanged.
2. Measure visible body size (exclude transparent padding).
3. Tune sprite scale until ratio is inside target band.
4. Validate at combat density (not only single-enemy preview).

## Runtime Layer Separation
- `MenuBackground`: fit-to-cover UI/menu concern only.
- `RunBackground`: infinite tiled world concern only.
- Do not reuse one fitting algorithm for both domains.

## Environment Generation Rule
- Obstacle randomness should affect:
  - type
  - position
  - cluster behavior
- Obstacle randomness should not affect:
  - per-instance arbitrary rotation (unless asset supports it intentionally)
  - per-instance arbitrary non-integer scale

## Validation Checklist (Every Art Iteration)
1. Start run for 2 minutes and inspect:
   - player readability at center and edge,
   - obstacle readability under density,
   - no background seam or missing tile.
2. Open menu at multiple window sizes:
   - no visible edge leakage on menu background.
3. Confirm gameplay collisions:
   - tree/rock collider matches expected blocked area, not sprite silhouette noise.
4. If any target fails:
   - tune camera baseline first,
   - then prefab scales,
   - then source art detail density.
   - Do not patch with random per-instance scaling.

## Change Control
- Any change to baseline camera zoom, player base visual size, or reference canvas must update:
  - this doc,
  - `docs/FANTASY_PIXEL_STYLE_SPEC.md`,
  - related runtime scene/script parameters.

## Current Runtime Baseline (Applied)
- `Scenes/Player.tscn`
  - `Camera2D.zoom = (1.35, 1.35)` as baseline camera.
- `Scenes/World/WorldRoot.tscn`
  - run background `TileScale = (3, 3)`.
  - run background `FlipXChance = 0.3`.
- `Scripts/Systems/Director/StabilitySystem.cs`
  - `StructuralFractureCameraZoomMultiplier = 1.04`
  - `CollapseCriticalCameraZoomMultiplier = 1.08`
- `Enemies/Slime.tscn`
  - Slime replacement uses larger visual scale to compensate heavy transparent padding in source frames.
