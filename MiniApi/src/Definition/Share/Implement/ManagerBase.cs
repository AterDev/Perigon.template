using EntityFramework.AppDbContext;
using EntityFramework.AppDbFactory;
using Perigon.PostgreSQL;
using Perigon.PostgreSQL.Update;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Share.Implement;

/// <summary>
/// Base manager class without database access.
/// </summary>
public abstract class ManagerBase(ILogger logger)
{
    protected readonly ILogger _logger = logger;
}

public abstract class ManagerBase<TDbContext>(TDbContext dbContext, ILogger logger)
    : ManagerBase(logger)
    where TDbContext : DefaultDbContext
{
    protected readonly TDbContext _dbContext = dbContext;
}

/// <summary>
/// Generic manager base class for Perigon.PostgreSQL entity operations.
/// </summary>
public abstract class ManagerBase<
    TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>(
        TDbContext dbContext, ILogger logger)
    : ManagerBase<TDbContext>(dbContext, logger), IDisposable
    where TDbContext : DefaultDbContext
    where TEntity : class, IEntityBase, new()
{
    private readonly bool _ownsContext;
    protected readonly IUserContext _userContext = default!;
    protected IQueryable<TEntity> Queryable { get; set; } = dbContext.Set<TEntity>().Where(e => !e.IsDeleted);
    protected readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();

    protected ManagerBase(AppDbFactory dbContextFactory, IUserContext userContext, ILogger logger)
        : this(
            dbContextFactory.CreateDbContext() as TDbContext
                ?? throw new InvalidOperationException($"Unsupported context type {typeof(TDbContext).Name}."),
            logger)
    {
        _userContext = userContext;
        _ownsContext = true;
    }

    public async Task<TEntity?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.Id == id && !e.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected async Task<TDto?> FindAsync<TDto>(
        Expression<Func<TEntity, TDto>> selector,
        Expression<Func<TEntity, bool>>? whereExp = null,
        CancellationToken cancellationToken = default)
        where TDto : class, new()
    {
        return await ApplyFilter(whereExp)
            .Select(selector)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(q => q.Id == id && !q.IsDeleted)
            .AnyAsync(cancellationToken);
    }

    protected async Task<List<TDto>> ListAsync<TDto>(
        Expression<Func<TEntity, TDto>> selector,
        Expression<Func<TEntity, bool>>? whereExp = null,
        CancellationToken cancellationToken = default)
        where TDto : class, new()
    {
        return await ApplyFilter(whereExp)
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public async Task<PageList<TItem>> PageListAsync<TFilter, TItem>(
        TFilter filter,
        Expression<Func<TEntity, TItem>> selector,
        CancellationToken cancellationToken = default)
        where TFilter : FilterBase
        where TItem : class, new()
    {
        Queryable = Queryable is IOrderedQueryable<TEntity>
            ? Queryable
            : Queryable.OrderByDescending(t => t.CreatedTime);

        var count = await Queryable.CountAsync(cancellationToken);
        var data = await Queryable
            .Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20))
            .Take(filter.PageSize.GetValueOrDefault(20))
            .Select(selector)
            .ToListAsync(cancellationToken);

        ResetQuery();
        return new PageList<TItem>
        {
            Count = count,
            Data = data,
            PageIndex = filter.PageIndex.GetValueOrDefault(1),
        };
    }

    protected async Task<PageList<TItem>> PageListAsync<TFilter, TItem>(
        TFilter filter,
        Func<TEntity, TItem> selector,
        CancellationToken cancellationToken = default)
        where TFilter : FilterBase
        where TItem : class
    {
        Queryable = Queryable is IOrderedQueryable<TEntity>
            ? Queryable
            : Queryable.OrderByDescending(entity => entity.CreatedTime);

        var count = await Queryable.CountAsync(cancellationToken);
        var entities = await Queryable
            .Skip((filter.PageIndex.GetValueOrDefault(1) - 1) * filter.PageSize.GetValueOrDefault(20))
            .Take(filter.PageSize.GetValueOrDefault(20))
            .ToListAsync(cancellationToken);

        ResetQuery();
        return new PageList<TItem>
        {
            Count = count,
            Data = entities.Select(selector).ToList(),
            PageIndex = filter.PageIndex.GetValueOrDefault(1),
        };
    }

    protected async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedTime = DateTimeOffset.UtcNow;
        return await _dbSet.InsertAsync(entity, cancellationToken);
    }

    protected async Task BulkInsertAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        var rows = entities as TEntity[] ?? entities.ToArray();
        foreach (var entity in rows)
        {
            entity.UpdatedTime = DateTimeOffset.UtcNow;
        }

        await _dbSet.BulkInsertAsync(rows, cancellationToken: cancellationToken);
    }

    protected async Task<int> UpdateAsync(
        Guid id,
        Expression<Func<UpdateSetters<TEntity>, UpdateSetters<TEntity>>> setters,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(entity => entity.Id == id && !entity.IsDeleted)
            .ExecuteUpdateAsync(setters, cancellationToken: cancellationToken);
    }

    protected async Task<int> DeleteOrUpdateAsync(
        IEnumerable<Guid> ids,
        bool softDelete = true,
        CancellationToken cancellationToken = default)
    {
        var idsList = ids.ToList();
        if (idsList.Count == 0)
        {
            return 0;
        }

        return softDelete
            ? await _dbSet
                .Where(entity => idsList.Contains(entity.Id))
                .ExecuteUpdateAsync(
                    setter => setter
                        .Set(entity => entity.IsDeleted, true)
                        .Set(entity => entity.UpdatedTime, DateTimeOffset.UtcNow),
                    cancellationToken: cancellationToken)
            : await _dbSet
                .Where(entity => idsList.Contains(entity.Id))
                .ExecuteDeleteAsync(cancellationToken: cancellationToken);
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.TransactionAsync(operation, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction operation failed");
            throw;
        }
    }

    protected Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(_ => operation(), cancellationToken);
    }

    protected async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.TransactionAsync(operation, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction operation failed");
            throw;
        }
    }

    protected Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        return ExecuteInTransactionAsync(_ => operation(), cancellationToken);
    }

    public abstract Task<bool> HasPermissionAsync(Guid id);

    public void Dispose()
    {
        if (_ownsContext)
        {
            _dbContext.Dispose();
        }
    }

    protected void ResetQuery()
    {
        Queryable = _dbSet.Where(e => !e.IsDeleted);
    }

    protected IQueryable<TEntity> ApplyFilter(Expression<Func<TEntity, bool>>? whereExp)
    {
        var query = _dbSet.Where(e => !e.IsDeleted);
        return whereExp is null ? query : query.Where(whereExp);
    }
}
