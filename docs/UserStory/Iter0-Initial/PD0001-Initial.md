# PD0001 - Initial Product Design

## 文档信息

- Iteration: `Iter0-Initial`
- ProductDesign: `PD0001-Initial`
- Module: `TemplateAndCli`
- Status: `complete`
- LastUpdated: `2026-09-14`

## 背景与目标

模板通过项目引用加载源代码生成器时，生成项目构建可能锁定 `bin` 下的生成器 DLL，导致后续构建复制失败。本迭代统一使用 `Perigon.AspNetCore.SourceGeneration` 1.1.1 NuGet 包，并合并 ApiStandard 与 MiniApi 的生成能力。

## 范围

### 包含

- 两套模板的服务/共享项目使用 `Perigon.AspNetCore.SourceGeneration` 1.1.1 NuGet 包。
- 模板生成项目不包含源代码生成器项目和仓库内部维护文档。
- Perigon.CLI 新服务模板和创建解决方案流程不再构建源代码生成器项目。

### 不包含

- 不引入新的生成代码契约或 Roslyn 兼容基线；仅合并 ApiStandard 与 MiniApi 已有的生成能力。

## 需求与验收场景

### REQ-001 源代码生成器使用稳定 NuGet 包

- Given: 使用任一 Perigon 模板或通过 CLI 添加 Web 服务。
- When: 还原并构建解决方案。
- Then: 项目通过 `Perigon.AspNetCore.SourceGeneration` 1.1.1 生成代码，不引用或构建 `SourceGeneration.csproj`。

## 设计决策

| ID | Decision | Rationale | Alternatives |
|---|---|---|---|
| D-002 | 使用集中包管理声明 `Perigon.AspNetCore.SourceGeneration` 1.1.1，并从模板产物中排除源生成器项目 | 避免构建时覆盖被 Roslyn/IDE 使用的项目输出 DLL，同时保持服务项目直接获得分析器 | 保留 `ProjectReference` 或由 CLI 显式构建源生成器都会继续保留文件锁风险 |
| D-003 | 将 ApiStandard 与 MiniApi 的源生成器合并为同一 1.1.1 包能力集合 | 保持 Manager、Module、Localizer 与 Minimal API endpoint group 生成行为一致，避免两个模板分别演进产生冲突 | 继续维护两套不同生成器会导致包消费结果不一致 |

## 实现状态

- Status: `complete`
- Implemented requirements: `REQ-001`
- Latest evidence: 两套模板 solution build、ApiStandard 20/20 测试、MiniApi 1/1 测试、CLI 155/155 测试、MiniApi Native AOT 运行验证、源生成器和模板包构建均通过；`Perigon.AspNetCore.SourceGeneration` 1.1.1 已发布。
