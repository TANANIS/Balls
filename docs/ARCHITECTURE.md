# Project Genesis Rules & Architecture Guide

Godot 4.x + C# | 2D top-down geometric survival shooter

## 0. First Principles

1. Centralized arbitration:
   Only `CombatSystem` decides whether damage is valid. No other module may deduct HP.
2. Sensors do not arbitrate:
   `Hitbox/Hurtbox` only detect and send `DamageRequest`.
3. Unidirectional data flow:
   `Emitter -> Request -> System Resolve -> Apply`.

## 1. Scene Tree Convention

```text
Game (Node2D)
├─ World (Node2D)
├─ Player (CharacterBody2D)
├─ Enemies (Node2D)
├─ Projectiles (Node2D)
├─ Systems (Node)
│  ├─ CombatSystem (Node)
│  ├─ SpawnSystem (Node)
│  ├─ TimeSystem (Node, optional)
│  └─ DebugSystem (Node, optional)
└─ UI (CanvasLayer)
```

Rules:
- `Systems/*` must not depend on `UI`.
- `Enemies` / `Projectiles` are container roots.
- `SpawnSystem` handles pacing only, not enemy AI.

## 2. Combat Rules

- Damage path:
  `DamageRequest -> CombatSystem.RequestDamage -> IDamageable.TakeDamage`
- Forbidden:
  - Directly modifying HP in sensors/bullets.
  - Calling health damage APIs outside `CombatSystem`.

## 3. Hitbox / Hurtbox Rules

- `PlayerHurtbox` must join group `PlayerHurtbox` in `_Ready()`.
- `EnemyHitbox` uses interval tick for contact damage.
- `EnemyHurtbox` only forwards damage to `EnemyHealth`.

## 4. Group Rules

Groups are identity, not capability.

Recommended constants:

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

## 5. Debug Rules

- Prefix logs, e.g. `[EnemyHitbox] ...`
- Heavy logs only under debug switch.
- Workarounds must explain why.

## 6. Non-Negotiable Checklist

Reject if any:
- Direct HP edits inside sensor/bullet scripts.
- Health damage called outside `CombatSystem`.
- Using `IDamageable` for identity checks.
- Using `AreaEntered` as continuous contact damage.
- Scattered timer-node core logic.
- Scene tree changes without path/name sync.
