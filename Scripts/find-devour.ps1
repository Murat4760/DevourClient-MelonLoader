# Shared by setup.ps1 and uninstall.ps1: locates the DEVOUR install folder.
function Test-DevourDir($dir) { return ($dir -and (Test-Path -LiteralPath (Join-Path $dir 'DEVOUR.exe'))) }

function Find-Devour {
    $libs = New-Object System.Collections.Generic.List[string]
    foreach ($key in 'HKCU:\Software\Valve\Steam', 'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam', 'HKLM:\SOFTWARE\Valve\Steam') {
        try {
            $p = Get-ItemProperty -Path $key -ErrorAction Stop
            foreach ($v in @($p.SteamPath, $p.InstallPath)) { if ($v) { $libs.Add(($v -replace '/', '\')) } }
        } catch {}
    }
    foreach ($steam in @($libs)) {
        $vdf = Join-Path $steam 'steamapps\libraryfolders.vdf'
        if (Test-Path -LiteralPath $vdf) {
            foreach ($m in [regex]::Matches((Get-Content -LiteralPath $vdf -Raw), '"path"\s+"([^"]+)"')) {
                $libs.Add(($m.Groups[1].Value -replace '\\\\', '\'))
            }
        }
    }
    foreach ($drive in [IO.DriveInfo]::GetDrives()) {
        if ($drive.DriveType -ne 'Fixed' -or -not $drive.IsReady) { continue }
        foreach ($sub in 'SteamLibrary', 'Steam', 'Games\Steam', 'Games\SteamLibrary', 'Program Files (x86)\Steam', 'Program Files\Steam') {
            $libs.Add((Join-Path $drive.RootDirectory.FullName $sub))
        }
    }
    foreach ($lib in ($libs | Select-Object -Unique)) {
        $game = Join-Path $lib 'steamapps\common\Devour'
        if (Test-DevourDir $game) { return (Resolve-Path -LiteralPath $game).Path }
    }
    return $null
}
