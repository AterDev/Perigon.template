# PD0006：统一 PostgreSQL 数据库

- 两套模板默认且仅内置 PostgreSQL；ApiStandard 使用 EF Core/Npgsql，MiniApi 使用 Perigon.PostgreSQL。
- 移除 ApiStandard 的数据库类型选择、SQL Server 依赖和兼容分支。迁移资源、租户连接和分析连接均使用 PostgreSQL。
- 已有 SQL Server 项目需自行迁移数据、连接字符串和 EF 迁移，不能直接沿用原数据库；SQLite 仅用于隔离单元测试。
