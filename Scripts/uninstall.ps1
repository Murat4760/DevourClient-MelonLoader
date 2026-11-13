# DevourClient uninstaller. Run by uninstall.bat.
# Removes DevourClient AND MelonLoader from DEVOUR, plus everything build.bat downloaded/built in this folder.
# Usage (optional): uninstall.bat "D:\SteamLibrary\steamapps\common\Devour"
param(
    [string]$DevourPath,
    [switch]$Elevated
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'find-devour.ps1')

function Fail($msg) {
    Write-Host ''
    Write-Host "[X] $msg" -ForegroundColor Red
    if ($Elevated) { Read-Host 'Press Enter to close' | Out-Null }
    exit 1
}

Write-Host '==> Looking for DEVOUR' -ForegroundColor Cyan
if ($DevourPath) { $DevourPath = $DevourPath.Trim('"', ' ') }
if (-not (Test-DevourDir $DevourPath)) { $DevourPath = Find-Devour }
while (-not (Test-DevourDir $DevourPath)) {
    Write-Host "    Couldn't find DEVOUR automatically." -ForegroundColor Yellow
    Write-Host '    In Steam: right-click DEVOUR > Manage > Browse local files, copy that folder path and paste it here.'
    $DevourPath = (Read-Host '    DEVOUR folder (or leave empty to cancel)').Trim('"', ' ')
    if (-not $DevourPath) { Fail 'Cancelled, nothing was deleted.' }
}
$Game = $DevourPath
Write-Host "    [OK] $Game" -ForegroundColor Green

# Need write access to the game folder
try {
    $probe = Join-Path $Game ('.write_test_' + [guid]::NewGuid().ToString('N'))
    [IO.File]::WriteAllText($probe, 'x'); Remove-Item -LiteralPath $probe -Force
} catch {
    if ($Elevated) { Fail "Can't write to $Game even as administrator." }
    Write-Host '    The DEVOUR folder needs administrator rights, asking Windows for permission...'
    $elevArgs = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', "`"$PSCommandPath`"", '-DevourPath', "`"$Game`"", '-Elevated')
    try { $p = Start-Process powershell -Verb RunAs -ArgumentList $elevArgs -Wait -PassThru } catch { Fail 'Administrator permission was declined, nothing was deleted.' }
    exit $p.ExitCode
}

# Everything that gets removed. Game files (DEVOUR.exe, DEVOUR_Data, ...) are never touched.
$targets = New-Object System.Collections.Generic.List[string]
foreach ($n in 'MelonLoader', 'Mods', 'Plugins', 'UserData', 'UserLibs', 'version.dll', 'version.dll.off', 'dobby.dll') {
    $targets.Add((Join-Path $Game $n))
}
Get-ChildItem -LiteralPath $Game -Directory -Filter 'MelonLoader.backup-*' -ErrorAction SilentlyContinue | ForEach-Object { $targets.Add($_.FullName) }
$notice = Join-Path $Game 'NOTICE.txt'   # shipped by MelonLoader; only removed if it's MelonLoader's
if ((Test-Path -LiteralPath $notice) -and ((Get-Content -LiteralPath $notice -Raw) -match 'MelonLoader')) { $targets.Add($notice) }
foreach ($n in '.dotnet', 'DevourClient\bin', 'DevourClient\obj') { $targets.Add((Join-Path $Root $n)) }
$targets.Add((Join-Path $env:TEMP 'dotnet-install.ps1'))
$targets.Add((Join-Path $env:TEMP 'MelonLoader.x64.v0.6.4.zip'))

$found = @($targets | Where-Object { Test-Path -LiteralPath $_ })
if ($found.Count -eq 0) {
    Write-Host ''
    Write-Host 'Nothing to remove, DevourClient and MelonLoader are not installed.' -ForegroundColor Green
    if ($Elevated) { Read-Host 'Press Enter to close' | Out-Null }
    exit 0
}

Write-Host ''
Write-Host 'This will permanently delete:' -ForegroundColor Yellow
$found | ForEach-Object { Write-Host "    $_" }
Write-Host ''
Write-Host '    WARNING: Mods, Plugins and UserData also hold any OTHER MelonLoader mods you installed.' -ForegroundColor Yellow
Write-Host '    Your DEVOUR game files and save progress (stored by Steam) are not touched.'
Write-Host ''
if ((Read-Host 'Type YES to uninstall') -ne 'YES') { Fail 'Cancelled, nothing was deleted.' }

if (Get-Process -Name DEVOUR -ErrorAction SilentlyContinue) {
    Write-Host '    DEVOUR is running. Close the game to continue (waiting)...' -ForegroundColor Yellow
    while (Get-Process -Name DEVOUR -ErrorAction SilentlyContinue) { Start-Sleep -Seconds 2 }
    Start-Sleep -Seconds 2
}

Write-Host ''
Write-Host '==> Removing' -ForegroundColor Cyan
$failed = 0
foreach ($t in $found) {
    try {
        Remove-Item -LiteralPath $t -Recurse -Force
        Write-Host "    [OK] $t" -ForegroundColor Green
    } catch {
        Write-Host "    [X] $t : $($_.Exception.Message)" -ForegroundColor Red
        $failed++
    }
}

Write-Host ''
if ($failed -gt 0) { Fail "$failed item(s) could not be deleted (file in use?). Close DEVOUR/Steam and run uninstall.bat again." }
Write-Host 'Uninstalled. DEVOUR is back to normal.' -ForegroundColor Green
Write-Host 'You can now delete this DevourClient folder too. If the game acts up, use Steam > DEVOUR > Properties > Installed Files > Verify integrity.'
if ($Elevated) { Read-Host 'Press Enter to close' | Out-Null }
exit 0
