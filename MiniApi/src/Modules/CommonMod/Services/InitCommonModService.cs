using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommonMod.Services;

/// <summary>
/// module init host service
/// </summary>
public class InitCommonModService(
    ILogger<InitCommonModService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // using var scope = serviceProvider.CreateScope();

        try
        {
            logger.LogInformation("CommonMod initializing...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CommonMod initialization failed");
            return;
        }
        finally
        {
        }
    }
}
