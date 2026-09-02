using System.Security.Claims;
using System.ComponentModel.DataAnnotations.Schema;
using Entity;
using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration;
using Perigon.AspNetCore.Abstraction;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Perigon.AspNetCore.Constants;
using Perigon.AspNetCore.Options;
using ServiceDefaults;
using Share.Implement;
using Share.Services;
using TUnit.Assertions;

namespace UnitTest;

public sealed class TenantFrameworkTests
{
    [Test]
    [Category("Unit")]
    public async Task MigrationModel_WhenTenantEntityHasOrdinaryIndex_AddsTenantIdToGeneratedOperation()
    {
        await using var context = CreateConventionContext();
        var differ = context.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
        var designTimeModel = context.GetService<IDesignTimeModel>().Model;

        var operations = differ.GetDifferences(null!, designTimeModel.GetRelationalModel());
        var index = operations
            .OfType<CreateIndexOperation>()
            .Single(operation => operation.Table == "TenantFrameworkTestEntities");

        await Assert.That(index.Columns.Length).IsEqualTo(2);
        await Assert.That(index.Columns[0]).IsEqualTo(nameof(ITenantEntityBase.TenantId));
        await Assert.That(index.Columns[1]).IsEqualTo(nameof(TenantFrameworkTestEntity.Code));
        await Assert.That(index.IsUnique).IsTrue();
        await Assert.That(index.Filter).IsEqualTo("\"IsDeleted\" = 0");
    }

    [Test]
    [Category("Unit")]
    public async Task MigrationModel_WhenTenantIndexIsAlreadyConfigured_DoesNotDuplicateTenantId()
    {
        await using var context = CreateConventionContext();
        var differ = context.GetInfrastructure().GetRequiredService<IMigrationsModelDiffer>();
        var designTimeModel = context.GetService<IDesignTimeModel>().Model;

        var operations = differ.GetDifferences(null!, designTimeModel.GetRelationalModel());
        var indexes = operations
            .OfType<CreateIndexOperation>()
            .Where(operation => operation.Table == "TenantFrameworkTestEntitiesWithIndex")
            .ToArray();

        await Assert.That(indexes.Length).IsEqualTo(1);
        await Assert.That(indexes[0].Columns).IsEquivalentTo(
            [nameof(ITenantEntityBase.TenantId), nameof(TenantFrameworkTestEntityWithIndex.Code)]
        );
    }

    [Test]
    [Category("Unit")]
    public async Task DefaultDbContextSeeding_WhenDatabaseIsCreated_CreatesDefaultTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var optionsBuilder = new DbContextOptionsBuilder<DefaultDbContext>();
        optionsBuilder.UseSqlite(connection).UseDefaultDbContextSeeding();

        await using var context = new DefaultDbContext(optionsBuilder.Options);
        await context.Database.EnsureCreatedAsync();

        var tenant = await context.Tenants
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(t => t.Domain == DefaultDbContextSeeding.DefaultTenantDomain);

        await Assert.That(tenant).IsNotNull();
        await Assert.That(tenant!.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(tenant.Name).IsEqualTo(AppConst.Default);
        await Assert.That(string.IsNullOrWhiteSpace(tenant.DbConnectionString)).IsFalse();
        await Assert.That(tenant.AnalysisConnectionString)
            .IsEqualTo(tenant.DbConnectionString);
    }

    [Test]
    [Category("Unit")]
    public async Task DefaultDbContextSeeding_WhenAnalysisConnectionIsConfigured_StoresIt()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        const string analysisConnectionString = "Host=analysis-db;Database=analysis";
        var optionsBuilder = new DbContextOptionsBuilder<DefaultDbContext>();
        optionsBuilder
            .UseSqlite(connection)
            .UseDefaultDbContextSeeding(analysisConnectionString);

        await using var context = new DefaultDbContext(optionsBuilder.Options);
        await context.Database.EnsureCreatedAsync();

        var tenant = await context.Tenants
            .SingleAsync(t => t.Domain == DefaultDbContextSeeding.DefaultTenantDomain);

