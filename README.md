# Perigon.templates

`Perigon.templates`是基于`Aspire`,`ASP.NET Core`和`Entity Framework Core`的快速开发模板，提供一个设计良好的项目工程结构。

模板已被集成在`Perigon.CLI`代码辅助工具中，建议使用该命令行工具来创建解决方案，以获得更好的体验。

## 要求
- .NET 10.0 SDK 或更高版本
- Aspire 13.0 或更高版本

## 文档

关于模板详细的说明和使用方法，请查阅[使用文档](https://www.dusi.dev/docs/perigon.html)！

## 安装

## 使用源代码安装

- 拉取源代码
- 执行`install.ps1`脚本安装。

## 使用Nuget安装

模板已经发布到[`nuget`](https://www.nuget.org/packages/Perigon.template)上，请根据你的项目版本下载对应的模板。

```pwsh
dotnet new --install Perigon.templates --preview
```

## 创建项目

```pwsh
dotnet new perigon-webapi  
```

or

```pwsh
dotnet new perigon-webapi -n <projectname>
```

## 数据库

模板默认支持`PostgreSQL`和`SqlServer`，你可以在`AppHost`项目的`appsettings.json`中进行选择。


## 数据迁移

可直接运行`scripts\EFMigrations.ps1`脚本生成迁移内容，程序在启动时会执行迁移。

```pwsh
cd scripts
.\EFMigrations.ps1
```

该脚本提供一个参数，指定迁移生成时的名称，如`.\EFMigrations.ps1  Init` .

## 运行项目

直接运行`AppHost`项目即可。


默认管理账号：`admin@default.com/Perigon.2026`.
