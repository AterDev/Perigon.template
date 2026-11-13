# PT0011：MiniApi 数据库初始化异步查询

- [x] 从 Aspire ApiService 日志确认异常来自 `DatabaseInitService` 对 `Users` 查询调用同步 `FirstOrDefault`。
- [x] 将查询改为 `Where(...).FirstOrDefaultAsync(stoppingToken)`，沿用 MiniApi 中已有的异步查询模式。
- [x] Aspire ApiService rebuild 成功，0 警告、0 错误；健康检查通过，初始化日志创建演示用户且不再报告同步查询异常。
- [x] `dotnet pack Pack.csproj --no-restore` 通过；模板包包含更新后的初始化源码，仓库根目录维护文档未打包。
