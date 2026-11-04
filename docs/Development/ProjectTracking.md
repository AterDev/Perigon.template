# 项目跟踪

- 2026-10-30：两套模板统一内置 PostgreSQL；ApiStandard 移除 SQL Server 支持，验证见 PT0006。

- 2026-10-29：MiniApi 升级 Perigon.PostgreSQL 至 0.2.0，统一数据库上下文；验证情况见 PT0005。
- 2026-11-04：调整 MiniApi 用户业务逻辑至 `CommonMod`，通过源生成器自动注册模块与 Manager，修复启动失败；排除旧生成文件造成的误判，验证情况见 PT0005。
