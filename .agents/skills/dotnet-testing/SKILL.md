---
name: dotnet-testing
description: "Write, organize, run, and diagnose .NET unit tests and Aspire distributed integration tests. Use when adding or changing tests in a .NET solution; selecting unit versus API, integration, or end-to-end coverage; working with TUnit, xUnit, NUnit, or MSTest; running selected test categories; or collecting and interpreting test results, including TRX reports."
---

# .NET testing

Use the smallest test boundary that proves the behavior. Inspect the solution's test project, packages, `global.json`, and existing test conventions before adding code. Do not change production behavior only to make it testable; introduce a narrow dependency boundary instead.

## Choose the test boundary

| Need | Test type | Dependencies |
| --- | --- | --- |
| Pure business rule, mapping, validation, error branch | Unit | Real subject, fakes/stubs for collaborators; no network, disk, clock, database, or container |
| One ASP.NET Core service's HTTP pipeline | Service integration | `WebApplicationFactory<TEntryPoint>` or the project's in-memory host |
| AppHost, multiple services, database/cache/message broker wiring | Aspire integration | `Aspire.Hosting.Testing` and the real AppHost |

Keep the unit-test majority fast and deterministic. Use Aspire only to validate cross-process contracts, resource references, migrations, authentication, and real infrastructure behavior. Aspire tests are black-box: communicate through HTTP, messages, or connections—not the internal DI container of a started service.

## Write unit tests

1. Name the test after the observable behavior: `Method_WhenCondition_ReturnsOutcome`.
2. Use Arrange–Act–Assert and make one behavior the main assertion. Prefer the real domain object; fake only the direct collaborators.
3. Test public behavior and meaningful error paths, boundaries, and authorization rules. Avoid asserting implementation details, log messages, private members, or incidental call order.
4. Pass time, randomness, IDs, and I/O behind dependencies so the test controls them.
5. Mark test intent consistently. With TUnit, use `[Category("Unit")]`; use the framework's equivalent trait/category in other projects.

For the repository's TUnit style:

```csharp
using TUnit.Assertions;

public sealed class PasswordPolicyTests
{
    [Test]
    [Category("Unit")]
    public async Task Validate_WhenPasswordIsTooShort_ReturnsError()
    {
        var policy = new PasswordPolicy(minimumLength: 12);

        var result = policy.Validate("short");

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Error).IsEqualTo("Password is too short.");
    }
}
```

## Write Aspire integration tests

Read [ApiStandard conventions](references/apistandard.md) before editing this template's `ApiTest` project. Keep the existing session fixture unless isolation requires a fresh AppHost.

1. Reference the AppHost and `Aspire.Hosting.Testing` from the test project.
2. Set an explicit testing environment before creating the builder. Make the AppHost choose isolated resource names and avoid developer data. Never use a developer or production database in a test.
3. Create the host with `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()`, configure logging/resilience if needed, then `BuildAsync()` and `StartAsync()`.
4. Wait for the target resource to reach `KnownResourceStates.Running` before creating its client. Use `app.CreateHttpClient("ResourceName")`; resource names must match the AppHost.
5. Send realistic requests, assert status plus externally visible response or persisted outcome, and use unique data per test. Do not rely on test execution order.
6. Stop and dispose the app in teardown. Delete only the explicitly named test resources after all connections are closed. Never interpolate untrusted data into cleanup SQL.

Typical TUnit test shape:

```csharp
[Test]
[Category("Integration")]
public async Task GetUserInfo_WhenAuthenticated_ReturnsCurrentUser(TestHttpClientData data)
{
    using var response = await data.HttpClient.GetAsync("/api/systemUser/userinfo");

    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    var user = await response.Content.ReadFromJsonAsync<UserInfoDto>();
    await Assert.That(user).IsNotNull();
}
```

Do not parallelize tests sharing a database, user, cache key, queue, or AppHost unless every shared resource is isolated. Set explicit resource-ready timeouts and retain useful host logs for failures.

## Run tests and capture results

First identify the runner. In a solution with `global.json` configured for `Microsoft.Testing.Platform`—as `ApiStandard` is—use MTP syntax. For a legacy VSTest project, use its `--filter` and `--logger` syntax instead.

```powershell
# Restore/build then run the template's integration tests.
dotnet test --project tests/ApiTest/ApiTest.csproj

# Run TUnit test categories (SDK 10+); do not use VSTest --filter.
dotnet test --project tests/ApiTest/ApiTest.csproj --treenode-filter '/*/*/*/*[Category=Unit]'
dotnet test --project tests/ApiTest/ApiTest.csproj --treenode-filter '/*/*/*/*[Category=Integration]'

# Run one TUnit class or test.
dotnet test --project tests/ApiTest/ApiTest.csproj --treenode-filter '/*/*/SystemUserTests/*'
dotnet test --project tests/ApiTest/ApiTest.csproj --treenode-filter '/*/*/SystemUserTests/GetUserInfo_ShouldReturnUserDetails'

# Persist machine-readable results. Requires Microsoft.Testing.Extensions.TrxReport.
dotnet test --project tests/ApiTest/ApiTest.csproj --report-trx --report-trx-filename ApiTest.trx --results-directory ./TestResults
```

For TUnit, inspect its automatically generated `TestResults/*-report.html` for totals, durations, output, and stack traces. For CI or tool consumption, inspect the generated `.trx`; report passed/failed/skipped counts, failed test names, first meaningful exception, and the artifact path. Preserve the command and exit code. A nonzero exit code is a failed run even when a report was written.

If `--report-trx` is not recognized, add a centrally managed `Microsoft.Testing.Extensions.TrxReport` package reference/version compatible with the repository, restore, and rerun. If a TUnit filter selects zero tests, verify `[Category]` spelling and use `--treenode-filter`, not `--filter`.

## Diagnose failures

Classify before fixing:

- **Compile/discovery:** run `dotnet restore` and `dotnet test --list-tests` (or MTP help); check target frameworks and test SDK/runner settings.
- **Unit failure:** reduce to the named test and inspect inputs and assertion; fix production code only when the expected behavior is correct.
- **Aspire startup/readiness:** verify Docker/DCP prerequisites, AppHost resource names, generated resource logs, and waits. Do not replace readiness waits with arbitrary delays.
- **State leakage/flakiness:** isolate names/data, clear only test-owned resources, or disable parallelism for that fixture.
- **Report missing:** distinguish runner output from report extensions; check `TestResults` and the command-line options supported by the active runner.

## References

Read [runner and reporting guidance](references/tunit-and-mtp.md) when choosing filters or reports. Consult official Aspire docs and the `dotnet/aspire` GitHub repository for API changes rather than relying on stale examples.
