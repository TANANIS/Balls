# System Flow Diagram
Last Synced: 2026-03-01


```mermaid
flowchart TD
    A[Game Start] --> B[GameFlowUI\nMenu Screen]
    B --> P[Pre-Run Event Loadout UI\n4 Slots]
    P --> P1[RunPlanBuilder]
    P1 --> P2[DistortionResolver\nMax same-domain chain = 2]
    P2 --> P3[AffinityResolver\nAdjacent slots only]
    P3 --> C[Start Run]

    C --> C1[Match Timer\n15:00]
    C1 --> C2[Phase Router\n0-3:45 / 3:45-7:30 / 7:30-11:15 / 11:15-15:00]
    C --> D[SpawnSystem]
    C --> E[ProgressionSystem]
    C --> N[StabilitySystem]
    C --> ED[EventDirector]
    C --> F[Player/Combat Loop]
    C --> U[HUD Overlay\nHP + XP + Countdown + Event Banner]

    C2 --> D
    C2 --> N
    ED -->|Slot Timestamp Reached| ER[EventRunner]
    ER -->|Apply Event Rule Set| F
    ER -->|Event Complete| RW[RewardService]
    RW -->|Shard Ledger Update| RB[Run Reward Buffer]

    D -->|Spawn Enemies| F
    F -->|Enemy Killed| G[CombatSystem]
    G -->|EnemyKilled Event| X[ExperienceDropSystem]
    G -->|MiniBoss Killed by Player| B1[Boss Bonus\n+1 Level +10 EXP]
    B1 --> E
    X -->|Spawn Pickup| Y[ExperiencePickup]
    Y -->|Player Collects| E

    E -->|EXP Filled -> Queue LevelUp| H[UpgradeMenu]
    H -->|Player Picks Upgrade| I[UpgradeSystem]
    I --> F
    E -->|Progress/Ready State| U

    F -->|Player Damaged| J[PlayerHealth]
    J -->|Current HP/Max HP| U
    J -->|Regen Tick 30s| J1[Priest Heal VFX]
    J1 -->|Lock attacks during VFX| F

    F -->|Player Died| L[GameFlowUI\nRestart Panel]
    C1 -->|15:00 Reached| R[Perfect Clear Panel]
    L --> M[ScoreSystem]
    R --> M

    M --> S[MetaProgressionService\nSettleRun]
    RB --> S
    S --> B
```

## Runtime Notes
- Event scheduling:
  - run plan is fixed pre-run; no mid-run random event picks.
  - distortion and affinity are precomputed from adjacent slot sequence.
- Time intensity rule:
  - "pressure" is game-time intensity.
  - balancing should use timestamps, durations, and tier time profiles.
- Spawn pacing sync:
  - catch-up uses `HordeTargetAliveRatio = 0.82` and `HordeCatchUpBudgetFactor = 0.22`.
  - Tier1 pacing row uses `spawn_interval 1.70~2.35`, `max_alive = 24`.
- Boss bonus sync:
  - defeating a miniboss grants immediate progression bonus (`+1` level and `+10` EXP, temporary tuning).
- EventRunner execution sync:
  - active slot effects are consumed by player/enemy movement and projectile loops each physics tick.
  - Blood Tide can enqueue directional rush packs through `SpawnSystem.EventSpawnDirectionalRush(...)`.
- Reward settlement sync:
  - completed slot rewards are buffered by director-owned `RewardService`.
  - end-state settlement passes `RunResult.DomainShardRewardsByDomain` into `MetaProgressionService.SettleRun(...)`.
- HUD HP rendering:
  - in-run HP is numeric only (`HP x/y`), segment blocks are removed.
