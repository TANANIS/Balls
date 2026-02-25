# Cards Changelog

## Usage Rule
- One entry per card change batch.
- Record only effective gameplay-facing changes.
- If numbers changed, always write old -> new.

## Entry Template
### YYYY-MM-DD - Batch Name
- Author:
- Scope: `Add` / `Update` / `Remove`
- Affected Layer(s): `Survival` / `CoreAttack` / `Subsystem` / `Modifier` / `Identity` / `Economy` / `MetaRules`
- Affected Pool Phase(s): `Early` / `Mid` / `Late`
- Summary:

#### Cards Added
- `CardId` | Layer | Rarity | Pool | Core Effect

#### Cards Updated
- `CardId`
  - Change:
  - Value: `old -> new`
  - Reason:

#### Cards Removed
- `CardId`
  - Reason:

#### Safety Fuse Notes
- StackLimit:
- Mutual Exclusion:
- Diminishing Return:
- Weight/Cost Escalation:

#### Balance Expectation
- Early-game impact:
- Mid-game impact:
- Late-game impact:
- Risk of dominant strategy:

#### Validation
- [ ] Catalog entries updated
- [ ] Runtime effect binding updated
- [ ] Pool routing checked
- [ ] In-run smoke test done

---

## Initial Baseline
### 2026-02-21 - Baseline Reset
- Scope: `Remove`
- Summary:
  - Cleared all runtime upgrade cards.
  - Cleared fallback options and default catalog entries.

### 2026-02-21 - Batch 01 Draft Cards
- Scope: `Add`
- Affected Layer(s): `CoreAttack`, `Survival`, `Economy`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Added first draft set of 11 cards for implementation.

#### Cards Added
- `ATK_SPEED_UP_15` | CoreAttack | TBD | Early/Mid/Late | Attack Speed +15%
- `ATK_COOLDOWN_DOWN_10` | CoreAttack | TBD | Early/Mid/Late | Cooldown -10%
- `ATK_PROJECTILE_PLUS_1` | CoreAttack | TBD | Early/Mid/Late | +1 Projectile
- `ATK_SPLIT_SHOT` | CoreAttack | TBD | Early/Mid/Late | Split Shot (MaxStack 2)
- `ATK_DAMAGE_UP_20` | CoreAttack | TBD | Early/Mid/Late | Damage +20%
- `ATK_CRIT_CHANCE_UP_10` | CoreAttack | TBD | Early/Mid/Late | Crit Chance +10%
- `SURV_MAX_HP_PLUS_1` | Survival | TBD | Early/Mid/Late | Max HP +1
- `SURV_SHIELD_COOLDOWN` | Survival | TBD | Early/Mid/Late | One-hit shield with cooldown
- `SURV_LIFESTEAL_CLOSE_KILL` | Survival | TBD | Early/Mid/Late | Chance to heal on kill
- `ECO_EXP_GAIN_UP_20` | Economy | TBD | Early | EXP Gain +20%
- `ECO_PICKUP_RADIUS_UP_25` | Economy | TBD | Early | Pickup Radius +25%

#### Safety Fuse Notes
- StackLimit: `ATK_SPLIT_SHOT` max 2.
- Mutual Exclusion: TBD.
- Diminishing Return: required for repeat multiplicative cards.
- Weight/Cost Escalation: TBD.

### 2026-02-21 - Batch 01 Round 1 Balance Baseline
- Scope: `Update`
- Affected Layer(s): `CoreAttack`, `Survival`, `Economy`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Added first playable balance baseline for all 11 cards.
  - Applied diminishing-return curves to repeated multiplicative upgrades.
  - Updated catalog `Weight` and `MaxStack` to match Round 1 tuning.

#### Cards Updated
- `ATK_SPEED_UP_15`
  - Change: stack curve
  - Value: `x0.87, x0.87, x0.87 -> x0.87, x0.89, x0.93`
  - Reason: reduce linear runaway from frequency stacking
- `ATK_COOLDOWN_DOWN_10`
  - Change: stack curve
  - Value: `x0.90, x0.90, x0.90 -> x0.90, x0.92, x0.94`
  - Reason: keep cooldown growth meaningful but bounded
- `ATK_DAMAGE_UP_20`
  - Change: stack curve
  - Value: `x1.20, x1.20, x1.20 -> x1.20, x1.15, x1.10`
  - Reason: enforce diminishing return on damage multiplier
- `ATK_CRIT_CHANCE_UP_10`
  - Change: stack curve
  - Value: `+10%, +10%, +10% -> +10%, +8%, +6%`
  - Reason: prevent late crit from dominating all builds
