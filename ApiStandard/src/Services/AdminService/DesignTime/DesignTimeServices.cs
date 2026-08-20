using EntityFramework.DesignTime;
using Microsoft.EntityFrameworkCore.Design;

namespace AdminService.DesignTime;

// EF Core discovers IDesignTimeServices from the startup assembly. Keep this adapter here;
// the actual migration customization lives with the DbContext in EntityFramework.
public sealed class DesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        EntityFrameworkDesignTimeServices.Configure(services);
    }
}
