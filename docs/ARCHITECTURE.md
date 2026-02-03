# Project Genesis Architecture

## Core Rules
- Only `CombatSystem` can finalize damage.
- Sensors (`Hitbox`, `Hurtbox`, `Bullet`) detect and submit `DamageRequest`; they never deduct HP directly.
- Data flow stays one-way: `Emitter -> Request -> Resolve -> Apply`.

## Runtime Layout
```text
Game
├─ World
├─ Player
├─ Enemies
├─ Projectiles
├─ Systems
│  ├─ Core
│  │  ├─ CombatSystem
│  │  └─ DebugSystem
│  ├─ Director
│  │  ├─ SpawnSystem
│  │  └─ PressureSystem
│  └─ Progression
│     └─ UpgradeSystem
└─ UI
   └─ UpgradeMenu
```

## System Boundaries
- `Core/*`: universal runtime services.
- `Director/*`: pacing and encounter orchestration.
- `Progression/*`: upgrade application and progression effects.
- `UI/*`: presentation and input only. UI may call systems; systems do not depend on UI.

## Pressure + Upgrade Model
- `CurrentPressure`: up/down world tension signal (enemy count, low HP, time).
- `UpgradeProgress`: upgrade meter, primarily driven by kills.
- Kills grant progress with pressure-based bonus:
  - `gain = KillProgressBase * (1 + pressureNorm * KillPressureBonusFactor)`
- Time adds a small passive drip (`TimeProgressPerSecond`) so pacing does not stall.
- Upgrade trigger rule:
  1. Meter reaches threshold -> `armed`.
  2. Next player kill triggers upgrade menu.
  3. Boss flow may force trigger directly (`ForceOpenForBoss`).

## Data-Driven Tuning
- Director balance tables are in `Data/Director/`.
- Runtime code should read tuning data from tables, not hard-coded magic values.
