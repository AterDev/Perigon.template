# PT0005：依赖与上下文调整

- [x] 升级 MiniApi 的 Perigon.PostgreSQL，保留工具已发布版本。
- [x] 删除重复上下文并统一服务注册与文档。
- [x] 对照 0.2.0 包检查模板使用的 API。
- [x] MiniApi 解决方案构建、模板打包及 win-x64 Native AOT 发布通过；AOT 发布存在裁剪和动态代码分析警告。
- [x] 检查包中仅包含 AppDbContext 下的 MiniApi 上下文，根目录维护文档未打包。
- 测试限制：解决方案测试发现零个测试并返回退出码 5；未验证实际 PostgreSQL 运行时操作。
