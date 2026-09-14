# PT0002 - SourceGeneratorPackage

## 文档信息

- Iteration: `Iter0-Initial`
- PlanTask: `PT0002-SourceGeneratorPackage`
- Module: `TemplateAndCli`
- Status: `complete`
- Progress: `3/3`
- LastUpdated: `2026-09-14`

## 目标与范围

将模板和 Perigon CLI 对源代码生成器的消费方式统一为 `Perigon.AspNetCore.SourceGeneration` NuGet 包 1.1.1，避免构建业务项目时反复覆盖被 Roslyn/IDE 加载的生成器项目 DLL，并合并 ApiStandard 与 MiniApi 的生成能力。

## 任务

- [x] 将 ApiStandard、MiniApi 的源生成器项目引用改为集中包管理的 1.1.1 包引用，并从生成模板中排除源生成器项目。
- [x] 修改 Perigon CLI 的服务项目模板和创建流程，统一使用 1.1.1 包。
- [x] 同步 Perigon.docs 双语使用文档和变更记录。

## 实现记录

- ApiStandard 与 MiniApi 的 ManagerSourceGen 已统一支持 Manager、Module、Localizer 以及 Minimal API endpoint group 自动发现和 `MapEndpointGroups()` 注册。
- 两个模板和 CLI 均改为直接引用 `Perigon.AspNetCore.SourceGeneration` 1.1.1；CLI 不再显式构建源生成器项目。
- MiniApi 增加 Native AOT 所需的 `JwtOption` 初始化和 ApiService JSON serializer context。
- 模板包排除了两个模板目录中的源生成器项目及仓库维护文档。

## 验证证据

- ApiStandard Release solution build：0 警告、0 错误；单元测试 20/20。
- MiniApi Release solution build：0 警告、0 错误；测试 1/1。
- MiniApi Native AOT `win-x64` 默认发布成功；运行时 `/alive`、GET `/api/samples/7`、POST `/api/samples` 均返回 200。`linux-musl-x64` 原生链接未在 Windows 主机执行。
- Perigon.CLI 测试 155/155。
- 源生成器 1.1.1 nupkg 和模板 nupkg pack 通过；模板包不包含源生成器项目或仓库维护文档。
- Perigon.docs 校验通过（191 个 Markdown 文件），文档构建通过。

## 完成摘要

- FinalStatus: `COMPLETE`
- Package: `Perigon.AspNetCore.SourceGeneration` 1.1.1 已发布。
- Remaining: 无。
