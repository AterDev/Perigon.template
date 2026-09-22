using EntityFramework;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Perigon.AspNetCore.Utils;
using TUnit.Assertions;

namespace UnitTest;

public sealed class ImprovementRegressionTests
{
    [Test]
    [Category("Unit")]
    public async Task PartialUpdateAsync_WhenDtoContainsProtectedFields_UpdatesBusinessFieldOnly()
    {
        var tenantId = Guid.CreateVersion7();
        var otherTenantId = Guid.CreateVersion7();
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ConventionTestDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new ConventionTestDbContext(options, tenantId);
        await context.Database.EnsureCreatedAsync();

        var entity = new TenantFrameworkTestEntity { Code = "before" };
        context.Add(entity);
        await context.SaveChangesAsync();
        var originalId = entity.Id;
        var originalCreatedTime = entity.CreatedTime;

        var otherEntity = new TenantFrameworkTestEntity { Code = "other-tenant" };
        await using (var otherContext = new ConventionTestDbContext(options, otherTenantId))
        {
            otherContext.Add(otherEntity);
            await otherContext.SaveChangesAsync();
        }

        context.ChangeTracker.Clear();
        var result = await context.PartialUpdateAsync<TenantFrameworkTestEntity, PartialUpdateDto>(
            originalId,
            new PartialUpdateDto
            {
                Id = Guid.CreateVersion7(),
                TenantId = otherTenantId,
                CreatedTime = DateTimeOffset.UtcNow.AddYears(10),
                Code = "after",
            },
            updateUpdatedTime: false,
            tenantId: tenantId
        );

        await Assert.That(result).IsEqualTo(1);

        var crossTenantResult = await context.PartialUpdateAsync<TenantFrameworkTestEntity, PartialUpdateDto>(
            otherEntity.Id,
            new PartialUpdateDto { Code = "must-not-change" },
            updateUpdatedTime: false,
            tenantId: tenantId
        );
        await Assert.That(crossTenantResult).IsEqualTo(0);

        var rows = await context.Set<TenantFrameworkTestEntity>()
            .IgnoreQueryFilters()
            .ToListAsync();
        await Assert.That(rows.Count).IsEqualTo(2);
        await Assert.That(rows.Single(row => row.Id == otherEntity.Id).Code).IsEqualTo("other-tenant");
        var persisted = rows.Single(row => row.Id == originalId);
        await Assert.That(persisted.Code).IsEqualTo("after");
        await Assert.That(persisted.Id).IsEqualTo(originalId);
        await Assert.That(persisted.TenantId).IsEqualTo(tenantId);
        await Assert.That(persisted.CreatedTime).IsEqualTo(originalCreatedTime);
    }

    [Test]
    [Category("Unit")]
    public async Task Between_WhenPropertyIsNested_UsesTheCompleteMemberAccess()
    {
        var items = new[]
        {
            new NestedRangeItem { Details = new NestedRangeDetails { Score = 9 } },
            new NestedRangeItem { Details = new NestedRangeDetails { Score = 10 } },
            new NestedRangeItem { Details = new NestedRangeDetails { Score = 15 } },
            new NestedRangeItem { Details = new NestedRangeDetails { Score = 20 } },
            new NestedRangeItem { Details = new NestedRangeDetails { Score = 21 } },
        };

        var matchingItems = items.AsQueryable()
            .Between(item => item.Details.Score, 10, 20)
            .ToList();

        await Assert.That(matchingItems.Select(item => item.Details.Score))
            .IsEquivalentTo([10, 15, 20]);

        var directMatches = items.Select(item => item.Details).AsQueryable()
            .Between(details => details.Score, 10, 20)
            .Select(details => details.Score)
            .ToList();
        await Assert.That(directMatches).IsEquivalentTo([10, 15, 20]);
    }

    private sealed class PartialUpdateDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string? Code { get; set; }
    }

    private sealed class NestedRangeItem
    {
        public required NestedRangeDetails Details { get; init; }
    }

    private sealed class NestedRangeDetails
    {
        public int Score { get; init; }
    }
}
