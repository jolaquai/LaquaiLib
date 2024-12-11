Set-Location $PSScriptRoot
$slnRoot = [System.IO.DirectoryInfo]::new((Get-Location)).Parent.FullName
$packLoc = [System.IO.Path]::Combine($slnRoot, "Scripts", "pack")

# On Error, stop the script
$ErrorActionPreference = "Stop"

# Move into /Scripts/pack
Set-Location -Path $packLoc

# Run a nuget push
& nuget push *.symbols.nupkg -Source "nuget.org"

# Move back to the scripts
Set-Location -Path "$slnRoot\Scripts"