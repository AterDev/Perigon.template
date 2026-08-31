# Project Tracking

> 全项目迭代、PD/PT、总进度和最近实现结果的索引。详细任务与每次 AI coding 记录保存在各迭代的 `PT####-Name.md`。

## 当前状态

- CurrentVersion:
- CurrentIteration: `Iter0-Initial`
- CurrentPD: `PD0001-Initial`
- CurrentPT: `PT0001-Initial`
- CurrentTask:
- Status: `planned`
- Progress: `0/0`
- LastUpdated:

## 迭代进度

| Iteration | Product designs | Plan tasks | Status | Progress | Blocker | Next action |
|---|---|---|---|---|---|---|
| `Iter0-Initial` | [PD0001-Initial](../UserStory/Iter0-Initial/PD0001-Initial.md) | [PT0001-Initial](./Iter0-Initial/PT0001-Initial.md) | planned | 0/0 | none | 确认初始需求与任务 |

## 最近实现记录

| Date | Iteration / PT | Task | Result | Verification / AOT | Remaining |
|---|---|---|---|---|---|

## 阻塞与决定

| ID | Iteration / PD / PT | Type | Description | Owner / decision needed | Exit condition |
|---|---|---|---|---|---|

## 已完成迭代

| Iteration | Completed | PD/PT summary | Verification | Changelog |
|---|---|---|---|---|

## 状态与同步规则

- PD/PT 文档状态使用 `draft | planned | in-progress | blocked | verifying | complete`；task 状态使用 `planned | in-progress | blocked | done | waived`。
- 每次 AI coding 后更新当前 PT 的实现记录、checkbox、进度、验证与 AOT evidence，再同步本文档。
- 代码改变需求、场景或设计时同步 PD；无 PD 影响时在 PT 记录原因。
- 实现和必需验证通过后才勾选 task；AOT-sensitive 任务缺少 publish/运行证据时不得完成。
- 完成前检查 Completeness、Correctness 和 Coherence；缺口追加 Convergence 任务。
