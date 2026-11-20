# PD0007：MiniApi .NET 11 升级

- MiniApi 项目目标为 `net11.0`，要求 .NET 11 SDK；独立源生成器继续面向 `netstandard2.0`，模板消费已发布的生成器包。
- 更新 NuGet、工具和前端依赖；MailKit 5 使用 `SendRequest`。AppHost 与 Aspire 包保持一致，容器运行依赖改用 .NET 11。
- ApiStandard 的框架和依赖不在本次范围内，CI 同时安装两套模板需要的 SDK。
