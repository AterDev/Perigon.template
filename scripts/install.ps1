$OutputEncoding = [Console]::OutputEncoding = [Text.UTF8Encoding]::UTF8

$scriptRoot = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..")).Path
$solutionPath = Join-Path $repoRoot "ApiStandard"
$packPath = Join-Path $repoRoot "nupkg"
$packProjectPath = Join-Path $repoRoot "Pack.csproj"
$requiredModules = @("SystemMod", "CommonMod");

Write-Host "Clean files"
# delete nupkg files
if (Test-Path $packPath) {
    Remove-Item $packPath -Force -Recurse
}
# delete files
$migrationPath = Join-Path $solutionPath "src/Definition/EntityFramework/Migrations"
if (Test-Path $migrationPath) {
    Remove-Item $migrationPath -Force -Recurse
}

try {
    # pack
    dotnet pack $packProjectPath -c release -o $packPath
    # get package info
    $VersionNode = Select-Xml -Path $packProjectPath -XPath '/Project//PropertyGroup/Version'
    $PackageNode = Select-Xml -Path $packProjectPath -XPath '/Project//PropertyGroup/PackageId'
    $Version = $VersionNode.Node.InnerText
    $PackageId = $PackageNode.Node.InnerText
    $packageFilePath = Join-Path $packPath "$PackageId.$Version.nupkg"

    #re install package
    dotnet new uninstall $PackageId
    dotnet new install $packageFilePath
}
catch {
    Write-Error $_.Exception.Message
}


    
