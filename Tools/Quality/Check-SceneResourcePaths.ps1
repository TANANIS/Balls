Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
Set-Location $repoRoot

$sourceFiles = Get-ChildItem -Recurse -File -Include *.tscn, *.tres | Select-Object -ExpandProperty FullName
$missing = @()

foreach ($file in $sourceFiles) {
    $content = Get-Content $file -Raw
    $matches = [regex]::Matches($content, 'path="res://([^"]+)"')
    foreach ($m in $matches) {
        $resPath = $m.Groups[1].Value
        $diskPath = Join-Path $repoRoot ($resPath -replace '/', '\')
        if (-not (Test-Path $diskPath)) {
            $missing += [PSCustomObject]@{
                Source = $file.Replace("$repoRoot\", "")
                Missing = "res://$resPath"
            }
        }
    }
}

if (@($missing).Count -eq 0) {
    Write-Host "No missing scene/resource paths found."
    exit 0
}

Write-Host "Missing scene/resource paths detected:" -ForegroundColor Red
$missing | Format-Table -AutoSize
exit 1
