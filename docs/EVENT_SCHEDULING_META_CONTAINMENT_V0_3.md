# Event Scheduling & Meta Containment System (V0.3)
Last Synced: 2026-03-01

## 1. Intent
- Replace any random mid-run event choice with deterministic pre-run scheduling.
- Player commits to a 4-slot event loadout before the run.
- Run risk and run reward are pre-declared by that loadout.
- Difficulty control is time-driven, not abstract-pressure-driven.

Narrative premise:
- A final fragmented deity, `Order`, grants the player limited pre-run scheduling power.
- This is the lore reason why the player can preselect catastrophic event slots.

Core philosophy:
- The player does not react to random events.
- The player pre-schedules catastrophic events and bears the consequences.

Time model rule:
- "Pressure" means game-time intensity on the timeline.
- Do not use a separate `basePressureValue` balancing axis.
- Tune difficulty by slot timing, event duration, and tier time profiles.

Definitions:
- `Domain Shard` (神性碎片):
  - out-of-run meta currency, attributed by event domain.
  - not an in-run stat currency and not an equipment substitute.
  - used to purchase domain power and other containment progression systems.
  - narrative framing: spending shards revives remnant divine power for one more controlled calamity cycle.
- `Domain Power`:
  - out-of-run consumable stock per domain (`Ice`, `Spacetime`, `War`).
  - each loadout slot consumes `1` power of its final selected domain at run start.
  - purchasing one bundle grants `+3` power for that domain.
- `Talisman` (護符):
  - out-of-run meta unlock module family.
  - scope: event unlock access, event upgrade paths, and class progression unlocks.
  - must avoid generic in-run "+x% damage" style passive design.

## 2. Run Structure
Each run has 4 event slots.

| Slot | Tier | Phase |
|---|---|---|
| Slot1 | Tier0 | Early |
| Slot2 | Tier1 | Mid |
| Slot3 | Tier2 | Late |
| Slot4 | Tier3 | Final |

Timing notes:
- Exact timestamps are owned by `EventDirector`.
- Slot timing should remain phase-aligned with the 15:00 run timeline.

## 3. Domain Model (Current Version)
Current domains:
- `Ice`
- `Spacetime`
- `War`

Rules:
- Each `EventDefinition` belongs to exactly one domain.
- Current element domain is locked to `Ice`.
- Domain interaction is only evaluated between adjacent slots.
- `Order` is currently a narrative helper domain only and is not selectable in run event slots for V0.3.

## 4. Distortion (Same-Domain Consecutive Rule)
Validation rule:
- Maximum same-domain consecutive events is `2`.
- Valid: `A A B C`
- Invalid: `A A A B`

Distortion levels:

| Chain Position | Distortion |
|---|---|
| First in chain | D0 |
| Second consecutive same-domain | D1 |

Distortion multipliers:

| Distortion | Time Intensity | Reward |
|---|---:|---:|
| D0 | x1.00 | x1.00 |
| D1 | x1.30 | x1.50 |

Computation rule:
- Distortion is computed per slot during `RunPlan` creation.

## 5. Affinity (Cross-Domain Adjacent Interaction)
Affinity is evaluated only between adjacent slots.

Relationship types:
- `Resonance`
- `Neutral`
- `Dissonance`

Current affinity matrix:

| Domain A | Domain B | Relation |
|---|---|---|
| Ice | Spacetime | Resonance |
| Ice | War | Dissonance |
| Spacetime | War | Resonance |

Assumption:
- Matrix is symmetric.

Affinity effects for adjacent different-domain slots:

| Relation | Time Intensity | Reward | Hybrid Variant Tag |
|---|---:|---:|---|
| Resonance | x1.20 | x1.30 | 30% chance |
| Neutral | x1.00 | x1.00 | No |
| Dissonance | x0.85 | x0.80 | No |

## 6. Runtime Activation Flow
When an event activates:
1. Load base `EventDefinition`.
2. Apply tier time profile scaling.
3. Apply slot distortion time multiplier.
4. Resolve affinity with previous slot.
5. Apply affinity time and reward multipliers.
6. If relation is resonance, roll hybrid variant tag.
7. Execute `EventRunner` with final rule set.