- `ECO_EXP_GAIN_UP_20`
  - Change: stack curve
  - Value: `x1.20, x1.20 -> x1.20, x1.15`
  - Reason: keep economy strong in early but avoid snowball
- `ECO_PICKUP_RADIUS_UP_25`
  - Change: stack curve
  - Value: `x1.25, x1.25 -> x1.25, x1.20`
  - Reason: preserve utility while limiting full-map vacuuming
- `SURV_MAX_HP_PLUS_1`
  - Change: max stack
  - Value: `3 -> 4`
  - Reason: allow one more safe fallback pick

#### Safety Fuse Notes
- StackLimit: unchanged (`ATK_SPLIT_SHOT` max 2; defensive utility mostly max 1).
- Mutual Exclusion: not applied in Round 1.
- Diminishing Return: enabled for speed/cooldown/damage/crit/economy multipliers.
- Weight/Cost Escalation: category bias system retained; explicit per-card decay pending.

### 2026-02-21 - Bilingual Card Localization Setup
- Scope: `Update`
- Affected Layer(s): `CoreAttack`, `Survival`, `Economy`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Added card localization keys (`TitleKey`, `DescriptionKey`) to upgrade definition.
  - Upgrade option build path now resolves localized card text first, then fallback text.
  - Added bilingual translation table for all 11 cards (`en`, `zh_TW`).

#### Validation
- [x] Catalog entries updated
- [x] Runtime effect binding updated
- [x] Pool routing checked
- [ ] In-run smoke test done

### 2026-02-25 - Split Projectile Prefab Separation (Shared Script Path)
- Scope: `Update`
- Affected Layer(s): `CoreAttack`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Introduced dedicated split-child projectile prefab while keeping shared projectile runtime script.
  - Added `SplitChildProjectileScene` selection path in `Bullet` so split spawn can use a variant prefab first.
  - Added scene-level speed override path (`RuntimeSpeedScale`) for projectile variants.
  - Split projectile now supports independent visuals/tuning without duplicating gameplay script.

#### Cards Updated
- `ATK_SPLIT_SHOT`
  - Change: split child spawn source
  - Value: `same projectile scene only -> prefer SplitChildProjectileScene prefab`
  - Reason: isolate split projectile visual/motion identity while retaining shared combat/VFX logic.

#### Safety Fuse Notes
- StackLimit: unchanged (`ATK_SPLIT_SHOT` max 4).
- Mutual Exclusion: unchanged (`ATK_PROJECTILE_PLUS_1` <-> `ATK_SPLIT_SHOT`).
- Diminishing Return: unchanged.
- Weight/Cost Escalation: unchanged.

#### Validation
- [x] Catalog entries updated
- [x] Runtime effect binding updated
- [x] Pool routing checked
- [ ] In-run smoke test done

### 2026-02-25 - Card Framework Pass (Layer/Phase Router/Validation)
- Scope: `Update`
- Affected Layer(s): `CoreAttack`, `Survival`, `Economy` (framework supports full layer set)
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Added explicit `UpgradeLayer` model with backward-compatible mapping from existing `UpgradeCategory`.
  - Added phase-based pool routing (`Early/Mid/Late`) with safety fallback when strict filter is disabled.
  - Added category weight decay mode (cost-increase style) for repeated same-category picks.
  - Added catalog integrity validation (duplicate IDs, invalid references, self prerequisite/exclusive checks).

#### Cards Updated
- `ALL (Batch 01 runtime cards)`
  - Change: selection framework
  - Value: `category-only pool -> layer + phase routed pool`
  - Reason: align runtime behavior with `docs/CARDS.md` framework contract.

#### Safety Fuse Notes
- StackLimit: runtime stack cap remains enforced by `MaxStack`.
- Mutual Exclusion: runtime `ExclusiveWith` check remains active; validation now warns missing refs.
- Diminishing Return: unchanged from Round 1 curves.
- Weight/Cost Escalation: category weight decay model added (`UseCategoryWeightDecay`, floor/step tunables).

#### Validation
- [x] Catalog entries updated
- [x] Runtime effect binding updated
- [x] Pool routing checked
- [x] In-run smoke test done

