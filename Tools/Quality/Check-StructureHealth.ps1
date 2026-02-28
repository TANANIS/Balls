param(
    [switch]$SkipBuild,
    [switch]$FailOnHardReview
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
Set-Location $repoRoot

$ext = @("*.cs", "*.md", "*.csv", "*.tres", "*.tscn", "*.gd", "*.json", "*.cfg", "*.txt")
$hardReviewThreshold = 300
$warningThreshold = 220

$failed = $false

Write-Host "== Structure Health Check =="
Write-Host "Repo: $repoRoot"

if (-not $SkipBuild) {
    Write-Host ""
    Write-Host "[1/4] Build"
    dotnet build ProjectGenesis.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed." -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

Write-Host ""
Write-Host "[2/4] Script line-size snapshot"
$scriptRows = Get-ChildItem Scripts -Recurse -Filter *.cs | ForEach-Object {
    [PSCustomObject]@{
        Path = $_.FullName.Replace("$repoRoot\", "")
        Lines = (Get-Content $_.FullName | Measure-Object -Line).Lines
    }
}

$totalScripts = @($scriptRows).Count
$warningCount = @($scriptRows | Where-Object { $_.Lines -ge $warningThreshold }).Count
$hardCount = @($scriptRows | Where-Object { $_.Lines -gt $hardReviewThreshold }).Count
$topRows = $scriptRows | Sort-Object Lines -Descending | Select-Object -First 10

Write-Host "Scripts total: $totalScripts"
Write-Host ">= $warningThreshold lines: $warningCount"
Write-Host "> $hardReviewThreshold lines: $hardCount"
Write-Host "Top 10 large scripts:"
$topRows | Format-Table -AutoSize

if ($FailOnHardReview -and $hardCount -gt 0) {
    Write-Host "Hard review threshold violated." -ForegroundColor Yellow
    $failed = $true
}

Write-Host ""
Write-Host "[3/4] Scene/resource path reference snapshot"
$pathsToCheck = @("res://Prefabs/", "res://Enemies/", "res://Scenes/")
$sceneResourceFiles = Get-ChildItem -Recurse -File -Include *.tscn, *.tres | Select-Object -ExpandProperty FullName
$pathCounts = @{}
foreach ($p in $pathsToCheck) {
    $pattern = [regex]::Escape($p)
    $count = (Select-String -Path $sceneResourceFiles -Pattern $pattern | Measure-Object).Count
    $pathCounts[$p] = $count
    Write-Host "$p => $count"
}

if ($pathCounts["res://Prefabs/"] -gt 0 -or $pathCounts["res://Enemies/"] -gt 0) {
    Write-Host "Legacy scene roots detected in resource references." -ForegroundColor Yellow
    $failed = $true
}

Write-Host ""
Write-Host "[4/4] UTF-8 BOM scan"
$bomFiles = @(Get-ChildItem -Recurse -File -Include $ext | Where-Object {
    if ($_.FullName -like "*\.godot\*") { return $false }
    if ($_.FullName -like "*\bin\*") { return $false }
    if ($_.FullName -like "*\obj\*") { return $false }
    $b = @(Get-Content $_.FullName -Encoding Byte -TotalCount 3)
    $b.Count -eq 3 -and $b[0] -eq 239 -and $b[1] -eq 187 -and $b[2] -eq 191
} | Select-Object -ExpandProperty FullName)

if ($bomFiles.Count -eq 0) {
    Write-Host "No BOM files found."
} else {
    Write-Host "BOM files detected:" -ForegroundColor Red
    $bomFiles | ForEach-Object { Write-Host $_ }
    $failed = $true
}

Write-Host ""
if ($failed) {
    Write-Host "Structure health check completed with failures." -ForegroundColor Red
    exit 1
}

Write-Host "Structure health check completed."
