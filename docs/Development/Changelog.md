# Changelog

> 只记录 Perigon.template 仓库已交付的用户可见行为、公开契约、配置或运维变化。

## Unreleased

### Changed

- ApiStandard、MiniApi 和 Perigon CLI 新服务改为通过 `Perigon.AspNetCore.SourceGeneration` 1.1.1 NuGet 包加载统一源生成器；不再复制或显式构建源生成器项目，并支持 Minimal API endpoint group 自动注册。
- 仓库维护文档统一放在根目录 `docs`，不再随模板生成到业务项目。

### Fixed

- ApiStandard 局部更新忽略 DTO 中的 Id、TenantId、CreatedTime，避免普通更新改变记录身份和租户归属。
- ApiStandard 批量插入先物化输入，确保字段赋值和插入使用同一批实体。
- ApiStandard、MiniApi 的 Between 保留嵌套属性访问链。
- MiniApi Native AOT 示例补充 `JwtOption` 配置初始化和 endpoint JSON metadata，确保生成路由可以在裁剪后的原生发布中启动并处理 JSON 请求。

### Removed

- 移除 ApiStandard 和 MiniApi 内置的 `ImageHelper.GenerateImageCaptcha` 图形验证码 helper、单元测试及其 SixLabors 依赖。

### Migration and compatibility

- 本批数据访问修复不改变方法签名或数据库结构。局部更新中 Id、TenantId、CreatedTime 将被忽略；租户迁移需要独立操作。null 跳过、软删除和时间范围语义保持不变。Attach stub 跨租户写入风险仍待后续修复，详见 PD0002。
- 现有项目若直接使用已删除的图形验证码 helper 或依赖其 SixLabors 传递包，需要自行迁移。
- 使用 MiniApi 时，endpoint group 仍需继承 `RestEndpointBase` 并提供符合约定的 `MapEndpoints` 方法。

