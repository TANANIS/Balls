param(
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path.TrimEnd('\')
Set-Location $repo

$imageFiles = Get-ChildItem Assets/Sprites -Recurse -File -Include *.png, *.jpg, *.jpeg, *.webp
$searchFiles = Get-ChildItem -Recurse -File | Where-Object {
    $p = $_.FullName
    if ($p -like "*\.git\*" -or $p -like "*\.godot\*" -or $p -like "*\bin\*" -or $p -like "*\obj\*") { return $false }
    if ($_.Extension -eq ".import") { return $false }
    if ($_.Extension -in @(".png", ".jpg", ".jpeg", ".webp", ".wav", ".mp3", ".ogg", ".dll", ".exe", ".pdb")) { return $false }
    return $true
} | Select-Object -ExpandProperty FullName

$unusedExact = foreach ($img in $imageFiles) {
    $rel = $img.FullName.Substring($repo.Length + 1)
    $resPath = "res://" + ($rel -replace "\\", "/")
    $hit = Select-String -Path $searchFiles -SimpleMatch -Pattern $resPath -ErrorAction SilentlyContinue
    if (-not $hit) { $rel }
}
$unusedExact = @($unusedExact)

$protectedPrefixes = @(
    "Assets\Sprites\Player\",
    "Assets\Sprites\Projectiles\ElementalBurst\",
    "Assets\Sprites\World\Terrain\Canonical\"
)

$deleteList = @($unusedExact | Where-Object {
    $rel = $_
    $keep = $false
    foreach ($prefix in $protectedPrefixes) {
        if ($rel.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            $keep = $true
            break
        }
    }
    -not $keep
})

$reportPath = Join-Path $repo "Tools/Quality/deleted_unused_images_2026-02-28.txt"
if ($deleteList.Count -eq 0) {
    [System.IO.File]::WriteAllText($reportPath, "", [System.Text.UTF8Encoding]::new($false))
    Write-Host "DeleteCount=0"
    exit 0
}

if (-not $DryRun) {
    foreach ($rel in $deleteList) {
        $imgPath = Join-Path $repo $rel
        $importPath = $imgPath + ".import"
        if (Test-Path $imgPath) { Remove-Item $imgPath -Force }
        if (Test-Path $importPath) { Remove-Item $importPath -Force }
    }
}

[System.IO.File]::WriteAllLines($reportPath, ($deleteList | Sort-Object), [System.Text.UTF8Encoding]::new($false))
Write-Host "DeleteCount=$($deleteList.Count)"
