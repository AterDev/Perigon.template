---
name: angular
description: "Angular 21+ 前端开发规范。Use when: Angular component, page, route, form, Material UI, Material 3, bootstrap-grid, responsive layout, dashboard, CRUD page, i18n, generated request client."
---

## 何时使用

本技能适用于使用 Angular 框架进行前端开发的任务。

## 项目结构

### 目录布局

```
src/
  ├── main.ts                    # 应用入口
  ├── app/
  │   ├── app.config.ts          # 应用配置
  │   ├── app.routes.ts          # 路由配置
  │   ├── layout/                # 布局容器
  │   ├── pages/                 # 页面组件
  │   ├── share/
  │   │   ├── components/        # 共享组件
  │   │   ├── pipe/              # 管道
  │   │   ├── auth.guard.ts      # 路由守卫
  │   │   ├── custom-paginator-intl.ts
  │   │   └── i18n-keys.ts       # i18n 键定义
  │   └── services/              # 服务和 API 客户端
  ├── assets/
  │   └── i18n/*.json            # 多语言文件
  ├── environments/              # 环境配置
  ├── styles/
  │   ├── styles.scss            # 全局样式
  │   ├── theme.scss             # Material 主题
  │   └── vars.scss              # CSS 变量
  └── proxy.conf.json            # 开发代理配置
```

<rules>

- **Standalone 组件**：不使用 NgModule
- **Angular Material**：统一的 UI 组件库
- **Signals 优先**：使用新的响应式 API
- **严格的类型安全**：TypeScript 严格模式
- ✓ 优先使用 async pipe 或 signals
- 避免在模板中使用函数调用
- 合理使用 `trackBy` 函数
</rules>

<workflow>

0. 生成前端请求服务及类型：通过perigon 提供的命令行工具`generate request`或`perigon mcp`工具，根据后端swagger文件生成前端代码，
`outputPath`参数为前端项目根目录下的`\src\app`目录的绝对路径(生成错了要删除)。
1. 创建独立组件：目录及文件结构
2. 配置路由和菜单
3. 实现ts逻辑和HTML模板
4. 进行页面设计自检：信息层级、布局密度、响应式、状态、空/错/加载态
5. **执行构建验证**（必须步骤）：验证编译无错误
6. 检查导入和依赖

优先通过使用 MCP 工具生成组件，Perigon提供`通过razor 模板生成代码`的能力，以获取可参考的代码结构和示例。

**特别注意**：生成的请求服务代码不要添加或修改，它是与接口保持一致的，包括类型定义，切勿重复定义类型。

</workflow>

## 构建验证（每次修改后必须执行）

### 验证前端构建
```pwsh
pnpm build
```

### 实时开发验证（可选）
```pwsh
pnpm start  # 启动开发服务器，实时查看编译错误
```

### 构建-修复循环
修改代码 → 构建 → 发现错误 → 修复 → 重新构建，直到无错误

## 组件开发

- `enumText`管道：用于将枚举值转换为对应的文本显示。
- `toKeyValue`管道：用于将枚举类型转换为键值对数组，以便在选项列表中使用。

### UI/UX设计

具备良好的交互常识和布局和审美。要考虑排版、边距，文字大小，颜色搭配，组件间距等，优先参考已定义好的`theme.scss`中的样式变量和组件样式，保持整体风格一致。

**设计输入**：
- 先判断页面类型：列表检索、表单编辑、详情阅读、仪表盘、设置页、登录页、向导流程、异常页。
- 先判断用户目标：快速查找、批量处理、录入校验、审计追踪、状态监控、配置维护。
- 先判断信息密度：字段数量、筛选条件数量、主次操作数量、是否需要批量操作、是否需要左右分栏。

**页面骨架**：
- 企业后台默认使用“页面标题/工具区 + 查询或内容区 + 表格/表单/详情 + 分页或底部操作”的结构。
- 列表页：顶部标题行保持简洁；筛选区使用可换行 flex 布局；主操作放右侧；高频操作露出，低频操作放菜单。
- 表单页：字段少时单列；字段多时桌面两列、移动端单列；相关字段用小节分组，不要把所有控件堆成一张大表单。
- 详情页：用 definition/list 风格或只读表单风格呈现；长文本、审计信息、关联数据分区展示。
- 仪表盘：先放关键指标和异常入口，再放趋势和明细；不要用大量装饰卡片稀释信息。
- 登录页可以更有品牌感，但仍要保证表单清晰、对比足够、移动端不裁切。

**响应式布局**：
- 参考 Material window size class 思路，而不是只针对某个设备：compact `<600px`、medium `600-839px`、expanded `>=840px`、desktop `>=1200px`。
- compact：单列、操作按钮可堆叠或收进菜单、表格允许横向滚动。
- medium：可显示更多筛选项或二列轻量内容，但不要压缩复杂表格。
- expanded/desktop：可以使用两栏布局、侧边详情、更多列和更高信息密度。
- 优先使用 `bootstrap-grid` 的 `d-flex`、`flex-wrap`、`gap-*`、`align-items-center`、`justify-content-between`、`flex-grow-1`、`col-*`、`row-cols-*`；不要使用 `container/row/col/w-100` 作为默认套路，只有明确需要 12 栅格时才使用 `row/col-*`。

