# 说明

本项目基于 `ASP.NET Core Minimal API / NativeAOT / Aspire / Perigon.PostgreSQL` 技术栈，提供结构清晰的模块化单接口服务项目结构，并对 AI 工具提供良好的支持。

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
* `src/Services/ApiService`: 接口服务项目，基于 ASP.NET Core Minimal API
	* `src/Services/ApiService/Endpoints`: Minimal API endpoint group 定义目录
	* `src/Services/ApiService/Managers`: 业务逻辑目录
	* `src/Services/ApiService/Models`: DTO、筛选对象和请求响应模型目录
	* `src/Services/ApiService/Services`: Service 内部辅助服务目录


## 项目运行

项目基于 `Aspire`，直接运行 `AppHost` 项目即可启动 PostgreSQL、缓存和 `ApiService`。

如使用`dotnet run  --project .\src\AppHost\AppHost.csproj`.

或者使用 `aspire run` 命令运行。

## AOT 与数据访问

- 本模板不使用 MVC Controller、EF Core、EF migration 和多租户。
- MiniApi 默认不走完整的 `Module` 开发模式，大部分后端代码直接写在 `ApiService` 项目中。
- Endpoint 目录中定义 endpoint group，继承 `RestEndpointBase` 并实现 `public static void MapEndpoints(IEndpointRouteBuilder endpoints)`。
- 业务代码优先按 `Endpoints/Managers/Models/Services` 分层，而不是先拆独立模块程序集。
- OpenAPI 文档地址为 `/openapi/v1.json`，服务项目与共享模型项目默认生成 XML 文档，XML 注释会由 `Microsoft.AspNetCore.OpenApi` 源生成器写入 OpenAPI 文档。Endpoint handler 应使用 public static typed Minimal API 方法，避免 `RequestDelegate` 风格导致 OpenAPI 无法描述接口。
- 数据访问只使用 `Perigon.PostgreSQL`。`DefaultDbContext` 使用 `DbContextOptions<TContext>` 和 `options.UseNpgsql(connectionString)` 的注册方式。新增实体后，在 `src/Definition/EntityFramework/DefaultDbContext.cs` 中添加 `DbSet<TEntity>` 属性，以便源生成器生成 AOT 友好的元数据。
- `ApiService` 默认按 NativeAOT 方式发布，可直接执行 `dotnet publish src/Services/ApiService/ApiService.csproj -c Release`。

## 文档

- [快速入门](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E5%BF%AB%E9%80%9F%E5%85%A5%E9%97%A8.html)
- [项目模板](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E9%A1%B9%E7%9B%AE%E6%A8%A1%E6%9D%BF/%E6%A6%82%E8%BF%B0.html)
- [开发规范](https://dusi.dev/docs/Perigon/zh-CN/10.0/%E6%9C%80%E4%BD%B3%E5%AE%9E%E8%B7%B5/%E5%BC%80%E5%8F%91%E8%A7%84%E8%8C%83%E4%B8%8E%E7%BA%A6%E5%AE%9A.html)


完整文档请阅读[Perigon官方文档](https://dusi.dev/docs/Perigon.html)。
