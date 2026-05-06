---
name: "Angular Razor Template"
description: "生成 Perigon Angular Razor 模板。Use when: templates/Angular, index/add/edit/detail Razor template, Angular Material, DTO-driven CRUD page generation."
agent: "agent"
---

# Angular Razor Template

使用 `Razor` 语法编写模板内容，以便 Razor 引擎生成对应的前端页面代码。

## 具体说明

- 前端框架:
  - Angular 21+
  - Angular Material 最新版本
  - Bootstrap 5 栅格系统用于布局

- 生成内容：
  - 列表页面组件: index.html.razor/index.ts.razor
  - 详情页面组件: detail.html.razor/detail.ts.razor
  - 添加页面组件: add.html.razor/add.ts.razor
  - 编辑页面组件: edit.html.razor/edit.ts.razor

- 要求：
  - 充分利用 Razor 中可使用的 C# 逻辑处理能力，通过方法封装来简化模板内容，避免复杂的模板语法拼接。
  - 使用最新的 Angular Material 组件库。直接使用组件样式，不需要自定义样式，布局使用 `bootstrap-grid` 提供的栅格系统。
  - 默认优先使用 signals 处理响应式数据，以及 Angular 21+ 推荐写法。
  - 组件文件名称不需要加 Component 后缀。
  - 特别注意 Razor 语法中 `@` 符号的使用，避免和 Angular 模板语法冲突。
  - 生成后要检查代码正确性，确保可以直接使用。
  - 生成前端内容时，避免与 Razor 语法冲突，如 Angular 前端模板也会使用 `@for` / `@if`。

导入以下内容以支持多语言和请求服务：

```typescript
import { AdminClient } from 'src/app/services/admin/admin-client';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { I18N_KEYS } from 'src/app/share/i18n-keys';
```

如当前是用户实体，则使用 `this.adminClient.user.xxx` 进行调用：

- 添加是调用 `this.adminClient.user.add(dto: UserAddDto)`
- 编辑是调用 `this.adminClient.user.update(id: string, dto: UserUpdateDto)`
- 获取详情是调用 `this.adminClient.user.detail(id: string)`
- 获取列表是调用 `this.adminClient.user.filter(filterDto: UserFilterDto)`
- 删除是调用 `this.adminClient.user.delete(id: string)`

对应的类型分别是：

- 列表筛选使用 `UserFilterDto`
- 列表项使用 `UserItemDto`
- 添加使用 `UserAddDto`
- 编辑使用 `UserUpdateDto`
- 详情使用 `UserDetailDto`

DTO 的路径根据模块和名称组合，路径示例：`src/app/services/admin/models/user-mod/user-add-dto.model.ts`。

## 执行步骤

1. 通过 Perigon MCP 工具的“创建 razor 模板”，获取 Razor 模板中可使用的变量和上下文信息。
2. 根据规范和页面具体要求生成页面组件代码，充分利用 Razor 语法带来的 C# 逻辑能力。
3. 通过 Perigon MCP 工具的“通过 razor 模板生成代码示例”验证模板可用性，并修正问题。
4. 将验证过的模板内容保存到 `templates\Angular` 目录下。

## 列表页面

列表由筛选组件 toolbar 和 table 组件组成。

- 筛选组件通过 `XXXFilterDto` 中包含的字段生成对应控件，如字符串输入框、枚举下拉框、布尔开关等。添加按钮放在筛选行最右侧。
- 表格组件通过 `XXXItemDto` 中的字段生成列，包含操作列和分页功能。操作列包含查看详情、编辑、删除按钮。
- 删除要求弹出确认对话框，使用已封装的 `ConfirmDialogComponent`。
- 编辑默认使用弹窗调用 `edit` 组件。
- 添加时弹窗调用 `add` 组件，添加成功后刷新列表。
- 枚举显示使用 `EnumTextPipe`；日期使用 `date` 管道格式化，默认格式为 `yyyy-MM-dd HH:mm:ss`。

## 添加页面

- 同时支持弹窗和路由两种方式打开，根据是否有弹窗数据决定。
- 根据 `XXXAddDto` 生成表单组件，实现添加功能，添加成功后关闭弹窗或跳转回列表页面。
- 根据 `XXXAddDto` 中的验证特性生成完整表单验证规则。

## 编辑页面

- 同时支持弹窗和路由两种方式打开，根据是否有弹窗数据决定。
- 先根据 id 获取详情数据回填，id 通过路由参数或弹窗数据传入。
- 根据 `XXXUpdateDto` 生成表单组件，实现编辑功能，编辑成功后关闭弹窗或跳转回列表页面。
- 根据 `XXXUpdateDto` 中的验证特性生成完整表单验证规则。

## 详情页面

- 同时支持弹窗和路由两种方式打开，根据是否有弹窗数据决定。
- 根据 id 获取详情数据展示，id 通过路由参数或弹窗数据传入。
- 根据 `XXXDetailDto` 生成详情展示组件。
- 包含返回按钮，返回列表页面。

## 表单内容说明

- TypeScript 中使用 getter 定义控件，在 HTML 使用 `[formControl]` 绑定。
- 表单使用最新的响应式表单。
- 使用统一的 `getValidatorMessage` 方法获取验证错误信息，支持多语言翻译。
- 枚举控件可使用 `ToKeyValuePipe` 转换为键值对数组，显示时使用 `EnumTextPipe`，注意导入正确路径。
- 大于 200 长度的控件使用 textarea。
- 布尔值和时间类型使用合适组件，如 checkbox 和 datepicker。