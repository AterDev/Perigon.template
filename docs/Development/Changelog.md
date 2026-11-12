# Changelog

> 只记录 Perigon.template 仓库已交付的用户可见行为、公开契约、配置或运维变化。

## Unreleased

### Changed

- MiniApi 将 `Perigon.PostgreSQL` 升级至 0.3.1，以测试目标数据库缺失时的自动创建；数据库角色需要 `CREATEDB` 权限。`Perigon.PostgreSQL.Tools` 保持 0.3.0。

- MiniApi 移除 Mapster 包、运行时映射扩展和全局映射配置；实体与 DTO 转换改由对应类型中的显式代码实现，并在项目 backend skill 中规定。

- MiniApi 的 net11.0 项目默认启用 .NET 11 Runtime Async 预览功能；可设置 `UseRuntimeAsync=false` 关闭。

- MiniApi 目标框架升级至 `net11.0`，要求 .NET 11 SDK，ASP.NET Core 包升级至 11.0.0，Native AOT 容器基础镜像升级至 `11.0-resolute`；ApiStandard 仍使用 .NET 10。

- MiniApi 源生成器恢复使用已发布 NuGet 包，默认 Native AOT 发布 RID 改为 `linux-x64`，Docker 发布使用兼容 glibc 的运行镜像。
- MiniApi 的 JSON context 改由 Perigon CLI 的 `aot jsonContext` 命令显式生成；修改 Endpoint、模块模型或服务结构后需要重新运行。该 CLI 命令处于开发阶段，尚未确认随已发布 Perigon.CLI 提供。
- MiniApi 非生产环境提供 Swagger UI（`/swagger`），读取现有的 `/openapi/v1.json`。
- MiniApi AppHost 仅在 Angular 项目存在时启动前端资源，支持 `frontType=None` 和 `frontType=Angular`。

- MiniApi 使用 Perigon.PostgreSQL 的 `AddDbContextPool`、实体索引特性和内置 OpenTelemetry 注册；连接池开启，连接超时 15 秒、命令超时 30 秒。登录 DTO 移入 CommonMod 的 `Models/UserDtos`，OpenAPI 中的类型 schema 名称随之变化，JSON 字段保持不变。
- 登录接口按实体使用 `UserEndpoints`、`User` OpenAPI tag 和 `/api/users/login` 路由，并提供请求、响应元数据；调用方需将 `/api/auth/login` 更新为新地址。
- MiniApi 增加 `ApiResult<T>`，接口仅需声明成功 DTO 即可返回 Minimal API 结果，并自动描述 200 和 400 响应；其他状态需按需声明元数据。
- MiniApi 错误响应统一为继承标准 `HttpValidationProblemDetails` 的 `ErrorResult`：Content-Type 为 `application/problem+json`，traceId 由框架生成，`errors` 在字段验证失败时包含详情；客户端需适配此契约变化。
- MiniApi 启用跨程序集自动模型验证，登录用户名和密码采用必填及长度约束，验证失败在进入 handler 前返回 400。
- MiniApi 的 Problem Details writer 对不接受 JSON 的请求也保持统一错误结构；生产环境隐藏未处理异常和数据库错误详情并记录日志，已开始的响应与取消请求不再重写。
- MiniApi 恢复 `BadRequest`、`NotFound`、`Forbid`、`Conflict` 等本地化错误 helper；仅传资源键，统一 writer 根据请求语言生成详情。
- 模板包包含 MiniApi 的 CommonMod 项目，确保生成项目能解析模块引用和接口 DTO。
- MiniApi 解决方案移除指向不存在的 AotSmoke 项目的引用，并包含 CommonMod 项目。

- MiniApi 将 `UserManager` 和默认演示用户初始化放在 `CommonMod`，通过源生成器自动注册业务模块和 Manager，避免共享基础层依赖业务逻辑。

- ApiStandard 固定使用 PostgreSQL，移除 SQL Server 支持、`Components:Database` 配置及迁移脚本的 `DatabaseType` 参数。已有 SQL Server 项目需自行迁移数据和 EF 迁移。

