using Perigon.PostgreSQL;
using Perigon.PostgreSQL.Options;
using System.Diagnostics.CodeAnalysis;

namespace Data;

public partial class DefaultDbContext(DbContextOptions<DefaultDbContext> options)
    : DbContext(options)
{
    public new DbSet<TEntity> Set<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
        where TEntity : class
    {
        return base.Set<TEntity>();
    }

    // Add DbSet<TEntity> properties here as entities are added.
}
