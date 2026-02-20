# Dev Log (Codex Internal)

## Session Snapshot (2026-02-20)
- Project: Godot C# top-down survival prototype.
- Run target: fixed 15-minute match.
- User pacing intent:
  - Red line = game pressure (stage-tail spikes then drop).
  - Green line = player growth (step-ups at upgrade moments).
- Core direction: survivor-like progression, not random event-heavy flow.

## Implemented In This Iteration
- Character architecture:
  - Role split completed (Ranged / Melee / TankBurst).
  - Melee nerfed (HP down, melee cooldown up, dash cooldown up, dash iframe down).
  - TankBurst buffed for anti-chase (ranged damage up + tank bullet bonus knockback/damage in CombatSystem).
- Progression:
  - Reworked to pickup-driven EXP.
  - EXP full -> queue level-up -> open upgrade menu.
  - Overflow and chained level-ups supported.
- Combat feel:
  - Enemy hit white flash + punch + small knockback.
- UI:
  - HP UI shows only during run.
  - Top EXP bar with ready state.
  - Top-right 15:00 countdown.
  - Dedicated Perfect Clear end-state (separate from failure).
  - Start menu local leaderboard for perfect clears (score/date/character).
- Director:
  - 15-minute, 4-phase pacing with phase-tail MiniBossHex schedule.
  - Universe special events disabled in current model.

## Files Worth Monitoring
- `Scripts/Systems/Director/PressureSystem.cs`
- `Scripts/Systems/Core/CombatSystem.cs`
- `Scripts/UI/GameFlowUI.cs`
- `Scripts/UI/GameFlowUI.State.cs`
- `Scripts/UI/GameFlowUI.Visuals.cs`
- `Scripts/UI/GameFlowUI.PerfectLeaderboard.cs`
- `Scenes/UI/Panels/StartPanel.tscn`
- `Scenes/UI/Panels/RestartPanel.tscn`
- `Scenes/UI/HudOverlay.tscn`

## Known Follow-Ups
- Balance pass:
  - Melee risk tuning pass #2.
  - Ranged role feel compensation.
  - Tank bonus tuning to avoid over-dominance.
- UX polish:
  - Leaderboard reset/clear action.
  - Small-window robustness (consider scroll container in start menu text area).
- Director:
  - Per-minute micro pacing within each phase.
  - Stage 1/2/3/4 random event-boss framework (future milestone).
