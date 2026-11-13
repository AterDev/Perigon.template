# PT0009：两套模板适配 MailKit 5

- [x] ApiStandard 的 `SmtpService` 补充 MailKit 命名空间，解决 MailKit 5 发送扩展的编译兼容问题。
- [x] MiniApi 的 `SmtpService` 使用 MailKit 5 `SendRequest`。
- [x] ApiStandard Debug 解决方案构建通过，0 警告、0 错误。
- [x] MiniApi Debug 解决方案构建通过；有 1 条既有 ASP0038 警告（CommonMod 的 `[ValidatableType]` 未找到本项目内的 `AddValidation` 调用），0 错误。
- 未运行测试。
