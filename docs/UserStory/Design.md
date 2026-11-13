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
| `Iter3-DependencyRefresh` | [PD0004-DependencyRefresh](./Iter3-DependencyRefresh/PD0004-DependencyRefresh.md) | ApiStandard 升级 Aspire 和依赖；MiniApi 保持现有基线 | complete |

| `Iter3-DependencyRefresh` | [PD0005-AspirePostgreSQL](./Iter3-DependencyRefresh/PD0005-AspirePostgreSQL.md) | 升级 Aspire 和 MiniApi PostgreSQL | in-progress |

| `Iter3-DependencyRefresh` | [PD0006-PostgreSQLOnly](./Iter3-DependencyRefresh/PD0006-PostgreSQLOnly.md) | 移除 SQL Server 配置及兼容逻辑 | complete |
| `Iter3-DependencyRefresh` | [PD0007-MiniApiNet11](./Iter3-DependencyRefresh/PD0007-MiniApiNet11.md) | MiniApi 采用 .NET 11、更新依赖及构建环境 | in-progress |

| `Iter4-Net11` | [PD0008-MiniApiNet11Aot](./Iter4-Net11/PD0008-MiniApiNet11Aot.md) | 完善 MiniApi .NET 11 AOT 与运行行为；ApiStandard 保持 .NET 10 | complete |
| `Iter5-MailKit5` | [PD0009-MailKit5Compatibility](./Iter5-MailKit5/PD0009-MailKit5Compatibility.md) | 两套模板使用 MailKit 5；ApiStandard 导入发送扩展，MiniApi 使用 `SendRequest` | complete |
| `Iter6-MiniApiFrontend` | [PD0010-MiniApiFrontend](./Iter6-MiniApiFrontend/PD0010-MiniApiFrontend.md) | MiniApi Angular 前端通过本地代理调用 MiniApi 登录 Endpoint，并基于实际用户响应维护界面登录状态 | complete |
| `Iter7-MiniApiDatabaseInitialization` | [PD0011-MiniApiDatabaseInitialization](./Iter7-MiniApiDatabaseInitialization/PD0011-MiniApiDatabaseInitialization.md) | MiniApi 数据库初始化通过异步查询检查演示用户，避免调用 Perigon.PostgreSQL 不支持的同步查询 | complete |

## 全局设计决策

| ID | Decision | Rationale | Alternatives | Related PD |
|---|---|---|---|---|
| D-001 | 仓库维护文档放在根目录 `docs` | 避免项目跟踪内容被模板复制到业务项目 | 将维护文档放在两个模板目录 | PD0001 |

## 已知限制与技术债

- Windows 主机无法执行 `linux-musl-x64` 原生链接；MiniApi 的 AOT 证据使用 `win-x64` 发布并记录在 PT 中。
