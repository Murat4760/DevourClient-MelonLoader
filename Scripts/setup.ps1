# DevourClient one-click setup. Run by build.bat; you normally never start this file yourself.
# It does everything needed on a fresh PC:
#   1. finds DEVOUR through Steam (any library folder / drive)
#   2. installs a .NET SDK into this folder if none is found (no admin needed)
#   3. installs MelonLoader v0.6.4 into DEVOUR if it is missing (or turns it back on if it was switched off)
#   4. starts DEVOUR once so MelonLoader can generate the game files, then closes it
#   5. builds DevourClient.dll and copies it into DEVOUR\Mods
# Usage (optional): build.bat "D:\SteamLibrary\steamapps\common\Devour"
param(
    [string]$DevourPath,
    [switch]$Elevated
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'   # makes downloads much faster in Windows PowerShell
try { [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12 } catch {}

$Root        = Split-Path $PSScriptRoot -Parent
$Project     = Join-Path $Root 'DevourClient\DevourClient.csproj'
$MLVersion   = 'v0.6.4'
$MLUrl       = "https://github.com/LavaGang/MelonLoader/releases/download/$MLVersion/MelonLoader.x64.zip"
$DotnetUrl   = 'https://dot.net/v1/dotnet-install.ps1'
$AppId       = 1274570

function Step($msg) { Write-Host ''; Write-Host "==> $msg" -ForegroundColor Cyan }
function Ok($msg)   { Write-Host "    [OK] $msg" -ForegroundColor Green }
function Info($msg) { Write-Host "    $msg" }
function Fail($msg) {
    Write-Host ''
    Write-Host "[X] $msg" -ForegroundColor Red
    if ($Elevated) { Read-Host 'Press Enter to close' | Out-Null }
    exit 1
}

function Download($url, $dest) {
    Info "Downloading $url"
    for ($i = 1; $i -le 3; $i++) {
        try { Invoke-WebRequest -Uri $url -OutFile $dest -UseBasicParsing; return }
        catch { if ($i -eq 3) { throw } ; Start-Sleep -Seconds 3 }
    }
}

# ---------------------------------------------------------------- 1. find DEVOUR
. (Join-Path $PSScriptRoot 'find-devour.ps1')

Step 'Looking for DEVOUR'
if ($DevourPath) { $DevourPath = $DevourPath.Trim('"', ' ') }
if (-not (Test-DevourDir $DevourPath)) { $DevourPath = Find-Devour }
while (-not (Test-DevourDir $DevourPath)) {
    Write-Host "    Couldn't find DEVOUR automatically." -ForegroundColor Yellow
    Write-Host '    In Steam: right-click DEVOUR > Manage > Browse local files, copy that folder path and paste it here.'
    $DevourPath = (Read-Host '    DEVOUR folder').Trim('"', ' ')
}
$Game = $DevourPath
Ok $Game

# Need write access to the game folder (some PCs keep Steam in a protected folder)
try {
    $probe = Join-Path $Game ('.write_test_' + [guid]::NewGuid().ToString('N'))
    [IO.File]::WriteAllText($probe, 'x'); Remove-Item -LiteralPath $probe -Force
} catch {
    if ($Elevated) { Fail "Can't write to $Game even as administrator." }
    Info 'The DEVOUR folder needs administrator rights, asking Windows for permission...'
    $elevArgs = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', "`"$PSCommandPath`"", '-DevourPath', "`"$Game`"", '-Elevated')
    try { $p = Start-Process powershell -Verb RunAs -ArgumentList $elevArgs -Wait -PassThru } catch { Fail 'Administrator permission was declined.' }
    exit $p.ExitCode
}

# The game must be closed while files are changed
if (Get-Process -Name DEVOUR -ErrorAction SilentlyContinue) {
    Write-Host '    DEVOUR is running. Close the game to continue (waiting)...' -ForegroundColor Yellow
    while (Get-Process -Name DEVOUR -ErrorAction SilentlyContinue) { Start-Sleep -Seconds 2 }
    Start-Sleep -Seconds 2
}

# ---------------------------------------------------------------- 2. .NET SDK
Step 'Checking .NET SDK'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$LocalDotnetDir = Join-Path $Root '.dotnet'
$Dotnet = $null

function Test-Sdk($exe) {
    try {
        $sdks = & $exe --list-sdks 2>$null
        foreach ($line in $sdks) { if ($line -match '^(\d+)\.' -and [int]$Matches[1] -ge 6) { return $true } }
    } catch {}
    return $false
}

$localExe = Join-Path $LocalDotnetDir 'dotnet.exe'
if ((Test-Path -LiteralPath $localExe) -and (Test-Sdk $localExe)) { $Dotnet = $localExe }
if (-not $Dotnet) {
    $cmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($cmd -and (Test-Sdk $cmd.Source)) { $Dotnet = $cmd.Source }
}
if (-not $Dotnet) {
    Info 'No .NET SDK found. Installing .NET 6 SDK into this folder (one time, ~200 MB, no admin needed)...'
    $installer = Join-Path $env:TEMP 'dotnet-install.ps1'
    Download $DotnetUrl $installer
    & powershell -NoProfile -ExecutionPolicy Bypass -File $installer -Channel 6.0 -InstallDir $LocalDotnetDir -NoPath
    if (-not (Test-Sdk $localExe)) { Fail '.NET SDK install failed. Install the .NET 6 SDK from https://dotnet.microsoft.com/download/dotnet/6.0 and run build.bat again.' }
    $Dotnet = $localExe
}
if ($Dotnet -eq $localExe) { $env:DOTNET_ROOT = $LocalDotnetDir; $env:DOTNET_MULTILEVEL_LOOKUP = '0' }
Ok $Dotnet

# ---------------------------------------------------------------- 3. MelonLoader
Step "Checking MelonLoader $MLVersion"
$VersionDll    = Join-Path $Game 'version.dll'
$VersionDllOff = Join-Path $Game 'version.dll.off'
$MLDir         = Join-Path $Game 'MelonLoader'
$MLCore        = Join-Path $MLDir 'net6\MelonLoader.dll'

$mlOk = $false
if (Test-Path -LiteralPath $MLCore) {
    $v = (Get-Item -LiteralPath $MLCore).VersionInfo.FileVersion
    if ($v -like '0.6.*') { $mlOk = $true } else {
        $bak = "$MLDir.backup-" + (Get-Date -Format 'yyyyMMdd-HHmmss')
        Info "Found MelonLoader $v, but this mod needs 0.6.x. Moving it to $bak"
        Move-Item -LiteralPath $MLDir -Destination $bak
    }
}

if ($mlOk -and -not (Test-Path -LiteralPath $VersionDll) -and (Test-Path -LiteralPath $VersionDllOff)) {
    Move-Item -LiteralPath $VersionDllOff -Destination $VersionDll
    Ok 'MelonLoader was switched off (version.dll.off), switched it back on'
}

if (-not $mlOk -or -not (Test-Path -LiteralPath $VersionDll)) {
    $zip = Join-Path $env:TEMP "MelonLoader.x64.$MLVersion.zip"
    Download $MLUrl $zip
    Expand-Archive -LiteralPath $zip -DestinationPath $Game -Force
    Remove-Item -LiteralPath $zip -Force -ErrorAction SilentlyContinue
    if (Test-Path -LiteralPath $VersionDllOff) { Remove-Item -LiteralPath $VersionDllOff -Force }
    if (-not (Test-Path -LiteralPath $MLCore)) { Fail 'MelonLoader did not install correctly.' }
    Ok "MelonLoader $MLVersion installed"
} else {
    Ok 'MelonLoader is installed'
}
foreach ($d in 'Mods', 'Plugins', 'UserData', 'UserLibs') {
    $p = Join-Path $Game $d
    if (-not (Test-Path -LiteralPath $p)) { New-Item -ItemType Directory -Path $p | Out-Null }
}

# ---------------------------------------------------------------- 3b. menu key
Step 'Choosing the key that opens the menu'
$KeyFile = Join-Path $Game 'UserData\DevourClient_MenuKey.txt'
$MenuKey = 'Insert'
if (Test-Path -LiteralPath $KeyFile) {
    $saved = (Get-Content -LiteralPath $KeyFile -Raw -ErrorAction SilentlyContinue)
    if ($saved -and $saved.Trim()) { $MenuKey = $saved.Trim() }
}

# Console key name -> Unity KeyCode name (the mod reads the Unity name)
function ConvertTo-UnityKey([ConsoleKey]$k) {
    $n = $k.ToString()
    if ($n -match '^D(\d)$')      { return "Alpha$($Matches[1])" }
    if ($n -match '^NumPad(\d)$') { return "Keypad$($Matches[1])" }
    if ($n -match '^([A-Z]|F\d{1,2})$') { return $n }
    switch ($n) {
        'Multiply' { return 'KeypadMultiply' }
        'Add'      { return 'KeypadPlus' }
        'Subtract' { return 'KeypadMinus' }
        'Divide'   { return 'KeypadDivide' }
        'Decimal'  { return 'KeypadPeriod' }
    }
    if ($n -in 'Insert', 'Delete', 'Home', 'End', 'PageUp', 'PageDown', 'UpArrow', 'DownArrow', 'LeftArrow', 'RightArrow', 'Tab', 'Backspace', 'Pause') { return $n }
    return $null
}

# Keys the game or the menu already uses
$Blocked = @('Escape', 'Spacebar', 'Alpha1', 'Alpha2', 'Alpha3', 'Alpha4', 'Alpha5', 'Alpha6', 'Alpha7')

Info "Current menu key: $MenuKey"
Info 'Press the key you want to open the menu with, or press Enter to keep it.'
while ($true) {
    $press = [Console]::ReadKey($true)
    if ($press.Key -eq 'Enter') { break }
    $unity = ConvertTo-UnityKey $press.Key
    if (-not $unity -or $Blocked -contains $unity -or $Blocked -contains $press.Key.ToString()) {
        Write-Host "    '$($press.Key)' can't be used (unsupported, or already used by the game/menu). Press another key." -ForegroundColor Yellow
        continue
    }
    $MenuKey = $unity
    break
}
Set-Content -LiteralPath $KeyFile -Value $MenuKey -Encoding ASCII
Ok "Menu key: $MenuKey (run build.bat again to change it)"

# ---------------------------------------------------------------- 4. first launch to generate game assemblies
$AsmCSharp = Join-Path $MLDir 'Il2CppAssemblies\Assembly-CSharp.dll'
if (-not (Test-Path -LiteralPath $AsmCSharp)) {
    Step 'First launch: starting DEVOUR once so MelonLoader can prepare the game files'
    Info 'This takes about 2-10 minutes. A black console window will appear, do not close it.'
    Info 'The game will be closed automatically when it is done.'
    $log = Join-Path $MLDir 'Latest.log'
    $started = Get-Date
    Start-Process "steam://rungameid/$AppId"
    $deadline = $started.AddMinutes(25)
    $done = $false
    while (-not $done) {
        Start-Sleep -Seconds 5
        Write-Host '.' -NoNewline
        if ((Get-Date) -gt $deadline) { Write-Host ''; Fail "Timed out. Check $log for errors, then run build.bat again." }
        if (-not (Test-Path -LiteralPath $log)) { continue }
        if ((Get-Item -LiteralPath $log).LastWriteTime -lt $started) { continue }
        $text = Get-Content -LiteralPath $log -Raw -ErrorAction SilentlyContinue
        if ($text -match 'Loading Mods from') { $done = $true }
        elseif ($text -match 'Failed to Generate Assemblies|Il2CppAssemblyGenerator.*(Failed|Exception)') {
            if (-not (Get-Process -Name DEVOUR -ErrorAction SilentlyContinue)) { Write-Host ''; Fail "MelonLoader could not prepare the game files. See $log" }
        }
    }
    Write-Host ''
    Start-Sleep -Seconds 3
    Get-Process -Name DEVOUR -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 3
    if (-not (Test-Path -LiteralPath $AsmCSharp)) { Fail "Game files were not generated. See $log" }
    Ok 'Game files prepared'
} else {
    Ok 'Game files already prepared'
}

# ---------------------------------------------------------------- 5. build + install
Step 'Building DevourClient.dll'
& $Dotnet build $Project -c Release -nologo -v:quiet -clp:ErrorsOnly "-p:DevourPath=$Game"
if ($LASTEXITCODE -ne 0) { Fail 'Build failed. Screenshot the red errors above. If DEVOUR just updated, the mod may need updating too.' }

$built = Join-Path $Root 'DevourClient\bin\Release\net6.0\DevourClient.dll'
Copy-Item -LiteralPath $built -Destination (Join-Path $Game 'Mods\DevourClient.dll') -Force
Ok "Installed to $(Join-Path $Game 'Mods\DevourClient.dll')"

Write-Host ''
Write-Host "All done! Start DEVOUR from Steam and press $MenuKey to open the menu." -ForegroundColor Green
Write-Host '  Something not working? Double-click check.bat for a diagnosis.'
Write-Host '  Visuals tab: Unlimited UV Light    Misc tab: Auto Respawn'
if ($Elevated) { Read-Host 'Press Enter to close' | Out-Null }
exit 0
