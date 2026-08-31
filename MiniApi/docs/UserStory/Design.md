# 产品与系统设计总览

> 本文档只保留全局架构约束、跨模块设计决定和设计目录。某个迭代/功能的详细产品与技术设计写入对应 `Iter*/PD####-Name.md`。

## 系统上下文

## 全局架构与模块边界

- API 使用 ASP.NET Core Minimal API。
- ApiService 默认启用 Native AOT、Trim 与 Request Delegate Generator。
- 数据库固定使用 PostgreSQL；AppHost 不包含内置 migration resource。

## Endpoint、服务与数据所有权

## Native AOT、Trim 与 JSON 基线

## 全局鉴权与安全约束

## 全局兼容、发布与回滚策略

## 功能设计索引

| Iteration | Product Design | Key decisions | Status |
|---|---|---|---|
| `Iter0-Initial` | [PD0001-Initial](./Iter0-Initial/PD0001-Initial.md) | 待记录 | draft |

## 全局设计决策

| ID | Decision | Rationale | Alternatives | Related PD |
|---|---|---|---|---|

## 已知限制与技术债
