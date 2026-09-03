# PT0001 - Initial Plan Task

> 本文件是生成项目的业务跟踪起点。模板仓库自身的维护记录不写入本文件，避免传播到新项目。

## 文档信息

- Iteration: `Iter0-Initial`
- PlanTask: `PT0001-Initial`
- Module: `Initial`
- Source: [PD0001-Initial](../../UserStory/Iter0-Initial/PD0001-Initial.md)
- Status: `complete`
- Progress: `3/3`
- Owner:
- LastUpdated: `2026-09-03`

## 目标与范围

### 目标

移除模板不再提供的图形验证码能力及其 SixLabors 依赖，同时保持现有登录/业务验证码配置不变。

### 包含

- `ApiStandard` 和 `MiniApi` 的 `Perigon.AspNetCore.Toolkit` 包引用、版本声明及图形验证码 helper/test。
- 工具包 README、程序集描述和 Perigon 双语文档中的过时能力说明。

### 不包含

- `IsNeedVerifyCode`、`VerifyCodeCachePrefix` 及发送/校验验证码的通用配置、文案。

## 技术上下文与决策

| Decision | Rationale | Alternatives |
|---|---|---|
| 移除 `ImageHelper` 及其单元测试，并删除全部 SixLabors 包引用 | 图形验证码不再由模板提供；实现和依赖均无其他源码调用 | 保留空 helper 或仅移除测试会留下失效公开能力 |
| 保留通用登录/业务验证码配置 | 当前调用链未将其与图形验证码实现绑定，扩大删除会改变非图形验证码范围 | 一并删除会造成不必要的兼容性破坏 |

## 任务

<!-- 添加任务后将 Progress 改为 done/total。示例：
- [ ] TASK-001 [PD0001/REQ-001/SC-001] 在 `src/...` 完成可验证动作
  - Depends on: none
  - Done when: 可观察结果
  - Verify: `精确命令`
  - AOT: `not-affected` 或 Native AOT 验证要求
-->

- [x] TASK-001 [PD0001] 从 `ApiStandard`、`MiniApi` 移除 SixLabors 版本/包引用及 `ImageHelper` 图形验证码实现和测试。
  - Depends on: none
  - Done when: 两个模板的源代码中不再包含 SixLabors、ImageHelper 或 GenerateImageCaptcha；MiniApi 不保留由该测试专用的空 UnitTest 项目；工具包和剩余测试项目可还原/构建。
  - Verify: `rg -n -i --hidden -g '!**/bin/**' -g '!**/obj/**' -g '!**/.git/**' 'SixLabors|ImageSharp|ImageHelper|GenerateImageCaptcha' .`; `dotnet build MiniApi/src/Perigon/Perigon.AspNetCore.Toolkit/Perigon.AspNetCore.Toolkit.csproj`; `dotnet build ApiStandard/src/Perigon/Perigon.AspNetCore.Toolkit/Perigon.AspNetCore.Toolkit.csproj`
- [x] TASK-002 [PD0001] 更新两个模板工具包的 README 与包描述，准确反映移除后的能力范围。
  - Depends on: TASK-001
  - Done when: 工具包说明和发布指导不再声称提供图片处理/图形验证码能力。
  - Verify: `rg -n -i 'image processing|ImageHelper|图形验证码|图片处理|验证码' ApiStandard/src/Perigon/Perigon.AspNetCore.Toolkit MiniApi/src/Perigon/Perigon.AspNetCore.Toolkit ApiStandard/.agents/skills/perigon/references/scripts.md`
- [x] TASK-003 [PD0001] 同步 Perigon.docs 中英文目录结构、发布字体说明和工具包能力描述。
  - Depends on: TASK-001
  - Done when: 双语文档不再把图形验证码或模板图片处理列为内置工具包能力，且文档校验通过。
  - Verify: `rg -n -i 'graphic verification|image processing|图形验证码|图片处理' C:/codes/Perigon.docs/Content/docs/Perigon`（无匹配）；`pwsh -NoProfile -File ./scripts/validate-docs.ps1`

## 验证策略

| Requirement / Scenario | Test level | Command or check | Expected result |
|---|---|---|---|
| SixLabors 与图形验证码能力已完全移除 | 静态结构 + build | `rg ...`; 两个 Toolkit `dotnet build` | 无残留源码引用，两个项目构建通过 |
| 通用验证码配置未被误删 | 静态结构 | 检查 `LoginSecurityPolicyOption`、`WebConst`、服务配置与本地化文件 | 原有非图形验证码配置仍存在 |
| 文档与模板能力一致 | 文档校验 | `pwsh -NoProfile -File ./scripts/validate-docs.ps1` | 文档校验通过 |

## 实现记录

> 每次 AI coding 后追加一条。无论 done、in-progress 或 blocked，只要修改了代码就必须记录。

### `2026-09-03` — `TASK-001`～`TASK-003` / 移除图形验证码能力

- Status: `done`
- Implementation: 两个模板移除 SixLabors 包版本/引用、`ImageHelper` 实现、图形验证码单元测试和 MiniApi 空 UnitTest 项目；同步 README、包描述、MiniApi 测试/AOT 指导及 Perigon 双语文档。
- Code evidence: `ApiStandard/Directory.Packages.props`、`MiniApi/Directory.Packages.props`、两个 `Perigon.AspNetCore.Toolkit.csproj`、两个 `ImageHelper.cs` 与测试文件删除。
- Verification:
  - （ApiStandard 根目录）`dotnet build MyProjectName.slnx` — `passed`（0 警告、0 错误）
  - （MiniApi 根目录）`dotnet build MyProjectName.slnx` — `passed`（0 警告、0 错误）
  - （ApiStandard 根目录）`dotnet test --project tests/UnitTest/UnitTest.csproj` — `passed`（20/20）
  - （Perigon.docs 根目录）`pwsh -NoProfile -File ./scripts/validate-docs.ps1` — `passed`（191 files）
  - （Perigon.docs 根目录）`pwsh -NoProfile -File ./build.ps1` — `passed`
- AOT evidence: `not-affected`；本次仅删除依赖与代码，MiniApi 解决方案构建已通过，未重复执行 NativeAOT publish。
- Documentation: 已同步两个模板的 PT/ProjectTracking/Changelog 及 `Perigon.docs` 双语页面；PD0001 为空白起始设计，无既有验收场景可更新。
- Remaining: 外部项目若直接引用已删除的公开 helper 或 SixLabors 传递依赖，需要自行迁移；通用验证码配置按范围保留。

## 阻塞与待确认项

| ID | Related task | Description | Decision / owner | Exit condition |
|---|---|---|---|---|

## 完成摘要

- FinalStatus: `COMPLETE`
- CompletedTasks: `3/3`
- RequirementsCovered: SixLabors 依赖、图形验证码 helper/调用和过时说明已移除；通用验证码配置保留。
- VerificationSummary: 两套解决方案 build、ApiStandard 单元测试、源码残留扫描、Perigon.docs 校验与构建均通过。
- AotSummary: 本次变更不引入 AOT-sensitive 能力；MiniApi solution build passed，NativeAOT publish 未重复执行。
- ResidualRisks: 删除公开 `ImageHelper.GenerateImageCaptcha` 属于兼容性变化，外部调用方需迁移。
