# Build a new portable preview without overwriting previous packages.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    & (Join-Path $PSScriptRoot 'check.ps1')
    function Invoke-Checked {
        param([string]$Program, [string[]]$Arguments)
        & $Program @Arguments
        if ($LASTEXITCODE -ne 0) { throw "$Program failed: $LASTEXITCODE" }
    }
    $packageId = 'ImpactLab-win-x64-' + [Guid]::NewGuid().ToString('N').Substring(0,8)
    $packageDir = Join-Path (Get-Location) ('artifacts/' + $packageId)
    New-Item -ItemType Directory -Path $packageDir | Out-Null
    Invoke-Checked -Program dotnet -Arguments @('publish', 'src/ImpactLab.App/ImpactLab.App.csproj', '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-o', $packageDir)
    Copy-Item -LiteralPath 'samples' -Destination $packageDir -Recurse
    Copy-Item -LiteralPath 'README.md','LICENSE' -Destination $packageDir
    Copy-Item -LiteralPath 'docs' -Destination $packageDir -Recurse
    foreach ($requiredFile in @('ImpactLab.App.exe')) {
        if (-not (Test-Path -LiteralPath (Join-Path $packageDir $requiredFile))) { throw "Missing package payload: $requiredFile" }
    }
    Compress-Archive -Path (Join-Path $packageDir '*') -DestinationPath ($packageDir + '.zip')
    Get-FileHash -LiteralPath ($packageDir + '.zip') -Algorithm SHA256
    Write-Host 'Portable preview created. Test on a clean Windows machine before publishing a release.'
} finally { Pop-Location }
