# 说明

`Perigon.templates` 项目模板的使用提供文档支持。

## 根目录

- docs: 项目文档存储目录
- scripts： 项目脚本文件目录
- src：项目代码目录
- test：测试项目目录
- .config：配置文件目录

## 代码目录src

* `src/Ater/Perigon.AspNetCore`: 基础类库，提供基础帮助类。
* `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
* `src/Definition/Entity`: 包含所有的实体模型，按模块目录组织。
* `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
* `src/Modules/`: 包含各个模块的程序集，主要用于业务逻辑实现
* `src/Modules/XXXMod/Managers`: 各模块下，实际实现业务逻辑的目录
* `src/Modules/XXXMod/Models`: 各模块下，Dto模型定义，按实体目录组织
* `src/Services/ApiService`: 是接口服务项目，基于ASP.NET Core Web API
* `src/Services/AdminService`: 后台管理服务接口项目

> [!NOTE]
> 这里不存在基于`模块`的开发，也没有这个概念。这里的模块是基于业务上的划分，将相应的业务实现在代码上进行拆分，实现关注点分离。


## 项目运行

项目基于`Aspire`，直接运行`AppHost`项目即可启动所有服务。
