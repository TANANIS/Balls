# Project Genesis Rules & Architecture Guide

Godot 4.x + C# | 2D top-down geometric survival shooter

## 0. First Principles

This project fears not bugs, but tangled functionality that blocks future expansion. Everyone must follow:

1. Centralized arbitration:
   Only `CombatSystem` decides whether damage is valid. No other module may deduct HP.

2. Sensors do not arbitrate:
   `Hitbox/Hurtbox` only detect and send `DamageRequest`. Invincibility, death, and hit validity live in `CombatSystem`.

3. Unidirectional data flow:
   `Emitter -> Request -> System Resolve -> Apply`
   No shortcuts like direct state mutation across modules.

## 1. Scene Tree Convention

Base skeleton for all scenes (Main/Game):

```
Game (Node2D)
¢u¢w World (Node2D)            # Map, obstacles, boundaries
¢u¢w Player (CharacterBody2D)  # Player entity
¢u¢w Enemies (Node2D)          # Enemy container
¢u¢w Projectiles (Node2D)      # Bullet container
¢u¢w Systems (Node)            # Global systems
¢x  ¢u¢w CombatSystem (Node)    # Damage arbitration (core)
¢x  ¢u¢w SpawnSystem (Node)     # Spawn rhythm & waves
¢x  ¢u¢w TimeSystem (Node)      # Time scale / pause (optional)
¢x  ¢|¢w DebugSystem (Node)     # Debug visualization (optional)
¢|¢w UI (CanvasLayer)          # HUD, death prompts
```

Rules:
- `Systems/*` must not depend on `UI`. UI only reads data or subscribes to events.
- `Enemies` / `Projectiles` are containers; do not scatter across the tree.
- `SpawnSystem` only handles spawn rhythm, not enemy AI.

## 2. Scripts Folder Convention

```
Scripts/
¢u¢w Player/
¢x  ¢u¢w Player.cs                # Player entity and assembly point (thin)
¢x  ¢u¢w PlayerMovement.cs         # Movement
¢x  ¢u¢w PlayerDash.cs             # Dash
¢x  ¢u¢w PlayerWeapon.cs           # Shooting / bullet spawn
¢x  ¢|¢w PlayerHurtbox.cs          # Player hurt sensor (Area2D)
¢u¢w Enemy/
¢x  ¢u¢w Enemy.cs                  # Enemy entity and movement/state (thin)
¢x  ¢u¢w EnemyHitbox.cs            # Enemy contact damage sensor (Area2D)
¢x  ¢u¢w EnemyHurtbox.cs           # Enemy hit sensor (Area2D)
¢x  ¢|¢w EnemyHealth.cs            # Enemy HP / death
¢u¢w Projectiles/
¢x  ¢|¢w Bullet.cs                 # Bullet behavior (send request on hit)
¢u¢w Systems/
¢x  ¢u¢w CombatSystem.cs           # Single arbitration point
¢x  ¢u¢w SpawnSystem.cs
¢x  ¢u¢w TimeSystem.cs (optional)
¢x  ¢|¢w DebugSystem.cs (optional)
¢u¢w Shared/
¢x  ¢u¢w DamageRequest.cs
¢x  ¢u¢w IDamageable.cs
¢x  ¢|¢w Groups.cs                 # Group constants (recommended)
¢|¢w _Legacy/                     # Archive old hitbox/hurtbox files
```

Rules:
- `Player.cs` / `Enemy.cs` must stay thin: assembly, state aggregation, minimal public API.
- Modules must not access other modules outside `Systems`, except their owner.
- Do not delete old files; move to `_Legacy/`.

## 3. Combat Rules

### 3.1 Only CombatSystem Can Deduct HP

Damage may only flow through:
- send `DamageRequest`
- `CombatSystem.RequestDamage(req)`
- `CombatSystem` validates then calls `IDamageable.TakeDamage()`

Forbidden:
- Directly modifying `Health.HP`
- Direct calls to `EnemyHealth.TakeDamage` or `PlayerHealth.TakeDamage` outside `CombatSystem`

### 3.2 DamageRequest Is a Request

Must include:
- `source`
- `target`
- `baseDamage`
- `worldPos`
- `tag` (optional: e.g. "bullet" / "contact")

### 3.3 IDamageable Responsibility

Minimum interface:
- `TakeDamage`
- `IsDead`, `IsInvincible`

## 4. Hitbox / Hurtbox Rules

### 4.1 PlayerHurtbox

- Type: `Area2D`
- Must:
  - `AddToGroup("PlayerHurtbox")`
  - Be the only target for enemy contact hit
- Does not deduct HP; only provides target for the system

### 4.2 EnemyHitbox

- Type: `Area2D`
- Behavior:
  - Track contact target (only group `PlayerHurtbox`)
  - Use tick interval for contact damage (e.g. 0.25s)
  - Each tick sends `DamageRequest` to `CombatSystem`
- Must not:
  - Deduct HP directly
  - Decide invincibility/death

### 4.3 EnemyHurtbox

- Type: `Area2D`
- Receives bullet hits (or bullet sends request directly)
- If it implements `IDamageable`, it only forwards to `EnemyHealth`

## 5. Timer / Tick Rules

Allowed:
1. System-level timers (e.g. SpawnSystem spawn interval)
2. Sensor-level ticks (e.g. EnemyHitbox contact interval)

Forbidden:
- Timer nodes scattered for core logic
- Shoving all tick logic into `CombatSystem`

## 6. Group Rules

Groups are identity, not capability.

### 6.1 Group Constants (recommended in `Scripts/Shared/Groups.cs`)

```csharp
public static class Groups
{
    public const string Player = "Player";
    public const string PlayerHurtbox = "PlayerHurtbox";
    public const string CombatSystem = "CombatSystem";
    public const string EnemyHitbox = "EnemyHitbox";
    public const string EnemyHurtbox = "EnemyHurtbox";
}
```

### 6.2 Hard Rules

- `PlayerHurtbox` must join group in `_Ready()` (not manual in tscn)
- `CombatSystem` must join `CombatSystem` group (early init preferred)

## 7. Debug Rules

- Debug messages must include prefix, e.g. `[EnemyHitbox] ...`
- Heavy logs only under Debug mode / DebugSystem switch
- Workarounds must be commented with reason and removal plan

## 8. Typical Feature Flow (Example)

Example: Add "Ice Bullet"
1. `Bullet.cs` hit sends `DamageRequest` (tag="ice_bullet")
2. `CombatSystem` resolves damage, then emits a `StatusEffectRequest` (or calls status module)
3. `Enemy` adds `EnemyStatus` module to receive effects (not handled in Bullet)

## 9. Non-Negotiable Checklist

Reject PR/commit if any:
- [ ] Direct HP modification inside Hitbox/Hurtbox/Bullet
- [ ] `Health.TakeDamage` called outside `CombatSystem`
- [ ] Use `IDamageable` to identify player (identity must use groups)
- [ ] Use `AreaEntered` as continuous contact damage
- [ ] Timer nodes scattered for core logic
- [ ] Scene tree changes without updating script paths/naming

