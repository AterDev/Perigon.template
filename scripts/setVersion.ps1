param(
    [string]$Version = "1.0.0"
)

$scriptRoot = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
$repoRoot = (Resolve-Path (Join-Path $scriptRoot "..")).Path

# 定义项目文件路径数组（基于脚本所在目录解析）
$projects = @(
    (Join-Path $repoRoot "Pack.csproj"),
    (Join-Path $repoRoot "ApiStandard\src\Perigon\Perigon.AspNetCore\Perigon.AspNetCore.csproj"),
    (Join-Path $repoRoot "ApiStandard\src\Perigon\Perigon.AspNetCore.Toolkit\Perigon.AspNetCore.Toolkit.csproj"),
    (Join-Path $repoRoot "ApiStandard\src\Perigon\Perigon.AspNetCore.SourceGeneration\Perigon.AspNetCore.SourceGeneration.csproj")
)

foreach ($project in $projects) {
    if (Test-Path $project) {
        Write-Host "Updating version in $project to $Version"
        # 读取文件内容
        $content = Get-Content $project -Raw
        
        # 使用正则表达式替换 Version 标签
        $newContent = $content -replace '<Version>.*?</Version>', "<Version>$Version</Version>"
        
        # 写回文件
        Set-Content $project $newContent -Encoding UTF8
    } else {
        Write-Warning "Project file not found: $project"
    }
}

Write-Host "✅ Version update completed. New version: $Version"
