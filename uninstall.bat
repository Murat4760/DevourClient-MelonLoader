@echo off
title DevourClient uninstaller
REM Removes DevourClient + MelonLoader from DEVOUR and everything build.bat downloaded. Asks before deleting.
REM Optional: drag your DEVOUR folder onto this file, or run:  uninstall.bat "D:\SteamLibrary\steamapps\common\Devour"

if not exist "%~dp0Scripts\uninstall.ps1" (
  echo [X] Scripts\uninstall.ps1 is missing. Extract the whole zip first, then run uninstall.bat from the extracted folder.
  pause
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Scripts\uninstall.ps1" %*
set "RESULT=%ERRORLEVEL%"
echo.
pause
exit /b %RESULT%
