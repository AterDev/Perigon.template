# ApiStandard 数据访问改进说明

- 日期：2026-09-22
- 范围：ApiStandard 多租户、EF 数据操作、通用扩展方法；共有工具修复同步 MiniApi。
- 状态：首批 DA-02、DA-07、DA-10 已完成；整体改进尚未完成。
- 实施记录：[PT0003](../../Development/Iter1-DataAccess/PT0003-DataAccess.md)

## 现状与结论

ApiStandard 使用 .NET 10、EF Core 10，按服务入口、Share/Manager、DbContext/Factory、Entity 分层。支持 PostgreSQL、SQL Server，以及共享数据库和租户独立连接串。已有租户与软删除命名查询过滤器、写入检查、租户索引约定及 20 项单元测试。

现有分层可以保留。优先补齐写入隔离与生命周期，不急于增加通用 Repository。查询过滤器不能替代所有写入路径的安全检查，ExecuteUpdate 和批量操作不会执行 ContextBase 的 SaveChanges 检查。

## 问题清单与实施顺序

以下路径均相对于仓库根目录。P1 表示优先修复的正确性或隔离问题；P2 表示后续完善。是否形成 HTTP 可利用路径取决于业务模块公开的 DTO 和更新入口。

| ID | 优先级 | 问题、证据和影响 | 建议 | 批次 |
|---|---|---|---|---|
| DA-01 | P1 | `ApiStandard/src/Definition/EntityFramework/AppDbContext/ContextBase.cs` 的 ApplyTenantOwnership 只检查内存实体 TenantId。SQLite 已复现：租户 B Attach 带租户 A 主键的 stub，伪填 TenantId=B 后更新成功；SQL WHERE 只有 Id。 | 将租户归属纳入实际更新/删除谓词，评估 TenantId 并发令牌及原始值保护；限制未加载实体的 Attach/Update。 | 后续专项 |
| DA-02 | P1 | `ApiStandard/src/Definition/EntityFramework/Extensions.cs` 按 DTO 同名属性生成 SET。SQLite 已复现：WHERE 限定租户 A，仍可 SET TenantId=B。 | 普通局部更新跳过 Id、TenantId、CreatedTime；后续再设计完整字段白名单和显式租户迁移入口。 | 首批 |
| DA-03 | P1 | `ApiStandard/src/Definition/Share/Services/TenantService.cs` 使用一天的本地缓存。多实例间没有自动失效，租户停用、删除、连接串修改可能延迟生效。 | 明确停用生效时限，设计提交后的跨节点失效或版本校验；保留后台任务对停用租户的既有约定。 | 后续专项 |
| DA-04 | P2 | `ApiStandard/src/Definition/Share/Implement/ManagerBase.cs` 手工创建 Context，却未定义释放责任。 | 评估 scoped 租户 Context 或由持有者释放，检查派生 Manager 与返回 IQueryable 的生命周期。 | 后续专项 |
| DA-05 | P2 | 每个 Manager 各自创建 Context，ExecuteInTransactionAsync 只覆盖自己的 Context。 | 定义跨 Manager 工作单元和事务边界；同一租户业务事务共享 Context。 | 后续专项 |
| DA-06 | P2 | ManagerBase 分页用 IOrderedQueryable 判断已有排序并修改共享 Queryable；异常时不恢复，CreatedTime 排序也缺少唯一兜底。 | 明确已有排序的识别/传入方式，追加稳定键，使用局部查询；避免覆盖业务自定义排序。 | 后续 |
| DA-07 | P1 | ManagerBase.BulkInsertAsync 先枚举赋值，再交给 BulkExtensions 二次枚举；延迟序列可能产生不同对象，丢失 TenantId/UpdatedTime 赋值。 | 输入只物化一次，字段赋值和数据库插入使用同一列表。 | 首批 |
| DA-08 | P2 | EntityBase 无版本冲突检测；ExecuteUpdate/ExecuteDelete 不自动应用乐观并发；软删除不更新 UpdatedTime。 | 结合业务选择版本令牌，显式添加版本谓词和影响行数检查，统一审计字段。 | 后续专项 |
| DA-09 | P2 | PartialUpdateAsync 跳过 null，不能区分未提供与清空；按 CLR 属性匹配可能接受未映射属性。 | 设计可表达字段存在性的 PATCH 契约，依据 EF 元数据验证属性；不在首批改变现有 null 语义。 | 后续专项 |
| DA-10 | P2 | 两套模板 Utils/Extensions.cs 的 Between 重新把叶子属性挂到根参数上，嵌套属性如 x.Detail.Score 会构造失败。 | 保留调用者原始 MemberExpression，维持访问链。 | 首批 |
| DA-11 | P2 | Between 的 long/double 上下界是 int；double 还有未使用参数；DateOnly 结束日转换为零点且依赖机器时区。 | 保持旧重载兼容并评估新重载歧义；另行定义业务时区与半开日期区间。 | 后续 |
| DA-12 | P2 | 自定义 Select<TResult> 与 Mapster 投影重叠，依赖同名同类型属性及无参构造。 | 统一投影策略，再考虑表达式缓存；不直接移除已有公开方法。 | 后续 |

