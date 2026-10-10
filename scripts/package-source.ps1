[CmdletBinding()]
param(
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$repositoryParent = Split-Path -Parent $repositoryRoot
$repositoryName = Split-Path -Leaf $repositoryRoot

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repositoryParent "$repositoryName-source-latest.zip"
}

$archivePath = [System.IO.Path]::GetFullPath($OutputPath)
$repositoryBoundary = $repositoryRoot.TrimEnd(
    [System.IO.Path]::DirectorySeparatorChar,
    [System.IO.Path]::AltDirectorySeparatorChar
) + [System.IO.Path]::DirectorySeparatorChar
if ($archivePath.StartsWith($repositoryBoundary, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "The source archive must be written outside the repository to avoid packaging itself."
}

$archiveDirectory = Split-Path -Parent $archivePath
[System.IO.Directory]::CreateDirectory($archiveDirectory) | Out-Null

$arguments = @(
    "-a",
    "-c",
    "-f", $archivePath,
    "--exclude=$repositoryName/.git",
    "--exclude=$repositoryName/.vs",
    "--exclude=$repositoryName/.idea",
    "--exclude=$repositoryName/.codex-tmp",
    "--exclude=$repositoryName/.codex_tmp",
    "--exclude=$repositoryName/artifacts",
    "--exclude=$repositoryName/TestResults",
    "--exclude=$repositoryName/**/bin",
    "--exclude=$repositoryName/**/obj",
    "--exclude=$repositoryName/**/*.user",
    "-C", $repositoryParent,
    $repositoryName
)

& tar @arguments
if ($LASTEXITCODE -ne 0) {
    throw "tar failed with exit code $LASTEXITCODE."
}

$file = Get-Item -LiteralPath $archivePath
$hash = Get-FileHash -LiteralPath $archivePath -Algorithm SHA256
[pscustomobject]@{
    Path = $file.FullName
    SizeBytes = $file.Length
    Sha256 = $hash.Hash
    CreatedUtc = $file.LastWriteTimeUtc.ToString("O")
}
