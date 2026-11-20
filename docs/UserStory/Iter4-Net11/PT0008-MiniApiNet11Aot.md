# PT0008：MiniApi .NET 11 AOT 与运行行为完善

- [x] 安装并验证本机 .NET SDK 11.0.100 与 .NET 11 运行时。
- [x] MiniApi 更新目标框架、SDK、框架包、容器基础镜像与 CI；ApiStandard 保持 .NET 10。
- [x] MiniApi Debug 解决方案构建通过，集成测试 15 项通过。
- [x] 打包并从包创建无前端 MiniApi 项目，生成项目构建通过；检查包含两套模板且未含仓库根目录文档和生成器源码。
- [x] Windows x64 Native AOT 发布通过；发布存在 Trim/AOT 警告，不能据此认定所有功能兼容。
- [x] Windows AOT 程序启动，健康检查、OpenAPI、Swagger 返回 200，空登录请求返回 400；未连接 PostgreSQL，未验证依赖数据库的业务。
- [x] 在 Docker Desktop WSL 2 后端使用 .NET 11 SDK Linux AOT 镜像完成 linux-x64 Native AOT 发布，并以模板 Dockerfile 构建、运行服务容器。连接独立 PostgreSQL 后初始化演示用户；健康检查、OpenAPI、Swagger 和登录成功，错误请求分别返回 400、403、404。发布存在 Trim/AOT 警告，其他功能仍需按需验证。

- [x] MiniApi 的 net11.0 项目默认开启 Runtime Async；检查 ApiService 的 Features 和 UseRuntimeAsync=false 退出路径，netstandard2.0 源生成器不受影响。Debug 构建及 15 项集成测试通过。
- [x] 用 wslc 3.0.5 直接执行 Linux x64 Native AOT 发布及 Dockerfile 构建，运行 wslc 服务和 PostgreSQL：数据库初始化、登录、健康检查、OpenAPI、Swagger、400/403/404 均通过；发布仍有 Trim/AOT 警告。wslc 仓库访问受限，SDK、基础镜像及 PostgreSQL 从本地镜像存档导入。
- [x] 模板包包含 Directory.Build.targets；从包生成的 MiniApi Debug 解决方案构建通过。
- [x] MiniApi 移除 Mapster 包版本和项目引用、`MapTo` / `Merge` 扩展、运行时全局配置及 AOT 冒烟项目；保留 `Extensions.cs` 中其他查询扩展。DTO/实体转换约定已写入随模板提供的 `perigon` backend skill。依赖树无 Mapster，模板包内容检查通过。
- [x] ApiService 独立 Debug build 及 Windows x64 Native AOT publish 通过；AOT publish 保留既有 Trim/AOT 分析警告。文档 build 检查 199 个 Markdown 文件通过。
- 完整 Debug 解决方案 build 未能写入默认 ApiService 输出：本机运行中的 ApiService 占用 DLL；改用临时输出目录编译 ApiService 成功。未重跑容器集成测试。
- [x] MiniApi 的 `Perigon.PostgreSQL` 版本升至 0.3.1；使用全新临时缓存、仅 NuGet.org 源恢复 ApiService 依赖并 Debug build 通过。数据库自动创建运行验证留给后续测试，连接角色需有 `CREATEDB`。
