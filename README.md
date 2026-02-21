# Balls
A 2D top-down real-time survival action prototype (Godot 4 + C#).

## Overview
This project is a system-driven prototype focused on:
- phase-based pacing (`StabilitySystem` + director data tables),
- centralized combat resolution (`CombatSystem`),
- pickup-driven progression (`ProgressionSystem` + `UpgradeSystem`).

Each run is a fixed 15:00 session:
- start from title menu,
- pick a character,
- survive escalating waves,
- clear at 15:00 (or fail on death),
- restart quickly.

## Current Runtime Flow
1. Enemy dies -> `ExperienceDropSystem` spawns `ExperiencePickup`.
2. Player picks up EXP -> `ProgressionSystem` fills upgrade progress.
3. Progress reaches requirement -> level-up charge queued.
4. `UpgradeMenu` opens and applies one upgrade through `UpgradeSystem`.
5. `SpawnSystem` scales pressure by stability phase and director CSV weights.

## Core Rules
- Only `CombatSystem` finalizes damage.
- Hitboxes/bullets submit requests; they do not deduct HP directly.
- UI reads runtime state; gameplay systems do not depend on UI.

## Key Data
Under `Data/Director/`:
- `PressureTierRules.csv`
- `EnemyDefinitions.csv`
- `TierEnemyWeights.csv`

## Tech
- Engine: Godot 4.x (Mono)
- Language: C#
- Platform: PC (prototype)
