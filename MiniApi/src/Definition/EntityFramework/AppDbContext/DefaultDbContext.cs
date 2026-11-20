using Entity;
using Perigon.PostgreSQL;
using Perigon.PostgreSQL.Attributes;
using Perigon.PostgreSQL.Options;

[assembly: UseSnakeCaseNaming]

namespace EntityFramework.AppDbContext;

#pragma warning disable CS8618 // Initialized by the DbSet source generator.
public partial class DefaultDbContext(DbContextOptions<DefaultDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}
#pragma warning restore CS8618
