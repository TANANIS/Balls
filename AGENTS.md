# AGENTS.md

This file is the operational contract for coding agents in this repository.
If any implementation detail is unclear, prioritize rules in this file first, then validate against source docs under `docs/`.

## 1) Non-Negotiable Rules

- All text/code/resource text files must be `UTF-8 without BOM`.
- Applies to: `*.cs`, `*.md`, `*.csv`, `*.tres`, `*.tscn`, `*.gd`, `*.json`, `*.cfg`, `*.txt`.
- Godot parser symptom for wrong encoding: `Parse Error: Expected '['` at line 1.
- Damage pipeline is centralized: only `CombatSystem` resolves and applies damage.
- Gameplay sensors (`Bullet`, `Hitbox`, `Hurtbox`) submit `DamageRequest`; they must not deduct HP directly.
- Keep balance tuning data-driven first (`Data/Director/*.csv`, upgrade catalog), code second.
- UI is presentation/input only. Systems may be called by UI; systems must not depend on UI implementation.

## 2) Required Pre-Commit Checks

1. Build:
   - `dotnet build ProjectGenesis.sln`
2. Encoding/BOM check (must return no files):
   - PowerShell:
   ```powershell
   $ext = @("*.cs","*.md","*.csv","*.tres","*.tscn","*.gd","*.json","*.cfg","*.txt")
   Get-ChildItem -Recurse -File -Include $ext |
     Where-Object {
       $b = Get-Content $_.FullName -Encoding Byte -TotalCount 3
       $b.Length -eq 3 -and $b[0] -eq 239 -and $b[1] -eq 187 -and $b[2] -eq 191
     } | Select-Object -ExpandProperty FullName
   ```

## 3) Runtime Architecture Contract

Source: `docs/ARCHITECTURE.md`, `docs/SYSTEM_FLOW.md`

- Runtime root:
  - `Game`
  - `World`
  - `Player`
  - `Enemies`
  - `Projectiles`
  - `Systems`
  - `CanvasLayer/UI`
- Core boundaries:
  - `Systems/Core`: universal runtime services (`CombatSystem`).
  - `Systems/Director`: pacing/encounter orchestration (`SpawnSystem`, `StabilitySystem`).
  - `Systems/Progression`: EXP + upgrade flow (`ProgressionSystem`, `UpgradeSystem`).
  - `Scripts/UI/*`: UI binding/state/presentation only.
  - `Scripts/Audio/*`: centralized playback API.

## 4) 15-Minute Gameplay Contract

Source: `docs/GAME_CONCEPT.md`, `docs/GameDirector_Design.md`

- Run duration is fixed at `15:00`.
- Current staged timeline:
  - `00:00-03:45`
  - `03:45-07:30`
  - `07:30-11:15`
  - `11:15-15:00`
- Tail miniboss markers:
  - `03:45`, `07:30`, `11:15`, `14:30~15:00`.
- EXP/upgrade path:
  - enemy death -> `ExperienceDropSystem` -> `ExperiencePickup` -> `ProgressionSystem` EXP -> queued level-up -> `UpgradeMenu` -> `UpgradeSystem`.
- HUD run-time contract:
  - HP and XP visible only after run starts.
  - countdown shown during active run.

## 5) Upgrade/Card System Contract

Source: `docs/CARDS.md`, `docs/CARDS_CHANGELOG.md`

- Dual-axis data model:
  - `Layer`: phase-pool routing.
  - `Category`: weight-decay/statistics axis.
- Early/Mid/Late pool weights:
  - Early: Survival 40 / CoreAttack 40 / Subsystem 10 / Modifier 5 / Economy 5.
  - Mid: Survival 20 / CoreAttack 40 / Subsystem 25 / Modifier 15.
  - Late: Survival 10 / CoreAttack 30 / Subsystem 30 / Modifier 30.
- Strong survival cards use timing gates:
  - `MinUpgradeCount`, `MinPhase`, optional `MaxPhase`.
- Multiplicative cards require safety fuses:
  - stack caps, diminishing returns, mutual exclusion where necessary.