Constraints:
- No global chain evaluation.
- Distortion and affinity are local-adjacent only.

## 7. Reward Formula
Per completed event:

```text
FinalShardReward =
BaseShard
* DistortionMultiplier
* AffinityMultiplier
* Purity
```

Purity rule:
- Current simplified range: `0.8 ~ 1.2`.

Domain attribution rule:
- Base rule for V0.3: event completion rewards are credited to the event's own domain wallet.
- If a hybrid variant is triggered by resonance:
  - V0.3 default remains single-domain credit (event domain).
  - Optional future rule can split by pair (for example 70/30), but must be explicitly documented if enabled.

## 8. Tier Contract
Tier expectations:

| Tier | Expected Time Intensity |
|---|---|
| Tier0 | Introduction tempo |
| Tier1 | Stable escalation |
| Tier2 | Combined intensity |
| Tier3 | Extreme / Final form |

Required `EventDefinition` fields:
- `id`
- `domain`
- `tierMask`
- `timeProfileId`
- `baseShardReward`
- `ruleSet`
- `possibleVariantTags`

Order rule:
- Resolved slot tier scaling applies before distortion/affinity multipliers.

## 9. Meta (Out-of-Run) Contract
Meta resources:
- `DomainShard[Ice]`
- `DomainShard[Spacetime]`
- `DomainShard[War]`

Storage:
- Persisted in save schema (`MetaSaveDto` and runtime state mirror).

Meta unlock progression can unlock:
- Domain power purchase entries and event access.
- Hybrid variants.
- Talismans.
- Future extension: allow 3 consecutive same-domain.
- Future extension: increase slot count.

Domain shard usage scope:
- Purchase domain power bundles in that domain (`+3` power per purchase).
- Unlock hybrid variant pools/tags for eligible domain pairs.
- Unlock talismans, event upgrade branches, class progression branches, and future system extensions.
- Do not use domain shards as a general-purpose in-run economy.

Transaction contract:
- Domain power purchase must go through `MetaProgressionService.TryPurchaseDomainPower(domainId, purchaseCount=1)`.
- One purchase grants exactly `3` power for target domain.
- Loadout consume at run start must go through `MetaProgressionService.TryConsumeDomainPower(domainId, amount=1)`.
- Hybrid unlock purchase must go through `MetaProgressionService.TryUnlockHybridVariant(variantId)`.
- Domain shard spend and persistence are atomic within the service transaction.

## 10. Pre-Run Loadout UI Contract
UI requirements:
- 4 slot selectors.
- Domain icon per slot.
- Distortion preview per slot.
- Affinity preview for adjacent pairs.
- Estimated total time intensity and reward preview.
- Domains with `0` remaining power must not be selectable in loadout reroll UI.
- On entering loadout, slots are auto-populated by random draw from currently available domain power.
- Player does not reroll freely; reroll is only triggered by selecting a domain and pressing `Change Calamity` for that slot.
- Tier is fixed by slot index (`Slot0..3` => `Tier0..3`); player only forces domain per slot.
- Forcing domain consumes tier `Order Sigil` at run start.
- Advanced events that are owned by progression (talisman/event unlock branch) must be part of that domain draw pool.
- Out-of-run must provide a dedicated `Event Purchase/Hybrid Unlock` panel before loadout:
  - shows current `Ice/Spacetime/War` shard wallet.
  - shows current domain power counts by domain.
  - allows click-to-buy domain power bundles (`+3` each purchase).
  - shows event list and short effect description for each domain.
  - updates loadout domain availability immediately after purchase.

## 11. Suggested Modules
- `EventDirector`
- `RunPlanBuilder`
- `DistortionResolver`
- `AffinityResolver`
- `EventRunner`
- `RewardService`
- `MetaProgressionService`
- `SaveDataSchema`

