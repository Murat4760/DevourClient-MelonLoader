@echo off
title DevourClient error checker
REM Checks the install and the last game log for problems. Changes nothing.
REM Optional: drag your DEVOUR folder onto this file, or run:  check.bat "D:\SteamLibrary\steamapps\common\Devour"

if not exist "%~dp0Scripts\check.ps1" (
  echo [X] Scripts\check.ps1 is missing. Extract the whole zip first, then run check.bat from the extracted folder.
  pause
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Scripts\check.ps1" %*
set "RESULT=%ERRORLEVEL%"
echo.
pause
exit /b %RESULT%
