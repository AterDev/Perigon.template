---
name: aspire
description: "Aspire 分布式应用编排与调试。Use when: aspire start, AppHost, resource status, logs, traces, dashboard, integrations, Aspire MCP, distributed app debugging. Do not use for ordinary dotnet build/test."
---

# Aspire Skill

This repository uses Aspire to orchestrate its distributed application. Resources are defined in `src/AppHost/AppHost.cs`.

## CLI command reference

| Task | Command |
|---|---|
| Start the app | `aspire start` |
| Start isolated (worktrees) | `aspire start --isolated` |
| Restart the app | `aspire start` (stops previous automatically) |
| Wait for resource healthy | `aspire wait <resource>` |
| Stop the app | `aspire stop` |
| List resources | `aspire describe` or `aspire resources` |
| Run resource command | `aspire resource <resource> <command>` |
| Start/stop/restart resource | `aspire resource <resource> start|stop|restart` |
| View console logs | `aspire logs [resource]` |
| View structured logs | `aspire otel logs [resource]` |
| View traces | `aspire otel traces [resource]` |
| Logs for a trace | `aspire otel logs --trace-id <id>` |
| Add an integration | `aspire add` |
| List running AppHosts | `aspire ps` |
| Update AppHost packages | `aspire update` |
| Search docs | `aspire docs search <query>` |
| Get doc page | `aspire docs get <slug>` |
| List doc pages | `aspire docs list` |
| Environment diagnostics | `aspire doctor` |
| List resource MCP tools | `aspire mcp tools` |
| Call resource MCP tool | `aspire mcp call <resource> <tool> --input <json>` |

Most commands support `--format Json` for machine-readable output. Use `--apphost <path>` to target a specific AppHost.

## Key workflows

### Running in agent environments

Use `aspire start` to run the AppHost when runtime verification, logs, traces, resource status, or integration debugging is required. When working in a git worktree, use `--isolated` to avoid port conflicts and to prevent sharing user secrets or other local state with other running instances:

```pwsh
aspire start --isolated
```

Use `aspire wait <resource>` to block until a resource is healthy before interacting with it:

```pwsh
aspire start --isolated
aspire wait myapi
```

Relaunching is safe — `aspire start` automatically stops any previous instance. Re-run `aspire start` whenever changes are made to the AppHost project.

### Debugging issues

When debugging runtime issues, inspect the app state before changing code:

1. `aspire describe` — check resource status
2. `aspire otel logs <resource>` — view structured logs
3. `aspire logs <resource>` — view console output
4. `aspire otel traces <resource>` — view distributed traces

### Adding integrations

Use `aspire docs search` to find integration documentation, then `aspire docs get` to read the full guide. Use `aspire add` to add the integration package to the AppHost.

After adding an integration, restart the app with `aspire start` for the new resource to take effect.

### Using resource MCP tools

Some resources expose MCP tools (e.g. `WithPostgresMcp()` adds SQL query tools). Discover and call them via CLI:

```pwsh
aspire mcp tools                                              # list available tools
aspire mcp tools --format Json                                # includes input schemas
aspire mcp call <resource> <tool> --input '{"key":"value"}'   # invoke a tool
```

## Important rules

- Start Aspire only for runtime debugging, integration verification, resource status, logs, traces, or dashboard work. For ordinary static changes, prefer `dotnet build` / `dotnet test`.
- **To restart, just run `aspire start` again** — it automatically stops the previous instance. NEVER use `aspire stop` then `aspire run`. NEVER use `aspire run` at all.
- Use `--isolated` when working in a worktree.
- **Avoid persistent containers** early in development to prevent state management issues.
- **Never install the Aspire workload** — it is obsolete.
- Prefer `aspire.dev` and `learn.microsoft.com/dotnet/aspire` for official documentation.

## Playwright CLI

If configured, use Playwright CLI for functional testing of resources. Get endpoints via `aspire describe`. Run `playwright-cli --help` for available commands.