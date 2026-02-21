# Game Concept

## 1. What Game Is This?

A 2D top-down real-time survival action game.
Each run has a fixed duration of 15 minutes.
Survive to 15:00 to complete the run.

## 2. Core Loop

1. Enemies continuously spawn and approach.
2. Screen density and pressure rise by phase.
3. Player survives via movement, attack, and dodge.
4. Enemy kills drop EXP pickups; collecting enough EXP pauses game for one growth choice.
5. Pick one, then continue at faster tempo.
6. Reach 15:00 or die -> restart flow.

## 2.1 Phase Timeline (15:00 total)

- `00:00 - 03:00` Universe Stable
- `03:00 - 07:00` Energy Anomaly
- `07:00 - 11:00` Structural Fracture
- `11:00 - 15:00` Collapse Critical

Design goal:
- Difficulty does not only scale numerically.
- Environment state changes across phases and drives play feel.

## 3. Player

- Visual: simple circle, no narrative setup.
- HP: fixed 3.
- Hit: lose 1 HP per hit.
- No passive regen (only via run upgrades).

## 4. Controls

- Move: WASD / left stick.
- Aim direction: mouse / right stick.
- Three action families:
  - Primary attack (rhythm)
  - Secondary action (high impact, longer cooldown)
  - Mobility/dodge

## 5. Combat Feel

- Fast and clear feedback.
- Kill feedback: slight zoom, brief pause, light burst.
- No combo system, no score pop-up dependency.

## 6. Enemy Philosophy

- Geometric enemies, readable behavior.
- Pressure comes from count, proximity, and reduced movement space.

## 7. Upgrade Moment (Current Runtime)

Level-up happens via survivor-style EXP:
1. Enemy death drops EXP pickup.
2. Player collects pickup to gain EXP.
3. EXP reaches requirement -> level-up charge queued.
4. Upgrade menu opens with three choices.
5. Pick one (or random fallback on cancel), then resume.

Pressure remains a pacing signal for spawn intensity, not the primary leveling trigger in default mode.

## 8. Growth Rules

- In-run only; resets next run.
- Must be immediately noticeable after choice.

## 8.1 Universe Event Cadence

- Universe event cadence target: every 3 minutes.
- Planned event timestamps:
  - `03:00`
  - `06:00`
  - `09:00`
  - `12:00`

## 9. Visual Direction

- Geometric, high saturation, heavy bloom.
- Dark background, bright enemies, brightest attacks.
- Keep primary color count limited for readability.
