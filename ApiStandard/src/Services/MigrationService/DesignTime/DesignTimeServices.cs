using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MigrationService.DesignTime;

public class DesignTimeServices : IDesignTimeServices
{
    private static string LogPath => Path.Combine(AppContext.BaseDirectory ?? Directory.GetCurrentDirectory(), "designtime_invoked.log");

    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        try
        {
            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] DesignTimeServices.ConfigureDesignTimeServices invoked in MigrationService startup project.\n");
        }
        catch { }

        Console.WriteLine("DesignTimeServices.ConfigureDesignTimeServices invoked (MigrationService)");

        try
        {
            // Build a temporary provider from the current service collection to resolve the original IMigrationsModelDiffer
            IServiceProvider tempProvider = null!;
            try
            {
                // Do not validate scopes during build here; we'll create a scope when resolving scoped services
                tempProvider = services.BuildServiceProvider(validateScopes: false);
                File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] Built temporary provider to resolve inner services.\n");
            }
            catch (Exception ex)
            {
                File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] Failed to build temporary provider: {ex}\n");
                return;
            }

            IMigrationsModelDiffer? inner = null;
            try
            {
                // Create scope to resolve scoped services safely
                using var scope = tempProvider.CreateScope();
                inner = scope.ServiceProvider.GetService<IMigrationsModelDiffer>();
            }
            catch (Exception ex)
            {
                File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] Exception while resolving IMigrationsModelDiffer in scope: {ex}\n");
                return;
            }

            if (inner == null)
            {
                File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] Could not resolve IMigrationsModelDiffer from temporary provider scope.\n");
                return;
            }

            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] Resolved IMigrationsModelDiffer instance from temporary provider scope.\n");

            // Register proxy wrapper that uses the already resolved inner instance to avoid recursive resolution
            services.AddSingleton<IMigrationsModelDiffer>(sp =>
            {
                var proxy = MigrationsModelDifferProxy.Create<IMigrationsModelDiffer>(inner);
                File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] Returning proxy instance from registration factory.\n");
                return proxy;
            });

            File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] Registered migrations differ proxy successfully.\n");
        }
        catch (Exception ex)
        {
            try { File.AppendAllText(LogPath, $"[{DateTime.UtcNow:O}] Exception while registering proxy: {ex}\n"); } catch { }
            Console.WriteLine("DesignTimeServices: exception while registering proxy: " + ex.Message);
        }
    }
}
