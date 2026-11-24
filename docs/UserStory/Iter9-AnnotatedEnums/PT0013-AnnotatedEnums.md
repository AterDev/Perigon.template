# PT0013 MiniApi OpenAPI 枚举

- [x] 移除本地枚举源生成器及 x-enumData，改用 OpenAPI 3.1 annotated enumerations。
- [x] 显式注册模板现有枚举并保留 Native AOT 元数据，跳过 Flags 与字符串枚举。
- [x] MiniApi Debug 解决方案构建、模板打包与包内容检查通过；win-x64 Native AOT 发布成功，原生 HTTP 输出验证 GenderType 的数字 const、名称及中文描述；发布仍存在其他裁剪/AOT 警告。
- [x] 中英文文档校验和构建通过；隔离 HTTP 验证 OpenAPI 3.1 输出。
- [ ] Aspire 集成测试未完成：本机无 Docker 命令，测试等待外部资源后中止；linux-x64 AOT 未验证。

- [x] 本地新版共享生成器包：编译期自动发现新增项目引用枚举，win-x64 Native AOT HTTP 验证负数、超过 Int32 的数值及中文描述。
- [x] 1.1.3 已通过 Linux 冷构建和验证后上传，模板依赖已切换；1.1.2 因缺任务 DLL 不可用。
- [x] 使用本地 1.1.2 包在生成模板项目中验证 `enum-meta.generated.xml` 的构建和发布根目录输出；嵌入资源与 Native AOT OpenAPI 已通过独立验证。

- [x] 维护测试验证 MSBuild 任务可发现当前项目和引用项目枚举，并处理描述转义、Flags 及别名。

- [x] 临时独立 Web 项目无需 ApiService 名称或 transformer 即生成自身和引用项目枚举元数据；普通类库不生成。

- [x] 本地新版包在 Windows win-x64 Native AOT 下从嵌入 XML 自动生成新增项目枚举的 OpenAPI；运行时移开外部 XML 后仍验证通过。
- [ ] macOS 构建和 Linux Native AOT 未实测；当前 Web 项目内其他源生成器在同次编译产生的枚举不包含在 MSBuild 任务扫描范围。

- [x] XML 任务在 Windows 的 .NET 10 SDK 与 .NET 11 SDK 验证，通过 Web 项目、传递引用、条件编译、嵌套类型、别名、增量构建与发布根目录检查。

- [x] SourceGeneration 1.1.2 本地包测试与新模板隔离生成、构建、发布根目录 XML 检查通过；专用 GitHub Actions 工作流已成功上传 NuGet。
- [x] NuGet 公开源全新缓存验证发现 1.1.2 缺少任务 DLL，构建报 MSB4062。

- [x] 已发布 1.1.2 公开包缺失 MSBuild 任务 DLL；已修复并发布 1.1.3，且从公开 NuGet 全新缓存重新验证。

- [x] 修正 NuGet 打包目标顺序；使用全新目录冷构建的修复预览包，通过隔离 Web/类库测试及新模板构建、发布目录 XML 验证。

- [x] 修复打包顺序；Linux CI 干净检出验证 task DLL 和 SDK 10/11 Web 项目 XML 生成后上传 1.1.3，两套模板已切换。
- [x] NuGet 公开索引可用；仅使用 nuget.org 与全新包缓存生成并构建新项目，验证构建和发布根目录的 `enum-meta.generated.xml`。
