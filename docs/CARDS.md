# Cards Spec

## Current Status
- Runtime card pool is active with Batch 01 (11 cards).
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
- Cooldown
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
- Modifier: 10%
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
- `Layer` (`Survival`, `CoreAttack`, `Subsystem`, `Modifier`, `Identity`, `Economy`, `MetaRules`)
- `Rarity`
- `Weight`
- `MaxStack`
- `Prerequisites`
- `ExclusiveWith`
- `CharacterGate` (optional; for identity layer)
- `UnlockCondition` (optional; meta progression)

## Batch 01 Draft Cards

### Core Attack - Frequency
- `ATK_SPEED_UP_15` : Attack Speed +15%
- `ATK_COOLDOWN_DOWN_10` : Cooldown -10%

### Core Attack - Quantity
- `ATK_PROJECTILE_PLUS_1` : +1 Projectile
- `ATK_SPLIT_SHOT` : Split Shot (`MaxStack = 2`)

### Core Attack - Power
- `ATK_DAMAGE_UP_20` : Damage +20%
- `ATK_CRIT_CHANCE_UP_10` : Crit Chance +10%

### Survival
- `SURV_MAX_HP_PLUS_1` : Max HP +1
- `SURV_SHIELD_COOLDOWN` : Shield (absorb one hit, cooldown-based)
- `SURV_LIFESTEAL_CLOSE_KILL` : Conditional Lifesteal (25% chance to heal 1 HP on kill)

### Economy
- `ECO_EXP_GAIN_UP_20` : EXP Gain +20%
- `ECO_PICKUP_RADIUS_UP_25` : Pickup Radius +25%

## Round 1 Balance Table (Playable Baseline)

This table is the first practical pass for in-run balancing.

| CardId | Layer | Base Effect (Stack 1) | Diminishing Curve | MaxStack | Base Weight |
|---|---|---|---|---:|---:|
| `ATK_SPEED_UP_15` | CoreAttack | Attack interval x`0.87` (~+15% rate) | x`0.89` (S2), x`0.93` (S3) | 3 | 14 |
| `ATK_COOLDOWN_DOWN_10` | CoreAttack | Cooldown x`0.90` | x`0.92` (S2), x`0.94` (S3) | 3 | 13 |
| `ATK_PROJECTILE_PLUS_1` | CoreAttack | `+1` projectile | linear | 2 | 9 |
| `ATK_SPLIT_SHOT` | CoreAttack | split level `+1` | linear (hard-capped) | 2 | 7 |
| `ATK_DAMAGE_UP_20` | CoreAttack | Damage x`1.20` | x`1.15` (S2), x`1.10` (S3) | 3 | 12 |
| `ATK_CRIT_CHANCE_UP_10` | CoreAttack | Crit chance `+10%` | `+8%` (S2), `+6%` (S3) | 3 | 8 |
| `SURV_MAX_HP_PLUS_1` | Survival | Max HP `+1` | linear | 4 | 12 |
| `SURV_SHIELD_COOLDOWN` | Survival | 1-hit shield, 60s cooldown | no stack | 1 | 8 |
| `SURV_LIFESTEAL_CLOSE_KILL` | Survival | On kill: 25% chance heal 1 HP | no stack | 1 | 7 |
| `ECO_EXP_GAIN_UP_20` | Economy | EXP gain x`1.20` | x`1.15` (S2) | 2 | 8 |
| `ECO_PICKUP_RADIUS_UP_25` | Economy | Pickup radius x`1.25` | x`1.20` (S2) | 2 | 8 |

### Derived Ceiling Snapshot (Round 1)
- `ATK_SPEED_UP_15` total rate multiplier at 3 stacks: about `1.39x`.
- `ATK_COOLDOWN_DOWN_10` total rate multiplier at 3 stacks: about `1.28x`.
- `ATK_DAMAGE_UP_20` total damage multiplier at 3 stacks: about `1.52x`.
- `ATK_CRIT_CHANCE_UP_10` expected DPS multiplier at 3 stacks (crit x1.5): about `1.12x`.

Use this snapshot as first-pass tuning anchors for playtests.

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
  - Keep `Title` / `Description` as fallback debug text

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

## Next Implementation Checklist
- [ ] Implement layer enum and map existing category model
- [ ] Add phase-based pool router (Early/Mid/Late)
- [ ] Add stack-limit and mutual-exclusion validation
- [ ] Add category weight decay (cost-increase model)
- [ ] Define first batch cards for each layer
