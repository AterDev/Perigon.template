# 说明

本项目基于 `Asp.Net Core/EF Core/Aspire`技术栈，提供结构清晰的项目结构，并对模块化，AI工具提供了良好的支持。

## 根目录

- docs: 项目文档存储目录
- scripts： 项目脚本文件目录
- src：项目代码目录
- test：测试项目目录
- .config：配置文件目录

## 代码目录src

* `src/Perigon/Perigon.AspNetCore`: 基础类库，提供基础帮助类。
* `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
* `src/Definition/Entity`: 包含所有的实体模型，按模块目录组织。
* `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
* `src/Modules/`: 包含各个模块的程序集，主要用于业务逻辑实现
	* `src/Modules/XXXMod/Managers`: 各模块下，实际实现业务逻辑的目录
	* `src/Modules/XXXMod/Models`: 各模块下，Dto模型定义，按实体目录组织
* `src/Services/ApiService`: 是接口服务项目，基于ASP.NET Core Web API
* `src/Services/AdminService`: 后台管理服务接口项目


## 项目运行

项目基于`Aspire`，使用 Aspire CLI 启动 AppHost 即可启动基础设施和服务。

```pwsh
aspire start --non-interactive
```

默认不启动 Angular 前端；如选择 Angular 前端并完成 `pnpm install`，可按 AppHost 中的可选示例启用它。

Standard 的 OpenAPI JSON 默认位于 `/swagger/v1/swagger.json`，模板不默认提供 Swagger UI。

Standard 使用 Aspire 的 `AddEFMigrations` 管理 EF Core 迁移：`AdminService` 提供 EF CLI 必需的设计时启动适配入口，具体的迁移定制实现位于 `Definition/EntityFramework/DesignTime`，迁移文件位于 `Definition/EntityFramework`；AppHost 本地启动时会先应用 `DefaultDbContext` 的待处理迁移，再启动 API 与后台服务，发布到 Kubernetes 时使用一次性 Job 执行迁移。默认系统租户通过 `DefaultDbContext` 的 `UseSeeding`/`UseAsyncSeeding` 在迁移后幂等初始化；该全局租户不会设置 `TenantId`。修改实体后运行 `.\scripts\EFMigrations.ps1 Init` 生成迁移。模板默认不包含 `SystemMod`，因此不会预置管理员账号。

单元测试不会启动 Aspire：

```pwsh
dotnet test --project tests/UnitTest/UnitTest.csproj
```

需要 Docker/Podman 和真实服务时，再运行 `tests/ApiTest` 中标记为 `Integration` 的测试。

## 文档

- [快速入门](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E5%BF%AB%E9%80%9F%E5%85%A5%E9%97%A8.html)
- [项目模板](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E9%A1%B9%E7%9B%AE%E6%A8%A1%E6%9D%BF/%E6%A6%82%E8%BF%B0.html)
- [开发规范](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E6%9C%80%E4%BD%B3%E5%AE%9E%E8%B7%B5/%E5%BC%80%E5%8F%91%E8%A7%84%E8%8C%83%E4%B8%8E%E7%BA%A6%E5%AE%9A.html)


完整文档请阅读[Perigon官方文档](https://dusi.dev/docs/Perigon.html)。
