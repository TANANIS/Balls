# Dev Log (Codex Internal, Trimmed)

## Purpose
- This file is a short working memory for Codex.
- Detailed historical context should be read from git history and docs.
- Keep only currently actionable information.

## Current Runtime Baseline (2026-02-25)
- Project: Godot C# top-down survival run (`15:00` target).
- Upgrade pipeline:
  - runtime source is `UpgradeSystem` + `UpgradeCatalog`.
  - card pool currently uses Batch 01 (`11` active cards).
  - dual-axis contract is active:
    - `Layer` for phase pool routing,
    - `Category` for decay/statistics.
- Pool routing:
  - phase pools: Early / Mid / Late.
  - Early pool ratio aligned to 100%.
- Safety fuses:
  - stack caps and mutual exclusion validation are active.
  - category weight decay is active.
- Debug panel (F3):
  - bilingual panel text.
  - opening F3 pauses game; closing restores prior pause state.
  - direct apply upgrade uses debug path (`DebugApplyUpgrade`) and bypasses gate/exclusive/prerequisite checks while still respecting stack cap and character compatibility.

## High-Signal Recent Changes

### 2026-02-25 - Card Framework Alignment
- Removed cooldown card (`ATK_COOLDOWN_DOWN_10`) from runtime/catalog/localization/apply path.
- Added timing gates for strong survival cards via card definition fields:
  - `MinUpgradeCount`, `MinPhase`, optional `MaxPhase` gate.
- Updated docs and changelog to reflect dual-axis contract and gating rules.

### 2026-02-25 - Projectile Identity Split
- `ATK_PROJECTILE_PLUS_1`:
  - same-axis tight-spread volley (single-target pressure), `MaxStack = 2`.
- `ATK_SPLIT_SHOT`:
  - on-hit split from enemy position,
  - split count scaling: `3 -> 4 -> 5 -> 6` (360 deg spread),
  - non-chain split children,
  - split child damage path uses fractional accumulation baseline (`0.5`).
- Mutual exclusion enforced between `ATK_PROJECTILE_PLUS_1` and `ATK_SPLIT_SHOT`.

### 2026-02-25 - Shared Script + Variant Prefab Projectile Convention
- Shared runtime script remains `Scripts/Projectiles/Bullet.cs`.
- Split child spawn now prefers exported prefab field:
  - `SplitChildProjectileScene`.
- Fallback order:
  - `SplitChildProjectileScene -> source projectile scene -> current scene file`.
- Dedicated split prefab:
  - `Prefabs/SplitProjectile.tscn`.
- Current split visual baseline:
  - `Assets/Sprites/SPLITBULLET/1..7.png`,
  - flight `0..5`, impact `6`,
  - scale tuned to about `2/3` of primary,
  - collider radius `9`.

### 2026-02-25 - New Modifier Card `MOD_ELEMENTAL_BURST`
- Availability and weight:
  - `MinPhase = Early`, `Weight = 8`, `MaxStack = 1`.
- Runtime behavior:
  - charge every `5s`,
  - charged state is retained if player does not fire,
  - next shot becomes explosive,
  - detonate on first hit or max travel distance.
- Current tuned values:
  - radius `130`,
  - damage scale `1.20x`,
  - max distance `280`,
  - max targets `5`.
- VFX:
  - projectile frames: `Assets/Sprites/MOD_ELEMENTAL_BURST/1..8.png`,
  - explosion animation: `explotion1..5.png` (fade starts on final frame),
  - fallback rune: `explotion.png`.
- Audio:
  - `Assets/Sound/Player/sfx_player_elemental_burst.wav`,
  - loaded by `AudioManager.Setup`,
  - played on detonation in `Bullet`.

### 2026-02-25 - Spawn Stall Mitigation
- Added far-enemy recycle/leash handling to reduce situations where enemy count cap blocks new spawns while old enemies remain ineffective/off-screen.

### 2026-02-25 - Fantasy Copy Refresh (UI + Character + Card Text)
- Start/pause/end and upgrade-related UI strings were rewritten to remove sci-fi leftovers and align with fantasy wording.
- Character presentation text cleanup:
  - `Mage / 法師` naming now replaces old `Ranger Core / 遊俠核心`.
  - Melee/Tank zh_TW placeholders were replaced with finalized descriptions.
- Card display copy refresh (mechanics unchanged, ids unchanged):
  - `Split Shot -> Shatter Shot`
  - `+1 Projectile -> +1 Arc Bolt`
  - `EXP Gain -> Essence Gain`
  - `Shield -> Ward Shield`
  - `Kill-Chance Lifesteal -> Blood Rite`
- Fallback-path consistency:
  - `Data/Characters/*.tres` + `GameFlowUI` fallback builders now use the same revised wording.
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).

### 2026-02-25 - README Sync (Current Mainline Snapshot)
- Updated `README.md` to reflect current mainline status:
  - fantasy presentation direction,
  - live card/progression snapshot,
  - projectile shared-script + variant-prefab convention,
  - F3 debug panel behavior summary,
  - branch note (`legacy/old-scifi-main` archive).
- Validation:
  - `dotnet build ProjectGenesis.sln` succeeded (0 errors, 0 warnings).

## Canonical Specs and Docs
- Card spec: `docs/CARDS.md`
- Card change history: `docs/CARDS_CHANGELOG.md`
- Active backlog: `docs/TODO.md`

## Open Work (Current)
- Implement deterministic draw simulator (editor/console):
  - fixed seed,
  - 10,000 runs,
  - N picks per run,
  - output card rates, pair/triple frequency, ceiling-combo probability,
  - verify phase survival ratio error <= 2%.
- Run in-game smoke pass for the latest card/runtime tuning.
- Keep localization rows aligned when adding/removing cards.
