# PT0007：MiniApi .NET 11 升级

- [x] MiniApi Release 解决方案与独立源生成器构建通过，均为 0 警告、0 错误。
- [x] 模板包生成的隔离 MiniApi 项目 Release 构建通过，0 警告、0 错误。
- [x] Angular 前端依赖安装和生产构建通过；中英文文档校验和构建通过。
- TypeScript 保持 Angular 22 支持的 6.0.3；pnpm 12 的最小发布年龄策略阻止当天发布的 Angular 包安装，暂保留已验证的 pnpm 9.14.2。
- [ ] 集成测试启动失败：当前环境未安装 Docker；容器运行及集成测试待具备 Docker 的环境验证。
