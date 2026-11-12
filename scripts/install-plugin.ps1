param(
    [string]$ParatextDir = "D:\Transdata\Paratext 9"
)

$ErrorActionPreference = "Stop"

Write-Host "Installing Number Conversions Plugin to Paratext 9..." -ForegroundColor Cyan

$targetPluginDir = Join-Path $ParatextDir "plugins\NumberConversions"
if (-not (Test-Path $targetPluginDir)) {
    New-Item -ItemType Directory -Force -Path $targetPluginDir | Out-Null
    Write-Host "Created plugin directory: $targetPluginDir" -ForegroundColor Green
}

$sourceDir = Join-Path $PSScriptRoot "..\dist\NumberConversions"
if (-not (Test-Path $sourceDir)) {
    $sourceDir = Join-Path $PSScriptRoot "..\src\NumberConversions.Plugin\bin\Release\net48"
}

Copy-Item "$sourceDir\NumberConversions.ptxplg" -Destination $targetPluginDir -Force
Copy-Item "$sourceDir\NumberConversions.Core.dll" -Destination $targetPluginDir -Force

Write-Host "Copied plugin assemblies successfully:" -ForegroundColor Green
Get-ChildItem $targetPluginDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize

# Run Paratext Plugin Cache updater if present (or Paratext updates automatically on launch)
$cacheExe = Join-Path $ParatextDir "UpdateParatextPluginCache.exe"
if (Test-Path $cacheExe) {
    try {
        Write-Host "Triggering Paratext Plugin Cache refresh..." -ForegroundColor Cyan
        Start-Process -FilePath $cacheExe -Wait -WindowStyle Hidden -ErrorAction SilentlyContinue
        Write-Host "Paratext Plugin Cache updated." -ForegroundColor Green
    }
    catch {
        Write-Host "Note: Paratext will automatically refresh the plugin cache upon launch." -ForegroundColor Yellow
    }
}

Write-Host "`nInstallation Complete! Restart Paratext 9 and open Tools -> Tamil Number Conversions..." -ForegroundColor Green
