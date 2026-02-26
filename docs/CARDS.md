# Cards Spec

## Current Status
- Runtime card pool is active with Batch 01 (13 cards).
- Card effects are bound in `UpgradeSystem` and `ProgressionSystem`.

## Document Purpose
- Define the structural design of the new card system.
- Keep card design, progression pacing, and balance constraints in one source of truth.

## Card System Layers

### 1) Survival Layer
Example cards:
- HP +1
- Max HP %
- Shield
- Damage Reduction
- Invulnerability Duration Up

Design intent:
- Survival is for recovering mistakes, not replacing gameplay decisions.

### 2) Core Attack Layer
Example cards:
- Attack Speed
- Projectile Count
- Damage
- Split Shot
- Ricochet
- Multi Shot

Design intent:
- Main source of build identity and DPS growth.

### 3) Subsystem Layer
Example cards:
- Orbit Weapon
- Passive Turret
- Auto Homing Unit
- Ground Area Device

Design intent:
- Add parallel damage channels and map-control options.

### 4) Modifier Layer
Example cards:
- Freeze
- Paralysis
- Burn
- Knockback
- Sleep

Design intent:
- Add control/utility hooks that modify combat rhythm.

### 5) Character Identity Layer
Rules:
- Not part of the general random pool.
- Only offered during that specific character's progression.
- Unlocked by meta-progression requirements.

Design intent:
- Preserve role identity while keeping base pool shared.

### 6) Economy Layer
Example cards:
- EXP Gain Up
- Pickup Radius Up
- Bonus On Elite Kill
- Chain Level-Up Efficiency

Design intent:
- Control growth speed and resource conversion efficiency.
- Should alter run tempo, not replace combat decision-making.

### 7) Meta Rules / Director Interaction Layer
Example cards:
- Enemy Spawn Delay Window
- Elite Frequency Modifier
- Tier Tail Soften/Intensify
- Boss Preparation Grace

Design intent:
- Interact with pacing/director rules at a systems level.
- Must be tightly constrained to avoid breaking encounter readability.

## Upgrade Pools By Run Phase

### Early Pool
- Survival: 40%
- Core Attack: 40%
- Subsystem: 10%
- Modifier: 5%
- Economy: 5%
- Meta Rules / Director Interaction: 0%

### Mid Pool
- Survival: 20%
- Core Attack: 40%
- Subsystem: 25%
- Modifier: 15%
- Economy: 0%
- Meta Rules / Director Interaction: 0%

### Late Pool
- Survival: 10%
- Core Attack: 30%
- Subsystem: 30%
- Modifier: 30%
- Economy: 0%
- Meta Rules / Director Interaction: 0%

Note:
- Economy and Meta Rules layers are special layers.
- They are not in default random pool unless explicitly enabled per phase/build policy.

## Multiplicative Safety Fuses
- Same-category multiplicative cards must have `StackLimit`.
  - Example: Split max 2, Ricochet max 1.
- High-impact same-slot cards can use mutual exclusion.
  - Example: advanced Split and advanced Ricochet cannot coexist.
- Repeated picks can apply increasing acquisition cost.
  - Example: after each pick, reduce the weight of that same category.
- Diminishing Return curve is mandatory for core multiplicative stats.
  - Example (Attack Speed multiplier): `1.0 -> 1.3 -> 1.5 -> 1.6`.
  - Rule: do not use linear growth for repeated multiplicative gains.

## Survival Layer Constraints
- Avoid early no-brainer defense stacking (`Shield + Lifesteal + Damage Reduction`).
- Strong survival cards must use timing gates:
  - Gate fields: `MinUpgradeCount`, `MinPhase`, optional `MaxPhase`.
  - Gate rule (runtime): if both `MinUpgradeCount` and `MinPhase` are set, either one can unlock the card; `MaxPhase` limits late availability when enabled.
  - Current examples: `Shield` opens at `Mid` or `pick>=4`; `Lifesteal` opens at `Late` or `pick>=8`.
- Survival cards should have cost/condition:
  - Lifesteal triggers by kill chance (not guaranteed sustain).
  - Shield requires movement or no-hit maintenance.
  - Damage reduction trades off output or pickup radius.
- Rule: Survival must repair error tolerance, not become a dominant default build.

## Data Contract (Implementation-Oriented)
- `Id`
- `Title`
- `Description`
- `TitleKey` (for localization; e.g. `CARD.XXX.TITLE`)
- `DescriptionKey` (for localization; e.g. `CARD.XXX.DESC`)
- `Layer` (`Survival`, `CoreAttack`, `Subsystem`, `Modifier`, `Identity`, `Economy`, `MetaRules`) for phase-pool routing only
- `Category` (selection analytics + weight decay axis; independent from `Layer`)
- `Rarity`
- `Weight`
- `MaxStack`
- `MinUpgradeCount` (optional gate)
- `MinPhase` (optional gate)
- `UseMaxPhaseGate` (optional gate toggle)
- `MaxPhase` (optional gate; active only when `UseMaxPhaseGate=true`)
- `Prerequisites`
- `ExclusiveWith`
- `CharacterGate` (optional; for identity layer)
- `UnlockCondition` (optional; meta progression)

