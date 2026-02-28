# Meta Progression Implementation Plan (Phase 1-6)
Last Synced: 2026-02-28


## Scope
- Convert `docs/META_PROGRESSION_ARCHITECTURE.md` into concrete implementation steps.
- Keep architecture clean while minimizing early over-abstraction.
- Include Event Scheduling V0.3 economy dependencies:
  - domain shard wallet,
  - event charge purchases,
  - hybrid variant unlocks,
  - pre-run loadout gating.

## Phase 1 - Domain + Persistence Foundation
### Goal
- Establish state model and save/load pipeline without gameplay coupling.

### Files
- `Scripts/Meta/MetaProgressionState.cs`
- `Scripts/Meta/CharacterProgress.cs`
- `Scripts/Meta/MetaFlags.cs`
- `Scripts/Save/MetaSaveDto.cs`
- `Scripts/Save/JsonSaveStore.cs`
- `Scripts/Save/SaveMigrator.cs`

### Deliverables
- Runtime domain state exists and is independent from DTO.
- Save DTO versioning exists (`Version` field).
- Load path supports migration for missing/new fields.
- Service can bootstrap state from disk and save state back.

### Validation
- Round-trip test (load -> mutate -> save -> reload) preserves wallet/unlocks/levels/nodes.
- Missing file path initializes default empty state.

## Phase 2 - Economy + Settlement
### Goal
- Implement score-to-currency conversion with soft cap and breakdown.

### Files
- `Scripts/Meta/RunResult.cs`
- `Scripts/Meta/RewardBreakdown.cs`
- `Scripts/Meta/EconomyTuning.cs`
- `Scripts/Meta/PowerCurve.cs`
- `Scripts/Meta/RewardCalculator.cs`

### Deliverables
- `base = floor(score / scoreDivisor)` pipeline implemented.
- Soft-cap curve with optional linear tail implemented.
- Anti-duplicate settlement key (`RunId`) respected.

### Validation
- Deterministic reward for fixed inputs.
- High score returns monotonic increase with diminishing marginal gain.
- Duplicate settlement with same `RunId` is ignored.

## Phase 3 - Meta Transaction Service + Character Unlock Gate
### Goal
- Introduce single transaction entry and wire first integration point.

### Files
- `Scripts/Meta/MetaProgressionService.cs`
- `Scripts/UI/GameFlowUI.EndState.cs` (settlement trigger)
- `Scripts/UI/GameFlowUI.CharacterSelect.cs` (unlock gate)
- `Scripts/Runtime/RunContext.cs` (selection guard)

### Deliverables
- Transaction methods:
  - `SettleRun`
  - `TryUnlockCharacter`
  - query methods for unlock state
- End-state settlement is called once per run.
- Character select UI shows locked/unlocked state and blocks invalid selection.
- `RunContext` refuses locked character IDs.

### Validation
- Run end increases currency once.
- Locked character cannot be selected or started.
- Unlocked character is selectable across scene reload.

## Phase 4 - Character Level + Ability Tree
### Goal
- Complete A/B/C progression targets with definitions and spending flow.

### Files
- `Scripts/Defs/ProgressionDefs.cs`
- `Scripts/Defs/Models/CharacterDef.cs`
- `Scripts/Defs/Models/AbilityNodeDef.cs`
- `Scripts/Meta/MetaProgressionService.cs` (level/tree transactions)
- related UI panels (character progression display + purchase actions)

### Deliverables
- Character level-up transaction + cost curve.
- Ability tree unlock transaction with prerequisite checks.
- Currency spend tracking and rejection path on insufficient funds.
- Def-driven content with static source (replaceable later by JSON/Resource).

### Validation
- Level-up applies only when enough currency and requirements met.
- Ability node unlock enforces parent/prerequisite rules.
- Currency never becomes negative.

## Phase 5 - Event Economy + Loadout Gate
### Goal
- Bridge meta progression and pre-run event scheduling.

### Files
- `Scripts/Meta/MetaProgressionState.cs` (domain shards + event charge inventory)
- `Scripts/Meta/MetaProgressionService.cs` (event transactions/query)
- `Scripts/Meta/RunResult.cs` (event reward settlement payload)
- `Scripts/Save/MetaSaveDto.cs` (domain shard serialization)
- pre-run loadout UI controller module (slot lock states and preview gate)

### Deliverables
- Domain shard wallet stored and persisted.
- Event charge purchase transaction (`TryPurchaseEventCharges`, `+3` per purchase) implemented.
- Event charge consume transaction (`TryConsumeEventCharge`) implemented.
- Hybrid variant unlock transaction (`TryUnlockHybridVariant`) implemented.
- Pre-run loadout UI blocks zero-charge events.
- Run settlement writes event shard rewards exactly once.

Current status (2026-03-01):
- Domain shard wallet storage + save migration: implemented.
- Run settlement write-once for event shard rewards: implemented.
- Legacy bool event unlock + hybrid unlock transactions: implemented.
- Event charge purchase/consume migration: pending.
- Loadout gate migration (from unlocked-state to charge-count): pending.

### Validation
- Zero-charge event cannot be equipped in any slot.
- Event charge count persists across restart.
- Duplicate run settlement does not duplicate domain shards.
- Domain shard balance remains non-negative.

## Phase 6 - Event System Extensions
### Goal
- Add controlled expansion points without breaking deterministic core.

### Deliverables
- Optional unlock flag for `max same-domain consecutive = 3` (future gate).
- Optional unlock flag for additional slot count (future gate).
- Versioned migration path for new unlock flags.

### Validation
- Legacy saves migrate with deterministic defaults.
- Extension flags do not affect players who did not unlock them.

## Risk Controls (All Phases)
- Single writer rule: no wallet mutation outside `MetaProgressionService`.
- Save immediately after successful transaction.
- Keep public query API read-only and side-effect free.
- Keep formula parameters in `EconomyTuning` only.

## Definition Of Done
- A. New characters unlockable with currency.
- B. Character levels persist and affect progression UI.
- C. Character-specific ability tree supports unlock path and persistence.
- D. Domain shards settle from event completion and persist correctly.
- E. Event charge/hybrid gates are enforceable in pre-run loadout UI.
- Reward economy is soft-capped and resistant to high-score farming spikes.
