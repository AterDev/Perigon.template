# PD0005：Aspire 与 MiniApi PostgreSQL

- MiniApi 使用已发布的 `Perigon.AspNetCore.SourceGeneration` 包；Release 默认 RID 为 `linux-x64`，Docker 使用兼容 glibc 的运行镜像。

- 两套模板使用 Aspire 13.6.4；MiniApi 使用 Perigon.PostgreSQL 0.3.0，本地工具使用 0.3.0。
- MiniApi 保留 `EntityFramework.AppDbContext.DefaultDbContext`，使用 Perigon.PostgreSQL 0.3.0 的 `DbContextOptions<DefaultDbContext>` 和 `AddDbContextPool` 注入；连接字符串从宿主配置读取，Manager 和服务使用同一类型。启用连接池和 OpenTelemetry，连接与命令超时分别为 15 秒和 30 秒。
- 0.2.0 中模板使用的查询、写入和事务 API 可用；其他消费者应独立评估升级影响。

## MiniApi CommonMod 业务边界

- `UserManager` 和依赖该 Manager 的数据库演示用户初始化属于业务功能，放在 `CommonMod`；`Share` 只保留共享基础实现。
- ApiService 引用 `CommonMod`，由已发布的源生成器自动生成 `AddManagers()` 中的 `UserManager` 注册及 `AddModules()` 中的 `AddCommonMod()` 调用；模块扩展负责注册数据库初始化服务。
- `ServiceDefaults` 不直接依赖 `UserManager` 等业务类型。
- `User` 使用默认表名约定与实体索引特性；Endpoint 的 DTO 位于对应模块的 `Models/UserDtos`，OpenAPI schema 名称随 DTO 类型名更新。
- Endpoint 类型、路由组和 OpenAPI tag 按实体命名；`UserEndpoints` 使用 `/api/users/login`。`RestEndpointBase.ApiResult<T>` 支持直接返回成功 DTO 和常见 Minimal API 结果，并自动描述 200 DTO 和 400 `ErrorResult`；其他状态码由接口按需声明。
- 错误响应使用继承 `HttpValidationProblemDetails` 的 `ErrorResult` 和 `application/problem+json`，包含 traceId 与字段 `errors`；普通错误的 `errors` 为空。`RestEndpointBase` 的错误 helper 将资源键直接写入 detail，统一 writer 注入 Localizer 按请求语言解析，未匹配时保留原文，无需 Endpoint 注入 Localizer。
- ApiService 和 CommonMod 分别注册 .NET 10 自动验证，跨程序集登录 DTO 标记验证根并声明必填与长度规则；CommonMod 的验证源生成器生成对应元数据。
- 统一的 Problem Details writer 为异常、验证错误及空错误状态输出 `ErrorResult`；即使请求 Accept 不包含 JSON，也输出 `application/problem+json`。生产环境隐藏意外异常详情并记录日志，不再向已开始的响应写入，也不把取消的请求转为 500。
- Native AOT 的 JSON 根类型仍需在已注册的 `JsonSerializerContext` 中声明；Request Delegate Generator 不生成序列化元数据。此前的编译前 JSON 扫描实验已弃用，其他源生成能力使用已发布的 `Perigon.AspNetCore.SourceGeneration` NuGet 包。当前改为由 Perigon CLI 的 `aot jsonContext` 命令显式生成 `ApiJsonSerializerContext`；CLI 尚在实现，未宣称已发布或已完成验证。修改 Endpoint、模块模型或服务结构后需要重新运行；动态、反射、非泛型 `IResult`、运行时多态和无法确定的类型仍需手动注册。
- MiniApi 模板包需包含 CommonMod 项目及 DTO，保持生成项目的模块引用完整。
- 非生产环境使用 Swagger UI 展示 ASP.NET Core 生成的 `/openapi/v1.json`；生产环境不映射文档和 UI。
- AppHost 在 Angular 项目的 `package.json` 存在时才创建前端资源，以支持无前端的模板输出。
