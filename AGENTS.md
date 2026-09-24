# AGENTS

## 仓库作用域

- 本文件是 `Perigon.template` 模板仓库的仓库级协作说明，适用于根目录任务、跨模板修改、模板打包、源代码生成器和 CLI 集成维护。
- `ApiStandard/AGENTS.md` 与 `MiniApi/AGENTS.md` 是模板内容的一部分，供生成出来的项目使用，不是本仓库的工作流说明。
- 执行本仓库级任务时，不读取或应用 `ApiStandard/**/AGENTS.md`、`MiniApi/**/AGENTS.md` 的规则；只遵循本文件以及真正位于当前作用域内的仓库级说明。

## 文档边界

- 本仓库自身的产品需求、设计、迭代计划、实现记录和变更记录统一放在根目录 `docs`。
- `ApiStandard/docs` 与 `MiniApi/docs` 是生成项目的初始文档骨架；它们随模板提供给新项目，不用于记录本仓库当前维护任务。
- 本仓库的维护记录不得新增到模板目录，也不得把根目录 `docs` 打包进模板内容。

## 主要目录

- `ApiStandard`：标准 Web API 模板。
- `MiniApi`：Minimal API / Native AOT 模板。
- `docs`：本模板仓库的维护文档。
- `Pack.csproj`：模板 NuGet 包打包项目。

## 协作规则

- 以源码、项目文件、NuGet 包元数据和实际构建/测试结果为事实依据。
- 修改模板行为、依赖、生成内容或 CLI 模板输出时，检查两个模板是否需要保持一致，并同步根目录 `docs` 中对应的 PT、PD、ProjectTracking 和 Changelog。
- 模板项目引用源代码生成器时使用已发布的 `Perigon.AspNetCore.SourceGeneration` NuGet 包；源代码生成器源码仅作为仓库维护和打包来源，不应作为模板项目引用的 ProjectReference。
- 代码或模板变更后执行与影响范围匹配的 build、test、pack 和模板内容检查；没有通过必需验证不宣称完成。
- 不使用子目录模板中的维护记录替代根目录仓库记录，也不把仓库级记录传播到生成项目。
