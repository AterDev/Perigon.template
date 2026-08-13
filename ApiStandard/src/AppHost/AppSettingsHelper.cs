using Microsoft.Extensions.Configuration;

namespace AppHost;

/// <summary>
/// Stores Aspire configuration settings parsed from appsettings.
/// </summary>
public class AspireSetting
{
    public string DatabaseType { get; set; } = "PostgreSQL";
    public string CacheType { get; set; } = "Memory";
    public string DevPassword { get; set; } =
        "Perigon." + DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy");

    public int DbPort { get; set; } = 15432;
    public int CachePort { get; set; } = 16379;

    public bool UsesRedis => CacheType.Equals("Redis", StringComparison.OrdinalIgnoreCase)
        || CacheType.Equals("Hybrid", StringComparison.OrdinalIgnoreCase);
}

public static class AppSettingsHelper
{
    /// <summary>
    /// Loads Aspire configuration from appsettings and parses required values.
    /// </summary>
    /// <param name="environment">The environment name, e.g. "Development".</param>
    /// <returns>AspireSetting instance with parsed values.</returns>
    public static AspireSetting LoadAspireSettings(IConfiguration config)
    {
        var components = config.GetSection("Components");
        var databaseType = components["Database"] ?? "PostgreSQL";
        var cacheType = components["Cache"] ?? "Memory";

        if (!cacheType.Equals("Memory", StringComparison.OrdinalIgnoreCase)
            && !cacheType.Equals("Redis", StringComparison.OrdinalIgnoreCase)
            && !cacheType.Equals("Hybrid", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Unsupported Components:Cache value '{cacheType}'. Use Memory, Redis, or Hybrid."
            );
        }

        return new AspireSetting
        {
            DatabaseType = databaseType,
            CacheType = cacheType,
            DbPort = databaseType.ToLowerInvariant() switch
            {
                "postgresql" => 15432,
                "sqlserver" => 11433,
                _ => 13306,
            },
        };
    }
}
