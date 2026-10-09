param(
 [Parameter(Mandatory=$true)][string]$GameFolder,
 [Parameter(Mandatory=$true)][string]$LoaderFolder
)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$managed=Join-Path $GameFolder 'EtG_Data\Managed'
$output=Join-Path $projectRoot 'build'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$refs=@('mscorlib.dll','System.dll','System.Core.dll','Assembly-CSharp.dll','Assembly-CSharp-firstpass.dll','UnityEngine.dll','UnityEngine.CoreModule.dll','UnityEngine.IMGUIModule.dll','UnityEngine.ImageConversionModule.dll','UnityEngine.JSONSerializeModule.dll','UnityEngine.TextRenderingModule.dll')
$arguments=@('/nologo','/noconfig','/target:library','/nostdlib+',('/out:'+(Join-Path $output 'GungeonSurvivors.dll')))
foreach($ref in $refs) { $path=Join-Path $managed $ref; if(!(Test-Path -LiteralPath $path)) {throw "Missing inspected reference: $path"}; $arguments+='/reference:'+$path }
$arguments+='/reference:'+(Join-Path $LoaderFolder 'BepInEx\core\BepInEx.dll')
$arguments+=@(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'plugin') -Filter '*.cs' | ForEach-Object {$_.FullName})
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" @arguments
if($LASTEXITCODE -ne 0) {throw 'Plugin compilation failed'}
Write-Output (Join-Path $output 'GungeonSurvivors.dll')




