# PD0002 — MailKit 5 Compatibility

## 来源与范围

- Iteration: `Iter0-Initial`
- Module: `MailKit5Compatibility`
- Status: `done`
- Scope: `Perigon.AspNetCore.Toolkit` SMTP email sending compatibility with the centralized MailKit 5 package.

## Requirements

- REQ-001: The API Standard solution MUST compile with the MailKit version selected in `Directory.Packages.props`, while preserving the existing SMTP message content and delivery flow.

## Acceptance scenarios

- SC-001: With MailKit 5 selected, `dotnet build MyProjectName.slnx` completes successfully without a compile error in `SmtpService`.

## Design

- Use the `MailKit` namespace extension overload for sending a `MimeMessage`, as required by MailKit 5.
- Keep the existing SMTP connection, authentication, message construction, and disconnection behavior unchanged.
