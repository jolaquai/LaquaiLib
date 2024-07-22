Set-Location $PSScriptRoot
$slnRoot = [System.IO.DirectoryInfo]::new((Get-Location)).Parent.FullName
$packLoc = [System.IO.Path]::Combine($slnRoot, "Scripts", "pack")

# Get all projects (they start with LaquaiLib)
$projects = Get-ChildItem -Path $slnRoot -Filter LaquaiLib* -Recurse -Directory

# On Error, stop the script
$ErrorActionPreference = "Stop"

# Visit each project directory, then run dotnet pack
foreach ($project in $projects) {
    Set-Location $project.FullName
    & dotnet pack --include-source --output $packLoc

    # Delete the nupkg without the source
    $nupkg = Get-ChildItem -Path $packLoc -Filter *.nupkg
    $nupkg | Where-Object { $_.Name -notlike "*symbols*" } | Remove-Item
}