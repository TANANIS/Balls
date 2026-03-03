## Script Refactor Layout (2026-02)
Last Synced: 2026-03-03


> Status: Archived snapshot of refactor outcomes in 2026-02. Current architecture should be validated against live scripts first.

## 2026-03-03 Additions (UI Robustness Pass)
- Added boot title gate before menu flow:
  - `Scenes/UI/Panels/TitleScreen.tscn`
  - `Scripts/UI/GameFlowUI.TitleScreen.cs`
- Converted `GameFlowUI` binding path constants to Inspector-overridable `Export NodePath` fields:
  - `Scripts/UI/GameFlowUI.References.cs`
  - `Scripts/UI/GameFlowUI.EventUnlockPanelController.cs`
  - `Scripts/UI/GameFlowUI.EventLoadoutPanelController.cs`
  - `Scripts/UI/GameFlowUI.LocalizationPaths.cs`
- Added inspector-driven UI tuning group for layout/runtime placement values:
  - `Scripts/UI/GameFlowUI.LayoutConfig.cs`

## 2026-03-03 Additions (UI Controller Separation Pass)
- Added shared pre-run UI state model:
  - `Scripts/UI/Models/GameFlowUiSharedState.cs`
- Added start-page controllers (per-page binding/input/presentation ownership):
  - `Scripts/UI/Pages/StartMainPageController.cs`
  - `Scripts/UI/Pages/StartSettingsPageController.cs`
  - `Scripts/UI/Pages/StartCardsPageController.cs`
  - `Scripts/UI/Pages/StartCharacterSelectPageController.cs`
- Updated page scenes to attach corresponding controller scripts:
  - `Scenes/UI/Panels/StartMainScroll.tscn`
  - `Scenes/UI/Panels/StartSettingsPage.tscn`
  - `Scenes/UI/Panels/StartCardsPage.tscn`
  - `Scenes/UI/Panels/StartCharacterSelectPage.tscn`
- `GameFlowUI` role refinement:
  - keeps routing/state transition ownership for start flow,
  - consumes controller events instead of binding all start-page child nodes directly.

## 2026-02-28 Additions (Structure Cleanup Pass)
- `ObstacleFieldGenerator` split:
  - `Scripts/World/ObstacleFieldGenerator.cs` (core state + frame orchestration)
  - `Scripts/World/ObstacleFieldGenerator.Spawn.cs`
  - `Scripts/World/ObstacleFieldGenerator.Environment.cs`
  - `Scripts/World/ObstacleFieldGenerator.Variants.cs`
  - `Scripts/World/ObstacleFieldGenerator.Runtime.cs`
- `PlayerWeapon` split:
  - `Scripts/Player/PlayerWeapon.cs` (core tick/orchestration)
  - `Scripts/Player/PlayerWeapon.Attack.cs`
  - configuration + stat mutation merged into core.
  - elemental-burst logic merged into attack.
- `PlayerHealth` VFX split:
  - `Scripts/Player/PlayerHealth.Vfx.cs` (shield visual flow)
  - `Scripts/Player/PlayerHealth.Vfx.Damage.cs`
  - `Scripts/Player/PlayerHealth.Vfx.Priest.cs`
- `Bullet` homing split was consolidated:
  - `Scripts/Projectiles/Bullet.cs` (runtime lifecycle)
  - `Scripts/Projectiles/Bullet.Collision.cs` (collision + homing + retarget helpers)
- `ProceduralTerrainBackground` split expansion:
  - `Scripts/World/ProceduralTerrainBackground.Mask.Noise.cs`
  - `Scripts/World/ProceduralTerrainBackground.Tiling.Caps.cs`
  - `Scripts/World/ProceduralTerrainBackground.Tiling.Resolve.cs`

Director, Core, Progression, UI, Player, Enemy, Audio, Projectile systems were split into partial files to make responsibilities explicit while keeping runtime behavior intact.

### SpawnSystem
- `Scripts/Systems/Director/SpawnSystem.cs`: node lifecycle, spawn loop, and internal data structs.
- `Scripts/Systems/Director/SpawnSystem.Runtime.cs`: runtime state orchestration, tier/runtime snapshot updates, dependency resolution, and debug spawn helpers.
- `Scripts/Systems/Director/SpawnSystem.Pacing.cs`: phase multipliers, opening-ramp math, and tier-tail pacing helpers.
- `Scripts/Systems/Director/SpawnSystem.MiniBossSchedule.cs`: phase-tail miniboss scheduling and spawn execution.
- `Scripts/Systems/Director/SpawnSystem.Selection.cs`: weighted enemy selection and elite injection.
- `Scripts/Systems/Director/SpawnSystem.Csv.cs`: CSV loading and parsing helpers.
- `Scripts/Systems/Director/SpawnSystem.SpawnFactory.cs`: wave scheduling, spawn queueing, and enemy instantiation.
- `Scripts/Systems/Director/SpawnSystem.BoundsAndPlacement.cs`: pack-center and offset placement helpers.
- `Scripts/Systems/Director/SpawnSystem.Recycle.cs`: far-enemy recycle tracking and cleanup.

### ProgressionSystem
- `Scripts/Systems/Progression/ProgressionSystem.cs`: EXP meter, requirement curve, queued level-up flow, and upgrade-menu trigger ownership.

