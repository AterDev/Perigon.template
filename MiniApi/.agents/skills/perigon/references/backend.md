# MiniApi 后端开发

## 实体与 DbContext

- 实体通常继承 `EntityBase`，明确字符串长度、decimal 精度、唯一性、软删除和时间语义。
- `Data.DefaultDbContext` 由 DI 使用 `DbContextOptions<DefaultDbContext>` 注册，数据库固定为 PostgreSQL。
- `EntityFramework.AppDbContext.DefaultDbContext` 用于显式连接字符串场景；不要虚构 ApiStandard 的多数据库 factory 或租户上下文。
- 新增实体后在相应 `DefaultDbContext` 添加显式 `DbSet<TEntity>`，保留 `DynamicallyAccessedMembers` 约束并执行 AOT publish 验证。
- MiniApi 不内置 AppHost migration 资源。数据库 schema 的生成、部署顺序、失败恢复和回滚责任必须在计划与部署管线中明确。

## 请求/响应模型

- 按功能放在 `ApiService/Models` 或相邻功能目录，一个公开契约类型一个文件；不要把 EF 实体直接作为 HTTP 契约。
- Add/Update 排除 Id、系统时间、软删除和不可赋值字段；Update 可空性准确表达更新语义。
- 列表/详情避免泄漏导航、无界集合、长文本、二进制或敏感字段。
- Filter 只暴露有明确语义和可控成本的筛选；所有列表必须有边界或分页。
- 新增或改变 JSON 契约时检查 AOT/Trim 与真实 HTTP 序列化。需要 converter、多态或运行时类型时使用可静态生成的 System.Text.Json 元数据策略。

## Manager 与 Service

- 数据访问和业务流程放 Manager；第三方或基础设施依赖放可注入 Service。Endpoint 不直接堆积 DbContext 查询与事务。
- CRUD Manager 按需要继承 `ManagerBase<TDbContext,TEntity>`；无实体时使用非泛型基类。
- 非抽象、非泛型 Manager 由源码生成器注册，不要重复手写 DI 注册。
- Manager 返回领域结果或响应模型，不返回 `IResult`，不依赖 `HttpContext`。
- 优先复用分页、CRUD、批量和事务能力。表达式必须保持数据库可翻译和 AOT 可分析；不要用运行时编译表达式替代查询。
- 工厂或手动创建的上下文及时释放；事务范围小而明确。
- 业务异常使用项目既有 `BusinessException`/全局中间件策略，不吞掉错误。

## Minimal API Endpoint

- Endpoint group 继承 `RestEndpointBase`，实现 `public static void MapEndpoints(IEndpointRouteBuilder endpoints)`；源码生成器会生成 `MapEndpointGroups()` 调用。
- 使用 `MapGroup` 统一路径、tag 和授权策略。路由、HTTP 谓词和 OperationId/metadata 应稳定且唯一。
- Handler 优先具名 `public static` typed method，显式表达路由、查询、header、body 和 DI 绑定；避免反射驱动绑定、运行时扫描或无法由 Request Delegate Generator 分析的动态行为。
- Endpoint 只负责 HTTP 边界、输入验证、授权、调用 Manager/Service、状态码和响应塑形。
- 使用标准状态码和项目现有错误模型。认证、授权、not-found、conflict 和验证失败路径均应有契约与测试。
- 公开契约变化同步 OpenAPI 和客户端，并通过实际 HTTP JSON 路径测试；不能只直接调用 handler。

## 横切能力

- 共享能力放 `Definition/Share` 或 `ServiceDefaults`，服务专属能力留在 ApiService；不要为少量代码增加新层。
- 缓存键包含必要的数据隔离维度，值大小与过期策略按变化频率设置。
- 日志使用 `ILogger` 与 OpenTelemetry，不记录 token、密码或敏感正文。
- JWT/OAuth 生产配置通过 Secret/环境变量注入；生产 OAuth/OIDC 要求 HTTPS metadata。
- 反射、动态代码、程序集扫描、通用 JSON helper 和新第三方包都是 AOT-sensitive；实现和审查时使用 `native-aot`。

## 数据库变更

先修改实体、`DbSet` 和映射，再使用项目选定的迁移方式生成并审查 schema 变化。不要手写已生成迁移，也不要修改已进入生产的历史迁移。部署前明确迁移执行者、备份/恢复点、前后版本兼容窗口和 API 启动顺序。
