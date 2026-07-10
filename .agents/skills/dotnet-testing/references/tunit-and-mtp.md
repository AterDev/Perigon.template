# TUnit and Microsoft.Testing.Platform

`ApiStandard/global.json` chooses Microsoft.Testing.Platform (MTP). On .NET 10+, use the MTP form:

```powershell
dotnet test --project tests/ApiTest/ApiTest.csproj
dotnet test --project tests/ApiTest/ApiTest.csproj --treenode-filter '/*/*/*/*[Category=Integration]'
dotnet test --project tests/ApiTest/ApiTest.csproj --report-trx --report-trx-filename ApiTest.trx --results-directory ./TestResults
```

TUnit is not VSTest. Do not pass `--filter "Category=Integration"`; use `--treenode-filter`. The shape is `/Assembly/Namespace/Class/Method`, with `*` wildcards. Examples:

```text
/*/*/SystemUserTests/*                           # class
/*/*/SystemUserTests/GetUserInfo_ShouldReturnUserDetails  # method
/*/*/*/*[Category=Unit]                          # category
/*/*/*/*[Category!=Integration]                  # exclusion
```

On older SDKs, use `dotnet test -- --treenode-filter '...'`. TUnit writes an HTML report by default under `TestResults/{AssemblyName}-{os}-{tfm}-report.html`. Generate TRX only when the compatible `Microsoft.Testing.Extensions.TrxReport` extension is referenced.

Sources, consulted 2026-07-10:

- [Aspire testing overview](https://learn.microsoft.com/en-us/dotnet/aspire/testing/overview)
- [Manage the AppHost in tests](https://learn.microsoft.com/en-us/dotnet/aspire/testing/manage-app-host)
- [dotnet/aspire](https://github.com/dotnet/aspire)
- [TUnit test filters](https://tunit.dev/docs/execution/test-filters/)
- [Microsoft.Testing.Platform test reports](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-test-reports)