**Material 3 视觉规则**：
- 优先使用 Angular Material 组件表达语义：按钮、图标按钮、菜单、tabs、chips、table、paginator、dialog、snackbar、form-field、slide-toggle、checkbox、select、datepicker。
- 优先使用 `theme.scss`、`vars.scss`、Angular Material token 或组件 API，不直接覆盖 Material 私有 DOM 结构。
- 色彩只用于层级和状态：primary 用于主操作，tertiary/accent 用于辅助强调，error 用于错误和危险操作。
- 不做一页一种颜色风格；不要堆叠渐变、发光、阴影、超大圆角。后台页面应清爽、稳定、可重复使用。
- 文字层级要克制：页面标题、分组标题、表格文字、辅助说明分别使用稳定大小；不要在普通后台页面使用 hero 级标题。
- 卡片只用于独立内容块、统计项、弹窗或可重复条目；不要“卡片套卡片”。页面大区块更适合自然分隔、间距和边框。

**组件选择**：

要充分考虑用户交互的便利性和视觉特点，选择合适的组件，比如多选，批量操作，内容展示等，要根据实际业务特点去选择。


- 单选少量选项：`mat-button-toggle-group` 或 `mat-radio-group`。
- 多选筛选：`mat-select multiple` 或 chips + menu。
- 二元设置：`mat-slide-toggle`；表格行选择：`mat-checkbox`。
- 危险操作：icon button + tooltip + confirm dialog；批量危险操作必须二次确认。
- 长列表：`mat-table` + `mat-paginator`；简单键值：list/detail layout；少量状态：chips；复杂状态：tag + tooltip。
- 行内主操作不超过 2 个；更多操作放 `mat-menu`。

**状态设计**：
- 每个页面至少考虑 loading、empty、error、disabled、saving/submitting 状态。
- 空状态只说明当前结果为空并提供最直接动作，不写冗长说明。
- 表单校验错误显示在控件附近，提交失败用 snackbar 或页面级错误提示。
- 所有 icon-only 按钮都要有 `matTooltip` 或 `aria-label`。

**AI 设计自检清单**：
- 页面第一屏是否能看出当前页面、主要对象和主要操作？
- 标题、筛选、操作、结果是否形成清晰层级？
- 桌面端是否浪费大面积空白，移动端是否出现文字/按钮挤压？
- 是否存在横向滚动？允许的只有内部表格或代码/长文本区域。
- 是否出现硬编码颜色、内联样式、重复自定义按钮样式？
- 是否使用 i18nKeys，没有硬编码用户可见字符串？
- 是否包含加载、空、错、禁用、提交中状态？
- 页面是否符合“企业后台工具”气质，而不是营销页或装饰型 landing page？

**样式层级**：
- **全局样式**：`styles.scss` - 基础样式和重置
- **主题样式**：`theme.scss` - Material 主题定制
- **CSS 变量**：`vars.scss` - 颜色、间距等变量
- **组件样式**：每个组件的 `.scss` 文件 - 局部样式

**样式规范(重要)**：
- 先理解`theme.scss`中定义的 Material 主题和组件样式变量，优先使用这些样式。
- 使用Angular Material提供的组件和样式类，而不是自己定义样式类和样式。
- 关注行内元素垂直居中对齐
- 整体页面不要出现水平滚动条(内部表格除外)，要注意组件的宽度和外层容器的宽度关系
- ✗ 不要在组件中使用内联样式，而是在scss中定义。
- 使用 bootstrap-grid 的 flex util 布局，而不是 container/row/col/w-100 等固定套路。
- 不使用 `::ng-deep` 覆盖 Material 内部结构，除非是修复已有组件兼容问题且没有 token/API 可用。
- 组件样式优先写布局、间距和少量局部状态，不要重写 Material 组件整体皮肤。
- 间距优先使用项目已有 `gap-1/gap-2/gap-3`、Bootstrap spacing/flex 工具或少量组件级变量。
- 对固定格式 UI 设置稳定尺寸或响应式约束，例如 toolbar、表格操作列、统计卡、图标按钮、弹窗宽度。
- 移动端必须检查文字换行、按钮宽度、表单字段高度和弹窗最大宽度。

### 常用页面布局模板

