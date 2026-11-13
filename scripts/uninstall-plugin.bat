@echo off
title Uninstall Tamil Number Conversions Plugin
echo ===================================================================
echo   Uninstall Tamil Number Conversions Plugin from Paratext 9
echo ===================================================================
echo.
echo Notice: If Paratext 9 is open, please close it now.
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0uninstall-plugin.ps1"
echo.
pause
