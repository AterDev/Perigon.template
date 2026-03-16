$OutputEncoding = [Console]::OutputEncoding = [Text.UTF8Encoding]::UTF8
$scriptRoot = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
$root = (Resolve-Path (Join-Path $scriptRoot "..")).Path
$packagePath = Join-Path $root "ApiStandard\src\Perigon"
$outputPath = Join-Path $root "nupkg"

Write-Host $root
$corePackagePath = Join-Path $packagePath "Perigon.AspNetCore"
$sourceGeneratorPackagePath = Join-Path $packagePath "Perigon.AspNetCore.SourceGeneration"


try {
    dotnet pack (Join-Path $corePackagePath "Perigon.AspNetCore.csproj") -c Release -o $outputPath

    dotnet pack (Join-Path $sourceGeneratorPackagePath "Perigon.AspNetCore.SourceGeneration.csproj") -c Release -o $outputPath
}
catch {
    Write-Error $_.Exception.Message
}
