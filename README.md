# Balls  
A 2D Top-Down Real-Time Survival Action Game (System-Driven Prototype)

This repository contains a **playable and actively evolving prototype** of a 2D top-down real-time survival action game built with **Godot Engine 4 (C# / Mono)**.

The project is not focused on content scale or visual polish.  
Its primary goal is to explore **system-driven game architecture**, especially **pressure-based pacing, centralized combat arbitration, and strictly controlled data flow**.

---

## 🎮 Game Overview

**Genre**  
2D Top-Down · Real-Time · Survival Action

**Player Experience**
- Enemies continuously spawn and close in on the player
- Screen density and tactical pressure increase over time
- The player survives via movement, positioning, ranged and melee attacks
- At critical moments, the game pauses and presents upgrade choices
- Death immediately restarts the run — no stages, no checkpoints

Each run is short, intense, and fully restartable.

---

## 🧠 Core Design Goals

This project is built around three non-negotiable goals:

1. **Predictable and controllable pacing**  
   Difficulty escalation must be *designed*, not left to raw RNG.

2. **Strict responsibility boundaries between systems**  
   Systems must not “peek” into each other’s internal state.

3. **Architecture that survives iteration**  
   The codebase is expected to be rewritten, refactored, and extended without collapsing.

---

## 🔁 High-Level Data Flow

The entire game loop follows a **single-direction data flow**:

PressureSystem
↓
Director
↓
SpawnSystem
↓
EnemyFactory
↓
Enemy


### Critical Rule
> **Enemies never read pressure values directly.**

All difficulty, pacing, and composition decisions are mediated through explicit data structures, not shared state.

---

## 🧩 System Responsibilities

### 1. PressureSystem — World Pressure State

**Role**  
Maintains global pressure as an immutable snapshot.

**Responsibilities**
- Tracks pressure value (0–100)
- Derives pressure tier and intensity
- Aggregates abstract world metrics:
  - Time progression
  - Enemy count / density
  - Player survivability signals (e.g. HP ratio)
- (Optionally) reacts to player performance with capped influence
- 
**Output**
```csharp
PressureState {
  float value;
  int tier;
  float intensity;
}


This system has no knowledge of enemies, spawn points, or generation rules.

2. Director — Pacing & Strategy Translation

Role
Translates PressureState into a concrete spawning strategy.

Responsibilities

Maps pressure tiers to gameplay rules

Produces a SpawnPlan snapshot, containing:

Spawn rate or interval

Budget per wave

Max alive enemies

Spawn distance constraints

Enemy type weight distributions

Tier-specific special rules (e.g. chargers, tanks, flanking)

The Director is the only system that understands how pressure becomes pacing.

3. SpawnSystem — Execution Layer

Role
Executes the current SpawnPlan.

Responsibilities

Controls spawn timing (wave-based or interval-based)

Resolves spawn positions relative to the player

Enforces alive limits

Issues spawn requests without deciding enemy composition

Output

SpawnRequest {
  enemyTypeId;
  position;
  initialParams;
}


The SpawnSystem does not read raw pressure values.

4. EnemyDistributor — Composition Logic

Role
Selects enemy groups, not individual RNG picks.

Responsibilities

Consumes:

Budget constraints

Enemy weights

Tier gates

Produces balanced enemy packs:

Prevents repetitive RNG streaks

Avoids early high-pressure combinations

Supports designed compositions (e.g. swarm + charger + tank)

Enemy selection is budget-driven, not purely random.

5. EnemyFactory — Instantiation Boundary

Role
Materializes enemies from data.

Responsibilities

Maps enemyTypeId to scenes

Instantiates PackedScenes

Applies initial parameters (HP multipliers, speed modifiers, behavior seeds)

This layer cannot access pressure, pacing, or generation logic.

6. Enemy — Terminal Entity

Role
Owns only its local behavior and state.

Responsibilities

Movement and attack behavior

Receiving damage

Emitting death events

On death, enemies emit events only:

EnemyDied(enemyId, tags, position)


They never modify global systems directly.

⚔️ Combat System Philosophy

Combat is governed by centralized arbitration.

Attacks generate DamageRequest

Only CombatSystem may resolve damage

No entity directly modifies another entity’s HP

Invulnerability, cooldowns, and death checks are handled centrally

This avoids:

Frame-order race conditions

Dash / contact damage inconsistencies

Distributed damage logic bugs

📊 Data-Driven Design

Most pacing and spawning logic is externalized via CSV:

PressureTierRules.csv

EnemyDefinitions.csv

TierEnemyWeights.csv

Benefits:

Difficulty tuning without recompilation

Clear separation between design data and execution logic

Safe fallbacks when data is missing or invalid

🛠 Technology Stack

Engine: Godot Engine 4.x

Language: C# (Mono)

Platform: PC (prototype)

Focus: System architecture, pacing control, combat reliability

🚧 Project Status

This is an active prototype under heavy iteration.

Current focus:

Pressure → pacing → spawning pipeline

Combat feel and determinism

Architecture stability under refactor

Out of scope (for now):

Narrative content

Art polish

Audio production

Monetization or release planning

📌 Purpose of This Repository

This project exists to:

Explore system-driven survival gameplay

Demonstrate clean responsibility separation in game architecture

Serve as a foundation for future 2D or 3D action games

📄 License

This project is currently unlicensed.
All rights reserved unless otherwise stated.
