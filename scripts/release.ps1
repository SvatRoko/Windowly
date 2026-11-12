param()
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root 'artifacts'
$publish=Join-Path $out 'Windowly-win-x64'
if(Test-Path -LiteralPath $publish){$resolved=(Resolve-Path -LiteralPath $publish).Path;if($resolved -ne (Join-Path $root 'artifacts\Windowly-win-x64')){throw 'Unsafe output path'};Remove-Item -LiteralPath $resolved -Recurse -Force}
New-Item -ItemType Directory -Force $publish | Out-Null
dotnet publish "$root/src/Windowly/Windowly.csproj" -c Release -r win-x64 --self-contained true -o $publish
if($LASTEXITCODE -ne 0){throw 'Publish failed'}
Copy-Item "$root/README.md","$root/LICENSE" $publish
$zip=Join-Path $out 'Windowly-win-x64.zip'
Compress-Archive -Path "$publish/*" -DestinationPath $zip -Force
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive=[IO.Compression.ZipFile]::OpenRead($zip)
try{foreach($required in @('Windowly.exe','Windowly.dll','coreclr.dll','README.md','LICENSE')){if($required -notin $archive.Entries.FullName){throw "Missing $required"}}}finally{$archive.Dispose()}
Write-Output "Verified $zip"
