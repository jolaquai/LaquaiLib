<#
.SYNOPSIS
    Hard-errors the build if any [UnsafeAccessor] member under -SourceRoot is not referenced by name from any .cs file under -TestRoot.

.DESCRIPTION
    [UnsafeAccessor] methods bind to private BCL members by name and break silently (MissingFieldException/MissingMethodException
    at the first call site) if a runtime renames or removes the target. Coverage tools can't see them either: the methods are
    bodiless (RVA 0), so line/branch coverage reports either omit them entirely or vacuously report them as covered. The only
    thing that can be enforced at build time is "does some test file at least mention this member" - a textual proxy for
    "somebody will notice if this accessor stops binding." Actual binding correctness is proven separately, at test time, by a
    reflection-driven canary test that invokes every accessor against a real receiver.

    This is deliberately a coarse, same-line "<DeclaringType> ... <MemberName>" text match, not a semantic call-site check.
#>
param(
    [Parameter(Mandatory)][string]$SourceRoot,
    [Parameter(Mandatory)][string]$TestRoot
)

$ErrorActionPreference = 'Stop'

function Get-AccessorDeclarations([string]$root)
{
    $declarations = [System.Collections.Generic.List[pscustomobject]]::new()
    $files = Get-ChildItem -Path $root -Filter '*.cs' -Recurse -File
    foreach ($file in $files)
    {
        $lines = Get-Content -LiteralPath $file.FullName
        $currentClass = $null
        $pendingAttr = $false
        for ($i = 0; $i -lt $lines.Count; $i++)
        {
            $line = $lines[$i]

            if ($line -match '^\s*(?:public|internal)\s+static\s+(?:partial\s+)?class\s+(\w+)')
            {
                $currentClass = $Matches[1]
            }

            $hasAttr = $line -match '\[UnsafeAccessor\('
            if ($hasAttr) { $pendingAttr = $true }

            if ($hasAttr -or $pendingAttr)
            {
                # Trailing "identifier(paramlist);" - anchored at EOL so a tuple/array return type earlier
                # on the line (which itself may contain parens) can never be mistaken for the parameter list.
                if ($line -match '(\w+)\s*\(([^()]*)\)\s*;\s*$')
                {
                    if (-not $currentClass)
                    {
                        throw "Failed to sort accessor member into a containing class: $($file.FullName):$($i + 1)"
                    }
                    $declarations.Add([pscustomobject]@{
                        ClassName  = $currentClass
                        MemberName = $Matches[1]
                        File       = $file.FullName
                        Line       = $i + 1
                    })
                    $pendingAttr = $false
                }
            }
        }
    }
    return $declarations
}

function Get-TestLines([string]$root)
{
    $files = Get-ChildItem -Path $root -Filter '*.cs' -Recurse -File |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
    $lines = [System.Collections.Generic.List[string]]::new()
    foreach ($file in $files)
    {
        Get-Content -LiteralPath $file.FullName | ForEach-Object { $lines.Add($_) }
    }
    return $lines
}

$declarations = Get-AccessorDeclarations -root $SourceRoot
if ($declarations.Count -eq 0)
{
    Write-Host "Check-AccessorTestCoverage: no [UnsafeAccessor] declarations found under '$SourceRoot' - nothing to check."
    exit 0
}

$testLines = Get-TestLines -root $TestRoot

$uncovered = [System.Collections.Generic.List[pscustomobject]]::new()
foreach ($decl in $declarations)
{
    $classPattern = [regex]::Escape($decl.ClassName)
    $memberPattern = [regex]::Escape($decl.MemberName)
    $covered = $false
    foreach ($line in $testLines)
    {
        if ($line -match "\b$classPattern\b" -and $line -match "\b$memberPattern\b")
        {
            $covered = $true
            break
        }
    }
    if (-not $covered)
    {
        $uncovered.Add($decl)
    }
}

if ($uncovered.Count -gt 0)
{
    foreach ($decl in $uncovered)
    {
        # Canonical MSBuild diagnostic shape so Exec surfaces these as clickable Error List entries.
        Write-Host "$($decl.File)($($decl.Line),1): error LAQTEST001: [UnsafeAccessor] member '$($decl.ClassName).$($decl.MemberName)' is not referenced by name in any .cs file under '$TestRoot'. Add a test that exercises it (see CLAUDE.test-coverage-gaps.md section 4.2)."
    }
    Write-Host "Check-AccessorTestCoverage: $($uncovered.Count)/$($declarations.Count) [UnsafeAccessor] members have no test reference."
    exit 1
}

Write-Host "Check-AccessorTestCoverage: all $($declarations.Count) [UnsafeAccessor] members are referenced by name in test source."
exit 0
