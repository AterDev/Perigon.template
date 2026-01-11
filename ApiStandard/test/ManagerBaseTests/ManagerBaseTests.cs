using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Entity;
using EntityFramework.AppDbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Perigon.AspNetCore.Abstraction;
using Perigon.AspNetCore.Models;
using Share.Implement;
using TUnit;
using TUnit.Core;

namespace ManagerBaseTests;

public class ManagerBaseTests
{
    [Test]
    public async Task UpdateAsync_updates_fields_and_timestamp_when_requested()
    {
        await using var connection = CreateOpenConnection();
        var tenantId = Guid.NewGuid();
        Guid entityId;
        DateTimeOffset originalUpdated;

        await using (var arrangeContext = CreateContext(connection))
        {
            var entity = new TestEntity { Name = "Old", Description = "Desc", TenantId = tenantId };
            arrangeContext.TestEntities.Add(entity);
            await arrangeContext.SaveChangesAsync();
            entityId = entity.Id;
            originalUpdated = entity.UpdatedTime;
        }

        await using (var manager = CreateManager(connection, tenantId))
        {
            var dto = new TestUpdateDto { Name = "New", Description = "Updated" };
            var affected = await manager.UpdateAsync(entityId, dto, updateTime: true);
            AssertTrue(affected == 1, "Expected one row to be updated.");
        }

        await using (var assertContext = CreateContext(connection))
        {
            var entity = await assertContext.TestEntities.FindAsync(entityId);
            AssertNotNull(entity, "Updated entity should exist.");
            AssertEqual("New", entity!.Name, "Name should be updated.");
            AssertEqual("Updated", entity.Description, "Description should be updated.");
            AssertTrue(entity.UpdatedTime > originalUpdated, "UpdatedTime should be refreshed when updateTime is true.");
        }
    }

    [Test]
    public async Task UpdateAsync_does_not_change_timestamp_when_disabled()
    {
        await using var connection = CreateOpenConnection();
        var tenantId = Guid.NewGuid();
        Guid entityId;
        DateTimeOffset originalUpdated;

        await using (var arrangeContext = CreateContext(connection))
        {
            var entity = new TestEntity { Name = "Old", Description = "Desc", TenantId = tenantId };
            arrangeContext.TestEntities.Add(entity);
            await arrangeContext.SaveChangesAsync();
            entityId = entity.Id;
            originalUpdated = entity.UpdatedTime;
        }

        await using (var manager = CreateManager(connection, tenantId))
        {
            var dto = new TestUpdateDto { Description = "Updated" };
            var affected = await manager.UpdateAsync(entityId, dto, updateTime: false);
            AssertTrue(affected == 1, "Expected one row to be updated.");
        }

        await using (var assertContext = CreateContext(connection))
        {
            var entity = await assertContext.TestEntities.FindAsync(entityId);
            AssertNotNull(entity, "Updated entity should exist.");
            AssertEqual("Old", entity!.Name, "Name should remain unchanged when not provided.");
            AssertEqual("Updated", entity.Description, "Description should be updated.");
            AssertEqual(originalUpdated, entity.UpdatedTime, "UpdatedTime should remain unchanged when updateTime is false.");
        }
    }

    [Test]
    public async Task DeleteOrUpdateAsync_soft_delete_sets_flag_and_respects_query_filter()
    {
        await using var connection = CreateOpenConnection();
        var tenantId = Guid.NewGuid();
        Guid entityId;

        await using (var arrangeContext = CreateContext(connection))
        {
            var entity = new TestEntity { Name = "Deletable", Description = "Desc", TenantId = tenantId };
            arrangeContext.TestEntities.Add(entity);
            await arrangeContext.SaveChangesAsync();
            entityId = entity.Id;
        }

        await using (var manager = CreateManager(connection, tenantId))
        {
            var affected = await manager.DeleteAsync(new[] { entityId }, softDelete: true);
            AssertTrue(affected == 1, "Expected one row to be soft deleted.");
        }

        await using (var assertContext = CreateContext(connection))
        {
            var entity = await assertContext.TestEntities.IgnoreQueryFilters().FirstAsync(e => e.Id == entityId);
            AssertTrue(entity.IsDeleted, "Entity should be marked as deleted.");

            var visible = await assertContext.TestEntities.FirstOrDefaultAsync(e => e.Id == entityId);
            AssertTrue(visible is null, "Entity should be filtered out by the global query filter after soft delete.");
        }
    }

