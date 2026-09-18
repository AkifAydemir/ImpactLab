param(
    [string]$EvidencePath = "artifacts/verification/verify-evidence.json"
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$evidenceFile = Join-Path $repo $EvidencePath
$stages = [System.Collections.Generic.List[object]]::new()
$evidence = [ordered]@{
    schemaVersion = 1
    generatedUtc = [DateTimeOffset]::UtcNow.ToString('O')
    overallStatus = 'not-run'
    dotnet = [ordered]@{
        available = $false
        version = $null
    }
    stages = $stages
    error = $null
}

function Add-Stage {
    param(
        [string]$Name,
        [string]$Status,
        [int]$ExitCode,
        [string]$Summary
    )

    $stages.Add([ordered]@{
        name = $Name
        status = $Status
        exitCode = $ExitCode
        summary = $Summary
    })
}

function Save-Evidence {
    $directory = Split-Path -Parent $evidenceFile
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    $json = $evidence | ConvertTo-Json -Depth 8
    [System.IO.File]::WriteAllText(
        $evidenceFile,
        $json,
        [System.Text.UTF8Encoding]::new($false))
}

function Invoke-DotNetStage {
    param(
        [string]$Name,
        [string[]]$Arguments
    )

    & dotnet @Arguments
    $code = $LASTEXITCODE
    if ($code -ne 0) {
        Add-Stage $Name 'failed' $code "dotnet $($Arguments -join ' ') failed."
        throw "$Name failed with exit code $code."
    }

    Add-Stage $Name 'passed' 0 "dotnet $($Arguments -join ' ') passed."
}

Push-Location $repo
try {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnet) {
        Add-Stage 'sdk' 'environment-blocked' 127 '.NET SDK executable was not found on PATH.'
        $evidence.overallStatus = 'environment-blocked'
        Save-Evidence
        exit 4
    }

    try {
        $versionOutput = & dotnet --version
        $versionCode = $LASTEXITCODE
    }
    catch {
        Add-Stage 'sdk' 'environment-blocked' 126 'dotnet executable was found but the SDK query could not start.'
        $evidence.overallStatus = 'environment-blocked'
        $evidence.error = $_.Exception.Message
        Save-Evidence
        exit 4
    }

    if ($versionCode -ne 0) {
        Add-Stage 'sdk' 'environment-blocked' $versionCode 'dotnet executable was found but the SDK could not be queried.'
        $evidence.overallStatus = 'environment-blocked'
        $evidence.error = "dotnet --version could not execute successfully (exit code $versionCode)."
        Save-Evidence
        exit 4
    }

    $version = ($versionOutput | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($version)) {
        Add-Stage 'sdk' 'environment-blocked' 126 'dotnet executable returned an empty SDK version.'
        $evidence.overallStatus = 'environment-blocked'
        $evidence.error = 'dotnet --version returned no SDK version.'
        Save-Evidence
        exit 4
    }

    $evidence.dotnet.available = $true
    $evidence.dotnet.version = $version
    Add-Stage 'sdk' 'passed' 0 ".NET SDK $version is available."

    Invoke-DotNetStage 'restore' @('restore', '.\ImpactLab.sln')
    Invoke-DotNetStage 'build' @('build', '.\ImpactLab.sln', '-c', 'Release', '--no-restore')
    Invoke-DotNetStage 'tests' @(
        'test',
        '.\tests\ImpactLab.Core.Tests\ImpactLab.Core.Tests.csproj',
        '-c', 'Release',
        '--no-build',
        '--results-directory', '.\artifacts\verification\test-results',
        '--logger', 'trx;LogFileName=ImpactLab.Core.Tests.trx'
    )

    $evidence.overallStatus = 'passed'
    Save-Evidence
    exit 0
}
catch {
    if ($evidence.overallStatus -eq 'not-run') {
        $evidence.overallStatus = 'failed'
    }
    $evidence.error = $_.Exception.Message
    Save-Evidence
    exit 1
}
finally {
    Pop-Location
}
