---
name: "Angular Guidelines"
description: "Use when editing Angular files: component, template, route, form, Material UI, signals, i18n, generated request client."
applyTo: ["src/ClientApp/WebApp/**/*.ts", "src/ClientApp/WebApp/**/*.html", "src/ClientApp/WebApp/**/*.scss", "src/ClientApp/WebApp/**/*.json"]
---

# Angular Guidelines

- 使用 Angular 21+ standalone 组件，不使用 NgModule。
- UI 优先使用 Angular Material；布局优先复用现有样式和 Bootstrap grid，避免内联样式。
- 状态优先使用 signals、async pipe 或框架推荐响应式写法；模板中避免调用复杂函数。
- 模板控制流使用 `@if`、`@for`、`@switch`，`@for` 必须包含 `track`。
- 表单使用 Typed Reactive Forms；在 FormGroup 内优先用 `[formControl]` + getter，不优先使用 `formControlName`。
- 从 `src/app/modules/share/i18n-keys` 导入 `I18N_KEYS`，并在组件中暴露 `i18nKeys = I18N_KEYS`（或等价字段）。模板使用 `{{ i18nKeys.common.save | translate }}`；不要使用 `{{ 'common.save' | translate }}` 这类字面量 key。
- 调用 `TranslateService` 时传入 `I18N_KEYS` 的成员，例如 `translate.instant(this.i18nKeys.common.save)`；不要传入或拼接字面量 key。动态校验 key 必须从 `I18N_KEYS.validation` 查找。
- `i18n-keys.ts` 由 `pnpm i18n:keys` 生成，不要手动修改。
- 不手动修改 Perigon 生成的请求服务和 DTO；后端 OpenAPI 变化后重新生成。
- 将跨模块复用的组件、守卫、管道、i18n 键与 Material 聚合导入放在 `src/app/modules/share`，并通过 `src/app/modules/share/...` 导入；不要创建顶层 `src/app/share`。
- 前端验证在 `src/ClientApp/WebApp` 下执行 `pnpm build`。
