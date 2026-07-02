# GitHub Copilot Instructions

本仓库是设计良好的.NET解决方案项目，包括前后端代码，是基于`Perigon.templates`模板的WebApi项目。并使用`Perigon.CLI`工具进行项目脚手架搭建和代码生成。

## 总体指导原则

- 准确和有效性为第一原则，保持高效与严谨。
- 验证是必要流程：代码修改完成后，根据影响范围必须进行编译和测试验证(至少有核心功能的验证)。
- 代码可读性，可维护性是第一位的，这要求添加适当的注释，局部变量，减少复杂的方法嵌套调用，保持代码结构清晰。
- 没有要求的情况下，不要在项目内生成任何总结/更新/测试相关的文档。
- 优先使用系统、IDE、项目内提供的工具和能力，如`pwsh/bash`、`dotnet`、`pnpm`，代码搜索和重构等；

## 关键技术栈
1. 基于最新的C# 14语言特性
2. 后端强依赖于：Aspire 13+,ASP.NET Core 10,NativeAOT,Perigon.PostgreSQL
3. 前端默认使用Angular，具体以仓库中现有代码为准

## 项目结构

- 前端在 `src/ClientApp/WebApp` 
- 后端接口服务在 `src/Services` 
- 实体定义在 `src/Definition/Entity`
- MiniApi 默认不走完整的 `Module` 开发模式，大部分后端代码直接写在具体的 Service 项目中
- `src/Services/ApiService` 内通常按 `Endpoints/`、`Managers/`、`Models/`、`Services/` 分层组织代码
- Share共享项目在 `src/Definition/Share`
- 服务扩展在 `src/Definition/ServiceDefaults`
- 文档在 `docs/`
- 脚本在 `scripts/`
- 测试在 `tests/`
- razor模板在 `templates/`

## MiniApi 开发约定

- 默认在 `src/Services/ApiService` 内完成接口开发，而不是先拆独立模块程序集。
- `Endpoints/` 放 Minimal API endpoint group；类继承 `RestEndpointBase`，通过 `public static void MapEndpoints(IEndpointRouteBuilder endpoints)` 暴露路由。
- `Managers/` 放业务逻辑，负责封装查询、写入和业务规则；Endpoint 只做参数绑定、权限校验和结果转换。
- `Models/` 放 DTO、筛选对象和请求/响应模型；优先使用显式模型，不依赖隐式动态结构。
- `Services/` 放服务内部使用的辅助服务或集成封装；只有在逻辑确实需要复用时再抽取。
- `src/Definition/*` 仍用于实体、共享模型、基础扩展和数据库上下文；不要把 Service 私有逻辑反向塞回 Share。
- `builder.AddModules()` 在模板中保留为兼容扩展点，不代表 MiniApi 默认采用模块化开发。

## CLI和MCP工具

- **Perigon** 和 **Aspire** 是本仓库最重要的工具，凡是任务相关时应优先考虑并主动使用。
- 涉及项目脚手架、模块/服务添加、代码生成、OpenAPI客户端生成、MCP配置时，优先使用 **Perigon** 相关能力。
- 涉及分布式应用启动、资源状态检查、日志/链路排查、集成配置时，优先使用 **Aspire** 相关能力；普通构建/测试优先用 `dotnet build/test`。
- Perigon提供代码生成和示例代码，优先使用它提供的能力，以获取更加准确的参考。
- 充分利用 `Microsoft Learn` 和 `GitHub` 等工具搜索官方文档、示例代码和解决方案；需要前端功能验证时使用 Playwright。

## 清理

对于任务中产生的临时脚本/log/文档等内容，请在任务完成后进行清理，确保仓库整洁。

## 思维模型

作为经验丰富的工程师解决问题：

1. 从相关技术的官方文档中查询解决方案
2. 通过Web搜索，尤其是GitHub中寻找类似问题的解决方案
3. 从不猜测代码，确保每一行代码都有明确的目的

作为以目标为导向的架构师编写代码：

1. 命名清晰，简洁，易理解
2. 代码结构清晰，模块划分合理
3. 方法和类符合单一职责原则，并要添加必要的注释说明