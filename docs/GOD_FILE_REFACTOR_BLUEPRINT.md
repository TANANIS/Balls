# God File Refactor Blueprint (2026-02-25)

## Objective
- Reduce high-risk large files and mixed-responsibility modules.
- Keep gameplay behavior unchanged during refactor.
- Make future card/projectile/UI additions cheaper and safer.

## Risk Baseline (Current Snapshot)
- Single-file risk (top):
  - `Scripts/Projectiles/Bullet.cs` (`666` lines)
  - `Scripts/Systems/Core/DebugCheatSystem.cs` (`499` lines)
  - `Scripts/Systems/Director/SpawnSystem.cs` (`483` lines)
  - `Scripts/World/ObstacleFieldGenerator.cs` (`340` lines)
  - `Scripts/Player/PlayerWeapon.cs` (`332` lines)
- Module-cluster risk (partial total):
  - `GameFlowUI`: `1741` lines / `12` files
  - `SpawnSystem`: `1408` lines / `8` files
  - `Player`: `804` lines / `8` files
  - `UpgradeSystem`: `738` lines / `5` files
  - `Bullet`: `717` lines / `2` files

## Refactor Priorities
1. `Bullet` cluster
2. `DebugCheatSystem`
3. `UpgradeSystem` cluster
4. `GameFlowUI` cluster
5. `SpawnSystem` cluster (cleanup pass)

## Execution Status (2026-02-25)
- [x] Phase 1 (`Bullet`)
- [x] Phase 2 (`DebugCheatSystem`)
- [x] Phase 3 (`UpgradeSystem`)
- [x] Phase 4 (`GameFlowUI`)
- [x] Phase 5 (`SpawnSystem`)

## Phase Plan

### Phase 1 - Projectile Core Decomposition (`Bullet`)
Goal:
- Extract split-shot, elemental-burst, and VFX concerns from one file.

Target split:
- `Bullet.Core.cs`
  - runtime init, move/lifetime, facing, frame tick entry.
- `Bullet.SplitShot.cs`
  - split spawn count/angles/non-chain rules.
- `Bullet.ElementalBurst.cs`
  - burst detonation condition, AoE query/damage emit, owner callback.
- `Bullet.Vfx.cs`
  - projectile frame build, explosion animation/fallback rune.
- keep `Bullet.Collision.cs`
  - collision filter and hit request bridge.

Acceptance:
- No behavior delta for:
  - split count curve (`3->4->5->6`)
  - non-chain split rule
  - elemental burst trigger rules and tuned values
- Build success and in-run smoke:
  - primary hit, split hit, burst detonation by hit/distance/lifetime

### Phase 2 - Debug Tool Separation (`DebugCheatSystem`)
Goal:
- Keep debug menu feature growth from polluting one class.

Target split:
- `DebugCheatSystem.UI.cs`
  - panel build and controls.
- `DebugCheatSystem.Actions.cs`
  - spawn/upgrade/time/player actions.
- `DebugCheatSystem.Localization.cs`
  - bilingual labels and lookup helpers.
- `DebugCheatSystem.Pause.cs`
  - open/close pause ownership and restore logic.

Acceptance:
- F3 open/close behavior unchanged.
- Existing debug actions unchanged (`no-damage`, direct upgrade apply, etc.).

### Phase 3 - Upgrade Policy Consolidation (`UpgradeSystem`)
Goal:
- Isolate selection policy from apply-effects path.

Target split:
- `UpgradeSystem.Apply.cs`
  - id -> gameplay mutation only.
- `UpgradeSystem.Eligibility.cs`
  - gates, prerequisites, exclusives, stack checks.
- `UpgradeSystem.PoolPolicy.cs`
  - phase routing, rarity pity, category decay.
- `UpgradeSystem.Validation.cs` (keep/expand)
  - integrity checks and warnings.

Optional extraction:
- pure helper service for deterministic simulation reuse.

Acceptance:
- Offer distribution remains aligned to current docs policy.
- Existing saves/runs still apply card stacks identically.

### Phase 4 - UI Flow Controllers (`GameFlowUI`)
Goal:
- Turn partial-file cluster into explicit controllers with narrow responsibilities.

Target shape:
- `UIStateController` (panel flow and run/menu state machine)
- `UIBinding` (node refs + signal wiring)
- `UIPresenter` (text rendering and data-to-UI transforms)
- `MetaProgressionPanelController`
- `EndStateController`

Note:
- Keep scene hierarchy and NodePath contracts stable in this phase.

Acceptance:
- No regression in:
  - start -> character select -> run flow
  - pause/settings flow
  - end-state and leaderboard refresh

### Phase 5 - SpawnSystem Base File Cleanup
Goal:
- Remove residual mixed concerns from `SpawnSystem.cs`.

Target split:
- `SpawnSystem.Lifecycle.cs`
- `SpawnSystem.SpawnFactory.cs` (spawn request -> scene instantiate)
- `SpawnSystem.BoundsAndPlacement.cs`

Acceptance:
- Spawn counts, pacing, and recycle behavior unchanged in smoke runs.

## Guardrails
- Hard rule:
  - new/updated runtime file target <= `300` lines.
  - warning threshold at `220` lines for review.
- Refactor unit:
  - each phase ships in small commits, each commit keeps build green.
- No behavior change unless explicitly marked as balance patch.

## Validation Checklist Per Phase
- Compile:
  - `dotnet build ProjectGenesis.sln` = 0 error.
- Runtime smoke:
  - launch run, one full upgrade cycle, one death/end-state flow.
- Focus smoke for target module:
  - e.g. projectile phase tests split/burst edge cases.
- Docs sync:
  - update `docs/CARDS.md` / `docs/CARDS_CHANGELOG.md` / `log.md` when behavior contracts are touched.

## Suggested Execution Order (Low-Risk)
1. Phase 1 (`Bullet`)  
2. Phase 2 (`DebugCheatSystem`)  
3. Phase 3 (`UpgradeSystem`)  
4. Phase 4 (`GameFlowUI`)  
5. Phase 5 (`SpawnSystem`)  

Rationale:
- First cut highest single-file risk with low scene-graph impact.
- Keep high-traffic progression/UI modules for later after core split patterns are stable.
