# Art Differentiation Plan: Fantasy Pixel Style

## Status
- Branch target: `feature/art-fantasy-pixel-style`
- Scope in this document: art direction + integration plan.
- Non-goal in this phase: gameplay rule changes.
- Style + size baseline source of truth:
  - `docs/FANTASY_PIXEL_STYLE_SPEC.md`

## Why This Plan Exists
- Current runtime logic is stable and data-driven; visual replacement should not break flow contracts.
- Existing visuals are mostly geometric placeholders and already flagged for replacement in `docs/TODO.md`.
- We need a controlled migration path from current assets to a readable fantasy pixel style.

## Runtime Contracts That Must Stay Intact
- Run structure remains `15:00` with 4 staged pressure windows.
- Core progression stays pickup EXP -> level-up queue -> upgrade menu.
- UI remains presentation-only and should not own gameplay state.
- Skill VFX attachment stays on `Player/SkillVfxRoot`.
- Text encoding rule stays UTF-8 (Godot text resources must be UTF-8 without BOM).

## Current Visual Touchpoints (Asset Map)
- Player core sprite:
  - `Data/Characters/RangedCharacter.tres`
  - `Data/Characters/MeleeCharacter.tres`
  - `Data/Characters/TankBurstCharacter.tres`
- Player/attack prefabs:
  - `Scenes/Player.tscn`
  - `Prefabs/WizardProjectile.tscn`
  - `Prefabs/PriestProjectile.tscn`
  - `Prefabs/MeleeVFX.tscn`
- Enemy visuals:
  - `Enemies/Slime.tscn`
  - `Enemies/Orc.tscn`
  - `Enemies/EliteOrc.tscn`
  - `Enemies/EliteSlime.tscn`
  - `Enemies/Lancer.tscn`
- Progression and drops:
  - `Prefabs/ExperiencePickup.tscn`
- World/background:
  - `Scenes/World/WorldRoot.tscn`
  - `Assets/Sprites/Environment/bg_run_forest_tile.png`
  - `Assets/Sprites/Environment/obstacle_big_rock.png`
  - `Assets/Sprites/Environment/obstacle_small_tree.png`
  - `Prefabs/Obstacles/ObstacleBigRock.tscn`
  - `Prefabs/Obstacles/ObstacleSmallTree.tscn`
  - `Scripts/World/InfiniteTiledBackground.cs`
  - `Scripts/World/ObstacleFieldGenerator.cs`
- Menu/HUD surfaces:
  - `Scenes/UI/Panels/StartPanel.tscn`
  - `Scenes/UI/Panels/PausePanel.tscn`
  - `Scenes/UI/Panels/RestartPanel.tscn`
  - `Scenes/UI/HudOverlay.tscn`

## Target Art Direction (Fantasy Pixel)
- Perspective: top-down 2D pixel style.
- Theme: arcane fantasy ruins + energy anomalies.
- Readability contract:
  - Player silhouette is always highest readability.
  - Enemy classes are distinguishable by shape and 1 accent color.
  - Attack and danger telegraphs use high-contrast value shifts, not only hue.
- Palette direction:
  - World: desaturated stone/earth base.
  - Player/ally effects: cyan + gold accents.
  - Enemy threats: crimson/purple accents.
  - Keep total simultaneous accent colors low per screen.

## Production Constraints (Important)
- Keep existing file paths stable when possible to minimize scene/script churn.
- If introducing new files, keep naming deterministic and lowercase for skill sprites.
- Preserve collision/hitbox gameplay geometry; sprite change must not silently modify balance.
- Replace visuals in slices and validate each slice with a quick run smoke test.

## Execution Plan (Ordered)

### Phase 1: Visual Baseline + Pipeline Hardening
- Goal:
  - Define pixel import defaults and one consistent base resolution target.
- Tasks:
  - Lock style tokens and size baseline in `docs/FANTASY_PIXEL_STYLE_SPEC.md`.
  - Set/verify Godot import presets for pixel art (filtering/mipmap behavior).
  - Build first replacement set with fixed size contract:
    - `Wizard`, `Slime`, `WizardProjectile/PriestProjectile`, `ExpPickup`, `Shield`.
- Exit criteria:
  - No blur artifacts in movement/camera.
  - First replacement set matches the size table and passes smoke test.

### Phase 2: Combat-Critical Runtime Assets First
- Goal:
  - Replace the assets that directly affect moment-to-moment readability.
- Tasks:
- Player cores (`Wizard`, `Knight`, `Priest`).
- Projectile/melee/pickup (`Wizard-Attack_Effect*`, `Priest-Attack_Effect*`, `Melee.png`, `ExpPickup.png`).
  - Enemy sprites for all currently spawned classes.
  - Shield visual fallback (`Assets/Sprites/Skills/Shield/shield.png`) in matching style.
- Exit criteria:
  - 15:00 run is fully playable with no missing textures.
  - Enemy class recognition remains clear under high density.

### Phase 3: Environment + Phase Identity
- Goal:
  - Give each run stage stronger fantasy-pixel atmosphere while preserving clarity.
- Tasks:
  - Replace world background and obstacle textures.
  - Introduce stage tint/overlay variants bound to existing phase timing (visual-only).
  - Keep obstacle readability and collision expectation consistent.
- Exit criteria:
  - Players can still read navigable space quickly.
  - Stage transitions are visible without UI explanation.

### Phase 4: UI Skinning Pass
- Goal:
  - Move menu/HUD from placeholder look to style-consistent fantasy pixel UI.
- Tasks:
  - Panel frames, button states, icon pass for start/pause/restart/hud.
  - HP/XP bar final art replacement.
  - Preserve all current localization and panel flow behavior.
- Exit criteria:
  - No layout overflow regression at common window sizes.
  - UI remains bilingual-safe and readable.

### Phase 5: Polish + Performance Gate
- Goal:
  - Stabilize final look and avoid regressions.
- Tasks:
  - Adjust hit flash, knockback flash, and VFX intensity per enemy size tier.
  - Review draw-call and texture memory impact.
  - Fix any import/path inconsistencies and dead placeholders.
- Exit criteria:
  - `dotnet build ProjectGenesis.sln` passes.
  - Manual run smoke test passes for start -> run -> end-state -> back-to-meta.

## Definition Of Done (Art Differentiation Branch)
- All placeholder core combat visuals replaced with fantasy pixel assets.
- No runtime missing-resource warnings in normal play.
- All updated docs are synced (`README.md`, this plan, and relevant asset notes).
- Branch remains mergeable without gameplay logic regression.

## Collaboration Workflow
- Work in small vertical slices: asset set -> wire -> smoke test -> commit.
- For each slice, include:
  - changed paths list,
  - before/after screenshots,
  - known temporary placeholders.
- Keep art naming and folder organization stable so future content scales cleanly.


