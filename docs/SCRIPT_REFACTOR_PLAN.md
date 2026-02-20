## Script Refactor Layout (2026-02)

Director, Core, Progression, UI, Player, Enemy, Audio, Projectile systems were split into partial files to make responsibilities explicit while keeping runtime behavior intact.

### SpawnSystem
- `Scripts/Systems/Director/SpawnSystem.cs`: node lifecycle and spawn loop.
- `Scripts/Systems/Director/SpawnSystem.Runtime.cs`: runtime state orchestration, late-game scaling, upgrade-driven events.
- `Scripts/Systems/Director/SpawnSystem.Selection.cs`: weighted enemy selection and elite injection.
- `Scripts/Systems/Director/SpawnSystem.Csv.cs`: CSV loading and parsing helpers.
- `Scripts/Systems/Director/SpawnSystem.Types.cs`: internal data structs.

### PressureSystem
- `Scripts/Systems/Director/PressureSystem.cs`: pressure/progress loop and trigger flow.
- `Scripts/Systems/Director/PressureSystem.TiersCsv.cs`: tier rule loading/parsing and runtime tier application.

### UpgradeSystem
- `Scripts/Systems/Progression/UpgradeSystem.cs`: lifecycle and upgrade application entry.
- `Scripts/Systems/Progression/UpgradeSystem.Options.cs`: option pool construction and random pick logic.
- `Scripts/Systems/Progression/UpgradeSystem.Types.cs`: option DTO type.

### DebugSystem
- `Scripts/Systems/Core/DebugSystem.cs`: singleton, logging API, input toggle flow.
- `Scripts/Systems/Core/DebugSystem.Overlay.cs`: in-game overlay UI rendering.

### GameFlowUI
- `Scripts/UI/GameFlowUI.cs`: setup, references, and signal wiring.
- `Scripts/UI/GameFlowUI.State.cs`: start/game over/restart state transitions.
- `Scripts/UI/GameFlowUI.Visuals.cs`: vignette, score text, XP-bar refresh, 15:00 countdown refresh, responsive background scaling.
- `Scripts/UI/PlayerHealthBarDemo.cs`: runtime HP segment HUD binding.
- `Scripts/UI/GameFlowUI.PerfectLeaderboard.cs`: local Perfect 15:00 leaderboard persistence + start-menu rendering.

### Combat
- `Scripts/Systems/Core/CombatSystem.cs`: centralized damage processing and tank bullet bonus knockback/damage hook.

### UpgradeMenu
- `Scripts/UI/UpgradeMenu.cs`: lifecycle, input gate, and open/close flow.
- `Scripts/UI/UpgradeMenu.UI.cs`: UI node binding, button text refresh, panel centering.
- `Scripts/UI/UpgradeMenu.Options.cs`: option picking and application flow.

### Experience Progression
- `Scripts/Systems/Progression/ExperienceDropSystem.cs`: listens to kill events and drops pickups.
- `Scripts/Systems/Progression/ExperiencePickup.cs`: pickup collision + trigger upgrade flow.

### AudioManager
- `Scripts/Audio/AudioManager.cs`: singleton and public playback API surface.
- `Scripts/Audio/AudioManager.Setup.cs`: player pool setup, stream loading, event binding.
- `Scripts/Audio/AudioManager.Playback.cs`: kill-event playback and runtime playback internals.

### Player
- `Scripts/Player/Player.cs`: player facade and frame orchestration.
- `Scripts/Player/Player.Composition.cs`: module resolution/setup and death signal handling.
- `Scripts/Player/Player.State.cs`: damage/invincibility hooks and respawn reset.
- `Scripts/Player/Player.Bounds.cs`: movement bounds clamping.
- `Scripts/Player/Player.Character.cs`: character-definition application, slot routing, and ability compatibility helpers.
- `Scripts/Player/PlayerDash.cs`: dash state machine tick and movement ownership.
- `Scripts/Player/PlayerDash.Runtime.cs`: dash start/stop and stat mutations.
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
