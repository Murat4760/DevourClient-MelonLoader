@echo off
REM Builds DevourClient.dll and copies it into Devour\Mods.
REM If Devour is NOT in the default Steam folder, edit the next line.
set "DEVOUR=C:\Program Files (x86)\Steam\steamapps\common\Devour"

if not exist "%DEVOUR%\MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll" (
  echo [X] MelonLoader game files not found in "%DEVOUR%".
  echo     Install MelonLoader into Devour, launch the game once, close it, then run this again.
  pause & exit /b 1
)

dotnet build "%~dp0DevourClient\DevourClient.csproj" -c Release -p:DevourPath="%DEVOUR%"
if errorlevel 1 ( echo [X] Build failed - screenshot the red errors and send them to Claude. & pause & exit /b 1 )

if not exist "%DEVOUR%\Mods" mkdir "%DEVOUR%\Mods"
copy /Y "%~dp0DevourClient\bin\Release\net6.0\DevourClient.dll" "%DEVOUR%\Mods\" >nul
echo [OK] DevourClient.dll copied to "%DEVOUR%\Mods". Start the game and press INSERT.
pause
