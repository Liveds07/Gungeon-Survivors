param([string]$GameFolder='', [string]$CompanionFolder='', [switch]$SetupOnly)
$ErrorActionPreference='Stop'
$bundle=$PSScriptRoot
if (![Environment]::Is64BitOperatingSystem) { throw 'This package requires 64-bit Windows.' }
if (Get-Process -Name EtG -ErrorAction SilentlyContinue) { throw 'Close Enter the Gungeon before installing or launching this package.' }

# Verify distributed files before using the bundled executables.
$manifest=Get-Content -LiteralPath (Join-Path $bundle 'manifest.json') -Raw | ConvertFrom-Json
foreach($entry in $manifest.files) {
    $path=Join-Path $bundle $entry.path
    if (!(Test-Path -LiteralPath $path -PathType Leaf)) {throw "Missing package file: $($entry.path)"}
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $entry.sha256) {throw "Package file changed: $($entry.path). Extract a fresh ZIP."}
}

function Find-Game([string]$exe, [string]$name, [string]$given) {
    if ($given) {
        if (!(Test-Path -LiteralPath (Join-Path $given $exe))) {throw "Cannot find $exe in $given"}
        return (Resolve-Path -LiteralPath $given).Path
    }
    $roots=@()
    $steam=(Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction SilentlyContinue).SteamPath
    if($steam) {
        $roots+=$steam
        $vdf=Join-Path $steam 'steamapps\libraryfolders.vdf'
        if(Test-Path -LiteralPath $vdf) {
            $text=Get-Content -LiteralPath $vdf -Raw
            foreach($match in [regex]::Matches($text,'"path"\s+"([^"]+)"')) {$roots+=$match.Groups[1].Value.Replace('\\','\')}
        }
        foreach($root in $roots) {
            $candidate=Join-Path $root ('steamapps\common\'+$name)
            if(Test-Path -LiteralPath (Join-Path $candidate $exe)) {return $candidate}
        }
    }
    $epic=Join-Path $env:ProgramData 'Epic\EpicGamesLauncher\Data\Manifests'
    if(Test-Path -LiteralPath $epic) {
        foreach($file in Get-ChildItem -LiteralPath $epic -Filter '*.item') {
            try {$install=(Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json).InstallLocation} catch {continue}
            if($install -and (Test-Path -LiteralPath (Join-Path $install $exe))) {return $install}
        }
    }
    $shell=New-Object -ComObject Shell.Application
    $chosen=$shell.BrowseForFolder(0,"Select your installed $name folder",0,0)
    if(!$chosen) {throw "$name folder selection cancelled."}
    $path=$chosen.Self.Path
    if(!(Test-Path -LiteralPath (Join-Path $path $exe))) {throw "The selected folder does not contain $exe."}
    return $path
}

$state=Join-Path $env:LOCALAPPDATA 'GungeonSurvivors\portable'
$selection=Join-Path $state 'games.json'
if(Test-Path -LiteralPath $selection) {
    $previous=Get-Content -LiteralPath $selection -Raw | ConvertFrom-Json
    if(!$GameFolder -and (Test-Path -LiteralPath (Join-Path $previous.host 'EtG.exe'))) {$GameFolder=$previous.host}
    if(!$CompanionFolder -and (Test-Path -LiteralPath (Join-Path $previous.companion 'VampireSurvivors.exe'))) {$CompanionFolder=$previous.companion}
}
$GameFolder=Find-Game 'EtG.exe' 'Enter the Gungeon' $GameFolder
$CompanionFolder=Find-Game 'VampireSurvivors.exe' 'Vampire Survivors' $CompanionFolder
$lab=Join-Path $state 'game'
if([IO.Path]::GetFullPath($GameFolder).TrimEnd('\') -eq [IO.Path]::GetFullPath($lab).TrimEnd('\')) {throw 'Select your original installation, not the portable test copy.'}
New-Item -ItemType Directory -Path $state -Force | Out-Null
$marker=Join-Path $state 'copied-from.txt'
if(!(Test-Path -LiteralPath $marker)) {
    Write-Host 'Creating a separate Gungeon copy. This may take several minutes.'
    # Exclude root-level mod folders, but retain EtG_Data/Plugins native libraries.
    & robocopy.exe $GameFolder $lab /E /R:1 /W:1 /XD (Join-Path $GameFolder 'BepInEx') (Join-Path $GameFolder 'Mods') (Join-Path $GameFolder 'Plugins') /XF winhttp.dll version.dll doorstop_config.ini /NFL /NDL /NJH /NJS
    if($LASTEXITCODE -ge 8) {throw "Game copy failed (robocopy $LASTEXITCODE). Check free disk space."}
    Set-Content -LiteralPath $marker -Value $GameFolder -Encoding UTF8
} elseif((Get-Content -LiteralPath $marker -Raw).Trim() -ne $GameFolder) {throw 'This portable lab belongs to another game installation. Keep the original folder selection.'}
if(!(Test-Path -LiteralPath (Join-Path $lab 'EtG_Data\Managed\Assembly-CSharp.dll'))) {throw 'The host is not the supported Windows Mono Gungeon installation.'}
if(!(Test-Path -LiteralPath (Join-Path $CompanionFolder 'VampireSurvivors_Data\resources.assets'))) {throw 'The companion installation is missing resources.assets.'}

# The private copy uses the same save location as the original game.
$saves=Join-Path $env:USERPROFILE 'AppData\LocalLow\Dodge Roll\Enter the Gungeon'
if(Test-Path -LiteralPath $saves) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $backups=Join-Path $state 'save-backups'
    New-Item -ItemType Directory -Path $backups -Force | Out-Null
    $backup=Join-Path $backups ((Get-Date -Format 'yyyyMMdd-HHmmss-ffff')+'.zip')
    [IO.Compression.ZipFile]::CreateFromDirectory($saves,$backup)
    Write-Host "Save backup: $backup"
}
Copy-Item -Path (Join-Path $bundle 'loader\*') -Destination $lab -Recurse -Force
$plugin=Join-Path $lab 'BepInEx\plugins\GungeonSurvivors'
$configFolder=Join-Path $lab 'BepInEx\config'
New-Item -ItemType Directory -Path $plugin,$configFolder -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $bundle 'mod\GungeonSurvivors.dll') -Destination $plugin -Force
$python=Join-Path $bundle 'runtime\python.exe'
& $python -I -c 'import UnityPy; from PIL import Image; print("Companion reader dependencies ready")'
if($LASTEXITCODE -ne 0) {throw 'The bundled Python dependencies could not load.'}
$config=Join-Path $configFolder 'local.gungeon-survivors.cfg'
if(Test-Path -LiteralPath $config) {Copy-Item -LiteralPath $config -Destination ($config+'.backup-'+(Get-Date -Format 'yyyyMMdd-HHmmss-ffff'))}
@"
[Content]
CompanionFolder = $CompanionFolder
PythonExecutable = $python
ToolsFolder = $(Join-Path $bundle 'tools')
DesignSheets = $(Join-Path $bundle 'design\sheets.json')

[Testing]
EnableTestCommands = false
"@ | Set-Content -LiteralPath $config -Encoding UTF8
@{host=$GameFolder;companion=$CompanionFolder} | ConvertTo-Json | Set-Content -LiteralPath $selection -Encoding UTF8
if($SetupOnly) {Write-Host 'Setup completed. Gameplay has not been tested by this command.'; exit 0}
Write-Host 'Starting the private Gungeon copy. Start a run to use the survivor picker.'
Start-Process -FilePath (Join-Path $lab 'EtG.exe') -WorkingDirectory $lab
