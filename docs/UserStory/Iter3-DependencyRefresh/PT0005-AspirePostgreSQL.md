# PT0005：依赖与上下文调整

- [x] 升级 MiniApi 的 Perigon.PostgreSQL，保留工具已发布版本。
- [x] 删除重复上下文并统一服务注册与文档。
- [x] 对照 0.2.0 包检查模板使用的 API。
- [x] MiniApi 解决方案构建、模板打包及 win-x64 Native AOT 发布通过；AOT 发布存在裁剪和动态代码分析警告。
- [x] 检查包中仅包含 AppDbContext 下的 MiniApi 上下文，根目录维护文档未打包。
- 测试限制：解决方案测试发现零个测试并返回退出码 5；未验证实际 PostgreSQL 运行时操作。
- [x] Aspire 实际启动发现 `DatabaseInitService` 无法解析 `UserManager` 并导致 ApiService 退出；将 Manager 和初始化服务移入 `CommonMod`，由模块和源生成器完成注册。
- [x] 模块边界调整后通过 Aspire 验证 ApiService 与 PostgreSQL 健康；调用 `/api/auth/login` 成功返回数据库中的演示用户。
- 外部 Perigon 文档未改：本修复恢复快速入门中已有的 Aspire 启动步骤，不新增用户配置或操作要求。
- [x] 重新生成并检查 NuGet 1.1.1 的实际输出：`AddManagers()` 注册 `CommonMod.Managers.UserManager`，`AddModules()` 调用 `AddCommonMod()`；已移除手动注册绕过。
- [x] 移除手动注册后重新运行 Aspire：ApiService 健康，`/api/auth/login` 成功返回演示用户；验证后停止 AppHost。
- 空生成文件的原因是误读 2026-09-14 的旧 `obj/generated` 文件；默认 `EmitCompilerGeneratedFiles=false`，构建不会更新磁盘上的旧文件。诊断时需使用 `dotnet build src/Services/ApiService/ApiService.csproj -t:Rebuild -p:EmitCompilerGeneratedFiles=true -p:CompilerGeneratedFilesOutputPath=obj/generated` 输出当前生成代码。本次构建 0 警告、0 错误。
