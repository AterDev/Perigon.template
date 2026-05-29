[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Service,

    [Parameter(Mandatory = $true)]
    [string]$ImageName,

    [string]$Tag = 'latest',

    [string]$Configuration = 'Release',

    [switch]$InstallFonts,

    [ValidateSet('font-wqy-zenhei', 'font-noto-cjk')]
    [string]$CjkFontPackage = 'font-wqy-zenhei',

    [switch]$NoRestore
)

<#
.SYNOPSIS
Publishes a single ASP.NET Core service and builds a Docker image from its Dockerfile.

.DESCRIPTION
This script is intended for packaging one standalone service image at a time.
If you need distributed application orchestration, dependencies, or environment composition,
use the Aspire AppHost instead of this script.

.EXAMPLE
.\scripts\PublishDocker.ps1 -Service ApiService -ImageName myprojectname-api-service

.EXAMPLE
.\scripts\PublishDocker.ps1 -Service ApiService -ImageName myprojectname-api-service -Tag v1 -InstallFonts
#>

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$runtimeIdentifier = 'linux-musl-x64'

function Format-ImageSize {
    param([long]$Bytes)

    if ($Bytes -ge 1GB) {
        return ('{0:N2} GiB' -f ($Bytes / 1GB))
    }

    return ('{0:N2} MiB' -f ($Bytes / 1MB))
}

$projectPath = Join-Path $repoRoot "src/Services/$Service/$Service.csproj"
$dockerfilePath = Join-Path $repoRoot "src/Services/$Service/Dockerfile"
$publishDir = Join-Path $repoRoot "artifacts/publish/$Service"
$publishDirForDocker = "artifacts/publish/$Service"
$imageTag = "${ImageName}:$Tag"

if (-not (Test-Path $projectPath)) {
    throw "Project file not found: $projectPath"
}

if (-not (Test-Path $dockerfilePath)) {
    throw "Dockerfile not found: $dockerfilePath"
}

if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

$publishArgs = @(
    'publish', $projectPath,
    '--configuration', $Configuration,
    '--runtime', $runtimeIdentifier,
    '--self-contained', 'true',
    '--output', $publishDir,
    '/p:PublishTrimmed=true',
    '/p:PublishAot=true',
    '/p:PublishReadyToRun=false',
    '/p:InvariantGlobalization=false',
    '/p:UseAppHost=true',
    '/p:StripSymbols=true',
    '/p:DebugType=None',
    '/p:DebugSymbols=false'
)

if ($NoRestore) {
    $publishArgs += '--no-restore'
}

try {
    Write-Host "Publishing $Service..."
    & dotnet @publishArgs

    Write-Host "Building $imageTag..."
    $dockerBuildArgs = @(
        'build',
        '--file', $dockerfilePath,
        '--build-arg', "PUBLISH_DIR=$publishDirForDocker",
        '--build-arg', "INSTALL_FONTS=$($InstallFonts.IsPresent.ToString().ToLowerInvariant())",
        '--build-arg', "CJK_FONT_PACKAGE=$CjkFontPackage",
        '--tag', $imageTag,
        $repoRoot
    )

    & docker @dockerBuildArgs

    $sizeBytes = [long](& docker image inspect $imageTag --format '{{.Size}}')
    Write-Host "$imageTag size: $(Format-ImageSize $sizeBytes)"
}
finally {
    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }
}