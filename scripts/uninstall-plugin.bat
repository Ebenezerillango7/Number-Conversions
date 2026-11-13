@echo off
title Uninstall Tamil Number Conversions Plugin
echo ===================================================================
echo   Uninstall Tamil Number Conversions Plugin from Paratext 9
echo ===================================================================
echo.
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "try { Get-ChildItem -Path '%~dp0*' -File | Unblock-File -ErrorAction SilentlyContinue } catch {}; & '%~dp0uninstall-plugin.ps1' -WaitForParatextExit -NotifyWhenDone"
echo.
pause
