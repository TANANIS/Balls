# Game Concept
Last Synced: 2026-03-01


## 1. What Game Is This?

A 2D top-down real-time survival action game with a fixed `15:00` run.
Survive to `15:00` to clear the run.

## 2. Core Loop

1. Before run start, player configures a 4-slot `Event Loadout`.
2. Run begins and enemies continuously spawn by tier/phase pacing.
3. At slot timestamps, `EventDirector` activates the preselected event.
4. Player survives via movement, attack, and dodge while time intensity scales.
5. Enemy kills drop EXP pickups; level-up opens upgrade selection.
6. Defeating a phase-tail boss grants an immediate bonus: `+1` level and `+10` EXP (temporary tuning).
7. Event completion grants domain shards through event reward calculation.
8. Reach `15:00` or die, then settle results into meta progression.

## 2.1 Phase Timeline (15:00 total, canonical)

- `00:00 - 03:45`: Stage 1 (`Ramp-In`)
- `03:45 - 07:30`: Stage 2 (`First Stress Cycle`)
- `07:30 - 11:15`: Stage 3 (`Build Check`)
- `11:15 - 15:00`: Stage 4 (`Final Climb`)

Design goals:
- Difficulty scales by both enemy pacing and scheduled event time intensity.
- Phase identity should be readable through environment and combat tempo.

## 2.2 Event Slot Timeline (Current Spec)

- `Slot1`: Tier0 / Early
- `Slot2`: Tier1 / Mid
- `Slot3`: Tier2 / Late
- `Slot4`: Tier3 / Final

Notes:
- Exact activation timestamps are owned by `EventDirector`.
- Mid-run random event selection is not part of this model.

## 3. In-Run Mainline

- The player enters a collapsing ward-zone and endures four scheduled catastrophes.
- A fragmented `Order` deity provides the foresight mechanism that enables slot scheduling.
- Slot order is player-authored pre-run; consequence handling is in-run skill execution.
- Distortion and affinity are applied on activation, then converted into time-intensity/reward changes.

## 4. Out-Of-Run Mainline

- Event completion yields domain-specific shards (`Ice`, `Spacetime`, `War`).
- Domain shards are spent in meta progression to purchase:
  - event charge bundles (`+3` uses per purchase),
  - hybrid variants,
  - talisman-driven event/class progression branches,
  - future systemic expansions.
- Meta choices influence future loadout possibilities and risk budgeting.

## 5. Player

- HP and combat profile are character-dependent and data-driven.
- No universal passive regen baseline; sustain is role/upgrade-defined.
- Combat responsibility is execution under scheduled risk, not random reaction.

## 6. Controls

- Move: `WASD` / left stick.
- Aim: mouse / right stick.
- Action families:
  - primary attack (rhythm),
  - secondary action (higher impact, longer cooldown),
  - mobility/dodge.

## 7. Upgrade Moment (Runtime Contract)

Level-up flow:
1. Enemy death drops `ExperiencePickup`.
2. Player collects pickup and gains EXP.
3. EXP reaches requirement and queues level-up charge.
4. Upgrade menu opens and consumes one queued charge.
5. Pick one upgrade, then resume run.

Rules:
- EXP overflow is preserved.
- Upgrade pacing is pickup-driven, not time-drip-driven.
- Phase-tail boss clear bonus is granted immediately on kill: `+1` level and `+10` EXP.

## 8. Combat Feel

- Fast, readable feedback with clear hit confirmation.
- Pressure should feel authored by loadout plus director pacing.
- Avoid ambiguity spikes that hide player agency.

## 9. Enemy Philosophy

- Enemies remain behavior-readable and role-distinct.
- Threat comes from density, proximity, lane denial, and scheduled event modifiers.

## 10. Visual Direction

- Fantasy pixel atmosphere with gameplay readability first.
- High contrast between player, enemies, and threat telegraphs.
- Keep accent groups limited for moment-to-moment clarity.
