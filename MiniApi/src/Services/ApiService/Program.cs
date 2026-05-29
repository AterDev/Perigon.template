WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
builder.AddServiceDefaults();

// 框架依赖服务:options, cache, dbContext
builder.AddFrameworkServices();

// Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
builder.AddMiddlewareServices();
builder.Services.AddOpenApi();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(
        WebConst.User,
        policy =>
        {
            policy.RequireRole(WebConst.User);
        }
    );

// Managers, auto register by source generator
builder.Services.AddManagers();

// Optional module extensions, kept as a compatibility hook
builder.AddModules();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

app.MapEndpointGroups();

// 使用中间件
app.UseMiddlewareServices();

await app.RunAsync();
