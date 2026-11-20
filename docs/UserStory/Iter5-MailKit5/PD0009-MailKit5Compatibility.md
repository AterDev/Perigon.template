# PD0009：两套模板适配 MailKit 5

- Iteration：`Iter5-MailKit5`
- Module：`MailKit5Compatibility`
- Status：`done`
- Scope：ApiStandard 与 MiniApi 的 SMTP 邮件发送服务，以及集中管理的 MailKit 版本。

## Requirements

- REQ-001：两套模板升级到 MailKit 5 后均可构建，且保留现有 SMTP 消息内容、连接和发送流程。

## Acceptance scenarios

- SC-001：使用 MailKit 5 构建 ApiStandard 与 MiniApi 解决方案成功。

## Design

- ApiStandard 显式导入 `MailKit` 命名空间，以使用 MailKit 5 的 `MimeMessage` 发送扩展。
- MiniApi 使用 MailKit 5 的 `SendRequest` 发送消息。
- 保持既有 SMTP 连接、认证、消息构造和断开连接行为。
