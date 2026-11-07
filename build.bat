@echo off
title DevourClient setup
REM One click: finds DEVOUR, installs .NET + MelonLoader if needed, builds the mod and puts it in DEVOUR\Mods.
REM Optional: drag your DEVOUR folder onto this file, or run:  build.bat "D:\SteamLibrary\steamapps\common\Devour"

if not exist "%~dp0Scripts\setup.ps1" (
  echo [X] Scripts\setup.ps1 is missing.
  echo     Extract the whole zip first ^(right-click ^> Extract All^), then run build.bat from the extracted folder.
  pause
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Scripts\setup.ps1" %*
set "RESULT=%ERRORLEVEL%"
echo.
pause
exit /b %RESULT%
