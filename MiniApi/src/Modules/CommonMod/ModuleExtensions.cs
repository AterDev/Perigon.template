using Microsoft.Extensions.Hosting;
namespace CommonMod;

[DisplayName("AuthorName::ModuleDisplayName")]
[Description("Module Description")]
public static class ModuleExtensions
{
    /// <summary>
    /// Registers module services.
    /// </summary>
    public static IHostApplicationBuilder AddCommonMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    // The module services registration
    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        return builder;
    }

}
