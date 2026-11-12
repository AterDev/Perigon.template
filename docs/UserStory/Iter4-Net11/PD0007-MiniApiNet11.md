# PD0007：MiniApi 升级 .NET 11

- MiniApi 的运行、AppHost 和测试项目目标框架统一改为 `net11.0`，SDK 基线为 11.0.100；ApiStandard 保持 .NET 10。
- Microsoft ASP.NET Core 和同版本框架扩展包使用 11.0.0，保留独立版本的其他依赖及已发布的源生成器包；生成器维护项目保持 `netstandard2.0`。
- Native AOT Linux 镜像使用 `mcr.microsoft.com/dotnet/runtime-deps:11.0-resolute`。继续使用现有验证行为，不在本次升级中新增验证消息本地化。
- CI 同时安装 .NET 10 和 11；包生成项目和用户文档分别说明两个模板的 SDK 要求。

- MiniApi 的 net11.0 项目统一启用预览功能 Runtime Async；UseRuntimeAsync=false 可按项目关闭，不改变 netstandard2.0 源生成器。
- MiniApi 不使用对象映射包、映射生成器或映射工具。DTO/实体转换由 AI 在对应类型中编写显式方法；DTO 可创建或更新实体，实体类只实现自身领域行为，避免 `Definition/Entity` 反向引用业务模块 DTO。
- MiniApi 使用 `Perigon.PostgreSQL` 0.3.1，以便在目标数据库缺失时由 `EnsureCreatedAsync` 通过维护库创建；连接角色需要 `CREATEDB`。本地 PostgreSQL 工具仍为 0.3.0。