## Batch 01 Draft Cards

### Core Attack - Frequency
- `ATK_SPEED_UP_15` : Combat Tempo +15%

### Core Attack - Quantity
- `ATK_PROJECTILE_PLUS_1` : Skill Split (same-axis +1 split shot, wider spread, `MaxStack = 2`)
- `ATK_SPLIT_SHOT` : Shatter Shot (`MaxStack = 4`, split count `3->4->5->6`, max stack `360 deg`, child damage `50%`, non-chain)

### Core Attack - Power
- `ATK_DAMAGE_UP_20` : Damage +10%

### Survival
- `SURV_MAX_HP_PLUS_1` : Max HP +1
- `SURV_SHIELD_COOLDOWN` : Ward Shield (absorb one hit, cooldown-based)
- `SURV_LIFESTEAL_CLOSE_KILL` : Blood Rite (12% chance to heal 1 HP on kill)

### Economy
- `ECO_EXP_GAIN_UP_20` : Essence Gain +20%
- `ECO_PICKUP_RADIUS_UP_25` : Essence Reach +25%

### Modifier
- `MOD_ELEMENTAL_BURST` : Elemental Burst (charges every 5s; next shot explodes on first hit or max distance)
- `MOD_ARCANE_TRACKING` : Arcane Tracking (ranged shots seek nearest enemy ahead after launch)
- `MOD_PIERCE` : Piercing Sigil (extra penetration count `1 -> 2 -> 3`, `MaxStack = 3`)
- `MOD_RICOCHET` : Rebound Sigil (on-hit bounce count `1 -> 2 -> 3`, `MaxStack = 3`)

Display-name note:
- Card balancing remains keyed by `Id` (stable), while runtime `Title`/`Description` strings may change for theme/lore direction.

## Round 1 Balance Table (Playable Baseline)

This table is the first practical pass for in-run balancing.

| CardId | Layer | Base Effect (Stack 1) | Diminishing Curve | MaxStack | Base Weight |
|---|---|---|---|---:|---:|
| `ATK_SPEED_UP_15` | CoreAttack | Attack interval x`0.87` (~+15% rate) | x`0.89` (S2), x`0.93` (S3) | 3 | 14 |
| `ATK_PROJECTILE_PLUS_1` | CoreAttack | `+1` same-axis split shot (wider spread) | linear | 2 | 6 |
| `ATK_SPLIT_SHOT` | CoreAttack | on-hit split from enemy pos: `3->4->5->6` (max stack `360 deg`, child x`0.50`, non-chain) | linear (hard-capped, stack increases split count) | 4 | 7 |
| `ATK_DAMAGE_UP_20` | CoreAttack | Damage x`1.10` | x`1.08` (S2), x`1.06` (S3) | 3 | 12 |
| `SURV_MAX_HP_PLUS_1` | Survival | Max HP `+1` | linear | 4 | 12 |
| `SURV_SHIELD_COOLDOWN` | Survival | 1-hit shield, 60s cooldown | no stack | 1 | 8 |
| `SURV_LIFESTEAL_CLOSE_KILL` | Survival | On kill: 12% chance heal 1 HP | no stack | 1 | 7 |
| `ECO_EXP_GAIN_UP_20` | Economy | Essence gain x`1.20` | x`1.15` (S2) | 2 | 8 |
| `ECO_PICKUP_RADIUS_UP_25` | Economy | Essence pickup radius x`1.25` | x`1.20` (S2) | 2 | 8 |
| `MOD_ELEMENTAL_BURST` | Modifier | every `5s` charge next shot into explosive round (first hit or max distance, radius `130`, damage x`1.20`, target cap `5`) | no stack | 1 | 8 |
| `MOD_ARCANE_TRACKING` | Modifier | ranged shots gain homing (`turn rate 620`, forward target filter) | no stack | 1 | 6 |
| `MOD_PIERCE` | Modifier | extra penetration `+1` per stack | linear | 3 | 7 |
| `MOD_RICOCHET` | Modifier | extra ricochet `+1` per stack | linear | 3 | 6 |

### Derived Ceiling Snapshot (Round 1)
- `ATK_SPEED_UP_15` total rate multiplier at 3 stacks: about `1.39x`.
- `ATK_DAMAGE_UP_20` total damage multiplier at 3 stacks: about `1.26x`.

Use this snapshot as first-pass tuning anchors for playtests.

Additional rule:
- `ATK_PROJECTILE_PLUS_1` and `ATK_SPLIT_SHOT` are mutually exclusive archetypes (single-target line pressure vs crowd-clear branching).

