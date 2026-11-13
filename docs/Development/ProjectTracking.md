# 项目跟踪

- 2026-11-13：MiniApi 数据库初始化改用 `FirstOrDefaultAsync`，修复 async-only PostgreSQL 查询提供程序拒绝同步执行的问题；验证见 PT0011。
- 2026-11-13：修复 MiniApi Angular 登录仍调用旧 Admin API 的问题，启用开发服务器代理转发到 ApiService；验证见 PT0010。
- 2026-11-12：MiniApi 升级至 .NET 11、Aspire 13.6.5 和最新依赖，MailKit 5 兼容性已适配；验证见 PT0007、PT0008。
- 2026-11-12：ApiStandard 与 MiniApi 的邮件发送服务适配 MailKit 5；两套解决方案构建验证见 PT0009。
- 2026-11-11：MiniApi 显式启用 .NET 11 Runtime Async，WSL Containers 运行验证见 PT0008。
- 2026-11-12：MiniApi 将 Perigon.PostgreSQL 升级至 0.3.1，验证数据库不存在时的创建路径见 PT0008。
- 2026-11-12：MiniApi 移除 Mapster 和运行时映射扩展，转换规则写入 backend skill；验证见 PT0008。
- 2026-11-11：MiniApi 升级至 .NET 11，SDK、运行时包、镜像和 CI 同步；验证见 PT0008。
- 2026-11-06：MiniApi 源生成器切回包引用，Native AOT 和 Docker 发布默认改用 `linux-x64`。
- 2026-11-06：撤销 MiniApi 编译前 JSON 自动扫描实验，改由 Perigon CLI 的 `aot jsonContext` 命令显式生成 `ApiJsonSerializerContext`；CLI 命令仍处于开发阶段，验证和发布版本待完成。
- 2026-10-30：两套模板统一内置 PostgreSQL；ApiStandard 移除 SQL Server 支持，验证见 PT0006。

- 2026-10-29：MiniApi 升级 Perigon.PostgreSQL 至 0.2.0，统一数据库上下文；验证情况见 PT0005。
- 2026-11-04：调整 MiniApi 用户业务逻辑至 `CommonMod`，通过源生成器自动注册模块与 Manager，修复启动失败；排除旧生成文件造成的误判，验证情况见 PT0005。
- 2026-11-05：MiniApi 改用 Perigon.PostgreSQL 的上下文选项注入与实体索引特性，整理模块 DTO 和 Perigon skill；验证见 PT0005。
- 2026-11-05：MiniApi 按实体命名 UserEndpoints，并启用 Perigon.PostgreSQL 上下文池、连接池与 OpenTelemetry；验证见 PT0005。
- 2026-11-05：登录接口组改为 User，路由调整为 `/api/users/login` 并补充 OpenAPI 元数据；验证见 PT0005。
- 2026-11-05：MiniApi 新增非生产环境 Swagger UI，修复本地 Angular 依赖安装并兼容无前端模板；验证见 PT0005。
- 2026-11-05：登录接口使用强类型 Minimal API 返回值，去除可自动推断的 OpenAPI 元数据配置；验证见 PT0005。
- 2026-11-05：MiniApi 增加 `ApiResult<T>`，简化成功与统一错误结果并集中提供 OpenAPI 元数据；验证见 PT0005。
- 2026-11-05：MiniApi 错误 helper 延迟获取请求标识，移除登录 Endpoint 对 `HttpContext` 的依赖；验证见 PT0005。
- 2026-11-05：MiniApi 移除自定义错误类型并改用 ASP.NET Core Problem Details；验证见 PT0005。
- 2026-11-05：MiniApi 启用跨程序集自动验证，并以基于 Problem Details 的 `ErrorResult` 统一错误响应；验证见 PT0005。
- 2026-11-05：审核 MiniApi 异常与错误响应，修复内容协商回退及异常处理中断场景，评估 AOT JSON 源生成方案；验证见 PT0005。
- 2026-11-05：MiniApi 错误 helper 改为传入资源键，由统一 writer 解析本地化详情；验证见 PT0005。
- 2026-11-06：此前的 MiniApi 编译前 JSON 自动扫描实验标记为已弃用；MiniApi 的其他源生成能力已恢复使用已发布的 NuGet 包。JSON context 改由 Perigon CLI 的 `aot jsonContext` 显式生成，CLI 尚未完成验证和发布。
