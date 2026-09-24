# PT0004 - API 行为契约

## 文档信息

- Iteration: `Iter2-ApiBehavior`
- ProductDesign: [PD0003](../../UserStory/Iter2-ApiBehavior/PD0003-ApiBehavior.md)
- PlanTask: `PT0004-ApiBehavior`
- Module: `ApiBehavior`
- Status: `complete`
- Progress: `2/2`
- LastUpdated: `2026-09-24`

## 目标与范围

在两套模板中为 `IUserContext` 提供基于 `role_id` Claims 的角色 ID 列表，并在 ApiStandard 中为 MVC 自动模型验证失败建立结构化 HTTP 400 响应。

## 任务

- [x] ApiStandard 和 MiniApi 增加 `IUserContext.RoleIds`，过滤无效/空 GUID 并去重。
- [x] ApiStandard MVC 自动验证错误返回标题、详情、状态码和 TraceId。

## 实现记录

- 两套模板从所有 `role_id` Claims 读取 GUID，忽略无效和空 GUID，并去重。
- ApiStandard `InvalidModelStateResponseFactory` 使用 `CustomBadRequest`；`detail` 汇总各模型状态项的第一条非空错误消息。

## 验证记录

- ApiStandard Release solution build：通过，0 警告、0 错误；单元测试 22/22 通过。
- MiniApi Release solution build：通过，0 警告、0 错误；ApiTest Aspire smoke test 1/1 通过。
- `dotnet pack Pack.csproj -c Release --output ./artifacts --no-restore`：通过；包 ID/版本/ReleaseNotes 正确，含两套模板和两个模板配置文件，不含根目录维护文档。
- Perigon.docs 校验 191 个 Markdown 文件通过，`build.ps1` 完成双语文档站点构建。
- GitHub 发布工作流改为运行实际存在的 `MiniApi/tests/ApiTest/ApiTest.csproj`；原 `tests/UnitTest/UnitTest.csproj` 路径不存在。
- 发布流程：待合并至 `nuget` 并推送后确认 GitHub Actions / NuGet 发布状态。

## 兼容性与迁移

- 新增只读 `IUserContext.RoleIds` 属性；调用方需在令牌中签发 `role_id` GUID Claims 才能读取角色 ID。
- ApiStandard 自动模型验证错误从 ASP.NET Core 默认结构调整为 `{ Title, Detail, Status, TraceId }`；依赖旧响应结构的客户端需要更新解析逻辑。
- MiniApi 的验证错误响应行为未在本任务中更改。
