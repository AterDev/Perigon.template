# PT0005：ApiStandard 依赖更新与 1.3.17 发布

## 目标

按 PD0004 更新 ApiStandard 依赖基线，验证两套模板与包内容，发布 Perigon.templates 1.3.17，并同步双语文档。

## 任务

- [x] 更新 ApiStandard Aspire AppHost / hosting packages 至 13.6.1，并刷新该模板的运行时、测试和第三方依赖。
- [x] 将 EF Core Design 依赖限制为项目本地设计时工具所需，避免传递给模板使用方。
- [x] 更新包版本、release notes、README、根目录迭代记录与 Perigon.docs 双语页面。
- [x] 恢复、Release 构建两套模板；运行 CI 对应测试；打包并检查 nupkg 内容。
- [x] 构建并验证双语文档。
- [ ] 合并至 `nuget` 并发布，记录 CI 和 NuGet 结果。

## 实现与验证

两套模板均 restore/build 成功；ApiStandard 单测 22/22，MiniApi 集成测试 1/1；`Perigon.templates` 1.3.17 nupkg 元数据和内容范围通过检查；Perigon.docs 的 199 篇 Markdown 校验及完整构建通过。等待 nuget 分支发布工作流和 NuGet 页面确认。
