param(
    [string]$ParatextDir = ""
)

$ErrorActionPreference = "Stop"

Write-Host "Installing Tamil Number Conversions Plugin to Paratext 9..." -ForegroundColor Cyan

# Auto-detect Paratext installation directory if not specified
if ([string]::IsNullOrWhiteSpace($ParatextDir)) {
    $candidateDirs = @(
        "D:\Transdata\Paratext 9",
        "C:\Program Files\Paratext 9",
        "C:\Program Files (x86)\Paratext 9",
        "D:\Program Files\Paratext 9"
    )

    foreach ($dir in $candidateDirs) {
        if (Test-Path (Join-Path $dir "Paratext.exe")) {
            $ParatextDir = $dir
            break
        }
    }

    if ([string]::IsNullOrWhiteSpace($ParatextDir)) {
        Write-Error "Could not automatically locate Paratext 9. Please specify -ParatextDir '<path-to-paratext-9>'."
        exit 1
    }
}

Write-Host "Found Paratext 9 at: $ParatextDir" -ForegroundColor Green

# Verify Paratext is not running
$running = Get-Process -Name "Paratext*" -ErrorAction SilentlyContinue
if ($running) {
    Write-Warning "Paratext is currently running. Please close Paratext before installing the plugin."
    exit 1
}

$targetPluginDir = Join-Path $ParatextDir "plugins\NumberConversions"
if (-not (Test-Path $targetPluginDir)) {
    New-Item -ItemType Directory -Force -Path $targetPluginDir | Out-Null
    Write-Host "Created plugin directory: $targetPluginDir" -ForegroundColor Green
}

# Resolve source files location
$candidateSources = @(
    $PSScriptRoot,
    (Join-Path $PSScriptRoot "dist\NumberConversions"),
    (Join-Path $PSScriptRoot "..\dist\NumberConversions"),
    (Join-Path $PSScriptRoot "..\src\NumberConversions.Plugin\bin\Release\net48")
)

$sourceDir = ""
foreach ($cand in $candidateSources) {
    if (Test-Path (Join-Path $cand "NumberConversions.ptxplg")) {
        $sourceDir = $cand
        break
    }
}

if ([string]::IsNullOrWhiteSpace($sourceDir)) {
    Write-Error "Could not find plugin files ('NumberConversions.ptxplg'). Ensure the release package is unzipped properly."
    exit 1
}

Write-Host "Installing from: $sourceDir" -ForegroundColor Cyan
Copy-Item "$sourceDir\NumberConversions.ptxplg" -Destination $targetPluginDir -Force
Copy-Item "$sourceDir\NumberConversions.Core.dll" -Destination $targetPluginDir -Force

$uninstallScript = Join-Path $sourceDir "uninstall-plugin.ps1"
if (-not (Test-Path $uninstallScript)) {
    $uninstallScript = Join-Path $PSScriptRoot "uninstall-plugin.ps1"
}
if (Test-Path $uninstallScript) {
    Copy-Item $uninstallScript -Destination $targetPluginDir -Force
}

Write-Host "Copied plugin assemblies successfully:" -ForegroundColor Green
Get-ChildItem $targetPluginDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize

# Run Paratext Plugin Cache updater if present
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

Write-Host "`nInstallation Complete! Launch Paratext 9 and open Tools -> Tamil Number Conversions..." -ForegroundColor Green
