# DevourClient error checker. Run by check.bat.
# Read-only: looks at the install and the MelonLoader log, reports problems and how to fix them. Changes nothing.
# Usage (optional): check.bat "D:\SteamLibrary\steamapps\common\Devour"
param([string]$DevourPath)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'find-devour.ps1')

$script:problems = 0
function Ok($msg)   { Write-Host "    [OK] $msg" -ForegroundColor Green }
function Warn($msg, $fix) {
    Write-Host "    [!]  $msg" -ForegroundColor Yellow
    if ($fix) { Write-Host "         Fix: $fix" }
}
function Bad($msg, $fix) {
    $script:problems++
    Write-Host "    [X]  $msg" -ForegroundColor Red
    if ($fix) { Write-Host "         Fix: $fix" }
}
function Done {
    Write-Host ''
    if ($script:problems -eq 0) { Write-Host 'No problems found.' -ForegroundColor Green; exit 0 }
    Write-Host "$($script:problems) problem(s) found. Apply the fixes above, then run check.bat again." -ForegroundColor Red
    exit 1
}

Write-Host '==> DevourClient error check' -ForegroundColor Cyan

# 1. game folder
if ($DevourPath) { $DevourPath = $DevourPath.Trim('"', ' ') }
if (-not (Test-DevourDir $DevourPath)) { $DevourPath = Find-Devour }
if (-not (Test-DevourDir $DevourPath)) { Bad "DEVOUR wasn't found." 'Drag your DEVOUR folder onto check.bat.'; Done }
$Game = $DevourPath
Ok "DEVOUR found: $Game"

# 2. MelonLoader
$VersionDll = Join-Path $Game 'version.dll'
$MLDir      = Join-Path $Game 'MelonLoader'
$MLCore     = Join-Path $MLDir 'net6\MelonLoader.dll'
if (Test-Path -LiteralPath $VersionDll) { Ok 'MelonLoader is switched on (version.dll)' }
elseif (Test-Path -LiteralPath "$VersionDll.off") { Bad 'MelonLoader is switched off (version.dll.off).' 'Run build.bat, it switches it back on.' }
else { Bad 'MelonLoader is not installed (version.dll missing).' 'Run build.bat.' }

if (Test-Path -LiteralPath $MLCore) {
    $v = (Get-Item -LiteralPath $MLCore).VersionInfo.FileVersion
    if ($v -like '0.6.*') { Ok "MelonLoader version $v" } else { Bad "MelonLoader $v is installed, the mod needs 0.6.x." 'Run build.bat, it installs the right version.' }
} else { Bad 'MelonLoader files are missing (MelonLoader\net6\MelonLoader.dll).' 'Run build.bat.' }

# 3. generated game assemblies
$AsmCSharp = Join-Path $MLDir 'Il2CppAssemblies\Assembly-CSharp.dll'
if (Test-Path -LiteralPath $AsmCSharp) { Ok 'Game files prepared by MelonLoader' }
else { Bad 'MelonLoader has not prepared the game files yet.' 'Run build.bat (it starts the game once to do this).' }

# 4. the mod itself
$ModsDir = Join-Path $Game 'Mods'
$ModDll  = Join-Path $ModsDir 'DevourClient.dll'
if (Test-Path -LiteralPath $ModDll) {
    Ok 'DevourClient.dll is in Mods'
    if ((Test-Path -LiteralPath $AsmCSharp) -and (Get-Item -LiteralPath $AsmCSharp).LastWriteTime -gt (Get-Item -LiteralPath $ModDll).LastWriteTime) {
        Warn 'The game files were regenerated after the mod was built (DEVOUR probably updated).' 'Run build.bat to rebuild the mod against the new game version.'
    }
} else { Bad 'DevourClient.dll is not in DEVOUR\Mods.' 'Run build.bat.' }
$dupes = @(Get-ChildItem -LiteralPath $ModsDir -Filter 'DevourClient*.dll' -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne 'DevourClient.dll' })
foreach ($d in $dupes) { Bad "Extra copy of the mod: $($d.Name) (two copies conflict)." "Delete $($d.FullName)" }

# 5. menu key
$KeyFile = Join-Path $Game 'UserData\DevourClient_MenuKey.txt'
if (Test-Path -LiteralPath $KeyFile) {
    $key = (Get-Content -LiteralPath $KeyFile -Raw).Trim()
    if ($key -match '^[A-Za-z][A-Za-z0-9]*$') { Ok "Menu key: $key" } else { Bad "Menu key file has an invalid value '$key'." 'Run build.bat and pick the key again.' }
} else { Ok 'Menu key: Insert (default)' }

# 6. MelonLoader log from the last game start
$log = Join-Path $MLDir 'Latest.log'
if (Test-Path -LiteralPath $log) {
    $lines = Get-Content -LiteralPath $log -ErrorAction SilentlyContinue
    Ok "Last game log: $((Get-Item -LiteralPath $log).LastWriteTime)"
    if (-not ($lines -match 'DevourClient')) {
        Bad 'The mod did not load during the last game start.' 'Run build.bat, then start DEVOUR again.'
    }
    $errs = @($lines | Where-Object { $_ -match '\[ERROR\]|Exception|Failed to load|Incompatible' })
    if ($errs.Count -gt 0) {
        Bad "$($errs.Count) error line(s) in the last game log. Most recent:" "If it mentions DevourClient after a game update, run build.bat. Otherwise screenshot this and report it. Full log: $log"
        $errs | Select-Object -Last 10 | ForEach-Object { Write-Host "         $_" -ForegroundColor DarkGray }
    } else { Ok 'No errors in the last game log' }
} else {
    Warn 'No game log yet (DEVOUR has not been started with MelonLoader).' 'Start DEVOUR once, then run check.bat again.'
}

Done
