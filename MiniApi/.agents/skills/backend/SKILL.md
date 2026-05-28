---
name: backend
description: "ASP.NET Core Minimal API / NativeAOT / Aspire / Perigon.PostgreSQL 后端开发规范。Use when: endpoint group, Manager, DTO, Entity, DbContext, PostgreSQL, AOT, BusinessException, Problem response, backend build, module architecture."
---

## 何时使用

任何涉及后端逻辑、模块结构、Minimal API endpoint、Manager、实体和 PostgreSQL 数据访问的内容。

## 项目结构层级

```sh
src/
├── Perigon/                 # 基础类库、工具扩展和源生成项目
├── Definition/
│   ├── Entity/              # 实体定义（按模块分文件夹）
│   ├── EntityFramework/     # 实体与数据库定义，基于 Perigon.PostgreSQL
│   ├── Share/               # 共享常量、扩展、服务
│   └── ServiceDefaults/     # 服务注册和中间件
├── Modules/
│   └── {ModuleName}/
│       ├── Managers/        # 业务逻辑层
│       ├── Controllers/     # Minimal API endpoint group 定义
│       ├── Models/          # DTO 定义（按实体分文件夹）
│       └── Services/        # 模块内服务（可选）
└── Services/
    ├── ApiService/          # 公共 API
    └── AdminService/        # 管理后台 API
```

## 核心约定

- 本模板面向 NativeAOT：优先使用 Minimal API、静态 handler、显式 DTO 投影和源生成可识别的 public 类型。
- 不使用 MVC Controller、`ControllerBase`、EF Core、EF migration、多数据库或多租户。
- 数据访问只使用 `Perigon.PostgreSQL`，当前只支持 PostgreSQL。
- `DefaultDbContext` 位于 `src/Definition/EntityFramework`。添加实体后要在其中添加 `DbSet<TEntity>` 属性，以便 Perigon.PostgreSQL 源生成实体元数据。
- 实体应使用 public parameterless constructor 和 public get/set 属性；需要自定义表/列时使用 `Perigon.PostgreSQL.Attributes`。
- Manager 承担业务逻辑，继承 `ManagerBase<TDbContext, TEntity>` 或 `ManagerBase<TDbContext>` / `ManagerBase`。
- Controller 目录保留，但其中定义 endpoint group：类继承 `RestControllerBase`，提供 `public static void MapEndpoints(IEndpointRouteBuilder endpoints)`。
- Endpoint handler 只做输入验证、权限验证和 HTTP 结果转换；业务验证放在 Manager，业务错误抛出 `BusinessException`。
- Endpoint group 会由源生成器自动收集并通过 `app.MapEndpointGroups()` 注册。

## AOT 编码要求

- 避免运行时反射扫描、动态程序集加载、MVC application parts、隐式模型绑定魔法和运行时生成代理。
- 服务项目启用 Request Delegate Generator；endpoint handler 使用 public static 方法，并返回 `IResult`、`TypedResults` 或 `Task<IResult>` 等 typed Minimal API 结果。
- 不要把 handler 写成 `RequestDelegate` / `Task` + `ExecuteAsync(HttpContext)` 风格，否则 OpenAPI 可能无法描述返回值；`Task<IResult>` handler 传给 `MapGet` 等方法时必要时显式 `(Delegate)HandlerAsync`，避免误匹配到 `RequestDelegate` 重载。
- 不要使用已废弃的 `WithOpenApi()` 补 metadata，它会引入 trimming/AOT warning；应使用 `AddOpenApi()`、typed handler、XML 注释和显式 `Produces` / `Accepts` metadata。
- DTO/响应类型使用具体 public class/record，保持 public setter；避免只依赖私有构造或反射赋值。
- 查询时优先写显式 `Select(x => new Dto { ... })`，不要依赖运行时自动映射。
- `Perigon.PostgreSQL` 更新使用 `ExecuteUpdateAsync(setter => setter.Set(...))`，删除使用 `ExecuteDeleteAsync()` 或 ManagerBase 的软删除方法。
- 批量写入使用 `BulkInsertAsync` / `UpsertManyAsync`，不要引入 EFCore.BulkExtensions。
- OpenAPI 使用 `Microsoft.AspNetCore.OpenApi` 的 `AddOpenApi` / `MapOpenApi`，启动地址使用 `/openapi/v1.json`。服务项目和被 OpenAPI 使用的引用项目应开启 `GenerateDocumentationFile`，让 XML 注释通过源生成器进入文档。

## 开发流程

1. 定义实体，并在 `DefaultDbContext` 添加对应 `DbSet<TEntity>`。
2. 定义 DTO 和筛选模型；列表和详情查询优先显式投影。
3. 在模块 `Managers` 中实现业务逻辑；不要让 Manager 之间直接调用。
4. 在 `Controllers` 中定义 endpoint group，handler 调用 Manager。
5. 执行 `dotnet build MyProjectName.slnx` 验证；发布 NativeAOT 前执行对应服务的 `dotnet publish -c Release -p:PublishAot=true`。

## 返回值约定

- 成功：返回 `IResult` / `TypedResults` / 具体 DTO。
- 错误：使用 `RestControllerBase` 中的 `Problem`、`NotFound`、`BadRequest` 等帮助方法，错误消息优先使用 `Localizer` 常量。
- 参数绑定：有歧义时使用 `[FromBody]` / `[FromQuery]` / `[FromRoute]`，或将复杂参数拆成明确 DTO。

## Aspire 集成

AppHost 是 Aspire 项目，默认创建 PostgreSQL 和可选 Redis。服务通过 `WithReference(database)` 获得 `ConnectionStrings__Default`，应用侧由 `AddFrameworkServices()` 注册 `DefaultDbContext`。