        await Assert.That(tenant.AnalysisConnectionString)
            .IsEqualTo(analysisConnectionString);
    }

    [Test]
    [Category("Unit")]
    public async Task AnalysisDbContext_WhenSavingTenantCatalog_AllowsWriteAndQuery()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AnalysisDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new AnalysisDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var tenant = new Tenant
        {
            Domain = "analysis-test.example",
            Name = "Analysis Test",
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var loadedTenant = await context.Tenants.SingleAsync(item => item.Id == tenant.Id);

        await Assert.That(loadedTenant.Id).IsEqualTo(tenant.Id);
        await Assert.That(loadedTenant.Name).IsEqualTo("Analysis Test");
    }

    [Test]
    [Category("Unit")]
    public async Task TenantContext_WhenTenantIdIsMissing_CanSaveAndQueryTenantCatalog()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<DefaultDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new DefaultDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var tenant = new Tenant
        {
            Domain = "catalog-test.example",
            Name = "Catalog Test",
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var loadedTenant = await context.Tenants.SingleAsync(item => item.Id == tenant.Id);

        await Assert.That(loadedTenant.Id).IsEqualTo(tenant.Id);
        await Assert.That(loadedTenant.TenantId).IsEqualTo(Guid.Empty);
    }

    [Test]
    [Category("Unit")]
    public async Task ClaimsTransformation_WhenTenantClaimIsMissing_DoesNotAddDefaultTenantClaims()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var optionsBuilder = new DbContextOptionsBuilder<DefaultDbContext>();
        optionsBuilder.UseSqlite(connection).UseDefaultDbContextSeeding();
        await using var context = new DefaultDbContext(optionsBuilder.Options);
        await context.Database.EnsureCreatedAsync();

        var defaultTenant = await context.Tenants
            .SingleAsync(t => t.Domain == DefaultDbContextSeeding.DefaultTenantDomain);
        await Assert.That(defaultTenant).IsNotNull();
        using var serviceProvider = CreateCacheServiceProvider();
        var cache = CreateCacheService(serviceProvider);
        var tenantService = new TenantService(
            context,
            cache,
            NullLogger<TenantService>.Instance
        );
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, Guid.CreateVersion7().ToString())],
                authenticationType: "test"
            )
        );

        var transformed = await new UserClaimsTransformation(tenantService, cache)
            .TransformAsync(principal);

        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantId) is null).IsTrue();
        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantType) is null).IsTrue();
        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantName) is null).IsTrue();
        await Assert.That(transformed.FindFirst(ClaimTypes.Role) is null).IsTrue();
    }

    [Test]
    [Category("Unit")]
    public async Task ClaimsTransformation_WhenTenantClaimIsValid_AddsTenantAndRoleClaims()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateSqliteContext(connection);
        await context.Database.EnsureCreatedAsync();

        var tenant = new Tenant
        {
            Domain = "claims-tenant.example",
            Name = "Claims Tenant",
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        using var serviceProvider = CreateCacheServiceProvider();
        var cache = CreateCacheService(serviceProvider);
        var tenantService = new TenantService(
            context,
            cache,
            NullLogger<TenantService>.Instance
        );
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(CustomClaimTypes.TenantId, tenant.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, Guid.CreateVersion7().ToString()),
                ],
                authenticationType: "test"
            )
        );

        var transformed = await new UserClaimsTransformation(tenantService, cache)
            .TransformAsync(principal);

        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantId)?.Value)
            .IsEqualTo(tenant.Id.ToString());
        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantType)?.Value)
            .IsEqualTo(tenant.Type.ToString());
        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantName)?.Value)
            .IsEqualTo(tenant.Name);
        await Assert.That(transformed.FindFirst(ClaimTypes.Role)?.Value)
            .IsEqualTo(WebConst.User);
    }

    [Test]
    [Category("Unit")]
    public async Task ClaimsTransformation_WhenTenantClaimIsUnknown_DoesNotAddDefaultTenantClaims()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateSqliteContext(connection);
        await context.Database.EnsureCreatedAsync();

        using var serviceProvider = CreateCacheServiceProvider();
        var cache = CreateCacheService(serviceProvider);
        var tenantService = new TenantService(
            context,
            cache,
            NullLogger<TenantService>.Instance
        );
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(CustomClaimTypes.TenantId, Guid.CreateVersion7().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, Guid.CreateVersion7().ToString()),
                ],
                authenticationType: "test"
            )
        );

        var transformed = await new UserClaimsTransformation(tenantService, cache)
            .TransformAsync(principal);

        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantType) is null).IsTrue();
        await Assert.That(transformed.FindFirst(CustomClaimTypes.TenantName) is null).IsTrue();
        await Assert.That(transformed.FindFirst(ClaimTypes.Role) is null).IsTrue();
    }

    [Test]
    [Category("Unit")]
    public async Task UserContext_WhenTenantClaimIsPresent_ExposesTenantId()
    {
        var tenantId = Guid.CreateVersion7();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    [new Claim(CustomClaimTypes.TenantId, tenantId.ToString())],
                    authenticationType: "test"
                )
            )
        };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        var userContext = new UserContext(accessor);

        await Assert.That(userContext.TenantId).IsEqualTo(tenantId);
    }

    [Test]
    [Category("Unit")]
    public async Task TenantService_WhenTenantIsCached_DoesNotReadCatalogAgain()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateSqliteContext(connection);
        await context.Database.EnsureCreatedAsync();

        var tenant = new Tenant
        {
            Domain = "cached-tenant.example",
            Name = "Cached Tenant",
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        using var serviceProvider = CreateCacheServiceProvider();
        var tenantService = new TenantService(
            context,
            CreateCacheService(serviceProvider),
            NullLogger<TenantService>.Instance
        );

        var loadedTenant = await tenantService.GetByIdAsync(tenant.Id);
        await Assert.That(loadedTenant).IsNotNull();

        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync();

        var cachedTenant = await tenantService.GetByIdAsync(tenant.Id);
        await Assert.That(cachedTenant).IsNotNull();
        await Assert.That(cachedTenant!.Name).IsEqualTo("Cached Tenant");
    }

    [Test]
    [Category("Unit")]
    public async Task TenantService_WhenTenantIsDisabled_PreservesMetadataForConnectionSelection()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateSqliteContext(connection);
        await context.Database.EnsureCreatedAsync();

        var tenant = new Tenant
        {
            Domain = "disabled-cache-tenant.example",
            Name = "Disabled Cache Tenant",
            Disabled = true,
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        using var serviceProvider = CreateCacheServiceProvider();
        var tenantService = new TenantService(
            context,
            CreateCacheService(serviceProvider),
            NullLogger<TenantService>.Instance
        );

        var loadedTenant = await tenantService.GetByIdAsync(tenant.Id);

        await Assert.That(loadedTenant).IsNotNull();
        await Assert.That(loadedTenant!.Disabled).IsTrue();
    }

    [Test]
    [Category("Unit")]
    public async Task TenantService_RefreshCacheAsync_ReplacesCachedTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateSqliteContext(connection);
        await context.Database.EnsureCreatedAsync();

        var tenant = new Tenant
        {
            Domain = "refresh-tenant.example",
            Name = "Before Refresh",
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        using var serviceProvider = CreateCacheServiceProvider();
        var tenantService = new TenantService(
            context,
            CreateCacheService(serviceProvider),
            NullLogger<TenantService>.Instance
        );

        _ = await tenantService.GetByIdAsync(tenant.Id);
        await context.Tenants
            .Where(item => item.Id == tenant.Id)
            .ExecuteUpdateAsync(updater => updater
                .SetProperty(item => item.Name, "After Refresh")
                .SetProperty(item => item.Disabled, true));

        var refreshedTenant = await tenantService.RefreshCacheAsync(tenant.Id);
        await Assert.That(refreshedTenant).IsNotNull();
        await Assert.That(refreshedTenant!.Name).IsEqualTo("After Refresh");
        await Assert.That(refreshedTenant.Disabled).IsTrue();

        var cachedTenant = await tenantService.GetByIdAsync(tenant.Id);
        await Assert.That(cachedTenant).IsNotNull();
        await Assert.That(cachedTenant!.Name).IsEqualTo("After Refresh");
        await Assert.That(cachedTenant.Disabled).IsTrue();
    }

    [Test]
    [Category("Unit")]
    public async Task AppDbFactory_WhenTenantCacheIsMissing_LoadsAndCachesTenant()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var catalogContext = CreateSqliteContext(connection);
        await catalogContext.Database.EnsureCreatedAsync();

        var tenant = new Tenant
        {
            Domain = "connection-cache-tenant.example",
            Name = "Connection Cache Tenant",
            DbConnectionString = "Host=tenant-db;Database=tenant",
            AnalysisConnectionString = "Host=tenant-analysis;Database=analysis",
        };
        catalogContext.Tenants.Add(tenant);
        await catalogContext.SaveChangesAsync();

        using var cacheProvider = CreateCacheServiceProvider();
        var cache = CreateCacheService(cacheProvider);
        using var tenantProvider = CreateTenantResolverProvider(connection, cache);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"ConnectionStrings:{AppConst.Default}"] =
                        "Host=default-db;Database=default",
                    [$"ConnectionStrings:{AppConst.Analysis}"] =
                        "Host=default-analysis;Database=analysis",
                }
            )
            .Build();
        var factory = new AppDbFactory(
            Options.Create(new ComponentOption { Database = DatabaseType.PostgreSql }),
            configuration,
            tenantProvider.GetRequiredService<IServiceScopeFactory>()
        );

        await using var tenantContext = factory.CreateDbContext(tenant.Id);

        await Assert.That(tenantContext.Database.GetConnectionString())
            .IsEqualTo(tenant.DbConnectionString);

        await catalogContext.Tenants.ExecuteDeleteAsync();

        await using var analysisContext = factory.CreateAnalysisDbContext(tenant.Id);
        await Assert.That(analysisContext.Database.GetConnectionString())
            .IsEqualTo(tenant.AnalysisConnectionString);
        await Assert.That(cache.GetMemory<Tenant>(TenantService.GetCacheKey(tenant.Id)))
            .IsNotNull();
    }

    [Test]
    [Category("Unit")]
    public async Task AppDbFactory_WhenTenantIsUnknown_ThrowsInsteadOfUsingDefaultConnection()
    {
        var exception = (Exception?)null;

        try
        {
            await using var context = CreateAppDbFactory().CreateDbContext(Guid.CreateVersion7());
        }
        catch (Exception caught)
        {
            exception = caught;
        }

        await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        await Assert.That(exception!.Message).Contains("tenant catalog");
    }

    [Test]
    [Category("Unit")]
    public async Task AppDbFactory_WhenTenantIdIsEmpty_ThrowsInsteadOfUsingDefaultConnection()
    {
        var exception = (Exception?)null;

        try
        {
            await using var context = CreateAppDbFactory().CreateDbContext(Guid.Empty);
        }
        catch (Exception caught)
        {
            exception = caught;
        }

        await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        await Assert.That(exception!.Message).Contains("non-empty TenantId");
    }

    [Test]
    [Category("Unit")]
    public async Task SingleTenantContext_WhenInsertingAndQuerying_UsesTheBoundTenant()
    {
        var tenantId = Guid.CreateVersion7();
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateConventionContext(connection, tenantId);
        await context.Database.EnsureCreatedAsync();

        var entity = new TenantFrameworkTestEntity { Code = "single-tenant" };
        context.Add(entity);
        await context.SaveChangesAsync();

        await Assert.That(entity.TenantId).IsEqualTo(tenantId);
        var rows = await context.Set<TenantFrameworkTestEntity>()
            .Where(item => item.Code == "single-tenant")
            .ToListAsync();

        await Assert.That(rows.Count).IsEqualTo(1);
        await Assert.That(rows[0].TenantId).IsEqualTo(tenantId);
    }

    [Test]
    [Category("Unit")]
    public async Task GlobalTenantFilter_WhenQueryOmitsTenantId_ReturnsOnlyCurrentTenantRows()
    {
        var tenantA = Guid.CreateVersion7();
        var tenantB = Guid.CreateVersion7();
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var context = CreateConventionContext(connection, tenantA))
        {
            await context.Database.EnsureCreatedAsync();
            context.AddRange(
                new TenantFrameworkTestEntity { Code = "active" },
                new TenantFrameworkTestEntity { Code = "deleted", IsDeleted = true }
            );
            await context.SaveChangesAsync();
        }

        await using (var context = CreateConventionContext(connection, tenantB))
        {
            context.Add(new TenantFrameworkTestEntity { Code = "other-tenant" });
            await context.SaveChangesAsync();

            var rows = await context.Set<TenantFrameworkTestEntity>().ToListAsync();
            await Assert.That(rows.Count).IsEqualTo(1);
            await Assert.That(rows[0].Code).IsEqualTo("other-tenant");
        }

        await using (var context = CreateConventionContext(connection, tenantA))
        {
            var rows = await context.Set<TenantFrameworkTestEntity>()
                .Where(entity => entity.Code != "ignored-by-test")
                .ToListAsync();

            await Assert.That(rows.Count).IsEqualTo(1);
            await Assert.That(rows[0].Code).IsEqualTo("active");

            var rowsIncludingDeleted = await context
                .Set<TenantFrameworkTestEntity>()
                .IgnoreQueryFilters([ContextBase.SoftDeletionFilterName])
                .ToListAsync();

            await Assert.That(rowsIncludingDeleted.Count).IsEqualTo(2);
            await Assert.That(rowsIncludingDeleted.Select(entity => entity.Code))
                .IsEquivalentTo(["active", "deleted"]);

            var sql = context.Set<TenantFrameworkTestEntity>()
                .Where(entity => entity.Code == "active")
                .ToQueryString();
            await Assert.That(sql).Contains(nameof(ITenantEntityBase.TenantId));
        }
    }

    [Test]
    [Category("Unit")]
    public async Task TenantManager_WhenTenantIdIsMissing_DoesNotRequireTenantContext()
    {
        var userContext = new TestUserContext();

        var manager = new TenantManager(
            CreateAppDbFactory(),
            userContext,
            NullLogger<TenantManager>.Instance
        );

        await Assert.That(manager).IsNotNull();
    }

    [Test]
    [Category("Unit")]
    public async Task TenantScopedManager_WhenTenantIdIsMissing_ThrowsImmediately()
    {
        var userContext = new TestUserContext();
        Exception? exception = null;

        try
        {
            _ = new TenantFrameworkTestEntityManager(
                CreateAppDbFactory(),
                userContext,
                NullLogger<TenantFrameworkTestEntityManager>.Instance
            );
        }
        catch (Exception caught)
        {
            exception = caught;
        }

        await Assert.That(exception).IsTypeOf<InvalidOperationException>();
        await Assert.That(exception!.Message).Contains("TenantId");
    }

    private static ConventionTestDbContext CreateConventionContext()
    {
        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        return new ConventionTestDbContext(options);
    }

    private static ConventionTestDbContext CreateConventionContext(
        SqliteConnection connection,
        Guid tenantId
    )
    {
        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ConventionTestDbContext(options, tenantId);
    }

    private static DefaultDbContext CreateSqliteContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<DefaultDbContext>()
            .UseSqlite(connection)
            .Options;
        return new DefaultDbContext(options);
    }

    private static ServiceProvider CreateCacheServiceProvider()
    {
        var services = new ServiceCollection();
        services.Configure<CacheOption>(_ => { });
        services.Configure<ComponentOption>(_ => { });
        services.AddMemoryCache();
        services.AddHybridCache();
        return services.BuildServiceProvider();
    }

    private static Perigon.AspNetCore.Services.CacheService CreateCacheService(
        IServiceProvider serviceProvider
    )
    {
        return new Perigon.AspNetCore.Services.CacheService(
            serviceProvider.GetRequiredService<HybridCache>(),
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<IOptions<CacheOption>>(),
            serviceProvider.GetRequiredService<IOptions<ComponentOption>>()
        );
    }

    private static ServiceProvider CreateTenantResolverProvider(
        SqliteConnection connection,
        Perigon.AspNetCore.Services.CacheService cache
    )
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(cache);
        services.AddScoped<DefaultDbContext>(_ => CreateSqliteContext(connection));
        services.AddScoped<TenantService>();
        services.AddScoped<ITenantResolver>(serviceProvider =>
            serviceProvider.GetRequiredService<TenantService>()
        );
        return services.BuildServiceProvider();
    }

    private static AppDbFactory CreateAppDbFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [$"ConnectionStrings:{AppConst.Default}"] =
                        "Host=localhost;Database=tenant-framework-tests;Username=test;Password=test",
                }
            )
            .Build();

        return new AppDbFactory(
            Options.Create(new ComponentOption { Database = DatabaseType.PostgreSql }),
            configuration,
            EmptyTenantScopeFactory
        );
    }

    private static IServiceScopeFactory CreateEmptyTenantScopeFactory()
    {
        var services = new ServiceCollection();
        services.AddScoped<ITenantResolver, EmptyTenantResolver>();
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private static readonly IServiceScopeFactory EmptyTenantScopeFactory =
        CreateEmptyTenantScopeFactory();

    private sealed class TestUserContext : IUserContext
    {
        public Guid UserId => Guid.Empty;
        public Guid? GroupId => null;
        public Guid TenantId { get; set; }
        public string? TenantType { get; set; }
        public string? UserName => null;
        public string? Email => null;
        public bool IsAdmin => false;
        public string? CurrentRole => null;
        public IReadOnlyList<string>? Roles => [];
        public IReadOnlyList<Guid> RoleIds => [];
        public HttpContext? HttpContext { get; set; }

        public bool IsRole(string roleName) => false;
    }

    private sealed class TenantManager(
        AppDbFactory dbContextFactory,
        IUserContext userContext,
        Microsoft.Extensions.Logging.ILogger logger
    ) : ManagerBase<DefaultDbContext, Tenant>(dbContextFactory, userContext, logger)
    {
        public override Task<bool> HasPermissionAsync(Guid id) => Task.FromResult(true);
    }

    private sealed class TenantFrameworkTestEntityManager(
        AppDbFactory dbContextFactory,
        IUserContext userContext,
        Microsoft.Extensions.Logging.ILogger logger
    ) : ManagerBase<DefaultDbContext, TenantFrameworkTestEntity>(
        dbContextFactory,
        userContext,
        logger
    )
    {
        public override Task<bool> HasPermissionAsync(Guid id) => Task.FromResult(true);
    }

    private sealed class EmptyTenantResolver : ITenantResolver
    {
        public Tenant? GetById(Guid tenantId) => null;
    }
}

[Index(nameof(Code), IsUnique = true)]
[Table("TenantFrameworkTestEntities")]
internal sealed class TenantFrameworkTestEntity : EntityBase
{
    public required string Code { get; set; }
}

internal sealed class ConventionTestDbContext : ContextBase
{
    public ConventionTestDbContext(DbContextOptions<ConventionTestDbContext> options, Guid? tenantId = null)
        : base(options)
    {
        SetTenantId(tenantId);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<TenantFrameworkTestEntity>();
        builder.Entity<TenantFrameworkTestEntityWithIndex>();
    }
}

[Index(nameof(TenantId), nameof(Code), IsUnique = true)]
[Table("TenantFrameworkTestEntitiesWithIndex")]
internal sealed class TenantFrameworkTestEntityWithIndex : EntityBase
{
    public required string Code { get; set; }
}
