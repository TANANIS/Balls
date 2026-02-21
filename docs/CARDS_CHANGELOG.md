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
