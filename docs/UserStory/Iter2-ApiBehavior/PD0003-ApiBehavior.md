# PD0003 - API 行为契约

## 文档信息

- Iteration: `Iter2-ApiBehavior`
- ProductDesign: `PD0003-ApiBehavior`
- Module: `ApiBehavior`
- Status: `complete`
- LastUpdated: `2026-09-24`

## 背景与目标

模板为认证用户提供统一上下文，并为 MVC 请求模型验证失败提供可预测的错误响应。业务模块需要读取稳定的角色标识；客户端也需要用固定字段解析自动验证错误。

## 范围

### 包含

- ApiStandard 和 MiniApi 的 `IUserContext` 暴露 `RoleIds`，从 `role_id` Claims 读取有效且唯一的 GUID。
- ApiStandard MVC 自动模型验证失败返回结构化 HTTP 400。

### 不包含

- 不负责登录端签发角色 Claims；调用方需在令牌中写入 `role_id`。
- 不改变 MiniApi 的 endpoint 验证或错误响应行为。

## 需求与验收场景

### REQ-001 提供当前用户的角色 ID

- Given: 认证主体含一个或多个 `role_id` Claims。
- When: 业务代码读取 `IUserContext.RoleIds`。
- Then: 返回有效的非空 GUID，过滤非法值和空 GUID，并移除重复项；没有有效 Claim 时返回空列表。

### REQ-002 稳定 MVC 自动验证错误响应

- Given: ApiStandard MVC 请求的模型状态无效。
- When: ASP.NET Core 自动生成验证错误响应。
- Then: 返回 HTTP 400，并包含 `title`、`detail`、`status` 和 `traceId`；`detail` 汇总各模型状态项的第一条非空错误消息。

## 设计决策

| ID | Decision | Rationale | Alternatives |
|---|---|---|---|
| D-004 | 将角色名称和角色 ID 分开暴露；`RoleIds` 只接收有效且去重后的 `role_id` GUID Claims | 角色名称可变且适合显示，授权逻辑可以使用不透明 ID | 将 ID 混入 `Roles` 字符串列表 |
| D-005 | 仅为 ApiStandard 的自动模型状态错误设置固定 400 响应字段 | 保持模板错误响应契约可预测，并包含请求 TraceId | 暴露框架默认的验证错误结构 |

## 实现状态

- Status: `complete`
- Implemented requirements: `REQ-001`, `REQ-002`
- Latest evidence: 两套模板均实现 `RoleIds`；ApiStandard 在 MVC `InvalidModelStateResponseFactory` 中使用 `CustomBadRequest`。发布及验证记录见 [PT0004](../../Development/Iter2-ApiBehavior/PT0004-ApiBehavior.md)。
