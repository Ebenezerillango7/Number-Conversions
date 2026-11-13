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

# Unblock files in source folder if downloaded from the web
try {
    Get-ChildItem -Path $PSScriptRoot -File | Unblock-File -ErrorAction SilentlyContinue
}
catch { }

# Verify Paratext is not running; wait if open
$running = Get-Process -Name "Paratext*" -ErrorAction SilentlyContinue
if ($running) {
    Write-Host "`nNotice: Paratext 9 is currently running." -ForegroundColor Yellow
    Write-Host "Please save your translation work and close Paratext 9..." -ForegroundColor Cyan
    Write-Host "Waiting for Paratext 9 to close before continuing..." -ForegroundColor Gray
    $running | Wait-Process -Timeout 300 -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "Paratext 9 closed. Resuming installation..." -ForegroundColor Green
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

# Unblock files to remove Windows "Mark of the Web" (Zone.Identifier) on downloaded zip files
try {
    Get-ChildItem -Path $targetPluginDir -File | Unblock-File -ErrorAction SilentlyContinue
    Write-Host "Unblocked plugin files for Windows security compliance." -ForegroundColor Green
}
catch { }

Write-Host "Copied plugin assemblies successfully:" -ForegroundColor Green
Get-ChildItem $targetPluginDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize

# Refresh Paratext Plugin Cache
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

try {
    [System.Reflection.Assembly]::LoadWithPartialName('System.AddIn') | Out-Null
    [System.AddIn.Hosting.AddInStore]::RebuildAddIns((Join-Path $ParatextDir 'plugins')) | Out-Null
    Write-Host "Paratext AddIn cache rebuilt successfully." -ForegroundColor Green
}
catch { }

Write-Host "`nInstallation Complete! Tamil Number Conversions plugin is ready in Paratext 9." -ForegroundColor Green

try {
    Add-Type -AssemblyName System.Windows.Forms -ErrorAction SilentlyContinue
    [System.Windows.Forms.MessageBox]::Show(
        "Tamil Number Conversions plugin has been successfully installed into Paratext 9!`n`nYou can now open Paratext and access Tools -> Tamil Number Conversions...",
        "Installation Successful",
        [System.Windows.Forms.MessageBoxButtons]::OK,
        [System.Windows.Forms.MessageBoxIcon]::Information
    ) | Out-Null
}
catch { }
