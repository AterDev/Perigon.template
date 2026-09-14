# 产品需求总览

> 本文档只记录 Perigon 模板仓库的产品目标、范围和迭代目录。具体需求与验收场景按迭代、功能模块编写在 `Iter*/PD####-Name.md`。

## 产品背景

Perigon.template 维护 ApiStandard 和 MiniApi 两套 .NET 项目模板，并为 Perigon.CLI 提供服务项目生成能力。

## 产品目标与成功指标

- 模板生成项目不携带仓库内部维护文档。
- 模板和 CLI 使用稳定的 NuGet 源代码生成器包。
- 两套模板的源代码生成能力保持一致，并可通过构建、测试和 Native AOT 验证。

## 总体范围

### 包含

- ApiStandard、MiniApi 模板及 Perigon.CLI 的模板生成逻辑。
- 模板包、源代码生成器包和 Perigon 文档的版本与兼容性维护。

### 不包含

- 业务项目自身的产品需求、部署配置和运行数据。

## 迭代与产品设计目录

| Iteration | Product Design | Module | Status | Summary |
|---|---|---|---|---|
| `Iter0-Initial` | [PD0001-Initial](./Iter0-Initial/PD0001-Initial.md) | TemplateAndCli | complete | 统一源代码生成器 NuGet 消费方式并合并两套模板的生成能力 |

## 编号规则

- 迭代目录：`Iter<number>-<IterationName>`。
- 产品设计：`PD<4-digit>-<ModuleName>.md`。
- 计划任务：`PT<4-digit>-<ModuleName>.md`。
- 这些文档只保存在仓库根目录 `docs`，不进入模板内容。
