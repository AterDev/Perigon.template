# 说明

本项目基于 `ASP.NET Core Minimal API / NativeAOT / Aspire / Perigon.PostgreSQL` 技术栈，提供结构清晰的多模块、多服务项目结构，并对 AI 工具提供良好的支持。

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
* `src/Definition/EntityFramework`: 实体与数据库定义，基于 Perigon.PostgreSQL
* `src/Modules/`: 包含各个模块的程序集，主要用于业务逻辑实现
	* `src/Modules/XXXMod/Managers`: 各模块下，实际实现业务逻辑的目录
	* `src/Modules/XXXMod/Controllers`: 各模块下，Minimal API endpoint group 定义目录
	* `src/Modules/XXXMod/Models`: 各模块下，Dto模型定义，按实体目录组织
* `src/Services/ApiService`: 公共接口服务项目，基于 ASP.NET Core Minimal API
* `src/Services/AdminService`: 后台管理服务接口项目


## 项目运行

项目基于 `Aspire`，直接运行 `AppHost` 项目即可启动 PostgreSQL、缓存和所有服务。

如使用`dotnet run  --project .\src\AppHost\AppHost.csproj`.

或者使用 `aspire run` 命令运行。

## AOT 与数据访问

- 本模板不使用 MVC Controller、EF Core、EF migration 和多租户。
- Controller 目录中定义 endpoint group，继承 `RestControllerBase` 并实现 `public static void MapEndpoints(IEndpointRouteBuilder endpoints)`。
- OpenAPI 文档地址为 `/openapi/v1.json`，服务项目与共享模型项目默认生成 XML 文档，XML 注释会由 `Microsoft.AspNetCore.OpenApi` 源生成器写入 OpenAPI 文档。Endpoint handler 应使用 public static typed Minimal API 方法，避免 `RequestDelegate` 风格导致 OpenAPI 无法描述接口。
- 数据访问只使用 `Perigon.PostgreSQL`。新增实体后，在 `src/Definition/EntityFramework/DefaultDbContext.cs` 中添加 `DbSet<TEntity>` 属性，以便源生成器生成 AOT 友好的元数据。
- 发布 NativeAOT 可使用 `dotnet publish -c Release -p:PublishAot=true`。

## 文档

- [快速入门](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E5%BF%AB%E9%80%9F%E5%85%A5%E9%97%A8.html)
- [项目模板](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E9%A1%B9%E7%9B%AE%E6%A8%A1%E6%9D%BF/%E6%A6%82%E8%BF%B0.html)
- [开发规范](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E6%9C%80%E4%BD%B3%E5%AE%9E%E8%B7%B5/%E5%BC%80%E5%8F%91%E8%A7%84%E8%8C%83%E4%B8%8E%E7%BA%A6%E5%AE%9A.html)


完整文档请阅读[Perigon官方文档](https://dusi.dev/docs/Perigon.html)。
