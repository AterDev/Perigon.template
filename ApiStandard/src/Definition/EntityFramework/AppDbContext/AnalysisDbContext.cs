namespace EntityFramework.AppDbContext;

/// <summary>
/// Analysis-related read/write data access.
/// </summary>
/// <param name="options">The options to be used by the context. Must not be null.</param>
public class AnalysisDbContext : ContextBase
{
    public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