**列表检索页**：
```html
<section class="d-flex flex-column gap-3">
  <header class="d-flex flex-wrap gap-2 align-items-center justify-content-between">
    <div>
      <h1>{{ i18nKeys.xxx.title | translate }}</h1>
      <p class="page-subtitle">{{ i18nKeys.xxx.subtitle | translate }}</p>
    </div>
    <button mat-flat-button color="primary">{{ i18nKeys.common.add | translate }}</button>
  </header>

  <form class="d-flex flex-wrap gap-2 align-items-center" [formGroup]="filterForm">
    <!-- 筛选项使用 mat-form-field，桌面横向排列，移动端自然换行 -->
  </form>

  <div class="table-shell">
    <!-- mat-table + mat-paginator -->
  </div>
</section>
```

**表单编辑页**：
```html
<section class="d-flex flex-column gap-3">
  <header class="d-flex flex-wrap gap-2 align-items-center justify-content-between">
    <h1>{{ i18nKeys.xxx.edit | translate }}</h1>
    <div class="d-flex gap-2 align-items-center">
      <button mat-button>{{ i18nKeys.common.cancel | translate }}</button>
      <button mat-flat-button color="primary" [disabled]="form.invalid || saving()">
        {{ i18nKeys.common.save | translate }}
      </button>
    </div>
  </header>

  <form class="form-grid" [formGroup]="form">
    <!-- desktop 两列，compact 单列；字段按业务分组 -->
  </form>
</section>
```

### 视觉验证

- 普通 TS/HTML/SCSS 修改后必须执行 `pnpm build`。
- 对新页面、复杂布局、登录页、仪表盘、响应式改动，优先启动前端并用浏览器工具检查桌面和移动端视口。
- 视觉检查重点：是否空白、是否溢出、是否遮挡、表格是否可横向滚动、按钮文字是否换行异常、暗色/亮色模式是否可读。

### 多语言

- 禁止使用硬编码字符串，而是定义和使用i18nKeys
- 使用 `translate` 管道进行文本翻译

### 表单管理

**Reactive Forms（推荐）**：
```typescript
import { FormControl, FormGroup } from '@angular/forms';

userForm = new FormGroup({
  name: new FormControl('', [Validators.required]),
  email: new FormControl('', [Validators.required, Validators.email])
});

// 类型安全
get nameControl() {
  return this.userForm.get('name') as FormControl;
}
```

**表单规范**：
- ✓ 使用类型化表单（Typed Forms）
- ✓ 提供清晰的验证消息
- ✓ 表单逻辑保留在组件中
- ✓ **在 FormGroup 内优先使用 `[formControl]`，不要用 `formControlName`**
- ✓ **在 TypeScript 中定义 getter 访问控件，避免字符串硬编码**
- ✓ 通过 getter 复用控件，保持一致性和类型安全

**模板示例**：
```html
<!-- ✅ 推荐：使用 [formControl] + getter，避免字符串硬编码 -->
<form [formGroup]="form">
  <mat-form-field>
    <mat-label>Name</mat-label>
    <input matInput [formControl]="name" />
    @if (name.errors) {
      <mat-error>{{ getValidatorMessage(name) }}</mat-error>
    }
  </mat-form-field>
</form>

```

**组件示例（通过 getter 访问）**：
```typescript
export class UserForm {
  form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]]
  });

  // ✅ 通过 getter 访问控件，避免字符串，便于复用和重构
  get name() { return this.form.get('name') as FormControl; }
  get email() { return this.form.get('email') as FormControl; }

  getValidatorMessage(control: AbstractControl | null): string {
    if (!control || !control.errors) { return ''; }
    const errors = control.errors;
    const key = Object.keys(errors)[0];
    const params = errors[key];
    return this.translate.instant(`validation.${key.toLowerCase()}`, params);
  }
}
```

## 服务和 API

### 服务位置
- **路径**：`app/services/`
- **HTTP 客户端**：`admin-client.ts` / 自定义客户端
- **基础服务**：`base.service.ts`
- **模型定义**：`models/` 和 `services/`

## 路由

### 路由配置
- **主路由**：`app/app.routes.ts`
- **页面组件**：`app/pages/*`
- **布局外壳**：`app/layout/*`

### 路由守卫

**认证守卫**：
- **位置**：`app/share/auth.guard.ts`
- **服务**：配合 `auth.service.ts` 使用

## 国际化（i18n）

### 文件结构

**翻译文件**：
- **位置**：`assets/i18n/*.json`
- **键定义**：`app/share/i18n-keys.ts`
- **键生成脚本**：`scripts/i18n-keys.js`

**JSON 文件**：
```json
{
  "common": {
    "save": "保存",
    "cancel": "取消",
    "delete": "删除"
  },
  "user": {
    "list": "用户列表",
    "detail": "用户详情"
  }
}
```

**使用方式**：
```html
<button>{{ i18nKeys.common.save | translate }}</button>
```

## 模板语法

**注意事项**：
- ✓ `@for` 循环必须包含 `track` 表达式
- ✓ Material Table 的 `*matHeaderRowDef` / `*matRowDef` 保留不变（这些是 Material 特有指令）
- ✗ 不要在项目中使用 `*ngIf` / `*ngFor`，应该使用 `@if` / `@for` 语法
