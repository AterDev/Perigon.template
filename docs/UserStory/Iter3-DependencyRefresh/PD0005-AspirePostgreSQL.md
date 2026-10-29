# PD0005：Aspire 与 MiniApi PostgreSQL

- 两套模板使用 Aspire 13.6.4；MiniApi 使用 Perigon.PostgreSQL 0.2.0，本地工具使用当前发布的 0.1.8。
- MiniApi 保留 `EntityFramework.AppDbContext.DefaultDbContext`，通过 scoped 工厂从默认连接字符串构造；Manager 和服务使用同一类型。
- 0.2.0 中模板使用的查询、写入和事务 API 可用；其他消费者应独立评估升级影响。
