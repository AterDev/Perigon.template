# PT0001 - Initial Plan Task

## 文档信息

- Iteration: `Iter0-Initial`
- PlanTask: `PT0001-Initial`
- Module: `TemplateAndCli`
- Status: `complete`
- Progress: `3/3`
- LastUpdated: `2026-09-03`

## 目标与范围

移除模板不再提供的图形验证码能力及其 SixLabors 依赖，同时保持现有登录/业务验证码配置不变。

## 已完成任务

- [x] 从 ApiStandard、MiniApi 移除 SixLabors 包引用、`ImageHelper` 图形验证码实现和测试。
- [x] 更新工具包 README、程序集描述和发布指导。
- [x] 同步 Perigon.docs 双语文档并完成文档校验。

## 验证

- 两套 solution build 通过。
- ApiStandard 单元测试 20/20 通过。
- Perigon.docs 校验和构建通过。

## 残余风险

删除公开的 `ImageHelper.GenerateImageCaptcha` 属于兼容性变化，外部调用方需迁移；通用验证码配置按范围保留。
