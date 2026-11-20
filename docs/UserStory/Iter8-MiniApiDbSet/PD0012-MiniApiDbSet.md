# PD0012 MiniApi DbSet 自动属性

MiniApi 引用 Perigon.PostgreSQL 0.3.4，通过 `public DbSet<User> Users { get; set; }` 声明集合。生成器生成强类型赋值代码，集合首次使用时才加载模型。上下文每次从池中租用时重新绑定属性，旧租约的集合仍会失效。

泛型 Manager 调用包的公开 `Set<TEntity>()` 并传递公共属性裁剪要求。模板显式保留 snake_case 数据库命名。ApiStandard 使用 EF Core 自己的集合初始化机制。
