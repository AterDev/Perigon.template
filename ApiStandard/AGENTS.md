# AGENTS

## 总体指导原则

- 从不猜测，确定性与准确是第一原则，保持高效与严谨。
- 代码和文档的可读性和可维护性优先，通过适当注释、清晰命名和局部变量降低复杂度。
- 没有明确要求时，不要在项目内生成内容总结、更新记录或测试报告类文档。
- 优先使用操作系统、IDE 和项目内已有工具与能力，例如 `pwsh`、`dotnet`、`pnpm`、代码搜索和重构工具。

## 关键技术栈

1. 基于最新的 C# 14 语言特性。
2. 后端强依赖于 Aspire 13+、ASP.NET Core 10、EF Core 10。
3. 前端默认使用 Angular，具体以仓库现有代码为准。

## 项目结构

- 前端: `src/ClientApp/WebApp`
  - 基础共享依赖: `src/ClientApp/WebApp/src/app/modules/share`
  - 业务前端模块: `src/ClientApp/WebApp/src/app/modules/{module}`
- 后端接口服务: `src/Services`
- 实体定义: `src/Definition/Entity`
- 业务逻辑: `src/Modules`，按模块划分
- Share 共享项目: `src/Definition/Share`
- 服务扩展: `src/Definition/ServiceDefaults`
- 文档位于 `docs/`
- 脚本： `scripts/`
- 测试： `tests/`
- Razor 模板: `templates/`

## DTO 目录与命名规范（强制）

- 模块中的所有 DTO 必须放在该模块 `Models` 目录下按实体划分的 `{Entity}Dtos` 目录中，例如 `Models/ResourceDtos`、`Models/ResDefinitionDtos`。
- 一个文件只能定义一个类型；DTO 不得集中写在 `Contracts.cs`、`Dtos.cs` 等多类型文件中。
- 作为数据转换边界的类型必须以 `Dto` 结尾，禁止使用 `Input`、`Request`、`Response` 作为 DTO 类型名的一部分。新增、修改、筛选、详情和列表数据分别优先使用 `{Entity}AddDto`、`{Entity}UpdateDto`、`{Entity}FilterDto`、`{Entity}DetailDto`、`{Entity}ItemDto` 等命名。
- DTO 的嵌套属性也必须引用符合上述规则的 DTO；不要通过 `Input` 或实体类型充当 DTO 的替代品。
- 新增或修改模块时，必须同时检查目录、文件粒度、类型命名和所有引用；生成代码后也必须按此规则整理并完成构建验证。

## 工具优先级

- 涉及项目脚手架、模块或服务添加、代码生成、OpenAPI 客户端生成、MCP 配置时，优先使用 `Perigon` 相关能力。
- 涉及分布式应用启动、资源状态检查、日志链路排查、集成配置时，优先使用 `Aspire` 相关能力；普通构建和测试优先使用 `dotnet build` 或 `dotnet test`。
- 需要前端功能验证时，优先结合 Playwright 或前端构建校验。
- 新增或修改前端模块时，复用 `src/app/modules/share` 中的基础组件、守卫、管道、i18n 与 Material 聚合导入；不要重新创建顶层 `src/app/share`。
- 涉及页面布局、控件选择、交互流程、状态反馈、响应式设计或可访问性时，必须读取 `.agents/skills/ux/SKILL.md`，并按目标平台加载 `common.md` 与 Web、Desktop 或 Phone reference；Angular 实现同时遵循 Perigon 的 Angular reference。

### 必读 skill

任何实现、规划、审查任务开始前，必须先读取以下两个skill：

- `.agents/skills/perigon/SKILL.md`
- `.agents/skills/aspire/SKILL.md`

### 主要 skill

- `perigon`：Perigon CLI、MCP、脚手架、代码生成与模板约定
- `aspire`：Aspire AppHost、资源编排、运行、观察与相关子工作流
- `dotnet-guidelines`：.NET 开发规范
- `angular-guidelines`：Angular 开发规范
- `ux`：Web（Angular Material + Bootstrap）、Desktop（WPF/Avalonia）和 Phone（Android Material）的 UI 设计、控件与交互规范
- `commit-message`：提交信息生成规范
- `development-plan`：开发计划制定规范
- `playwright-cli`：前端自动化验证与页面交互
- `docs`：开发文档编写规范
- `code-review`：代码审查规范

## 思维模型

作为以目标为导向的架构师编写代码：

1. 命名清晰、简洁、易理解，充分利用语言的表达能力和类型系统，写出结构清晰、可维护的代码。
2. 代码结构清晰，模块划分合理，以面向对象思维定义和组织类和方法。
3. 方法和类符合单一职责原则，并补充必要说明。
