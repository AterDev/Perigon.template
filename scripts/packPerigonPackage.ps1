$OutputEncoding = [Console]::OutputEncoding = [Text.UTF8Encoding]::UTF8
$location = Get-Location
$root = Split-Path $location -Parent 
$packagePath = Join-Path $root "/ApiStandard/src/Perigon"

Write-Host $root
$corePackagePath = Join-Path $packagePath "Perigon.AspNetCore"
$sourceGeneratorPackagePath = Join-Path $packagePath "Perigon.AspNetCore.SourceGeneration"


try {
    Set-Location $corePackagePath
    dotnet pack -c Release -o "$root/nupkg"

    Set-Location $sourceGeneratorPackagePath
    dotnet pack -c Release -o "$root/nupkg"
}
catch {
    Write-Error $_.Exception.Message
}
finally {
    Set-Location $location
}