# Project Tracking

> 全项目迭代、PD/PT、总进度和最近实现结果的索引。详细任务与每次 AI coding 记录保存在各迭代的 `PT####-Name.md`。

## 当前状态

- CurrentVersion:
- CurrentIteration: `Iter0-Initial`
- CurrentPD: `PD0001-Initial`
- CurrentPT: `PT0001-Initial`
- CurrentTask: 已完成 SixLabors 与图形验证码实现移除
- Status: `complete`
- Progress: `3/3`
- LastUpdated: `2026-09-03`

## 迭代进度

| Iteration | Product designs | Plan tasks | Status | Progress | Blocker | Next action |
|---|---|---|---|---|---|---|
| `Iter0-Initial` | [PD0001-Initial](../UserStory/Iter0-Initial/PD0001-Initial.md) | [PT0001-Initial](./Iter0-Initial/PT0001-Initial.md) | complete | 3/3 | none | 无 |

## 最近实现记录

| Date | Iteration / PT | Task | Result | Verification | Remaining |
|---|---|---|---|---|---|
| 2026-09-03 | Iter0-Initial / PT0001-Initial | TASK-001～003 | 已完成两个模板的图形验证码依赖、实现、测试和文档清理 | 两套 solution build、ApiStandard UnitTest 20/20、文档校验/构建通过 | 外部调用方需迁移已删除公开 helper |

## 阻塞与决定

| ID | Iteration / PD / PT | Type | Description | Owner / decision needed | Exit condition |
|---|---|---|---|---|---|

## 已完成迭代

| Iteration | Completed | PD/PT summary | Verification | Changelog |
|---|---|---|---|---|

## 状态与同步规则

- PD/PT 文档状态使用 `draft | planned | in-progress | blocked | verifying | complete`；其中 task 状态使用 `planned | in-progress | blocked | done | waived`。
- 每次 AI coding 后必须更新当前 PT 的实现记录、checkbox、进度与验证证据，再同步本文档。
- 代码改变需求、验收场景或设计时同步 PD；无 PD 影响时在 PT 记录原因。
- 实现和必需验证通过后才勾选 task。没有 ready task 但仍有 pending task 时标记 `blocked`并写明解除条件。
- 完成前检查 Completeness、Correctness 和 Coherence；有缺口时在当前 PT 追加 Convergence 任务并返回执行循环。
