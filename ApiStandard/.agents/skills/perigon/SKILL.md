---
name: perigon
description: >-
  使用 Perigon CLI/MCP 做项目脚手架、模块/服务生成、DTO/Manager/Controller/Request 客户端生成、MCP 配置与 Studio。Use when: perigon new, perigon add module, perigon add service, perigon generate dto, perigon generate manager, perigon generate controller, perigon generate request, perigon module list, perigon module install, perigon module pack, perigon mcp init, perigon mcp start, perigon studio, perigon studio update.
---

# Perigon Skill

This repository uses `Perigon.CLI` for scaffolding, code generation, and MCP integration.

## When to use this skill

Use this skill when the user asks to:
- create a new solution, add a module or service, or open Studio;
- generate DTOs, managers, controllers, or typed request clients from an entity or OpenAPI document;
- install/list/pack module packages from a local zip file or official package name;
- initialize or start the MCP server for editor/agent integration;
- inspect CLI help before guessing options.

## CLI command reference

| Task | Command |
|---|---|
| Create a new solution | `perigon new <name>` |
| Add a module | `perigon add module <ModuleName>` |
| Add a service | `perigon add service <ServiceName>` |
| Generate DTO from an entity | `perigon generate dto <EntityPath>` |
| Generate manager from an entity | `perigon generate manager <EntityPath>` |
| Generate controller from an entity and target service | `perigon generate controller <EntityPath> <ServicePath|ServiceName>` |
| Generate typed request client/models from OpenAPI | `perigon generate request <path|url> <outputPath>` |
| Generate only models from OpenAPI | `perigon generate request <path|url> <outputPath> -m` |
| List official module packages | `perigon module list` |
| Install a module package | `perigon module install <PackagePath|OfficialName> <ServiceName>` |
| Pack a module into a zip | `perigon module pack <ModuleName> <ServiceName>` |
| Initialize MCP config | `perigon mcp init` |
| Start MCP server | `perigon mcp start` |
| Launch Studio | `perigon studio` |
| Update Studio | `perigon studio update` |
| Show command help | `perigon -h` or `perigon <command> -h` |

## Key workflows

### 1. Scaffolding and solution setup

- Prefer `perigon new <name>` for creating a new solution from the CLI.
- Prefer `perigon add module <ModuleName>` and `perigon add service <ServiceName>` for adding project structure.
- `perigon add module <ModuleName>` allows omitting the `Mod` suffix.

### 2. Code generation from entities and services

- Use `perigon generate dto <EntityPath>` to create DTO classes from an entity file.
- Use `perigon generate manager <EntityPath>` to create manager classes.
- Use `perigon generate controller <EntityPath> <ServicePath|ServiceName>` to generate controllers that target an existing service.
- Add `-f` / `--force` when you want to overwrite existing generated files.

### 3. OpenAPI / frontend request generation

Use `perigon generate request` when the task is to generate typed request services or models from a Swagger/OpenAPI document.

```bash
perigon generate request ./openapi.json ./src/services -t angular
```

Supported types: `angular` (default), `csharp`, `axios`.
Use `-m` / `--only-model` to generate only model files.

In this repository, the preferred frontend `outputPath` is the absolute path of `src/ClientApp/WebApp/src/app`.

### 4. Module package workflows

- Use `perigon module list` to inspect official module packages.
- Use `perigon module install <PackagePath|OfficialName> <ServiceName>` to install a local zip or an official module package into the current project.
- Use `perigon module pack <ModuleName> <ServiceName>` to package a module for distribution.
- `perigon module pack <ModuleName> <ServiceName>` expects the module name with the `Mod` suffix.

### 5. MCP and Studio workflows

- Use `perigon mcp init` to write the MCP stdio configuration into `.vscode/mcp.json`.
- Use `perigon mcp start` to launch the MCP server over stdio.
- Use `perigon studio` to open Perigon Studio, and `perigon studio update` to update it.

## Important rules

- **Use Perigon for scaffolding and code generation**, not for build/test/run.
- **Prefer generated code over hand-written boilerplate** for modules, services, DTOs, managers, controllers, and request clients.
- **For frontend API clients, prefer `perigon generate request`** and keep generated contracts consistent with backend OpenAPI definitions.
- **Do not manually edit generated request contracts unless necessary**; regenerate when the backend changes.
- **Check help before guessing options** with `perigon -h` or `perigon <command> -h`.
- **Prefer the current subcommand form** (`perigon module ...`) over older shorthand aliases when the help output is ambiguous.
