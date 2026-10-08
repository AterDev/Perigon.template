# PD0004：ApiStandard 依赖基线更新

## 背景

ApiStandard 依赖需要随 .NET 10 与 Aspire 的兼容版本更新；MiniApi 使用单独的 AppHost 和依赖集，当前不在本次升级范围内。

## 目标

- 将 ApiStandard AppHost 和 Aspire hosting 集成升级到 13.6.1。
- 更新 ApiStandard 的 EF Core、ASP.NET Core 认证、OpenTelemetry、测试和第三方依赖。
- 保持 EF Core Design 可用于迁移工具，同时阻止它作为传递依赖流入生成项目的下游消费方。
- 清楚记录 ApiStandard 与 MiniApi 的 Aspire 版本边界。

## 验收标准

- 两套模板在 .NET 10 下均可恢复和 Release 构建。
- ApiStandard 测试及 MiniApi CI 集成测试通过，模板包只包含预期模板内容。
- ApiStandard 文档要求 Aspire 13.6.1，MiniApi 文档继续要求 Aspire 13.5.0；双语快速入门和版本矩阵一致。

## 范围

- 本需求只刷新 ApiStandard。MiniApi 的包版本与 AppHost SDK 保持不变。
- 本需求不改变模板公开 API、配置键或数据库结构。