Current runtime status (2026-03-01):
- `EventDirector` slot-timestamp activation: implemented.
- `EventRunner` for initial 6 events: implemented (movement/range/projectile/pull/war hooks).
- `Resonance -> 30% Hybrid Variant Tag` runtime roll: implemented (`EventDirector`, unlock-gated by `MetaProgressionService.IsHybridVariantUnlocked`).
- Initial hybrid mechanical hooks (no dedicated VFX yet): implemented in `EventRunner` for `HYB_ICE_SPACE_GLACIAL_HORIZON` and `HYB_SPACE_WAR_WARP_ASSAULT`.
- Temporary in-run readability layer: implemented.
  - world telegraph overlay for active event influence zones/markers.
  - HUD event hint line + short hybrid trigger toast.
- `RewardService`: implemented (per-completed-event domain shard ledger, settled at run end through `MetaProgressionService.SettleRun`).
- Legacy `TryUnlockEvent` / `TryUnlockHybridVariant` transactions: implemented in current code.
- Domain power purchase (`+3`) and domain-power loadout consume: implemented.
- Loadout gate is domain-power based.
- Run-end panel per-domain shard breakdown (`Ice/Spacetime/War`): implemented.

## 12. Data Structures (Recommended)
```text
EventDefinition
  id: string
  domain: Domain
  tierMask: Tier[]
  timeProfileId: string
  baseShardReward: int
  ruleSet: EventRuleSet
  possibleVariantTags: string[]

EventChargeInventory
  eventId: string
  remainingCharges: int
  purchaseBundleSize: int (=3)

RunPlanSlot
  slotIndex: int
  eventId: string
  domain: Domain
  resolvedTier: Tier
  distortionLevel: DistortionLevel
  distortionTimeMultiplier: float
  distortionRewardMultiplier: float
  affinityWithPrevious: AffinityRelation
  affinityTimeMultiplier: float
  affinityRewardMultiplier: float
  hybridVariantEligible: bool
```

## 13. Validation Checklist
- Reject run plans with more than 2 consecutive same-domain slots.
- Distortion is deterministic for the same slot sequence.
- Affinity is evaluated only between slot `i-1` and `i`.
- Tier scaling order is deterministic and independent from affinity/distortion.
- Reward calculation is deterministic for fixed `Purity`.
- No abstract pressure scalar exists in event data; time profile is the balancing axis.
- Run cannot start if any selected slot event has remaining charge `< 1`.

## 14. Migration Notes
- Legacy random mid-run event selection is deprecated by this design.
- Legacy docs that describe "universe events removed" should be updated to:
  - deterministic pre-run loadout,
  - deterministic slot activation in-run.

## 15. Event Authoring Rule (Current Draft)
- All events in V0.3 affect both enemies and player.
- Event difficulty is authored by timeline parameters:
  - cadence,
  - duration,
  - concurrent object count,
  - area coverage/radius,
  - telegraph time.
- Distortion D1 should primarily strengthen one axis per event.
- All event randomness should use deterministic seed (`RunId + SlotIndex + EventId`).

## 16. Initial Event Catalog (3 Domains x 2 Events)

### 16.1 Ice Domain
#### `EVT_ICE_ICESTORM` (IceStorm)
- `domain`: `Ice`
- `tierMask`: `Tier0, Tier1, Tier2`
- `timeProfileId`: `ice_storm_field`
- `baseShardReward`: `18`
- `possibleVariantTags`: `ICE_DENSE_PATCH`, `ICE_LONG_GLIDE`
- D0 rule set:
  - Spawn random ice patches every `3.0s` during active window.
  - Ice patch lifetime `8.0s`.
  - Move speed multiplier in ice zone: `0.75` (player and enemies).
  - Max ice coverage ratio: `30%`.
- D1 override:
  - Max coverage ratio: `45%`.
  - Slide inertia multiplier: `1.35`.

#### `EVT_ICE_FROZEN_PULSE` (Frozen Pulse)
- `domain`: `Ice`
- `tierMask`: `Tier1, Tier2, Tier3`
- `timeProfileId`: `ice_center_ring`
- `baseShardReward`: `22`
- `possibleVariantTags`: `ICE_SPLIT_RING`, `ICE_FAST_FRONT`
- D0 rule set:
  - Release expanding ice ring from map center every `6.0s`.
  - Ring hit applies slow `30%` for `1.2s`.
  - Telegraph before pulse: `1.0s`.
