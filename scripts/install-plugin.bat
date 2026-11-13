@echo off
title Install Tamil Number Conversions Plugin
echo ===================================================================
echo    Install Tamil Number Conversions Plugin to Paratext 9
echo ===================================================================
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "try { Get-ChildItem -Path '%~dp0*' -File | Unblock-File -ErrorAction SilentlyContinue } catch {}; & '%~dp0install-plugin.ps1'"
echo.
pause