## 首批选择依据与兼容边界

首批只修改 DA-02、DA-07、DA-10，不改变方法签名、依赖版本、数据库结构、注册方式或普通业务调用方式。

- DA-02：DTO 可以继续携带系统字段，但这些字段被忽略。业务字段仍正常更新；null 仍表示跳过；IsDeleted 和 UpdatedTime 暂保留既有语义。依赖普通 PATCH 修改主键、创建时间或迁移租户的旧调用会改变行为，必须使用专门操作，不能视为兼容用法。
- DA-07：保持同一批实体、顺序、取消令牌和 BulkExtensions 调用，只把延迟输入物化一次。内存占用增加一个引用列表，与批量处理规模相关。
- DA-10：直接属性查询行为不变，嵌套属性使用原访问链；不改变范围端点、空值或时区语义。MiniApi 拥有相同实现，同步修复。

DA-01 虽为高优先级，但涉及跟踪更新、删除、并发异常和潜在模型迁移，不能用查询过滤器修改草率替代。首批完成不代表整体租户写入隔离已完善。

## 验收与验证边界

1. SQLite 局部更新同时提交业务字段、Id、TenantId、CreatedTime 时，只更新业务字段及既有自动更新时间；另一租户记录不受影响。
2. 直接属性与嵌套属性的 Between 保持包含上下界的查询结果。
3. BulkInsertAsync 物化后的同一列表用于字段赋值和提交，审核不存在原序列二次枚举。
4. 运行 ApiStandard 单元测试和两套模板构建；本地打包并检查修复进入模板、根目录维护文档未进入包。
5. 本批不启动真实数据库或 Aspire 应用，不宣称 PostgreSQL/SQL Server BulkInsert 集成验证、跨节点缓存验证或完整租户安全验收通过。

后续应补齐：Attach stub 跨租户修改/删除、关联实体跨租户外键、分页稳定性、事务回滚、并发覆盖及两种正式数据库的回归测试。

## 首批结果

- ApiStandard 单元测试 22/22 通过，新增 SQLite 受保护字段/跨租户目标更新用例，以及直接和嵌套 Between 的边界用例。
- 两套 solution 构建通过，0 警告、0 错误；首次 `--no-restore` 使用旧依赖产物失败，正常还原依赖后通过，无需修改依赖版本。
- 本地模板包构建及内容检查通过；三项修复进入对应模板，仓库根目录 docs 未进入包。
- Perigon.docs 的数据访问与 DTO 生成页面已同步中英文，文档校验及站点构建通过；包版本和文档元数据未变。
- 批量插入的一次物化通过实现审查验证，未执行 PostgreSQL/SQL Server BulkExtensions 实库测试。
