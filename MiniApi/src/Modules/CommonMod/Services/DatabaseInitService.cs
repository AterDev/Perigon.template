using Entity;
using EntityFramework.AppDbContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CommonMod.Managers;

namespace CommonMod.Services;

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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager>();
            await dbContext.EnsureCreatedAsync(stoppingToken);

            if (await userManager.FindByUserNameAsync("user", stoppingToken) is null)
            {
                await userManager.AddAsync(
                    new User
                    {
                        UserName = "user",
                        Password = "123456",
                        NickName = "Demo User",
                    },
                    stoppingToken);
                logger.LogInformation("Default user 'user' created.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed");
        }
    }
}
