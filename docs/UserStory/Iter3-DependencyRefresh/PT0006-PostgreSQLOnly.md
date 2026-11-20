# PT0006：统一 PostgreSQL 数据库

- [x] 清理 ApiStandard 数据库选择、SQL Server 依赖与兼容逻辑，保留 PostgreSQL 及 SQLite 单元测试支持。
- [x] 检查 MiniApi 已固定使用 PostgreSQL。
- [x] 两套解决方案构建通过；ApiStandard 单元测试 22 项通过。
- [x] 模板打包通过；包内含两个模板配置且未包含根目录维护文档或源生成器源码。两套模板均从包生成并构建通过。
- 未验证真实 PostgreSQL 运行时及已有 SQL Server 数据迁移。
