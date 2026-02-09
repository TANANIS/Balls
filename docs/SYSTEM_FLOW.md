# System Flow Diagram

```mermaid
flowchart TD
    A[Game Start] --> B[GameFlowUI\nMenu Screen]
    B -->|Start Button| C[Start Run]
    C --> D[SpawnSystem]
    C --> E[PressureSystem]
    C --> F[Player/Combat Loop]

    D -->|Spawn Enemies| F
    E -->|Pressure Target| E
    F -->|Enemy Killed| G[CombatSystem]
    G -->|EnemyKilled Event| E

    E -->|Upgrade Progress >= Threshold| H[UpgradeMenu]
    H -->|Player Picks Upgrade| I[UpgradeSystem]
    I --> F

    F -->|Player Damaged| J[PlayerHealth]
    J -->|Low HP| K[Low HP Vignette + Low HP SFX]

    F -->|Player Died| L[GameFlowUI\nRestart Panel]
    L -->|Show Final Score| M[ScoreSystem]

    D -->|Late Game (>= 60s)\nExponential Scaling| D
    D -->|Elite Injection / MiniBoss| F
```
