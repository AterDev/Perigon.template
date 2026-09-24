# 产品与系统设计总览

> 本文档只保留仓库级架构约束、跨模板设计决定和设计目录。某个迭代/功能的详细设计写入对应 `Iter*/PD####-Name.md`。

## 系统上下文

仓库包含两套模板目录 `ApiStandard`、`MiniApi`，以及根目录模板打包项目 `Pack.csproj`。Perigon.CLI 位于相邻仓库，通过嵌入模板内容生成服务项目。

## 全局架构与模块边界

- `ApiStandard` 面向传统 ASP.NET Core Web API。
- `MiniApi` 面向 Minimal API 和 Native AOT。
- `Perigon.AspNetCore.SourceGeneration` 是独立 NuGet analyzer 包，不作为生成项目复制到新项目中。
- 仓库维护文档只位于根目录 `docs`。

## 功能设计索引

| Iteration | Product Design | Key decisions | Status |
|---|---|---|---|
| `Iter0-Initial` | [PD0001-Initial](./Iter0-Initial/PD0001-Initial.md) | 源代码生成器统一使用 NuGet 包并合并能力 | complete |
| `Iter1-DataAccess` | [PD0002-ApiStandardDataAccess](./Iter1-DataAccess/PD0002-ApiStandardDataAccess.md) | 数据访问评估；先修复受保护字段更新、批量枚举与嵌套范围查询 | in-progress |
| `Iter2-ApiBehavior` | [PD0003-ApiBehavior](./Iter2-ApiBehavior/PD0003-ApiBehavior.md) | 暴露 role_id Claims，并统一 ApiStandard 自动模型验证错误响应结构 | complete |

## 全局设计决策

| ID | Decision | Rationale | Alternatives | Related PD |
|---|---|---|---|---|
| D-001 | 仓库维护文档放在根目录 `docs` | 避免项目跟踪内容被模板复制到业务项目 | 将维护文档放在两个模板目录 | PD0001 |

## 已知限制与技术债

- Windows 主机无法执行 `linux-musl-x64` 原生链接；MiniApi 的 AOT 证据使用 `win-x64` 发布并记录在 PT 中。
