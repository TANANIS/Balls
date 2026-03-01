param(
    [string]$PresetPath = "export_presets.cfg",
    [switch]$Apply
)

$required = @(
    "Data/Director/PressureTierRules.csv",
    "Data/Director/EnemyDefinitions.csv",
    "Data/Director/TierEnemyWeights.csv",
    "Data/Characters/CharacterStats.csv",
    "Data/Localization/Cards.csv",
    "Data/Localization/UI.csv"
)

if (-not (Test-Path -LiteralPath $PresetPath)) {
    Write-Error "File not found: $PresetPath"
    exit 2
}

$lines = Get-Content -LiteralPath $PresetPath
$updated = [System.Collections.Generic.List[string]]::new()
$changed = $false
$issues = 0

for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]

    if ($line -match '^include_filter="(.*)"$') {
        $current = $Matches[1]
        $parts = @()
        if (-not [string]::IsNullOrWhiteSpace($current)) {
            $parts = $current.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne "" }
        }

        $missing = @()
        foreach ($req in $required) {
            if (-not ($parts -contains $req)) {
                $missing += $req
            }
        }

        if ($missing.Count -gt 0) {
            $issues++
            Write-Warning "Missing CSV include(s) at line $($i + 1): $($missing -join ', ')"

            if ($Apply) {
                $all = @($parts + $missing) | Select-Object -Unique
                $newLine = 'include_filter="' + ($all -join ',') + '"'
                $updated.Add($newLine)
                $changed = $true
                continue
            }
        }
    }

    $updated.Add($line)
}

if ($Apply -and $changed) {
    $text = [string]::Join([Environment]::NewLine, $updated)
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText((Resolve-Path $PresetPath), $text, $utf8NoBom)
    Write-Host "Updated include_filter entries in $PresetPath"
}

if ($issues -gt 0) {
    if (-not $Apply) {
        Write-Host "Check failed. Re-run with -Apply to patch."
    }
    exit 1
}

$gameplayCsv = @(
    "Data/Director/PressureTierRules.csv",
    "Data/Director/EnemyDefinitions.csv",
    "Data/Director/TierEnemyWeights.csv",
    "Data/Characters/CharacterStats.csv"
)

foreach ($csv in $gameplayCsv) {
    $importPath = "$csv.import"
    if (-not (Test-Path -LiteralPath $importPath)) {
        continue
    }

    $raw = Get-Content -LiteralPath $importPath -Raw
    if ($raw -match 'importer="csv_translation"') {
        Write-Warning "Gameplay CSV appears imported as csv_translation: $importPath (recommend Keep File / No Import)"
    }
}

Write-Host "OK: all include_filter entries contain required CSV paths."
exit 0
