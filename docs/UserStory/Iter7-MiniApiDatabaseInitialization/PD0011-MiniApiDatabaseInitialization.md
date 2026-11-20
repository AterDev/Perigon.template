# PD0011：MiniApi 数据库初始化异步查询

## 背景

`Perigon.PostgreSQL` 使用仅支持异步查询的提供程序。MiniApi `DatabaseInitService` 在数据库创建后同步调用 `FirstOrDefault` 检查演示用户，导致初始化失败并记录 `Synchronous query execution is not supported`。

## 目标

- 数据库初始化过程中所有数据库查询均使用异步执行。
- 演示用户存在性检查传递服务停止令牌，并保持幂等插入行为。
- 保持 `ApiStandard` 的 EF Core 初始化逻辑不变。

## 验收标准

- `DatabaseInitService` 使用 `FirstOrDefaultAsync` 检查演示用户。
- ApiService 可通过 Aspire rebuild，并且初始化日志不再报告同步查询异常。
- 模板包包含更新后的 MiniApi 源码，仓库根目录维护文档不进入模板包。
