# Refactor Logic Risk Review (2026-02-25)

## Scope
- Reviewed post-refactor logic for:
  - `Bullet` cluster
  - `DebugCheatSystem` cluster
  - `UpgradeSystem` cluster
  - `GameFlowUI` cluster
  - `SpawnSystem` cluster

## Resolved In This Pass
1. Fixed garbled fallback localization text in `GameFlowUI` fallback paths.
- File: `Scripts/UI/GameFlowUI.cs`

2. Replaced fragile CSV split logic in debug localization lookup with quoted-field aware parser.
- File: `Scripts/Systems/Core/DebugCheatSystem.Localization.cs`

3. Added spawn-anchor self-heal before spawn tick early-return.
- File: `Scripts/Systems/Director/SpawnSystem.Lifecycle.cs`

4. Prevented "stack increments without gameplay effect" when `UpgradeSystem` has no player reference.
- File: `Scripts/Systems/Progression/UpgradeSystem.Apply.cs`

5. Added explicit warning when upgrade option pool is empty.
- File: `Scripts/Systems/Progression/UpgradeSystem.Options.cs`

6. Added movement input stabilization to reduce one-frame input drops causing jitter/auto-stop feel.
- Files:
  - `Scripts/Player/Player.CommandPipeline.cs`
  - `Scripts/Player/PlayerMovement.cs`

7. Added movement-freeze reapply cooldown to avoid repeated micro-stun chains under dense contact.
- File: `Scripts/Player/PlayerMovement.cs`

## Open Findings (High -> Low)

1. `Medium` - Damage freeze feel still needs tuning pass per character profile
Evidence:
- `Scripts/Player/Player.Composition.cs:73`
- `Scripts/Player/PlayerHealth.cs:19`
- `Scripts/Player/PlayerMovement.cs` (`FreezeReapplyCooldown`)
Risk:
- Freeze chaining has been throttled, but final feel still depends on `DamageMoveFreezeSeconds` and `FreezeReapplyCooldown` tuning values by character archetype.
Recommendation:
- Run feel test at high enemy density and tune:
  - `DamageMoveFreezeSeconds` (default `0.06`)
  - `FreezeReapplyCooldown` (default `0.18`)

2. `Medium-High` - UI NodePath coupling is brittle and fails silently
Evidence:
- `Scripts/UI/GameFlowUI.Binding.cs:12`
- `Scripts/UI/GameFlowUI.Binding.cs:119`
Risk:
- Heavy dependence on string NodePaths means scene hierarchy changes can break controls/signals silently (`GetNodeOrNull` returns null).
- Regressions may only surface at runtime in specific menus.
Recommendation:
- Add binding validation pass in `_Ready` (log missing critical nodes once).
- Consider grouping critical refs into a typed scene-root contract.

3. `Medium` - Upgrade fallback pool is still empty when catalog is unavailable
Evidence:
- `Scripts/Systems/Progression/UpgradeSystem.Options.cs:7`
- `Scripts/Systems/Progression/UpgradeSystem.Options.cs:70`
Risk:
- Current behavior is now observable (warning), but run progression still hard-stops if catalog is missing/invalid.
Recommendation:
- Provide minimal fallback options for safety or fail fast into explicit error UI.

4. `Medium-Low` - Settings persist on every slider tick (high write frequency)
Evidence:
- `Scripts/UI/GameFlowUI.SettingsUI.cs:37`
- `Scripts/UI/GameFlowUI.SettingsUI.cs:48`
Risk:
- Dragging sliders triggers repeated `ConfigFile.Save`, causing unnecessary I/O churn.
Recommendation:
- Debounce writes (200-400ms) or save when leaving settings panel.

5. `Low` - Category-bias semantics may conflict with expected "variety" mode
Evidence:
- `Scripts/Systems/Progression/UpgradeSystem.Options.cs:166`
Risk:
- With `UseCategoryWeightDecay = false`, repeated categories receive higher weight.
- Designers may assume this mode is neutral, but it is actually a focus/streak mode.
Recommendation:
- Document this behavior clearly or rename parameters to reflect intent.

## Player Movement Focus (This Request)
- Checked movement pipeline (`BuildFrameCommand -> ExecuteFrameCommand -> PlayerMovement.Tick`), dash ownership, and hurt-freeze queue flow.
- Confirmed two concrete jitter triggers were present:
  - exact zero-vector checks on runtime input (`Vector2 == Zero`) in movement logic,
  - one-frame input drops immediately switching to friction path.
- Applied fix:
  - deadzone normalization + short drop-grace buffering in command pipeline,
  - epsilon-based `hasInput` decision in movement integration.
- Not changed in this pass:
  - `DamageMoveFreezeSeconds` design intent (still active by default).
  - If reported "auto-stop" still occurs mostly during contact damage moments, this is now the top remaining candidate.

## Summary
- No compile/runtime-blocking regression detected in this pass (`dotnet build` clean).
- Highest remaining risks are contact-damage freeze feel, UI binding fragility, and fallback resilience (upgrade pool availability).
