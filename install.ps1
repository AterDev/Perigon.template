$OutputEncoding = [Console]::OutputEncoding = [Text.UTF8Encoding]::UTF8

$location = Get-Location
$solutionPath = Join-Path $location "ApiStandard"
$modulePath = Join-Path $solutionPath "src/Modules"
$packPath = Join-Path $location "nupkg"
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
    dotnet pack -c release -o $packPath
    # get package info
    $VersionNode = Select-Xml -Path ./Pack.csproj -XPath '/Project//PropertyGroup/PackageVersion'
    $PackageNode = Select-Xml -Path ./Pack.csproj -XPath '/Project//PropertyGroup/PackageId'
    $Version = $VersionNode.Node.InnerText
    $PackageId = $PackageNode.Node.InnerText

    #re install package
    dotnet new uninstall $PackageId
    dotnet new install $packPath\$PackageId.$Version.nupkg

    Set-Location $location;
}
catch {
    Write-Error $_.Exception.Message
    Set-Location $location;
}


    
