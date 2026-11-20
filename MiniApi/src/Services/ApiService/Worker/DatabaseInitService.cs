using CommonMod;
using EntityFramework.AppDbContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApiService.Worker;

/// <summary>
/// 初始化数据库结构并写入演示用户
/// </summary>
public sealed class DatabaseInitService(
    IServiceProvider serviceProvider,
    ILogger<DatabaseInitService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            await dbContext.EnsureCreatedAsync(stoppingToken);

            if (await InitModule.InitializeAsync(dbContext, stoppingToken))
            {
                logger.LogInformation("Default user 'user' created.");
            }

            foreach (var initialize in scope.ServiceProvider.GetServices<Func<DefaultDbContext, CancellationToken, Task>>())
            {
                await initialize(dbContext, stoppingToken);
            }
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Database initialization failed");
        }
    }
}
