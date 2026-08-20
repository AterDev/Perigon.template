using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFramework.DesignTime;

public static class EntityFrameworkDesignTimeServices
{
    public static void Configure(IServiceCollection services)
    {
        Console.WriteLine("EntityFrameworkDesignTimeServices.Configure invoked");
        try
        {
            IServiceProvider tempProvider = services.BuildServiceProvider(validateScopes: false);
            using var scope = tempProvider.CreateScope();
            IMigrationsModelDiffer? inner = scope.ServiceProvider.GetService<IMigrationsModelDiffer>();
            services.AddSingleton<IMigrationsModelDiffer>(sp =>
            {
                var proxy = MigrationsModelDifferProxy.Create<IMigrationsModelDiffer>(inner!);
                return proxy;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("DesignTimeServices: exception while registering proxy: " + ex.Message);
        }
    }
}
