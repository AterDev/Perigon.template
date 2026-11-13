# PD0010：MiniApi 前端与 Aspire API 适配

## 背景

MiniApi 的 Angular 登录页仍使用 ApiStandard 管理后台的 `/api/systemUser/*` 路由和 access token 流程，而 MiniApi 只提供 `/api/users/login` 并直接返回 `UserInfoDto`。前端开发代理也指向不存在的 17001 端口，且 Angular `serve` 未启用代理。

## 目标

- Angular 开发服务器将 `/api` 请求代理到 MiniApi `ApiService`。
- 登录页调用 `/api/users/login` 并处理 MiniApi 返回的用户信息。
- 前端登录状态恢复、路由保护和退出登录与当前登录响应保持一致。
- 保持 ApiStandard 管理前端的现有认证行为。

## 验收标准

- MiniApi 前端源代码不再依赖 `/api/systemUser/*`、旧 Admin 基址或不存在的 access token 响应。
- Angular `serve` 使用代理配置将 API 请求转发到 MiniApi 本地开发端口。
- 登录成功后使用返回的 `UserInfoDto` 进入首页；刷新后保持前端状态，退出后受保护页面回到登录页。
- MiniApi 前端构建通过，模板包不包含根目录维护文档。
