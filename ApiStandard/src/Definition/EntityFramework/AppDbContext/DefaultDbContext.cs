


namespace EntityFramework.AppDbContext;

/// <summary>
/// default data access for main business
/// </summary>
/// <param name="options"></param>
public partial class DefaultDbContext(DbContextOptions<DefaultDbContext> options)
    : ContextBase(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

    }
}
