Set-Location $PSScriptRoot
$slnRoot = [System.IO.DirectoryInfo]::new((Get-Location)).Parent.FullName
$packLoc = [System.IO.Path]::Combine($slnRoot, "Scripts", "pack")

# Get all projects (they start with LaquaiLib)
[string[]]$projects = @(
    "LaquaiLib.Core",
    "LaquaiLib",
    "LaquaiLib.Windows",
    "LaquaiLib.Compound",
    "LaquaiLib.Oxml",
    "LaquaiLib.EF",
    "LaquaiLib.DependencyInjection"
)

# On Error, stop the script
$ErrorActionPreference = "Stop"

# Delete the contents of the pack directory
Remove-Item -Path $packLoc\* -Recurse -Force

# Visit each project directory, then run dotnet pack
foreach ($project in $projects) {
    Set-Location -Path ([System.IO.Path]::Combine($slnRoot, $project))
    & dotnet clean --configuration Release
    & dotnet build --force --configuration Release
    & dotnet pack --include-source --output $packLoc  --configuration Release --no-build

    # Delete the nupkg without the source
    $nupkg = Get-ChildItem -Path $packLoc -Filter *.nupkg
    $nupkg | Where-Object { $_.Name -notlike "*symbols*" } | Remove-Item
}

# Move back to the scripts
Set-Location -Path "$slnRoot\Scripts"