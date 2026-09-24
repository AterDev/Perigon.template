# PT0003 - 数据访问首批改进

- Iteration: `Iter1-DataAccess`
- ProductDesign: [PD0002](../../UserStory/Iter1-DataAccess/PD0002-ApiStandardDataAccess.md)
- Status: `complete`
- LastUpdated: `2026-09-22`

## 任务

- [x] 整理完整评估、优先级、首批范围和兼容边界。
- [x] DA-02：局部更新忽略 Id、TenantId、CreatedTime。
- [x] DA-07：批量插入输入只物化一次。
- [x] DA-10：两套模板保留 Between 嵌套属性访问链。
- [x] 添加针对性回归测试并完成构建、测试、包内容检查。
- [x] 完成使用文档影响评估与必要同步。

## 验证记录

- `dotnet build ApiStandard/MyProjectName.slnx -v:q`：通过，0 警告、0 错误。
- `dotnet build MiniApi/MyProjectName.slnx -v:q`：通过，0 警告、0 错误。
- 在 ApiStandard 执行 `dotnet test --project tests/UnitTest/UnitTest.csproj -v:q`：22/22 通过；新增两项回归覆盖受保护字段、跨租户更新目标、直接与嵌套范围查询。
- 首次不还原依赖的构建遇到旧产物的源生成器/运行时包问题，正常还原后通过；未改依赖版本。
- `dotnet pack Pack.csproj -c Release -o $env:TEMP/perigon-dataaccess-pack -v:q`：通过；包内源文件包含三项修复，`content/docs/` 不存在。仅本地验证，未发布。
- BulkInsert 输入先 ToList，赋值和提交均使用该列表，通过代码审查；没有运行 PostgreSQL/SQL Server 实库批量测试。
- Perigon.docs `build.ps1`：文档校验和站点构建通过。

## 使用文档决策

- 同步 Perigon.docs 10.0 中英文“数据访问”“DTO 生成”共四页，说明受保护字段和仍待修复的 Attach 写入边界。
- BulkInsert 单次枚举与 Between 嵌套表达式修复不改变调用方式；现有使用文档无需为这两项新增说明。
- MiniApi 的 Between 与 ApiStandard 相同，已同步；MiniApi 不使用本次 EF PartialUpdate 路径，批量插入已有一次物化，不需要同步其余两项。
- 文档仓库 webinfo.json 包版本为 1.3.12，模板仓库为 1.3.15；既有差异不属于本批发布任务，未调整版本元数据。

## 剩余事项

PD0002 的 DA-01、DA-03～06、DA-08～09、DA-11～12 尚未实施。特别是 Attach stub 跨租户写入风险仍然存在。
