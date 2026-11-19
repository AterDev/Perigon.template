# PT0012 MiniApi DbSet 自动属性

- [x] 公开 Perigon.PostgreSQL 基类的 `Set<TEntity>()`，保留裁剪标注。
- [x] 源生成初始化集合属性，并在上下文池租用时重新绑定。
- [x] MiniApi 改用自动属性，移除包装方法。
- [x] Perigon.PostgreSQL 单元测试与 Windows x64 Native AOT 冒烟通过；MiniApi 使用本地 0.3.4 包完成 Debug 构建与 Windows x64 Native AOT 发布，模板包内容检查通过。
- [x] 记录 0.3.4 版本内容并生成 Release 包。MiniApi 从本机 NuGet 源恢复并构建通过，全局 `dotnet-perigon` 已更新到 0.3.4。
- [ ] 在可用的 PostgreSQL/Docker 环境运行数据库和 Aspire 集成测试。
- [ ] 将 Perigon.PostgreSQL 0.3.4 发布到模板消费者使用的包源。
