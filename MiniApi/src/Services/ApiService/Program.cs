using ApiService.Worker;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
builder.AddServiceDefaults();
builder.Services.AddSingleton<Share.Serialization.IAppJsonSerializer, ApiService.Serialization.AppJsonSerializer>();

// 框架依赖服务:options, cache, dbContext
builder.AddFrameworkServices();

// Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
builder.AddMiddlewareServices();
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
    options.AddSchemaTransformer<ApiService.OpenApi.EnumSchemaTransformer>();
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApiService.ApiJsonSerializerContext.Default);
});

// Managers, auto register by source generator
builder.Services.AddManagers();

// Modules, auto register by source generator
builder.AddModules();


builder.Services.AddHostedService<DatabaseInitService>();


WebApplication app = builder.Build();

app.MapDefaultEndpoints();

app.MapEndpointGroups();

// 使用中间件
app.UseMiddlewareServices();

await app.RunAsync();
