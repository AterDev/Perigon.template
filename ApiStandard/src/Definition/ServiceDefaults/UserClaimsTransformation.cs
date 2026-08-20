using System.Security.Claims;
using Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Perigon.AspNetCore.Services;

namespace ServiceDefaults;

public class LocalUserClaimsTransformation(DefaultDbContext context, CacheService cache)
    : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return principal;
        }

        var tenant = await ResolveTenantAsync(identity);
        if (tenant is not null)
        {
            ReplaceTenantClaims(identity, tenant);
        }

        var userIdentity = FindUserIdentity(principal);
        if (!string.IsNullOrWhiteSpace(userIdentity)
            && !identity.HasClaim(claim => claim.Type == ClaimTypes.Role))
        {
            var cacheKey = $"local-user-info:{userIdentity}";

            // the sample of get user roles from local system, you can replace this with your own implementation, such as query from database or call external service.
            var roles = await cache.GetOrCreateAsync(
                cacheKey,
                cancellation => new ValueTask<string[]>(
                    QueryRolesFromLocalSystemAsync(userIdentity, cancellation)
                )
            );

            identity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        return principal;
    }

    private async Task<Tenant?> ResolveTenantAsync(ClaimsIdentity identity)
    {
        var tenantIdValue = identity.FindFirst(CustomClaimTypes.TenantId)?.Value;
        if (tenantIdValue is not null)
        {
            return Guid.TryParse(tenantIdValue, out var tenantId) && tenantId != Guid.Empty
                ? await context.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tenantId)
                : null;
        }

        var tenant = await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Domain == DefaultDbContextSeeding.DefaultTenantDomain);

        return tenant
            ?? throw new InvalidOperationException(
                "The default tenant is not initialized. Apply the database migrations and seed data before authenticating users."
            );
    }

    private static void ReplaceTenantClaims(ClaimsIdentity identity, Tenant tenant)
    {
        foreach (var claimType in new[]
        {
            CustomClaimTypes.TenantId,
            CustomClaimTypes.TenantType,
            CustomClaimTypes.TenantName,
        })
        {
            foreach (var claim in identity.FindAll(claimType).ToArray())
            {
                identity.RemoveClaim(claim);
            }
        }

        identity.AddClaim(new Claim(CustomClaimTypes.TenantId, tenant.Id.ToString()));
        identity.AddClaim(new Claim(CustomClaimTypes.TenantType, tenant.Type.ToString()));
        identity.AddClaim(new Claim(CustomClaimTypes.TenantName, tenant.Name));
    }

    /// <summary>
    /// get user identity from claims, use 
    /// </summary>
    private static string? FindUserIdentity(ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue(ClaimTypes.Name)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task<string[]> QueryRolesFromLocalSystemAsync(
        string userIdentity,
        CancellationToken cancellation
    )
    {
        await Task.CompletedTask;

        _ = context;
        _ = cancellation;

        return userIdentity.Equals("admin", StringComparison.OrdinalIgnoreCase)
            ? [WebConst.User, WebConst.AdminUser]
            : [WebConst.User];
    }

}
