# Meta Progression Implementation Plan (Phase 1-4)
Last Synced: 2026-02-27


## Scope
- Convert `docs/META_PROGRESSION_ARCHITECTURE.md` into concrete implementation steps.
- Keep architecture clean while minimizing early over-abstraction.

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

## Risk Controls (All Phases)
- Single writer rule: no wallet mutation outside `MetaProgressionService`.
- Save immediately after successful transaction.
- Keep public query API read-only and side-effect free.
- Keep formula parameters in `EconomyTuning` only.

## Definition Of Done
- A. New characters unlockable with currency.
- B. Character levels persist and affect progression UI.
- C. Character-specific ability tree supports unlock path and persistence.
- Reward economy is soft-capped and resistant to high-score farming spikes.
