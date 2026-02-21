# Code Structure Audit (2026-02-21)

## Scope
- Script size and responsibility boundaries under `Scripts/`
- Scene/script attachment under `Scenes/`, `Prefabs/`, `Enemies/`, `MainScence.tscn`
- Potential orphan scripts

## Completed Actions
### 1) Orphan cleanup
- Removed orphan debug module:
  - `Scripts/Enemy/EnemyDebugEventModule.cs`
  - `Scripts/Enemy/EnemyDebugEventModule.cs.uid`
- Validation: no scene attachment and no runtime references.

### 2) `GameFlowUI` second split
- Added:
  - `Scripts/UI/GameFlowUI.References.cs`
  - `Scripts/UI/GameFlowUI.CharacterSelect.cs`
  - `Scripts/UI/GameFlowUI.EndState.cs`
  - `Scripts/UI/GameFlowUI.SettingsUI.cs`
  - `Scripts/UI/GameFlowUI.SettingsPersistence.cs`
- Slimmed:
  - `Scripts/UI/GameFlowUI.cs`
  - `Scripts/UI/GameFlowUI.State.cs`
  - `Scripts/UI/GameFlowUI.PauseSettings.cs`

### 3) `PlayerHealth` split
- Added:
  - `Scripts/Player/PlayerHealth.Core.cs`
  - `Scripts/Player/PlayerHealth.Shield.cs`
  - `Scripts/Player/PlayerHealth.Vfx.cs`
- Converted `Scripts/Player/PlayerHealth.cs` to state/exports/properties ownership only.

### 4) `SpawnSystem` split
- Added:
  - `Scripts/Systems/Director/SpawnSystem.Pacing.cs`
  - `Scripts/Systems/Director/SpawnSystem.MiniBossSchedule.cs`
- Slimmed:
  - `Scripts/Systems/Director/SpawnSystem.Runtime.cs`

## Current Size Snapshot (Top Files)
- `Scripts/Systems/Director/SpawnSystem.cs` (503)
- `Scripts/UI/GameFlowUI.References.cs` (354)
- `Scripts/Player/PlayerHealth.Vfx.cs` (323)
- `Scripts/Systems/Director/StabilitySystem.cs` (307)
- `Scripts/Player/PlayerWeapon.cs` (291)

## Residual Risks / Next Targets
- `Scripts/Systems/Director/SpawnSystem.cs` is still the largest file and mixes scheduling + spawn geometry + node instantiation.
- `Scripts/UI/GameFlowUI.References.cs` is data-heavy (constants/fields); acceptable but still dense.
- Folder naming mismatch remains (`Enemies/` scenes vs `Scripts/Enemy/` scripts).

## Verification
- `dotnet build 20260120.csproj`
  - result: success
  - warnings: 0
  - errors: 0
