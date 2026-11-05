using Entity;
using Perigon.PostgreSQL;
using Perigon.PostgreSQL.Options;
using System.Diagnostics.CodeAnalysis;

namespace EntityFramework.AppDbContext;

public partial class DefaultDbContext(DbContextOptions<DefaultDbContext> options)
    : DbContext(options)
{
    public new DbSet<TEntity> Set<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
        where TEntity : class
    {
        return base.Set<TEntity>();
    }

    public DbSet<User> Users => Set<User>();
}
