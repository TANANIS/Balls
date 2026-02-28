# Meta Progression Architecture (2026-02-23)
Last Synced: 2026-02-28


## Status
- Active design spec for upcoming implementation.
- Target: full architecture with lean class count (clean boundaries, low over-abstraction).

## Goals
- Add out-of-run progression driven by run score rewards.
- Support unlock targets:
  - A. New characters
  - B. Character levels
  - C. Character-specific ability tree
- Add domain-shard progression for event scheduling:
  - D. Event charge purchases and consume flow
  - E. Hybrid variant unlocks
- Add talisman progression umbrella for:
  - event unlock/charge access,
  - event upgrade tracks,
  - class progression tracks.
- Prevent reward inflation with smooth diminishing returns (soft cap curve).

## Design Principles
- Separate run-time and meta-time domains.
- Single transaction entry for all meta mutations.
- Data-first definitions, implementation-first abstractions.
- Keep event economy deterministic and previewable before run start.
- Keep extension points limited to:
  - reward curve,
  - progression definitions,
  - save backend.

## Lean Module Layout
```text
Scripts/
  Meta/
    MetaProgressionService.cs
    MetaProgressionState.cs
    CharacterProgress.cs
    MetaFlags.cs
    RunResult.cs
    RewardBreakdown.cs
    RewardCalculator.cs
    EconomyTuning.cs
    PowerCurve.cs
  Defs/
    ProgressionDefs.cs
    Models/
      CharacterDef.cs
      AbilityNodeDef.cs
  Save/
    MetaSaveDto.cs
    JsonSaveStore.cs
    SaveMigrator.cs
```

## Core Responsibilities
- `MetaProgressionService`:
  - the only write entry for currency/unlock/upgrade/tree/event actions.
  - validates preconditions, applies changes, persists state.
- `RewardCalculator`:
  - computes reward from `RunResult` with full breakdown.
  - includes domain shard reward settlement.
- `PowerCurve`:
  - smooth diminishing returns curve for score-to-currency conversion.
- `ProgressionDefs`:
  - static definitions for unlock costs, tree nodes, level requirements.
  - can be replaced by JSON/Resource later without service rewrite.
- `JsonSaveStore` + `SaveMigrator`:
  - persistence and schema migration.

## Reward Model (Soft Cap + Bonus)
- Input:
  - `score`
  - run metadata (`characterId`, `isPerfectClear`, etc.)
- Output:
  - `RewardBreakdown` fields:
    - `BaseCurrency`
    - `SoftCappedCurrency`
    - `BonusCurrency`
    - `FirstClearBonus`
    - `TotalCurrency`

### Formula
```text
base = floor(score / scoreDivisor)
soft = floor(softCap * (1 - exp(-base / softCap)) + tailLinear * base)
total = max(0, soft + bonus + firstClearBonus)
```

### Tuning Parameters (`EconomyTuning`)
- `scoreDivisor` (default 100)
- `softCap` (main diminishing-return control)
- `tailLinear` (small high-score growth tail; optional, e.g. 0.05~0.15)
- bonus rules:
  - perfect clear bonus
  - first clear bonus (per character / per milestone / global)

## Progression Domain Model
- `MetaProgressionState`:
  - `CurrencyWallet`
  - `DomainShardWalletByDomain`
  - `CurrencyEarnedTotal`
  - `CurrencySpentTotal`
  - `HashSet<string> UnlockedCharacterIds`
  - `Dictionary<string, int> EventChargesByEventId`
  - `HashSet<string> UnlockedHybridVariantIds`
  - `Dictionary<string, CharacterProgress> CharacterProgressById`
  - `HashSet<string> MetaFlags`
  - `HashSet<string> SettledRunIds` (anti-duplicate settlement)
- `CharacterProgress`:
  - `Level`
  - `UnlockedAbilityNodes`
  - optional character-specific flags

## Transaction APIs (Service Entry)
- `SettleRun(RunResult result) -> RewardBreakdown`
- `TryUnlockCharacter(string characterId) -> bool`
- `TryUpgradeCharacterLevel(string characterId) -> bool`
- `TryUnlockAbilityNode(string characterId, string nodeId) -> bool`
- `TryPurchaseEventCharges(string eventId, int purchaseCount=1) -> bool`
- `TryConsumeEventCharge(string eventId, int amount=1) -> bool`
- `TryUnlockHybridVariant(string variantId) -> bool`
- Query methods:
  - `CanUnlockCharacter(...)`
  - `CanUpgradeCharacterLevel(...)`
  - `CanUnlockAbilityNode(...)`
  - `IsCharacterUnlocked(...)`
  - `CanPurchaseEventCharges(...)`
  - `GetEventChargeCount(eventId)`
  - `CanUnlockHybridVariant(...)`
  - `GetDomainShardBalance(domainId)`

## Save Contract
- `MetaSaveDto` should include:
  - `Version`
  - wallet and totals
  - domain shard wallet
  - unlocked sets
  - event charge inventory and unlocked hybrid-variant set
  - character progression map
  - settled run IDs
- `SaveMigrator`:
  - migrate by version step.
  - default missing fields with deterministic fallbacks.

## Integration Points (Current Project)
- Run settlement trigger:
  - from `GameFlowUI` end-state path after score is finalized.
- Score source:
  - `ScoreSystem.Score`
- Event reward source:
  - event completion ledger accumulated during run and settled at run end.
  - current implementation source: `EventDirector.GetRunDomainShardRewardsSnapshot()` -> `RunResult.DomainShardRewardsByDomain`.
- Character availability:
  - gate in character-select flow (`GameFlowUI.CharacterSelect.cs`) through meta query.
- Event availability:
  - gate in pre-run loadout UI through charge-count query (`count > 0`).
- Runtime selection:
  - `RunContext` should reject non-unlocked character selection.
  - `RunContext` should reject run loadouts with zero-charge events.

## Safety / Exploit Controls
- Settlement idempotency via `SettledRunIds` and unique `RunId`.
- No direct wallet mutation outside `MetaProgressionService`.
- No direct domain-shard mutation outside `MetaProgressionService`.
- No direct event-charge mutation outside `MetaProgressionService`.
- Clamp all reward outputs to non-negative values.
- Persist immediately after successful transaction.

## Implementation Order
1. Implement domain models + save DTO/migrator/store.
2. Implement `RewardCalculator` + `PowerCurve` + `EconomyTuning`.
3. Implement `MetaProgressionService` transaction flow.
4. Integrate run settlement in end-state.
5. Integrate character unlock gating in character select.
6. Add level and ability-tree unlock flows.

## Non-Goals (First Pass)
- Cloud sync / account system.
- Server-authoritative anti-cheat.
- Multiple save slots.
