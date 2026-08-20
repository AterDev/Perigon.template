# Perigon.templates

`Perigon.templates`是基于`Aspire`,`ASP.NET Core`和`Entity Framework Core`的快速开发模板，提供一个设计良好的项目工程结构。

模板已被集成在`Perigon.CLI`代码辅助工具中，建议使用该命令行工具来创建解决方案，以获得更好的体验。

## 要求

- .NET 10.0 SDK（建议 10.0.103 或兼容的 .NET 10 SDK）
- Aspire CLI/SDK 13.4.6（与模板包 1.3.12 对应）

## 文档

关于模板详细的说明和使用方法，请查阅[使用文档](https://www.dusi.dev/docs/Perigon.html)！

## 安装

## 使用源代码安装

- 拉取源代码
- 执行`install.ps1`脚本安装。

## 使用Nuget安装

模板已经发布到[`nuget`](https://www.nuget.org/packages/Perigon.templates)上，请根据你的项目版本下载对应的模板。

```pwsh
dotnet new install Perigon.templates --version 1.3.12
```

## 模板说明

当前仓库提供两套可选模板：

- `MiniApi`：面向轻量接口服务，优先采用 Minimal API、NativeAOT、Request Delegate Generator 与 `Perigon.PostgreSQL`，适合高性能、低资源开销、对 AI 工具友好的后端服务。模板短名称为 `perigon-miniapi`。
- `ApiStandard`：面向传统 Web API 场景，基于 ASP.NET Core Web API、EF Core 与 Aspire，适合需要标准 MVC/Web API 生态、模块化组织和更传统开发方式的项目。

## MiniApi 模板的作用与使用场景

MiniApi 模板适合以下场景：

- 需要构建轻量级 API 服务，启动快、内存占用低。
- 希望优先使用 Minimal API 与 NativeAOT，获得更好的发布性能和运行效率。
- 需要与 AI/代码生成工具配合时，代码结构更直接、层次更清晰。
- 适用于网关、微服务、数据接口、内部系统接口等对性能和部署成本敏感的项目。

## 创建项目

下面给出两套模板的创建命令示例：

### 创建 MiniApi 模板

```pwsh
dotnet new perigon-miniapi -n MyMiniApi
```

### 创建 ApiStandard 模板

```pwsh
dotnet new perigon-webapi -n MyWebApi
```

如果你想指定模板名称并直接生成到当前目录，也可以这样：

```pwsh
dotnet new perigon-miniapi -n <projectname>
dotnet new perigon-webapi -n <projectname>
```

## 数据库

- `ApiStandard` 支持在 AppHost 的 `Components:Database` 中选择 `PostgreSQL` 或 `SqlServer`。
- `MiniApi` 固定使用 PostgreSQL，数据访问通过 `Perigon.PostgreSQL` 提供。

## 数据迁移

`ApiStandard` 使用 EF Core 和 Aspire `AddEFMigrations`。修改实体后，在
`ApiStandard` 目录运行 `.\scripts\EFMigrations.ps1 Init` 生成迁移；EF CLI 使用默认的
`AdminService` 作为启动项目，AppHost 启动或发布的 Kubernetes Job 会执行迁移。

`MiniApi` 不包含 EF Core 迁移服务，也不要求运行该迁移脚本；请按项目的 PostgreSQL 数据访问约定管理数据库变更。

## 运行项目

使用 Aspire CLI 启动 `AppHost`：

```pwsh
aspire start --non-interactive
```

模板默认不包含 `SystemMod`，因此不会预置可登录的管理员账号。安装模块后请按模块文档初始化账号，不要把示例凭据带入生产环境。