    [Test]
    public async Task ListAsync_applies_tenant_filter_when_multi_tenant()
    {
        await using var connection = CreateOpenConnection();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using (var arrangeContext = CreateContext(connection))
        {
            arrangeContext.TestEntities.AddRange(
                new TestEntity { Name = "TenantA", Description = "A", TenantId = tenantA },
                new TestEntity { Name = "TenantB", Description = "B", TenantId = tenantB }
            );
            await arrangeContext.SaveChangesAsync();
        }

        await using (var manager = CreateManager(connection, tenantA, isMultiTenant: true))
        {
            var list = await manager.ListAsync();
            AssertEqual(1, list.Count, "Only the current tenant's data should be returned.");
            AssertEqual(tenantA, list[0].TenantId, "Returned data should match user tenant.");
        }
    }

    [Test]
    public async Task PageListAsync_orders_and_pages_results()
    {
        await using var connection = CreateOpenConnection();
        var tenantId = Guid.NewGuid();

        await using (var arrangeContext = CreateContext(connection))
        {
            var entities = Enumerable.Range(1, 10)
                .Select(i => new TestEntity
                {
                    Name = $"Item{i:D2}",
                    Description = "Paged",
                    TenantId = tenantId
                });
            await arrangeContext.TestEntities.AddRangeAsync(entities);
            await arrangeContext.SaveChangesAsync();
        }

        await using (var manager = CreateManager(connection, tenantId))
        {
            var filter = new FilterBase
            {
                PageIndex = 2,
                PageSize = 3,
                OrderBy = new Dictionary<string, bool> { { nameof(TestEntity.Name), true } }
            };

            var page = await manager.PageAsync(filter);
            AssertEqual(10, page.Count, "Total count should reflect all records.");
            AssertEqual(3, page.Data.Count, "Second page should contain three items.");

            var names = page.Data.Select(d => d.Name).ToList();
            var expected = new List<string> { "Item04", "Item05", "Item06" };
            AssertSequenceEqual(expected, names, "Paging should return the correct slice in order.");
        }
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    private static TestDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;
        var context = new TestDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static TestManager CreateManager(SqliteConnection connection, Guid tenantId, bool isMultiTenant = false)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        var context = CreateContext(connection);
        return new TestManager(context, new TestUserContext(tenantId), loggerFactory.CreateLogger<TestManager>(), isMultiTenant);
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertNotNull(object? value, string message)
    {
        if (value is null)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertEqual<T>(T expected, T? actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(message + $" Expected: {expected}, Actual: {actual}");
        }
    }

    private static void AssertSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message)
    {
        if (!expected.SequenceEqual(actual))
        {
            throw new InvalidOperationException(message + $" Expected: [{string.Join(",", expected)}], Actual: [{string.Join(",", actual)}]");
        }
    }
}

internal sealed class TestManager : ManagerBase<TestDbContext, TestEntity>
{
    public TestManager(TestDbContext dbContext, IUserContext userContext, ILogger<TestManager> logger, bool isMultiTenant)
        : base(dbContext, userContext, logger, isMultiTenant)
    {
    }

    public Task<int> UpdateAsync(Guid id, TestUpdateDto dto, bool updateTime = true) => base.UpdateAsync(id, dto, updateTime);

    public Task<int> DeleteAsync(IEnumerable<Guid> ids, bool softDelete = true, CancellationToken cancellationToken = default) =>
        DeleteOrUpdateAsync(ids, softDelete, cancellationToken);

    public Task<List<TestItemDto>> ListAsync(Expression<Func<TestEntity, bool>>? predicate = null, CancellationToken cancellationToken = default) =>
        base.ListAsync<TestItemDto>(predicate, cancellationToken);

    public Task<PageList<TestItemDto>> PageAsync(FilterBase filter, CancellationToken cancellationToken = default) =>
        base.PageListAsync<FilterBase, TestItemDto>(filter, cancellationToken);

    public override Task<bool> HasPermissionAsync(Guid id) => Task.FromResult(true);
}

internal sealed class TestDbContext(DbContextOptions<TestDbContext> options) : ContextBase(options)
{
    public DbSet<TestEntity> TestEntities { get; set; } = null!;
}

internal class TestEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

internal class TestUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

internal class TestItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TenantId { get; set; }
}

internal sealed class TestUserContext(Guid tenantId) : IUserContext
{
    public Guid UserId => Guid.Empty;
    public Guid? GroupId => null;
    public Guid TenantId { get; } = tenantId;
    public string? UserName => "tester";
    public string? Email => "tester@example.com";
    public bool IsAdmin => true;
    public string? CurrentRole => "tester";
    public IReadOnlyList<string>? Roles => new[] { "tester" };
    public HttpContext? HttpContext { get; set; }
    public bool IsRole(string roleName) => true;
}
