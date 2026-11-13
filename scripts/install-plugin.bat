@echo off
title Install Tamil Number Conversions Plugin
echo ===================================================================
echo    Install Tamil Number Conversions Plugin to Paratext 9
echo ===================================================================
echo.
echo Notice: If Paratext 9 is open, please close it now.
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0install-plugin.ps1"
echo.
pause
