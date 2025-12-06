param(
    [string]$Version = "1.0.0"
)

# 定义项目文件路径数组（相对于脚本所在目录）
$projects = @(
    "..\Pack.csproj",
    "..\ApiStandard\src\Perigon\Perigon.AspNetCore\Perigon.AspNetCore.csproj",
    "..\ApiStandard\src\Perigon\Perigon.AspNetCore.Toolkit\Perigon.AspNetCore.Toolkit.csproj",
    "..\ApiStandard\src\Perigon\Perigon.AspNetCore.SourceGeneration\Perigon.AspNetCore.SourceGeneration.csproj"
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
