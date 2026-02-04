# Balls  
A 2D Top-Down Real-Time Survival Action Game (System-Driven Prototype)

This repository contains a playable and actively evolving prototype of a 2D top-down real-time survival action game built with Godot Engine 4 (C# / Mono).

The project is not focused on content scale or visual polish.
Its primary goal is to explore system-driven game architecture, especially pressure-based pacing, centralized combat arbitration, and strictly controlled data flow.

---

## Game Overview

Genre:
2D Top-Down · Real-Time · Survival Action

Player Experience:
- Enemies continuously spawn and close in on the player
- Screen density and tactical pressure increase over time
- The player survives via movement, positioning, ranged and melee attacks
- At critical moments, the game pauses and presents upgrade choices
- Death immediately restarts the run — no stages, no checkpoints

Each run is short, intense, and fully restartable.

---

## Core Design Goals

1. Predictable and controllable pacing
2. Strict responsibility boundaries between systems
3. Architecture that survives iteration

---

## High-Level Data Flow

PressureSystem
  ↓
Director
  ↓
SpawnSystem
  ↓
EnemyFactory
  ↓
Enemy

Critical rule:
Enemies never read pressure values directly.

---

## System Responsibilities

PressureSystem:
- Maintains global pressure state (0–100)
- Outputs immutable PressureState (value, tier, intensity)
- Has no knowledge of enemies or spawning

Director:
- Translates PressureState into SpawnPlan
- Owns all pacing and difficulty mapping logic

SpawnSystem:
- Executes SpawnPlan timing and positioning
- Emits SpawnRequest objects
- Does not read raw pressure

EnemyDistributor:
- Selects enemy groups using budget + weight rules
- Avoids repetitive RNG patterns
- Enables designed compositions

EnemyFactory:
- Instantiates enemies from enemyTypeId
- Applies initial parameters only

Enemy:
- Owns local behavior and state
- Emits EnemyDied events only

---

## Combat System Philosophy

- All attacks emit DamageRequest
- Only CombatSystem resolves damage
- No entity directly modifies another entity’s HP
- Centralized arbitration avoids race conditions

---

## Data-Driven Design

CSV-driven configuration:
- PressureTierRules.csv
- EnemyDefinitions.csv
- TierEnemyWeights.csv

Allows tuning without recompilation and enforces clean separation between data and logic.

---

## Technology Stack

Engine: Godot Engine 4.x
Language: C# (Mono)
Platform: PC (prototype)

---

## Project Status

Active prototype under heavy iteration.
Focus is on architecture, pacing control, and combat reliability.

---
