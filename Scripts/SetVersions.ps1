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

# Cmdline argument: Version to set to
$targetVersion = $args[0]

$regex = [regex]::new("<PackageVersion>.*?<\/PackageVersion>", [System.Text.RegularExpressions.RegexOptions]::Compiled -bor [System.Text.RegularExpressions.RegexOptions]::ExplicitCapture)

# Visit each project directory, then set the version
foreach ($project in $projects) {
    Set-Location -Path ([System.IO.Path]::Combine($slnRoot, $project))
    
    $csproj = Get-ChildItem -Path $project.FullName -Filter *.csproj
    $csprojContent = Get-Content $csproj.FullName
    # Only need to set <PackageVersion>, the rest reference $(PackageVersion)
    $csprojContent = $regex.Replace($csprojContent, "<PackageVersion>$targetVersion</PackageVersion>")
    [System.Xml.Linq.XElement]::Parse($csprojContent).Save($csproj.FullName)
}

# Move back to the scripts
Set-Location -Path "$slnRoot\Scripts"