- D1 override:
  - Pulse cadence changes to `4.5s`.

### 16.2 War Domain
#### `EVT_WAR_BLOOD_TIDE` (Blood Tide)
- `domain`: `War`
- `tierMask`: `Tier0, Tier1, Tier2`
- `timeProfileId`: `war_directional_tide`
- `baseShardReward`: `20`
- `possibleVariantTags`: `WAR_EDGE_LOCK`, `WAR_FOCUSED_ASSAULT`
- D0 rule set:
  - Determine one map edge as tide direction (seeded).
  - Spawn directional enemy tide every `8.0s`, total `3` waves.
  - Player gains `War Focus` during event:
    - damage multiplier `1.15`.
  - Edge warning telegraph: `1.0s`.
- D1 override:
  - Inject `1` elite per wave (event cap `3` elites).

#### `EVT_WAR_BERSERK_MARK` (Berserk Mark)
- `domain`: `War`
- `tierMask`: `Tier1, Tier2, Tier3`
- `timeProfileId`: `war_mark_cycle`
- `baseShardReward`: `24`
- `possibleVariantTags`: `WAR_CHAIN_FURY`, `WAR_BLOOD_ECHO`
- D0 rule set:
  - Mark player and a seeded subset of enemies with Berserk.
  - Berserk buff duration: `10.0s`.
  - Berserk modifiers (player and marked enemies):
    - move speed multiplier `1.20`,
    - attack speed multiplier `1.20`.
- D1 override:
  - Marked enemies explode on death:
    - explosion radius `72`,
    - fixed small AoE damage profile.

### 16.3 Spacetime Domain
#### `EVT_SPACE_EVENT_HORIZON` (Event Horizon)
- `domain`: `Spacetime`
- `tierMask`: `Tier2, Tier3`
- `timeProfileId`: `space_compress_zone`
- `baseShardReward`: `26`
- `possibleVariantTags`: `SPACE_SHEAR_DRIFT`, `SPACE_DOUBLE_EDGE`
- D0 rule set:
  - Spawn one compression zone.
  - Inside zone (player and enemies):
    - move speed multiplier `0.80`,
    - skill range multiplier `0.80`,
    - projectile range multiplier `0.75`.
  - Zone duration: `14.0s`.
- D1 override:
  - Compression zone drifts with fixed speed `40` units/s.

#### `EVT_SPACE_GRAVITY_WELL` (Gravity Well)
- `domain`: `Spacetime`
- `tierMask`: `Tier0, Tier1, Tier2, Tier3`
- `timeProfileId`: `space_pull_well`
- `baseShardReward`: `21`
- `possibleVariantTags`: `SPACE_TWIN_WELL`, `SPACE_PULSE_WELL`
- D0 rule set:
  - Spawn one gravity well at seeded position.
  - Well duration: `8.0s`.
  - Pull applies to player and enemies.
  - Counterplay: movement input can oppose pull and escape.
- D1 override:
  - Spawn `2` wells concurrently (radius unchanged).

## 17. Tier Time Profile Baseline (Recommended)
The following tier profile scales timeline intensity without introducing abstract pressure values:

| Resolved Tier | Active Window | Cadence Multiplier | Concurrent Cap Multiplier |
|---|---:|---:|---:|
| Tier0 | 35s | x1.00 | x1.00 |
| Tier1 | 42s | x0.90 | x1.10 |
| Tier2 | 48s | x0.80 | x1.25 |
| Tier3 | 55s | x0.72 | x1.40 |

Interpretation:
- Lower cadence multiplier means more frequent effects.
- Final event values = base rule set x tier profile x distortion/affinity multipliers.

## 18. Implementation Notes (First Pass)
- Pre-run loadout UI should preview for each slot:
  - domain,
  - D0/D1 state,
  - affinity relation with previous slot,
  - estimated shard reward.
- Runtime event banner should display:
  - event name,
  - domain icon,
  - distortion state (`D0`/`D1`),
  - relation marker (`Resonance`/`Neutral`/`Dissonance`).