### UpgradeSystem
- `Scripts/Systems/Progression/UpgradeSystem.cs`: lifecycle and upgrade application entry.
- `Scripts/Systems/Progression/UpgradeSystem.Options.cs`: option pool construction and random pick logic.
- `Scripts/Systems/Progression/UpgradeSystem.Types.cs`: option DTO type.


### GameFlowUI
- `Scripts/UI/GameFlowUI.cs`: startup flow and shared helper utilities.
- `Scripts/UI/GameFlowUI.References.cs`: exported node-path bindings, node references, scene resolution, signal wiring.
- `Scripts/UI/GameFlowUI.Binding.cs`: UI/event binding and interaction wiring.
- `Scripts/UI/GameFlowUI.UIStateController.cs`: panel transitions and run-state flow ownership.
- `Scripts/UI/GameFlowUI.MetaProgressionPanelController.cs`: character select/unlock/confirm and meta panel rendering flow.
- `Scripts/UI/GameFlowUI.EndStateController.cs`: death/perfect-clear flow, final summary, and leaderboard refresh.
- `Scripts/UI/GameFlowUI.PauseSettings.cs`: pause menu open/close and pause navigation.
- `Scripts/UI/GameFlowUI.SettingsUI.cs`: settings widgets, selection sync helpers, and save/load (`user://settings.cfg`).
- `Scripts/UI/GameFlowUI.Localization.cs`: locale mapping and localization utility helpers.
- `Scripts/UI/GameFlowUI.LocalizationPaths.cs`: exported node paths used by localization label binding.
- `Scripts/UI/GameFlowUI.Visuals.cs`: vignette, score text, XP-bar refresh, 15:00 countdown refresh, responsive background scaling.
- `Scripts/UI/GameFlowUI.LayoutConfig.cs`: inspector-exposed UI layout and responsive tuning parameters.
- `Scripts/UI/GameFlowUI.TitleScreen.cs`: boot title transition and any-input gate to menu flow.
- `Scripts/UI/PlayerHealthBarDemo.cs`: runtime HP segment HUD binding.

### Combat
- `Scripts/Systems/Core/CombatSystem.cs`: centralized damage processing and tank bullet bonus knockback/damage hook.

### UpgradeMenu
- `Scripts/UI/UpgradeMenu.cs`: lifecycle, input gate, UI binding, and option-pick/apply flow.

### Experience Progression
- `Scripts/Systems/Progression/ExperienceDropSystem.cs`: listens to kill events and drops pickups.
- `Scripts/Systems/Progression/ExperiencePickup.cs`: pickup collision + EXP grant into `ProgressionSystem`.

### AudioManager
- `Scripts/Audio/AudioManager.cs`: singleton + public playback API + runtime playback internals.
- `Scripts/Audio/AudioManager.Setup.cs`: player pool setup, stream loading, event binding.

### Player
- `Scripts/Player/Player.cs`: player facade and frame orchestration.
- `Scripts/Player/Player.Composition.cs`: module resolution/setup and death signal handling.
- `Scripts/Player/Player.State.cs`: damage/invincibility hooks and respawn reset.
- `Scripts/Player/Player.Bounds.cs`: movement bounds clamping.
- `Scripts/Player/Player.Character.cs`: character-definition application, slot routing, and ability compatibility helpers.
- `Scripts/Player/PlayerDash.cs`: dash state machine tick and movement ownership.
- `Scripts/Player/PlayerDash.Runtime.cs`: dash start/stop and stat mutations.
- `Scripts/Player/PlayerHealth.cs`: health/shield state fields, exported tuning, and public status properties.
- `Scripts/Player/PlayerHealth.Core.cs`: HP lifecycle, invincibility, regen, and damage/death handling.
- shield enablement/cooldown policy merged into `PlayerHealth.Core.cs`.
- `Scripts/Player/PlayerHealth.Vfx.cs`: shield visuals, hit flash, and damage flash material flow.
- `Scripts/Player/PlayerMelee.cs`: melee setup/input/cooldown flow.
- `Scripts/Player/PlayerMelee.Attack.cs`: melee hit query, filtering, and damage request emission.
- `Scripts/Player/PlayerMelee.Stats.cs`: melee stat mutation methods for upgrades.

### Character Runtime
- `Scripts/Characters/CharacterDefinition.cs`: data-driven character slots and base stats.
- `Scripts/Characters/AttackAbilityKind.cs`: attack slot enum (`None`, `Ranged`, `Melee`).
- `Scripts/Characters/MobilityAbilityKind.cs`: mobility slot enum (`None`, `Dash`).
- `Scripts/Runtime/RunContext.cs`: autoload state for selected character between menu and run.

Rule update: upgrades target logical slots (primary/secondary/mobility compatibility) instead of hard-coding ranged/melee ownership by node name.

### Enemy
- `Scripts/Enemy/Enemy.cs`: enemy frame loop and external notifications.
- `Scripts/Enemy/Enemy.Resolve.cs`: dependency/module resolution.
- `Scripts/Enemy/Enemy.Behavior.cs`: desired-velocity logic and event dispatch helpers.

### Projectile
- `Scripts/Projectiles/Bullet.cs`: lifetime and movement.
- `Scripts/Projectiles/Bullet.Collision.cs`: collision filtering and damage request emission.

Refactor rule: split by responsibility first, then extract reusable services only when duplication is proven.
