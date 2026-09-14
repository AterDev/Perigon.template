# Changelog

> 只记录 Perigon.template 仓库已交付的用户可见行为、公开契约、配置或运维变化。

## Unreleased

### Changed

- ApiStandard、MiniApi 和 Perigon CLI 新服务改为通过 `Perigon.AspNetCore.SourceGeneration` 1.1.1 NuGet 包加载统一源生成器；不再复制或显式构建源生成器项目，并支持 Minimal API endpoint group 自动注册。
- 仓库维护文档统一放在根目录 `docs`，不再随模板生成到业务项目。

### Fixed

- MiniApi Native AOT 示例补充 `JwtOption` 配置初始化和 endpoint JSON metadata，确保生成路由可以在裁剪后的原生发布中启动并处理 JSON 请求。

### Removed

- 移除 ApiStandard 和 MiniApi 内置的 `ImageHelper.GenerateImageCaptcha` 图形验证码 helper、单元测试及其 SixLabors 依赖。

### Migration and compatibility

- 现有项目若直接使用已删除的图形验证码 helper 或依赖其 SixLabors 传递包，需要自行迁移。
- 使用 MiniApi 时，endpoint group 仍需继承 `RestEndpointBase` 并提供符合约定的 `MapEndpoints` 方法。

