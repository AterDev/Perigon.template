using Microsoft.EntityFrameworkCore;
using Perigon.AspNetCore.Abstraction;

namespace EntityFramework.UnitTests;

public class ExtensionsTests
{
    [Test]
    public async Task PartialUpdateAsync_ShouldReturnOne_WhenEntityExists()
    {
        await using var db = CreateDbContext();
        var entity = new TestEntity
        {
            Name = "before",
            Description = "desc"
        };

        db.Entities.Add(entity);
        await db.SaveChangesAsync();

        var result = await db.PartialUpdateAsync<TestEntity, TestUpdateDto>(
            entity.Id,
            new TestUpdateDto { Name = "after" });

        await Assert.That(result).IsEqualTo(1);
    }

    [Test]
    public async Task PartialUpdateAsync_ShouldUpdateMatchedProperty_WhenDtoHasValue()
    {
        var databaseName = $"partial-update-{Guid.NewGuid()}";
        await using (var arrangeDb = CreateDbContext(databaseName))
        {
            arrangeDb.Entities.Add(new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "before",
                Description = "desc"
            });
            await arrangeDb.SaveChangesAsync();
        }

        Guid entityId;
        await using (var actDb = CreateDbContext(databaseName))
        {
            var seeded = await actDb.Entities.SingleAsync();
            entityId = seeded.Id;

            await actDb.PartialUpdateAsync<TestEntity, TestUpdateDto>(
                entityId,
                new TestUpdateDto { Name = "after" });
        }

        await using var assertDb = CreateDbContext(databaseName);
        var updated = await assertDb.Entities.SingleAsync(x => x.Id == entityId);

        await Assert.That(updated.Name).IsEqualTo("after");
    }

    [Test]
    public async Task PartialUpdateAsync_ShouldIgnoreNullDtoProperty_WhenPartialUpdating()
    {
        var databaseName = $"partial-update-{Guid.NewGuid()}";
        await using (var arrangeDb = CreateDbContext(databaseName))
        {
            arrangeDb.Entities.Add(new TestEntity
            {
                Id = Guid.NewGuid(),
                Name = "before",
                Description = "keep"
            });
            await arrangeDb.SaveChangesAsync();
        }

        Guid entityId;
        await using (var actDb = CreateDbContext(databaseName))
        {
            var seeded = await actDb.Entities.SingleAsync();
            entityId = seeded.Id;

            await actDb.PartialUpdateAsync<TestEntity, TestUpdateDto>(
                entityId,
                new TestUpdateDto
                {
                    Name = "after",
                    Description = null
                });
        }

        await using var assertDb = CreateDbContext(databaseName);
        var updated = await assertDb.Entities.SingleAsync(x => x.Id == entityId);

        await Assert.That(updated.Description).IsEqualTo("keep");
    }

    [Test]
    public async Task PartialUpdateAsync_ShouldReturnZero_WhenEntityDoesNotExist()
    {
        await using var db = CreateDbContext();

        var result = await db.PartialUpdateAsync<TestEntity, TestUpdateDto>(
            Guid.NewGuid(),
            new TestUpdateDto { Name = "after" });

        await Assert.That(result).IsEqualTo(0);
    }

    private static TestDbContext CreateDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName ?? $"partial-update-{Guid.NewGuid()}")
            .Options;

        return new TestDbContext(options);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities => Set<TestEntity>();
    }

    private sealed class TestEntity : IEntityBase
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
        public bool IsDeleted { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    private sealed record TestUpdateDto
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
    }
}
