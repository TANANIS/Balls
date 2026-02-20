# TODO (2026-02-20)

## Current Build Status
- [x] Character split to three roles (Ranged / Melee / TankBurst)
- [x] Run starts from menu -> character select -> single run
- [x] XP pickup flow: enemy death drops pickup, pickup grants EXP
- [x] XP model reworked to survivor-style: pickup adds EXP, full bar levels up
- [x] Four-phase 15-minute pressure timeline
- [x] Universe special events removed (phase-tail MiniBossHex used instead)
- [x] Enemy hit feedback with knockback + white flash effect
- [x] In-run HP UI enabled as HUD module
- [x] Top XP progress bar enabled (shows next-upgrade readiness)
- [x] 15:00 match countdown shown on top-right during run
- [x] Perfect 15:00 dedicated end-state UI
- [x] Start menu local leaderboard for Perfect 15:00 (score + date + character)

## Balance - Immediate
- [x] Melee nerf pass #1
- [x] Increase melee cooldown
- [x] Increase dash cooldown for melee
- [x] Reduce melee max HP
- [ ] Melee risk tuning pass #2 (after playtest)
- [ ] Ranged feel buff pass (damage cadence / projectile feel)
- [x] Tank anti-chase compensation: stronger ranged damage + bullet knockback

## Director And Spawn
- [x] 15:00 split into 4 phases
- [x] Phase-tail miniboss schedule active: 03:45 / 07:30 / 11:15 / 14:30+
- [ ] Minute-by-minute micro pacing per phase (1/2/3/4 minute nodes)
- [ ] Stage-specific survival logic doc with concrete spawn targets

## UI / UX
- [x] HP UI only shows after run starts
- [x] XP bar visible at top during active run
- [ ] Upgrade-ready HUD polish (icon/animation/sfx sync)
- [ ] EXP value differentiation by enemy type (normal/elite/boss)
- [ ] Replace HP/XP placeholder visuals with final art
- [ ] Leaderboard UX polish (clear/reset button, rank animation)

## Combat Feedback
- [x] Enemy white flash on hit
- [x] Enemy small knockback on hit
- [ ] Tune flash intensity per enemy size class
- [ ] Add optional hit-stop for melee heavy strikes

## Next Milestones
- [ ] Stage 1 random boss pool
- [ ] Stage 2 random reward event
- [ ] Stage 3 random global major event
- [ ] Stage 4 random major boss
- [ ] Melee build branch: `DASH + MELEE COMBO`