### 2026-02-25 - Cooldown Card Removal + Survival Gating Rules
- Scope: `Update` + `Remove`
- Affected Layer(s): `CoreAttack`, `Survival`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Removed `ATK_COOLDOWN_DOWN_10` from runtime catalog and localization.
  - Clarified dual-axis card contract: `Layer` drives phase pool routing; `Category` drives decay/statistics.
  - Added timing gate fields to card definition (`MinUpgradeCount`, `MinPhase`, optional `MaxPhase`).
  - Applied survival timing gates:
    - `SURV_SHIELD_COOLDOWN`: unlock at `Mid` or `pick>=4`
    - `SURV_LIFESTEAL_CLOSE_KILL`: unlock at `Late` or `pick>=8`
  - Corrected Early pool ratio to 100% (`Modifier 10% -> 5%`).

#### Cards Removed
- `ATK_COOLDOWN_DOWN_10`
  - Reason: highly overlapping growth axis with attack-speed card; reduced build differentiation.

#### Cards Updated
- `SURV_SHIELD_COOLDOWN`
  - Change: timing gate
  - Value: `none -> MinPhase=Mid OR MinUpgradeCount=4`
  - Reason: prevent early no-brainer defensive spike.
- `SURV_LIFESTEAL_CLOSE_KILL`
  - Change: timing gate
  - Value: `none -> MinPhase=Late OR MinUpgradeCount=8`
  - Reason: reserve sustain spike for later build maturity.

#### Safety Fuse Notes
- StackLimit: unchanged (`MaxStack` contract remains active).
- Mutual Exclusion: unchanged.
- Diminishing Return: unchanged for remaining multiplicative cards.
- Weight/Cost Escalation: unchanged (category decay model stays enabled).

#### Validation
- [x] Catalog entries updated
- [x] Runtime effect binding updated
- [x] Pool routing checked
- [ ] In-run smoke test done

### 2026-02-25 - Projectile Identity Split (On-Hit Split + Mutual Exclusion)
- Scope: `Update`
- Affected Layer(s): `CoreAttack`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Reworked `ATK_SPLIT_SHOT` from fire-time side-angle volley into on-hit split behavior.
  - Added non-chain guard: split child projectiles do not split again.
  - Tightened `ATK_PROJECTILE_PLUS_1` identity to same-axis narrow spread for single-target pressure.
  - Applied mutual exclusion between `ATK_PROJECTILE_PLUS_1` and `ATK_SPLIT_SHOT`.

#### Cards Updated
- `ATK_PROJECTILE_PLUS_1`
  - Change: role + weight + rarity + exclusive rule
  - Value: `generic +1 projectile / weight 9 / common -> same-axis tight spread / weight 6 / rare`
  - Reason: preserve elite-focus identity and reduce overlap with split archetype.
- `ATK_SPLIT_SHOT`
  - Change: trigger model + effect text + exclusive rule
  - Value: `fire-time side-angle shots -> on-hit split from enemy position (3->4->5->6; max=6 radial 360°; child 50% damage; non-chain)`
  - Reason: make it a true crowd-clear branch and avoid same-pattern growth overlap.

#### Safety Fuse Notes
- StackLimit: updated (`ATK_PROJECTILE_PLUS_1` max 2, `ATK_SPLIT_SHOT` max 4).
- Mutual Exclusion: added between `ATK_PROJECTILE_PLUS_1` and `ATK_SPLIT_SHOT` (runtime + catalog).
- Diminishing Return: unchanged for other multiplicative cards.
- Weight/Cost Escalation: unchanged (category decay model remains active).

#### Validation
- [x] Catalog entries updated
- [x] Runtime effect binding updated
- [x] Pool routing checked
- [ ] In-run smoke test done

### 2026-02-25 - Split Projectile Visual/Hit Sync Pass
- Scope: `Update`
- Affected Layer(s): `CoreAttack`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Switched split-child projectile visuals to dedicated 7-frame animation set (`SPLITBULLET`).
  - Separated flight and impact frame windows to reduce "already hit-looking while still flying" mismatch.
  - Tuned split-child visual/collider scale to match intended relative size (about 2/3 of primary projectile).
  - Removed global split-child hit-arm delay; kept short same-target ignore window only.

#### Cards Updated
- `ATK_SPLIT_SHOT`
  - Change: split child visual + hit timing sync
  - Value: `single-frame split texture -> 7-frame split animation`; `Flight 0..6 -> 0..5`, `Impact 6 -> 6`; `scale 3.33 -> 1.33`; `collider radius 12 -> 9`; `global hit-arm delay 0.05s -> 0.00s`
  - Reason: align hit readability, prevent perceived no-damage hits, and keep split projectile visually smaller than primary shots.

#### Safety Fuse Notes
- StackLimit: unchanged (`ATK_SPLIT_SHOT` max 4).
- Mutual Exclusion: unchanged (`ATK_PROJECTILE_PLUS_1` <-> `ATK_SPLIT_SHOT`).
- Diminishing Return: unchanged.
- Weight/Cost Escalation: unchanged.

