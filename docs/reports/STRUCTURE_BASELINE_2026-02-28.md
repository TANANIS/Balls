# Structure Baseline Report (2026-02-28)
Generated on: 2026-02-28

## Build and Encoding
- Build command: `dotnet build Oriluneia.sln`
- Result: success
- Warnings: `0`
- Errors: `0`

- BOM check command:
```powershell
$ext = @("*.cs","*.md","*.csv","*.tres","*.tscn","*.gd","*.json","*.cfg","*.txt")
Get-ChildItem -Recurse -File -Include $ext |
  Where-Object {
    $b = Get-Content $_.FullName -Encoding Byte -TotalCount 3
    $b.Length -eq 3 -and $b[0] -eq 239 -and $b[1] -eq 187 -and $b[2] -eq 191
  } | Select-Object -ExpandProperty FullName
```
- Result: no files returned

## Script Size Snapshot
- `Scripts/*.cs` total files: `143`
- files `>= 220` lines: `15`
- files `> 300` lines: `6`
- max line count: `863`

Top size risks:
1. `Scripts/World/ObstacleFieldGenerator.cs` (863)
2. `Scripts/Player/PlayerWeapon.cs` (422)
3. `Scripts/Player/PlayerHealth.Vfx.cs` (383)
4. `Scripts/World/ProceduralTerrainBackground.Tiling.cs` (376)
5. `Scripts/Projectiles/Bullet.cs` (372)
6. `Scripts/World/ProceduralTerrainBackground.Mask.cs` (332)

## Scene Distribution Snapshot
- total `.tscn`: `37`
- under `Enemies/`: `9`
- under `Prefabs/`: `17`
- under `Scenes/`: `9`

Observation:
- Scene placement is currently mixed across three roots (`Enemies`, `Prefabs`, `Scenes`), increasing path/reference maintenance cost.

## Reference Drift Snapshot (`.tscn` + `.tres`)
- references to `res://Prefabs/`: `16`
- references to `res://Enemies/`: `2`
- references to `res://Scenes/`: `12`

Observation:
- Legacy roots are still actively referenced and must be migrated in controlled batches.

## Working Tree Note
Baseline capture detected pre-existing local modifications not created by this pass:
- `Scenes/World/WorldRoot.tscn`
- `export_presets.cfg`
- `Data/Characters/CharacterStats.csv.import`
- `Data/Characters/CharacterStats.dash.translation`
- `Data/Characters/CharacterStats.max.translation`
- `Data/Characters/CharacterStats.melee.translation`
- `Data/Characters/CharacterStats.move.translation`
- `Data/Characters/CharacterStats.ranged.translation`