## Bilingual Workflow (zh_TW / en)
- Runtime source:
  - Card runtime text resolves from `TitleKey` / `DescriptionKey`.
  - If key is missing, fallback uses `Title` / `Description`.
- Translation table:
  - `Data/Localization/Cards.csv`
  - Columns: `keys`, `en`, `zh_TW`
- Project registration:
  - `project.godot` -> `[internationalization] locale/translations`
- New card checklist:
  - Add stable key pair in catalog (`CARD.<ID>.TITLE`, `CARD.<ID>.DESC`)
  - Add `en` and `zh_TW` rows in `Cards.csv`

## Skill VFX Asset Convention (Current)
- Base path: `Assets/Sprites/Skills/<SkillName>/`
- File naming:
  - Main sprite: lowercase, explicit purpose (example: `shield.png`)
  - Keep one `.import` generated by Godot next to each source texture
- Runtime binding rules:
  - Prefer exported `Texture2D` field for per-character/per-scene override
  - Keep a deterministic fallback `res://Assets/Sprites/Skills/<SkillName>/<file>.png`
  - Prefer `Player.GetSkillVfxRoot()` as the runtime attach point for player-side skill visuals
  - Do not hardcode non-skill VFX paths in gameplay systems
- Current implemented example:
  - Skill: `SURV_SHIELD_COOLDOWN`
  - Runtime: `Scripts/Player/PlayerHealth.cs` (`ShieldTexture` + fallback load)
  - Asset: `Assets/Sprites/Skills/Shield/shield.png`
  - Behavior states:
    - Ready: visible, ready color
    - Cooldown: dim/blink near cooldown end
    - Hit consume: short white flash, then hidden until cooldown completes

## Projectile Variant Convention (Current)
- Runtime script stays shared:
  - `Scripts/Projectiles/Bullet.cs` is the common behavior host (hit, damage request, split, effect animation).
- Variant control uses prefab split (not script split):
  - Primary projectile: existing character projectile prefabs (`WizardProjectile.tscn` / `PriestProjectile.tscn`).
  - Split projectile: `Prefabs/SplitProjectile.tscn`.
- Split generation rule:
  - `Bullet.SplitChildProjectileScene` is preferred for split child spawn.
  - Fallback order: `SplitChildProjectileScene -> source projectile scene -> current scene file`.
- Override policy:
  - Keep script logic unified.
  - Put variant-specific visuals and runtime tuning in prefab exports (`RuntimeSpeedScale`, effect textures/frames, collider size, etc.).
- Current split projectile baseline (`Prefabs/SplitProjectile.tscn`):
  - Visual frames: `Assets/Sprites/Projectiles/Split/Frames/split_bullet_01.png` ~ `split_bullet_07.png`.
  - Flight/impact frame split: `Flight 0..5`, `Impact 6`.
  - Relative size target: split child visual scale should stay around primary projectile `2/3` (`1.33` vs primary `2.0`).
  - Collider baseline: `CircleShape2D radius = 9` (kept close to visible silhouette).
  - Hit timing rule: split child has no global hit-arm delay; it only ignores the original hit target for a short window to avoid immediate same-target re-hit.
- Current elemental burst visual baseline (`MOD_ELEMENTAL_BURST` card):
  - Projectile animation frames: `Assets/Sprites/Projectiles/ElementalBurst/Projectile/elemental_burst_charge_01.png` ~ `elemental_burst_charge_08.png`.
  - Runtime frame rule for charged shot: `Flight 0..6`, `Impact 7`.
  - Explosion VFX prefers frame animation:
    - `Assets/Sprites/Projectiles/ElementalBurst/Explosion/elemental_burst_explosion_01.png` ~ `elemental_burst_explosion_05.png`
    - spawned at detonation center, scaled to explosion radius
    - normal playback timing (no extra hold on final frame); auto-cleanup after animation duration
  - Explosion VFX fallback:
    - first explosion frame fallback: `Assets/Sprites/Projectiles/ElementalBurst/Explosion/elemental_burst_explosion_01.png`
  - SFX:
    - `Assets/Sound/Player/sfx_player_elemental_burst.wav` (plays on detonation)
- Benefit:
  - Split projectiles can run slower and use dedicated art.
  - Future projectile-wide VFX/system hooks remain shared because all variants still run `Bullet.cs`.

## Next Implementation Checklist
- [x] Implement dual-axis contract (`Layer` for phase routing + `Category` for decay/statistics)
- [x] Add phase-based pool router (Early/Mid/Late)
- [x] Add stack-limit and mutual-exclusion validation
- [x] Add category weight decay (cost-increase model)
- [ ] Add deterministic draw simulator (editor/console, fixed seed, 10,000 runs, N picks/run) with report outputs for card rate, pair/triple combos, ceiling-combo probability, and phase survival ratio validation (target error <= 2%).
- [ ] Define first batch cards for each layer

