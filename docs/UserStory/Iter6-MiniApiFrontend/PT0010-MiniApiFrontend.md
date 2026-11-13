# PT0010：MiniApi 前端与 Aspire API 适配

- [x] 从 Aspire 日志确认前端因 Angular CLI 未安装完成而以 `'ng' is not recognized` 退出；`pnpm install` 卡在 222/246 个包，当前环境依赖安装未收尾。
- [x] 将 Angular `/api` 开发代理目标更正为 ApiService 的 `https://localhost:17002` 并启用 `proxyConfig`。
- [x] 登录页改用 MiniApi `/api/users/login`，按实际 `UserInfoDto` 保存前端登录状态并修正路由保护。
- [x] 使用本地缓存完成 `pnpm install --offline`；随后 Aspire 安装器显示 `Done`，frontend 资源运行于 `http://localhost:4200`。
- [x] `pnpm build` production build 通过。
- [x] `dotnet pack Pack.csproj --no-restore` 通过；模板包包含更新后的 Angular 配置和登录代码，不包含仓库根目录 `docs`。
