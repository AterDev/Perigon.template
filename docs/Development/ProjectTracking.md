# Project Tracking

> Perigon.template 仓库迭代、PD/PT、总进度和实现验证记录。本文档只位于仓库根目录，不进入 ApiStandard 或 MiniApi 模板。

## 当前状态

- CurrentIteration: `Iter2-ApiBehavior`
- CurrentPD: `PD0003-ApiBehavior`
- CurrentPT: `PT0004-ApiBehavior`
- CurrentTask: 角色 ID Claims 与无效模型状态响应已纳入模板包 1.3.16，等待发布工作流完成
- Status: `complete`
- Progress: `2/2`
- LastUpdated: `2026-09-24`

## 迭代进度

| Iteration | Product designs | Plan tasks | Status | Progress | Blocker | Next action |
|---|---|---|---|---|---|---|
| `Iter0-Initial` | [PD0001-Initial](../UserStory/Iter0-Initial/PD0001-Initial.md) | [PT0001-Initial](./Iter0-Initial/PT0001-Initial.md); [PT0002-SourceGeneratorPackage](./Iter0-Initial/PT0002-SourceGeneratorPackage.md) | complete | 3/3 | none | 无 |
| `Iter1-DataAccess` | [PD0002](../UserStory/Iter1-DataAccess/PD0002-ApiStandardDataAccess.md) | [PT0003](./Iter1-DataAccess/PT0003-DataAccess.md) | in-progress | 首批 6/6 | none | 优先处理 DA-01，剩余问题按 PD 分批实施 |
| `Iter2-ApiBehavior` | [PD0003](../UserStory/Iter2-ApiBehavior/PD0003-ApiBehavior.md) | [PT0004](./Iter2-ApiBehavior/PT0004-ApiBehavior.md) | complete | 2/2 | none | 无 |

## 最近实现记录

| Date | Plan task | Result | Verification | Remaining |
|---|---|---|---|---|
| 2026-09-24 | PT0004-ApiBehavior | 在两套模板增加 `IUserContext.RoleIds`，为 ApiStandard 自动模型验证错误增加结构化 HTTP 400 响应，并准备模板包 1.3.16 发布 | 两套 Release build；ApiStandard 22/22；MiniApi Aspire smoke test 1/1；包内容核验通过；双语文档校验 191 篇并构建通过 | 推送 `nuget` 后确认 GitHub Actions 和 NuGet 发布状态 |
| 2026-09-22 | PT0003-DataAccess | 整理数据访问改进说明，修复局部更新受保护字段、批量重复枚举和嵌套范围查询 | ApiStandard 22/22；两套 solution build；本地 pack/内容检查；双语文档校验/构建通过 | DA-01 等后续项仍未修复；未运行正式数据库批量集成测试 |
| 2026-09-03 | PT0001-Initial | 移除模板图形验证码实现及 SixLabors 依赖 | 两套 solution build、ApiStandard 20/20 测试、Perigon.docs 校验/构建通过 | 外部调用方需迁移已删除公开 helper |
| 2026-09-14 | PT0002-SourceGeneratorPackage | 两套模板和 CLI 切换到 `Perigon.AspNetCore.SourceGeneration` 1.1.1，并合并 Manager、Module、Localizer 与 Minimal API endpoint group 生成能力 | 两套 solution build、CLI 155/155 测试、MiniApi Native AOT 运行验证、包构建和文档校验通过；1.1.1 已发布 | 无 |

## 维护规则

- PD/PT、ProjectTracking 和仓库 Changelog 只保存在根目录 `docs`。
- 模板目录只保留生成项目所需文件，不新增仓库内部跟踪内容。
- 每次代码修改后更新对应 PT 的任务、实现结果和验证证据。