#### Validation
- [x] Catalog entries updated
- [x] Runtime effect binding updated
- [x] Pool routing checked
- [ ] In-run smoke test done

### 2026-02-25 - New Card: MOD_ELEMENTAL_BURST
- Scope: `Add`
- Affected Layer(s): `Modifier`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Added new modifier card `MOD_ELEMENTAL_BURST` (`元素引爆`).
  - Runtime behavior:
    - charges every `5s`,
    - charge is retained if player does not fire,
    - charged state converts only the next single projectile to explosive round,
    - explosive round detonates once on first hit or when max travel distance is reached,
    - after detonation, charge cooldown restarts.
  - Explosion baseline:
    - radius `130`,
    - damage scale `1.20x` of triggering projectile final damage path,
    - max travel distance `280`,
    - max targets `5`,
    - no stack (`MaxStack=1`).
  - Offer tuning:
    - `MinPhase: Mid -> Early`
    - `Weight: 6 -> 8`

#### Cards Added
- `MOD_ELEMENTAL_BURST` | Modifier | Rare | Early/Mid/Late | 5s charge -> next shot AoE detonation

#### Safety Fuse Notes
- StackLimit: `MOD_ELEMENTAL_BURST` max 1.
- Mutual Exclusion: none.
- Diminishing Return: not applicable (single-stack proc card).
- Weight/Cost Escalation: category decay still applies via `Category`.

#### Validation
- [x] Catalog entries updated
- [x] Runtime effect binding updated
- [x] Pool routing checked
- [ ] In-run smoke test done

### 2026-02-25 - MOD_ELEMENTAL_BURST Visual Import (Projectile + Rune)
- Scope: `Update`
- Affected Layer(s): `Modifier`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Imported dedicated elemental burst projectile animation frames (`1..8`).
  - Imported explosion animation frames (`explotion1..5`) and bound them as detonation VFX.
  - Added fallback rune texture (`explotion.png`) for environments where explosion frames are unavailable.
  - Clarified runtime presentation split:
    - charged projectile frame flow handles flight/impact,
    - explosion area VFX is spawned independently at detonation center,
    - fade starts on the final explosion frame so frame 5 acts as disappear frame.

#### Cards Updated
- `MOD_ELEMENTAL_BURST`
  - Change: visual presentation
  - Value: `default projectile visuals -> dedicated elemental frames + explosion animation with rune fallback`
  - Reason: keep projectile readability and area explosion feedback clear.

#### Validation
- [x] Runtime effect binding updated
- [ ] In-run smoke test done

### 2026-02-25 - MOD_ELEMENTAL_BURST Audio Hook
- Scope: `Update`
- Affected Layer(s): `Modifier`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Added dedicated detonation SFX for Elemental Burst.
  - Wired audio load/play path through `AudioManager` and bullet detonation trigger.

#### Cards Updated
- `MOD_ELEMENTAL_BURST`
  - Change: detonation feedback
  - Value: `no dedicated detonation clip -> sfx_player_elemental_burst.wav on explode`
  - Reason: improve proc readability and impact feedback.

#### Validation
- [x] Runtime effect binding updated
- [ ] In-run smoke test done

### 2026-02-25 - Fantasy Text Pass (UI + Character + Card Display Copy)
- Scope: `Update`
- Affected Layer(s): `All`
- Affected Pool Phase(s): `Early`, `Mid`, `Late`
- Summary:
  - Rewrote start-menu and key runtime UI copy to remove legacy sci-fi phrasing and align with fantasy tone.
  - Updated character display names/descriptions and fallback definitions:
    - `Ranger Core -> Mage` (`遊俠核心 -> 法師`)
    - refreshed Melee/Tank zh_TW descriptions from placeholder text to final copy.
  - Updated card display titles/descriptions (without changing gameplay ids/mechanics):
    - `Split Shot -> Shatter Shot`
    - `+1 Projectile -> +1 Arc Bolt`
    - `EXP Gain -> Essence Gain`
    - `Shield -> Ward Shield`
    - `Kill-Chance Lifesteal -> Blood Rite`
  - UI term harmonization examples:
    - `XP -> Essence`
    - `CHOOSE UPGRADE -> CHOOSE A BOON`
    - failure/collapse labels rewritten to dark-fantasy wording.

#### Validation
- [x] Localization source updated (`UI.csv`, `Cards.csv`)
- [x] Fallback text paths updated (`GameFlowUI*`, character resources)
- [x] Build check passed
