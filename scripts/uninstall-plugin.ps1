param(
    [string]$ParatextDir = "",
    [switch]$WaitForParatextExit,
    [switch]$NotifyWhenDone
)

$ErrorActionPreference = "Stop"

# Change working directory to TEMP so no handles are held on the plugin directory
Set-Location $env:TEMP

Write-Host "Uninstalling Tamil Number Conversions Plugin from Paratext 9..." -ForegroundColor Cyan

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
        # Check parent of script root
        $cand = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
        if (Test-Path (Join-Path $cand "Paratext.exe")) {
            $ParatextDir = $cand
        }
    }

    if ([string]::IsNullOrWhiteSpace($ParatextDir)) {
        Write-Error "Could not automatically locate Paratext 9. Please specify -ParatextDir '<path-to-paratext-9>'."
        exit 1
    }
}

Write-Host "Target Paratext directory: $ParatextDir" -ForegroundColor Green

$targetPluginDir = Join-Path $ParatextDir "plugins\NumberConversions"

if (-not (Test-Path $targetPluginDir)) {
    Write-Host "Plugin folder '$targetPluginDir' does not exist. Already uninstalled." -ForegroundColor Yellow
    if ($NotifyWhenDone) {
        Add-Type -AssemblyName System.Windows.Forms
        [System.Windows.Forms.MessageBox]::Show(
            "Tamil Number Conversions plugin is not installed in Paratext 9.",
            "Plugin Not Found",
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Information
        ) | Out-Null
    }
    exit 0
}

# If WaitForParatextExit is passed, wait for Paratext to close
if ($WaitForParatextExit) {
    Write-Host "Waiting for Paratext to exit..." -ForegroundColor Cyan
    $procs = Get-Process -Name "Paratext*" -ErrorAction SilentlyContinue
    if ($procs) {
        $procs | Wait-Process -Timeout 300 -ErrorAction SilentlyContinue
    }
    # Extra safety delay for Windows OS to release open DLL file locks
    Start-Sleep -Seconds 2
} else {
    # Check if Paratext is running
    $procs = Get-Process -Name "Paratext*" -ErrorAction SilentlyContinue
    if ($procs) {
        Write-Warning "Paratext is currently running. Please close Paratext before uninstalling."
        exit 1
    }
}

Write-Host "Removing plugin directory: $targetPluginDir..." -ForegroundColor Cyan
try {
    Remove-Item -Path $targetPluginDir -Recurse -Force
    Write-Host "Plugin directory removed successfully." -ForegroundColor Green
}
catch {
    Write-Error "Failed to remove plugin directory: $($_.Exception.Message)"
    exit 1
}

# Run Paratext Plugin Cache updater to rebuild AddIns.store
$cacheExe = Join-Path $ParatextDir "UpdateParatextPluginCache.exe"
if (Test-Path $cacheExe) {
    try {
        Write-Host "Updating Paratext Plugin Cache..." -ForegroundColor Cyan
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
}
catch { }

Write-Host "`nUninstallation Complete! Tamil Number Conversions plugin has been cleanly removed from Paratext 9." -ForegroundColor Green

if ($NotifyWhenDone) {
    Add-Type -AssemblyName System.Windows.Forms
    [System.Windows.Forms.MessageBox]::Show(
        "Tamil Number Conversions plugin has been successfully uninstalled from Paratext 9.`n`nYou can now reopen Paratext.",
        "Plugin Uninstalled",
        [System.Windows.Forms.MessageBoxButtons]::OK,
        [System.Windows.Forms.MessageBoxIcon]::Information
    ) | Out-Null
}
