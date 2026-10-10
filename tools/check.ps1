$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    & (Join-Path $PSScriptRoot 'check-ui.ps1')
    function Invoke-Checked {
        param([string]$Program, [string[]]$Arguments)
        & $Program @Arguments
        if ($LASTEXITCODE -ne 0) { throw "$Program failed with exit code $LASTEXITCODE" }
    }
    Invoke-Checked -Program dotnet -Arguments @('restore', 'ImpactLab.sln')
    Invoke-Checked -Program dotnet -Arguments @('build', 'ImpactLab.sln', '-c', 'Release', '--no-restore')
    Invoke-Checked -Program dotnet -Arguments @('test', 'ImpactLab.sln', '-c', 'Release', '--no-build')
    Write-Host 'Build and automated checks completed. Interactive UI is a separate check.'
} finally {
    Pop-Location
}
