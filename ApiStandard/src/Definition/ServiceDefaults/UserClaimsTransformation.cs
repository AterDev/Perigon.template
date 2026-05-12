using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
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

        if (identity.HasClaim(claim => claim.Type == ClaimTypes.Role))
        {
            return principal;
        }

        var userIdentity = FindUserIdentity(principal);
        if (string.IsNullOrWhiteSpace(userIdentity))
        {
            return principal;
        }

        var cacheKey = $"local-user-roles:{userIdentity}";

        // the sample of get user roles from local system, you can replace it with your own implementation, such as query from database or call external service.
        var roles = await cache.GetOrCreateAsync(
            cacheKey,
            cancellation => new ValueTask<string[]>(
                QueryRolesFromLocalSystemAsync(userIdentity, cancellation)
            )
        );

        identity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return principal;
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