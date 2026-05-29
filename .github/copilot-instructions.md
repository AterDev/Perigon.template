# GitHub Copilot Instructions

本仓库用于维护 `Perigon.templates` 的模板包与模板源码，根目录下同时包含两个可复用的后端模板：`MiniApi` 与 `ApiStandard`。

## 仓库作用

- 根目录的 `Pack.csproj`、`Program.cs`、`scripts/` 用于模板打包、版本处理与安装。
- `MiniApi/` 是基于 ASP.NET Core Minimal API、NativeAOT、Aspire 和 `Perigon.PostgreSQL` 的轻量模板。
- `ApiStandard/` 是基于 ASP.NET Core Web API、EF Core 和 Aspire 的标准模板。

## 目录结构

- `MiniApi/` 与 `ApiStandard/` 都按 `src/`、`tests/`、`docs/`、`scripts/`、`templates/` 组织。
- `src/AppHost` 是 Aspire 启动入口与资源编排。
- `src/Definition` 放实体、共享模型、服务默认配置和数据库上下文定义。
- `src/Services` 放服务宿主项目；当前 `MiniApi` 仅保留 `ApiService`。
- `src/ClientApp/WebApp` 是前端项目位置。

## 模板差异

- `MiniApi`：优先 Minimal API、NativeAOT、Request Delegate Generator、`Perigon.PostgreSQL`，适合轻量接口服务和更严格的 AOT 约束。
- `ApiStandard`：优先传统 ASP.NET Core Web API + EF Core，保留更常见的标准 Web API 开发体验。
- 数据访问差异：`MiniApi` 使用 `Perigon.PostgreSQL` 和 options 风格的 `AddDbContext<TContext>(options => options.UseNpgsql(...))`；`ApiStandard` 使用 EF Core。
- 服务形态差异：`MiniApi` 当前模板是单服务 `ApiService`；`ApiStandard` 仍保留 `ApiService` 与 `AdminService` 双服务结构。

## 协作要求

- 修改模板后优先做最小范围 `dotnet build` 或 `dotnet publish` 验证，再扩展到整模板验证。
- 变更模板时同步更新对应 README、脚本和 Copilot 指令，避免模板说明与实际结构脱节。