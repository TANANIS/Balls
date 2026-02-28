# Gameplay Balance and Systems Log (2026-02-28)

## Scope
- Player collision and damage-contact readability pass.
- Enemy speed/HP and spawn composition rebalance pass.
- Projectile continuation/effect-bullet damage policy pass.
- Obstacle navigation quality pass (spacing, shape, anti-stick).
- Localization parse and settings-label mojibake fixes.

## Data and Numeric Changes

### EnemyDefinitions (`Data/Director/EnemyDefinitions.csv`)
- `slime`: speed `96 -> 88`, hp unchanged (`4`).
- `orc`: hp `7 -> 6`, speed `132 -> 112`.
- `elite_orc`: hp `32 -> 26`, speed `64 -> 56`.
- `skeleton`: hp `11 -> 9`, speed `124 -> 104`.
- `skeleton_archer`: hp `10 -> 8`, speed `98 -> 82`.
- `werewolf`: hp `15 -> 12`, speed `164 -> 126`.
- `werebear`: hp `48 -> 36`, speed `119 -> 92`.
- `boss_lancer`: hp `120 -> 96`, speed `55 -> 46`.
- `boss_greatsword_skeleton`: hp `180 -> 140`, speed `74 -> 58`.

### Tier Weights (`Data/Director/TierEnemyWeights.csv`)
- Tier 2:
  - `slime 16 -> 4`
  - `orc 42 -> 34`
  - `skeleton 46 -> 42`
  - `elite_orc 38 -> 34`
  - `skeleton_archer 26 -> 30`
  - `werewolf 24 -> 30`
  - `werebear 14 -> 20`
- Tier 3:
  - `slime 6 -> 1`
  - `orc 24 -> 20`
  - `skeleton 34 -> 30`
  - `elite_orc 46 -> 44`
  - `skeleton_archer 34 -> 38`
  - `werewolf 42 -> 46`
  - `werebear 22 -> 28`

### Spawn/Director Numeric Tuning
- `SpawnSystem` miniboss scaling:
  - `PhaseMiniBossHpBase 120 -> 95`
  - `PhaseMiniBossHpStep 50 -> 35`
- New late-tier slime suppression:
  - `LateTierSlimeSuppressionStartTier = 2`
  - `LateTierSlimeWeightMultiplier = 0.20`
- `StabilitySystem` enemy speed multipliers:
  - `EnergyAnomalyEnemySpeedMultiplier 1.18 -> 1.08`
  - `StructuralFractureEnemySpeedMultiplier 1.35 -> 1.18`
  - `CollapseCriticalEnemySpeedMultiplier 1.65 -> 1.30`

### Player and Enemy Contact Geometry
- Player collider/hurtbox (`Scenes/Player.tscn`):
  - Body: `Circle radius 34 -> Rectangle 36x58` (vertical profile).
  - Hurtbox: `Circle radius 36 -> Rectangle 30x50`.
- Enemy contact rings reduced (`.tscn` hitbox/hurtbox):
  - Slime: `30/26 -> 25.5/22.1`
  - Orc: `27/22 -> 23/18.7`
  - EliteOrc: `38/34 -> 32.3/28.9`
  - Skeleton: `33/29 -> 28/24.6`
  - SkeletonArcher: `30/26 -> 25.5/22.1`
  - Werewolf: `35/29 -> 29.8/24.6`
  - Werebear: `33/29 -> 28/24.6`
  - Boss Lancer: `50/46 -> 42.5/39.1`
  - Boss Greatsword Skeleton: `50/46 -> 42.5/39.1`

### Progression Pickup Radius
- Added character-profile multiplier layer:
  - `RangedPickupRadiusMultiplier = 2.0` (new default).
  - `NonRangedPickupRadiusMultiplier = 1.0` (new default).
- Runtime radius now equals:
  - `pickup_card_multiplier * character_profile_multiplier`.

## Projectile and Card Runtime Behavior

### Effect Projectile Policy
- Added explicit effect-projectile runtime flags and base damage reference:
  - `baseDamageReference`
  - `effectProjectileBonusRatio` (default `0.30`)
  - `isEffectProjectile`
- Effect projectile classification now includes:
  - split-child projectiles,
  - pierce continuation state,
  - ricochet continuation state (including forward-continue fallback),
  - elemental burst transformed shot.
- Damage policy:
  - effect projectiles keep base damage and only keep `30%` of bonus damage above base.

### Arcane Tracking Compatibility
- Tracking state is preserved/forwarded to split children and continuation projectiles.
- No direct conflict found between homing and effect-projectile classification.

## Obstacle Gameplay Pass

### Collider Simplification
- Converted obstacle colliders from rectangle to circle and shrank radius:
  - BigRock `23`
  - BushSmallA `7`
  - BushSmallB `7`
  - RockTinyB `8`
  - RockTinyC `9`
  - SmallTree `17`
  - TreeSmallB `16`
  - TreeSmallC `17`

### Spacing and Navigation
- Added hard minimum obstacle spacing based on player height:
  - `MinObstacleSpacingPlayerHeights = 3.0`
  - fallback player collision height `58`.
- Spacing floor is applied in placement validation.

### Anti-Stick Bounce
- New runtime anti-stick response for both player and enemies:
  - if colliding with the same obstacle for more than `1` frame, apply repel.
- Defaults:
  - Player: distance `8`, speed `220`.
  - Enemy: distance `8`, speed `160`.
- Added shared helper:
  - obstacle collision detection,
  - repel direction resolve,
  - collider height estimate utility.

## Localization and Text Integrity

### Card Localization CSV Parse Fix
- `Data/Localization/Cards.csv` converted to robust CSV quoting.
- Fixes incorrect column splitting for descriptions containing commas.
- Regenerated translation outputs:
  - `Cards.en.translation`
  - `Cards.zh_TW.translation`

### Settings Language Label Fix
- Replaced mojibake script literal with Unicode-safe literal:
  - `\u7e41\u9ad4\u4e2d\u6587` for Traditional Chinese label.

## Scene Metadata Cleanup
- Removed debug/editor-only field from UI root:
  - `EditorFluxWallet = 999` removed from `Scenes/UI/GameFlowUIRoot.tscn`.

## Validation
- `dotnet build ProjectGenesis.sln`: pass.
- `Tools/Quality/Check-StructureHealth.ps1`: pass.
- `Tools/Quality/Check-SceneResourcePaths.ps1`: pass.
- BOM scan: pass.