- 两套模板的开发进度按 Git 用户名和月份分别记录，旧共享进度文件保留为只读历史。

- 两套模板 Aspire 升级至 13.6.4；MiniApi 的 Perigon.PostgreSQL 和本地工具升级至 0.3.0。
- MiniApi 统一使用 `EntityFramework.AppDbContext.DefaultDbContext`，删除重复上下文并调整依赖注入。

### Experimental / In Progress

- 已弃用编译前 JSON 元数据扫描实验：该方案曾使用本地 `Perigon.AspNetCore.SourceGeneration` 项目引用扫描模块和 Endpoint，但不再作为实现路径。MiniApi 的其他源生成能力已恢复使用已发布的 NuGet 包。
- 当前改为由 Perigon CLI 的 `aot jsonContext` 显式生成 `ApiJsonSerializerContext`；已完成本地验证，命令尚未发布。动态、反射、非泛型 `IResult`、运行时多态和无法推断的类型仍需手动注册。

## 1.3.17 - 2026-10-08

### Changed

- 两套模板的开发进度按 Git 用户名和月份分别记录，旧共享进度文件保留为只读历史。

- ApiStandard 的 Aspire AppHost 和 hosting 依赖升级至 13.6.1，并更新 .NET 10、EF Core、认证、OpenTelemetry、测试及第三方依赖。
- ApiStandard 的 EF Core Design 保留迁移工具所需能力，但不再作为传递依赖暴露给下游项目。
- 明确 Aspire 版本基线：ApiStandard 为 13.6.1，MiniApi 保持 13.5.0。

## 1.3.16 - 2026-09-24

### Changed

- 两套模板的开发进度按 Git 用户名和月份分别记录，旧共享进度文件保留为只读历史。

- ApiStandard、MiniApi 和 Perigon CLI 新服务改为通过 `Perigon.AspNetCore.SourceGeneration` 1.1.1 NuGet 包加载统一源生成器；不再复制或显式构建源生成器项目，并支持 Minimal API endpoint group 自动注册。
- ApiStandard 和 MiniApi 的 `IUserContext` 新增 `RoleIds`，读取并去重有效的 `role_id` GUID claims。
- ApiStandard 对 MVC 无效模型状态统一返回 HTTP 400，包含标题、验证详情、状态码和 TraceId。
- `GenSwagger.ps1` 会恢复本地工具清单、显式构建后再调用 Swagger 工具。
- 仓库维护文档统一放在根目录 `docs`，不再随模板生成到业务项目。

### Fixed

- ApiStandard 局部更新忽略 DTO 中的 Id、TenantId、CreatedTime，避免普通更新改变记录身份和租户归属。
- ApiStandard 批量插入先物化输入，确保字段赋值和插入使用同一批实体。
- ApiStandard、MiniApi 的 Between 保留嵌套属性访问链。
- MiniApi Native AOT 示例补充 `JwtOption` 配置初始化和 endpoint JSON metadata，确保生成路由可以在裁剪后的原生发布中启动并处理 JSON 请求。

### Removed

- 移除 ApiStandard 和 MiniApi 内置的 `ImageHelper.GenerateImageCaptcha` 图形验证码 helper、单元测试及其 SixLabors 依赖。

### Migration and compatibility

- 本批数据访问修复不改变方法签名或数据库结构。局部更新中 Id、TenantId、CreatedTime 将被忽略；租户迁移需要独立操作。null 跳过、软删除和时间范围语义保持不变。Attach stub 跨租户写入风险仍待后续修复，详见 PD0002。
- 现有项目若直接使用已删除的图形验证码 helper 或依赖其 SixLabors 传递包，需要自行迁移。
- `IUserContext.RoleIds` 依赖令牌中的 `role_id` GUID claims；已有令牌不包含这些 claims 时该列表为空。
- ApiStandard 的自动模型验证错误现在返回 `{ title, detail, status, traceId }` 结构，客户端若依赖旧的 ASP.NET Core 默认验证响应形状，需要调整解析逻辑。
- 使用 MiniApi 时，endpoint group 仍需继承 `RestEndpointBase` 并提供符合约定的 `MapEndpoints` 方法。

