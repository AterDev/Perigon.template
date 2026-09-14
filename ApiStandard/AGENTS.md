# AGENTS

## 总体原则

- 以仓库代码、配置、工具帮助和实际验证为事实依据；不确定内容显式调查或标记待确认。
- 代码和文档以可读、可维护为先，使用清晰命名、小而单一的职责和必要注释控制复杂度。
- 纯调研或只读审查不额外生成文档。任何 AI coding 都必须在结束前同步对应迭代 PT、`ProjectTracking.md` 和受影响 PD，记录实现结果和验证证据。
- 优先使用操作系统、IDE 和项目现有工具，例如 `pwsh`、`dotnet`、`pnpm`、Perigon 和 Aspire。
- 代码修改后执行与影响面匹配的最小充分验证。没有实现证据和通过的必需验证，不宣称任务完成。

## 技术栈

1. C# 14、.NET 10、ASP.NET Core 10 和 EF Core 10。
2. Aspire 13+ 负责分布式应用编排、运行和可观测性。
3. 前端默认使用 Angular，具体以仓库现有代码和版本为准。

## 项目结构

- 前端：`src/ClientApp/WebApp`
  - 共享能力：`src/ClientApp/WebApp/src/app/modules/share`
  - 业务模块：`src/ClientApp/WebApp/src/app/modules/{module}`
- 后端接口服务：`src/Services`
- 实体定义：`src/Definition/Entity`
- 业务逻辑：`src/Modules`，按模块划分
- 共享定义：`src/Definition/Share`
- 服务扩展：`src/Definition/ServiceDefaults`
- AppHost：`src/AppHost`
- 文档：`docs`
  - 产品需求/设计索引：`docs/UserStory/Demand.md`、`docs/UserStory/Design.md`
  - 迭代功能设计：`docs/UserStory/Iter<number>-<Name>/PD<4-digit>-<Module>.md`
  - 迭代计划与实现记录：`docs/Development/Iter<number>-<Name>/PT<4-digit>-<Module>.md`
  - 总进度：`docs/Development/ProjectTracking.md`
- 脚本：`scripts`
- 测试：`tests`
- Razor 模板：`templates`

## DTO 目录与命名（强制）

- 模块中所有 DTO 放在该模块 `Models/{Entity}Dtos` 目录，例如 `Models/ResourceDtos`。
- 一个文件只定义一个类型；不将 DTO 集中到 `Contracts.cs`、`Dtos.cs` 等多类型文件。
- 数据转换边界类型以 `Dto` 结尾，不使用 `Input`、`Request`、`Response` 作为 DTO 类型名的一部分。
- 新增、修改、筛选、详情和列表优先命名为 `{Entity}AddDto`、`{Entity}UpdateDto`、`{Entity}FilterDto`、`{Entity}DetailDto`、`{Entity}ItemDto`。
- DTO 的嵌套属性也引用符合规则的 DTO，不使用 `Input` 或实体代替。
- 新增或修改模块时同时检查目录、文件粒度、类型命名和所有引用；生成代码后仍要整理并构建验证。

## 工具与 skill 路由

- 脚手架、模块/服务、代码生成、OpenAPI 客户端和模板约定：使用 `perigon`。
- AppHost、分布式资源、启停、状态、日志、trace 或部署：使用 `aspire` 及它路由的子工作流。普通构建/测试使用 `dotnet build`/`dotnet test`。
- 需求、规格、设计、迭代 PD/PT、实现记录或变更说明：使用模板项目内的 `docs`。
- 任何 AI coding 后的进度/文档同步，以及计划执行、完成审计和收敛：使用 `delivery-loop`。
- TUnit、Microsoft.Testing.Platform 和 Aspire API 集成测试：使用 `test`。
- 代码/差异审查和质量门：使用 `code-review`。
- 页面布局、交互、状态、响应式或可访问性：使用 `ux`，并按平台读取 reference。Angular 实现同时遵循 `perigon` 的 Angular reference。
- .NET/NuGet API 形状和版本差异：使用 `dotnet-inspect`。
- 提交信息：使用 `commit-message`。

规划、实现和审查 Perigon 业务代码前读取 `.agents/skills/perigon/SKILL.md`。只有当任务涉及 Aspire 边界时才同时读取 `.agents/skills/aspire/SKILL.md`，避免为普通文档或单元测试启动分布式环境。

## AI coding 交付流程

1. 只读探索现状，确认范围和关键待确认项。
2. 确定迭代和功能模块，编写或更新对应 `PDnnnn-Name.md` 与 `PTnnnn-Name.md`；简单修复可用简短 PT，但不跳过跟踪。
3. 实现前检查需求覆盖、产物矛盾、无来源任务、依赖和验证空缺；阻断问题未解决时不开始。
4. 按依赖执行最小 ready task，每项实现后立即验证。只在完成条件满足且验证通过后勾选任务。
5. 实现后对照意图检查 Completeness、Correctness、Coherence。缺口以新任务追加，不改写已完成记录。
6. 每次代码修改后立即更新 PT 的 checkbox、进度、实现结果和验证证据，同步 `ProjectTracking.md` 及受影响 PD，直到完成或明确阻塞。

## 完成定义

只有同时满足以下条件才声明完成：

- 纳入范围的必须行为均已实现，无未说明范围漂移；
- 必需 build、test、契约、迁移、前端或运行验证已通过，精确命令可报告；
- 验收场景、失败路径、授权/租户和回归风险按影响面覆盖；
- 生成结果、DTO 边界、公开契约、文档和跟踪状态已同步；
- 无未解决的阻断问题，剩余风险和未运行验证已明确报告。

## 工程决策

1. 优先主流、通用且活跃维护的官方或开源方案。
2. 复用现有 `src/Perigon`、`Definition/Share`、`ServiceDefaults` 和模块能力，避免过度抽象或为设计模式而设计。
3. 以类型系统、清晰分层和单一职责表达业务意图。
