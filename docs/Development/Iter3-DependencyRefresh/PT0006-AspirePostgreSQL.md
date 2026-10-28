# PT0006：Aspire 与 MiniApi PostgreSQL 升级

- [x] 升级两套模板的 Aspire 至 13.6.4，配套预览包至 13.6.4-preview.1.26523.11。
- [x] MiniApi 升级 Perigon.PostgreSQL 并固定本地工具 0.1.8。
- [x] 更新仓库记录和双语文档；不发布模板包。
- [ ] 完成验证并记录结果。

## 验证结果

- 两套模板 Release 构建通过，均 0 警告、0 错误；ApiStandard 单测 22/22 通过。
- MiniApi 本地工具恢复通过；集成测试因 Docker 守护进程不可用而失败，需在 Docker 可用时重跑。
- 模板包构建及工具清单、根目录 docs 排除检查通过；双语文档 199 篇校验与完整构建通过。

