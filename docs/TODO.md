# TODO (2026-02-21)
Last Synced: 2026-02-27


## Current Build Status
- [x] Character split to three roles (Ranged / Melee / TankBurst)
- [x] Run starts from menu -> character select -> single run
- [x] XP pickup flow: enemy death drops pickup, pickup grants EXP
- [x] XP model reworked to survivor-style: pickup adds EXP, full bar levels up
- [x] Four-phase 15-minute pressure timeline
- [x] Universe special events removed (phase-tail Lancer used instead)
- [x] Enemy hit feedback with knockback + white flash effect
- [x] In-run HP UI enabled as HUD module
- [x] Top XP progress bar enabled (shows next-upgrade readiness)
- [x] 15:00 match countdown shown on top-right during run
- [x] Perfect 15:00 dedicated end-state UI
- [x] Start menu Perfect 15:00 leaderboard (stored in meta save profile)
- [x] Documentation sync: upgrade flow unified to EXP pickup canonical path

## Balance - Immediate
- [x] Melee nerf pass #1
- [x] Increase melee cooldown
- [x] Increase dash cooldown for melee
- [x] Reduce melee max HP
- [ ] Melee risk tuning pass #2 (after playtest)
- [ ] Ranged feel buff pass (damage cadence / projectile feel)
- [x] Early/mid spawn pressure relief pass (`Tier0` catch-up + `Tier1` pacing density)
- [x] Enemy contact hitbox recalibration pass (oversized touch-damage rings reduced)
- [x] Tank anti-chase compensation: stronger ranged damage + bullet knockback
- [ ] Subsystem card batch #1 (at least 2-3 cards) to reduce Core/Survival over-concentration
- [ ] Re-run `Tools/CardDrawSim` after Subsystem batch and verify phase Survival ratio error <= 2%

## Director And Spawn
- [x] 15:00 split into 4 phases
- [x] Phase-tail miniboss schedule active: 03:45 / 07:30 / 11:15 / 14:30+
- [ ] Minute-by-minute micro pacing per phase (1/2/3/4 minute nodes)
- [ ] Stage-specific survival logic doc with concrete spawn targets
- [ ] Add 2 high-tier enemy types (tier 3+) and integrate into `EnemyDefinitions.csv` + `TierEnemyWeights.csv`
- [ ] GreatswordSkeleton: implement dedicated boss attack logic (separate behavior module, not shared miniboss defaults)

## UI / UX
- [x] HP UI only shows after run starts
- [x] XP bar visible at top during active run
- [x] HP HUD fallback switched to numeric-only (`HP x/y`) for current playtest cycle
- [ ] Upgrade-ready HUD polish (icon/animation/sfx sync)
- [x] EXP value differentiation by enemy type (normal/elite/boss)
- [ ] Replace HP/XP placeholder visuals with final art
- [ ] Leaderboard UX polish (rank animation / presentation)

## Fantasy Pixel Environment
- [ ] 草地陰影：決定是否重新啟用 `bg_run_grass_overlay_shadow`，並完成透明度/權重調整

## Combat Feedback
- [x] Enemy white flash on hit
- [x] Enemy small knockback on hit
- [x] Priest projectile scale down pass (readability)
- [ ] Tune flash intensity per enemy size class
- [ ] Add optional hit-stop for melee heavy strikes

## Current Follow-ups
- [ ] Priest regen VFX readability pass (timing, scale, offset) after 30s sustain tuning.
- [ ] Verify ranged continuation damage falloff feel after pierce/ricochet nerf pass.
- [ ] Run one full 15:00 balance smoke with numeric HP UI and new recycle aggressiveness.
- [ ] Verify elite_orc chain dash readability after animation rebind (`attack_03` slower second segment).
- [ ] Verify orc dash threat feel after `DashSpeedMultiplier` downshift (`3.0 -> 2.55`).

## Next Milestones
- [ ] Stage 1 random boss pool
- [ ] Stage 2 random reward event
- [ ] Stage 3 random global major event
- [ ] Stage 4 random major boss
- [ ] Melee build branch: `DASH + MELEE COMBO`
- [ ] Universe event framework return pass (postponed until 4-stage pacing is stable)

## Skill Layer - Next Focus
- [ ] Finalize skill-layer scope and naming contract (`SkillId`, category, rarity, stack policy)
- [ ] Define skill data source and authoring format (Resource/CSV) and migration plan
- [ ] Implement runtime skill application entry and compatibility gates
- [ ] Integrate skill-layer choices into upgrade menu presentation

## Test / Verification - Difficulty Curve
- [ ] Add deterministic balance simulation runner (fixed seed) for minute marks: `01:00 / 03:45 / 07:30 / 11:15 / 14:30`
- [ ] Add progression curve tests: EXP required per level should match expected piecewise curve (especially after level 5)
- [ ] Add spawn-system regression tests: tier pick validity, budget bounds, max_alive bounds, no-empty-wave under normal configs
- [ ] Add combat TTK snapshot tests by archetype vs slime/orc baseline (expected hit-to-kill range)
- [ ] Add CI gate/report for key metrics: player DPS proxy, enemy EHP proxy, spawn pressure proxy

## Meta Progression - Out-Of-Run
- [x] Architecture spec created: `docs/META_PROGRESSION_ARCHITECTURE.md`
- [x] Phase implementation plan created: `docs/META_PROGRESSION_IMPLEMENTATION_PLAN.md`
- [x] Phase 1: Domain + persistence foundation
- [x] Phase 2: Economy + settlement (soft cap curve)
- [x] Phase 3: Transaction service + character unlock gate
- [x] Phase 4 (framework): Character level + ability tree domain/defs/transaction gate
- [x] Save isolation + delete-save UX (profile-partitioned save path and in-menu delete flow)
- [ ] Phase 4 (content): Assign real node effects and bind to runtime gameplay systems

## Refactor - God File Risk
- [x] God file risk audit pass (top-10 largest scripts by responsibility, not just LOC)
- [x] Baseline refactor blueprint created: `docs/GOD_FILE_REFACTOR_BLUEPRINT.md`
- [x] Execute Phase 1 (`Bullet` decomposition)
- [x] Execute Phase 2 (`DebugCheatSystem` decomposition)
- [x] Execute Phase 3 (`UpgradeSystem` policy/apply split)
- [x] Execute Phase 4 (`GameFlowUI` controller boundaries)
- [x] Execute Phase 5 (`SpawnSystem` base-file cleanup)
- [x] Split `GameFlowUI` into explicit boundaries:
  - [x] `UIStateController` (panel transitions + flow state only)
  - [x] `UIBinding` (node refs + signal wiring only)
  - [x] `UIPresenter` (text/data rendering only)
- [x] Extract `MetaProgressionPanelController` from `GameFlowUI`:
  - [x] character select/unlock/confirm flow
  - [x] Flux/level/tree framework rendering
- [x] Extract `EndStateController` from `GameFlowUI`:
  - [x] settlement summary rendering
  - [x] restart/back-to-meta actions
  - [x] perfect-clear leaderboard refresh trigger
- [x] Consolidate duplicated settings logic into shared `SettingsPresenter`:
  - [x] start-menu settings panel
  - [x] in-run pause settings panel
- [x] Add simple script-size guardrail doc:
  - [x] target max LOC per behavior script
  - [x] rule for when to split by responsibility
