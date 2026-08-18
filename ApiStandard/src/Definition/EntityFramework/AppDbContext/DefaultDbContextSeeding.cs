using Perigon.AspNetCore.Constants;

namespace EntityFramework.AppDbContext;

public static class DefaultDbContextSeeding
{
    public static DbContextOptionsBuilder UseDefaultDbContextSeeding(
        this DbContextOptionsBuilder optionsBuilder
    )
    {
        return optionsBuilder
            .UseSeeding((context, _) => SeedDefaultTenant(context))
            .UseAsyncSeeding((context, _, cancellationToken) =>
                SeedDefaultTenantAsync(context, cancellationToken)
            );
    }

    private static void SeedDefaultTenant(DbContext context)
    {
        const string domain = "default.com";

        var tenants = context.Set<Tenant>();
        if (tenants.IgnoreQueryFilters().Any(t => t.Domain == domain))
        {
            return;
        }

        // Tenant is the global tenant catalog root. ContextBase intentionally ignores
        // Tenant.TenantId, so this seed must not assign a tenant id to the entity.
        tenants.Add(new Tenant
        {
            Domain = domain,
            Name = AppConst.Default,
            Description = "This is default tenant, created by system.",
        });

        context.SaveChanges();
    }

    private static async Task SeedDefaultTenantAsync(
        DbContext context,
        CancellationToken cancellationToken
    )
    {
        const string domain = "default.com";

        var tenants = context.Set<Tenant>();
        if (
            await tenants
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Domain == domain, cancellationToken)
        )
        {
            return;
        }

        // Keep the async path equivalent to the synchronous path used by EF tooling.
        tenants.Add(new Tenant
        {
            Domain = domain,
            Name = AppConst.Default,
            Description = "This is default tenant, created by system.",
        });

        await context.SaveChangesAsync(cancellationToken);
    }
}