- Known enforced archetype rule:
  - `ATK_PROJECTILE_PLUS_1` and `ATK_SPLIT_SHOT` are mutually exclusive.
- New/changed cards must sync:
  - `UpgradeId`
  - catalog (`Data/Upgrades/DefaultUpgradeCatalog.tres`)
  - localization keys/text (`Data/Localization/Cards.csv`)
  - runtime apply switch (`UpgradeSystem.Apply`)
  - changelog (`docs/CARDS_CHANGELOG.md`)

## 6) Projectile/VFX Contract

Source: `docs/CARDS.md` (`Projectile Variant Convention`), `docs/ARCHITECTURE.md`

- Shared runtime behavior stays in `Scripts/Projectiles/Bullet.cs`.
- Variants should be prefab-driven (`WizardProjectile`, `PriestProjectile`, `SplitProjectile`, etc.).
- Split shot child spawn prefers `SplitChildProjectileScene`.
- Player skill visuals attach under `Player/SkillVfxRoot`.
- Do not hardcode random asset paths in gameplay logic; keep deterministic fallback paths.

## 7) Character/Meta Progression Contract

Source: `docs/META_PROGRESSION_ARCHITECTURE.md`, `docs/META_PROGRESSION_IMPLEMENTATION_PLAN.md`

- `MetaProgressionService` is the single write entry for meta mutations.
- Reward uses soft-cap curve + optional tail.
- Persist immediately after successful transaction.
- Character unlock/level/tree checks must go through service APIs.
- Run settlement must be idempotent (`RunId` anti-duplicate).

## 8) Art + Pixel Scale Contract

Source: `docs/PIXEL_SCALE_MASTER_SPEC.md`, `docs/FANTASY_PIXEL_STYLE_SPEC.md`, `docs/ART_DIFFERENTIATION_FANTASY_PIXEL_PLAN.md`

- Gameplay reference canvas: `480x270`.
- Player source baseline: `32x32`.
- Camera baseline should satisfy player occupancy `~6%` of gameplay height.
- Pixel import defaults:
  - filter `Nearest`
  - mipmaps off
  - repeat disabled unless intentional tiling
- Collision is gameplay-first; do not auto-scale colliders from arbitrary visual scaling.
- Avoid random non-integer obstacle scale/rotation as a substitute for authored variation.

## 9) Refactor and File-Size Guardrails

Source: `docs/SCRIPT_SIZE_GUARDRAILS.md`, `docs/SCRIPT_REFACTOR_PLAN.md`, `docs/SCENE_SPLIT_NOTES.md`

- Script size thresholds:
  - target `<= 300` lines
  - warning `>= 220`
  - hard review `> 300`
- Split by responsibility (lifecycle, policy, apply, binding, presenter, etc.), not by arbitrary line blocks.
- Current scene composition root is valid:
  - `Scenes/Player.tscn`
  - `Scenes/Systems/SystemsRoot.tscn`
  - `Scenes/UI/GameFlowUIRoot.tscn`
  - `Scenes/World/WorldRoot.tscn`
- Keep `MainScence.tscn` as composition root unless strong reason to split further.

## 10) Current Priority Backlog (Operational)

Source: `docs/TODO.md`

- Immediate:
  - melee risk tuning pass #2
  - ranged feel buff pass
  - minute-by-minute pacing refinement
  - upgrade-ready HUD polish
  - final HP/XP art replacement
- Card system:
  - deterministic draw simulator (fixed seed, 10,000 runs, distribution report).
- Meta progression:
  - bind real ability-tree node effects to runtime gameplay.

## 11) Documentation Maintenance Rule

- If behavior/data contracts change, update the relevant docs in the same change set:
  - architecture/system flow changes -> `docs/ARCHITECTURE.md`, `docs/SYSTEM_FLOW.md`
  - upgrade/card changes -> `docs/CARDS.md`, `docs/CARDS_CHANGELOG.md`
  - balancing priorities -> `docs/TODO.md`
  - structural refactor boundary changes -> `docs/SCRIPT_REFACTOR_PLAN.md` or guardrail docs
- Keep `AGENTS.md` as an execution-focused index, not a changelog.

