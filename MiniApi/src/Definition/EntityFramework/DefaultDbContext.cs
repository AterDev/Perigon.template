using Perigon.PostgreSQL;
using System.Diagnostics.CodeAnalysis;

namespace Data;

public partial class DefaultDbContext(string connectionString)
    : DbContext(builder => builder.UsePostgres(connectionString))
{
    public new DbSet<TEntity> Set<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
        where TEntity : class
    {
        return base.Set<TEntity>();
    }

    // Add DbSet<TEntity> properties here as entities are added.
}
