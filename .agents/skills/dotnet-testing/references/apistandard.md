# ApiStandard test conventions

Inspect the current source before applying these notes; they describe the template at the time this skill was added.

- `tests/ApiTest/ApiTest.csproj` targets `net10.0`, uses TUnit and `Aspire.Hosting.Testing`, and references `src/AppHost/AppHost.csproj` plus `AdminService`.
- `global.json` selects `Microsoft.Testing.Platform`; pass MTP options directly to `dotnet test` with the .NET 10 SDK.
- `GlobalHooks.SetUp` sets `ASPIRE_ENVIRONMENT=Testing`, creates `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()`, configures logging/resilience, builds, and starts one `DistributedApplication` for the TUnit test session.
- `TestHttpClientData` is a shared `ClassDataSource` fixture. It creates a client for the AppHost resource named `AdminService`, waits for it to be `Running`, then authenticates once and adds the bearer token.
- `AppHost.cs` turns `ASPIRE_ENVIRONMENT=Testing` into the test database name `MyProjectName_test`. Its teardown drops that exact PostgreSQL database before stopping and disposing the host.

Follow the same lifecycle for tests that need the distributed application. If adding another service client, use its exact AppHost resource name and wait for that resource. If the test must mutate state, create test-specific users/data and clean them up, or isolate the database per test suite. Do not use fixed developer credentials or data outside an explicitly provisioned test environment.

The current `SystemUserTests` contains a commented HTTP/assertion example. Replace it with real assertions only after verifying endpoint behavior and test seed data; do not treat a commented test body as coverage.
