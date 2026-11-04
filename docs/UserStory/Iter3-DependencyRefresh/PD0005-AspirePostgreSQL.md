# PD0005：Aspire 与 MiniApi PostgreSQL

- 两套模板使用 Aspire 13.6.4；MiniApi 使用 Perigon.PostgreSQL 0.2.0，本地工具使用当前发布的 0.1.8。
- MiniApi 保留 `EntityFramework.AppDbContext.DefaultDbContext`，通过 scoped 工厂从默认连接字符串构造；Manager 和服务使用同一类型。
- 0.2.0 中模板使用的查询、写入和事务 API 可用；其他消费者应独立评估升级影响。

## MiniApi CommonMod 业务边界

- `UserManager` 和依赖该 Manager 的数据库演示用户初始化属于业务功能，放在 `CommonMod`；`Share` 只保留共享基础实现。
- ApiService 引用 `CommonMod`，由已发布的源生成器自动生成 `AddManagers()` 中的 `UserManager` 注册及 `AddModules()` 中的 `AddCommonMod()` 调用；模块扩展负责注册数据库初始化服务。
- `ServiceDefaults` 不直接依赖 `UserManager` 等业务类型。
