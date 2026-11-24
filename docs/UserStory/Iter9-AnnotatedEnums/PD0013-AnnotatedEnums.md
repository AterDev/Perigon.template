# PD0013 MiniApi OpenAPI 枚举

MiniApi 使用 OpenAPI 3.1 描述非 Flags 整数枚举，成员通过 `oneOf`、数字 `const`、`title` 和 `DescriptionAttribute` 生成 `description`。具有相同值的别名使用 `anyOf`，避免 `oneOf` 多次匹配；名称或描述缺失时使用成员名。文档中的枚举约束只允许声明的值，虽然 .NET 默认数字枚举反序列化可能接受其他值。

删除 MiniApi 本地 OpenAPI 源生成器和 `x-enumData` 扩展。`EnumSchemaTransformer` 查询编译时生成的嵌入 XML，新增枚举无需手动注册。字符串枚举及 Flags 保持框架默认 schema。消费端需支持 OpenAPI 3.1 annotated enumerations；工具是否把它呈现或生成成枚举取决于实现。

共享生成器包中的 MSBuild 任务面向引用该包的 SDK Web 项目，在编译前扫描当前项目及传递项目引用，生成 `enum-meta.generated.xml`，嵌入程序集并复制至构建和发布目录根部；无需特定项目名或 transformer，不扫描第三方包。MiniApi 的 transformer 独立消费这些数据。1.1.2 因漏打包任务 DLL 不可用；模板源码使用修复后的 1.1.3，公开 NuGet 源与全新缓存的新项目构建和发布验证通过。

当前 Web 项目使用独立 MSBuild 任务解析编译输入与项目引用程序集；其他源生成器仅在本次 C# 编译时产生的枚举不在该任务的输入中。
