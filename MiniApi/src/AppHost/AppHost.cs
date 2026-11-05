using AppHost;
using Perigon.AspNetCore.Constants;

var builder = DistributedApplication.CreateBuilder(args);
var aspireSetting = AppSettingsHelper.LoadAspireSettings(builder.Configuration);
var isTesting = builder.Configuration["ASPIRE_ENVIRONMENT"]?.ToLowerInvariant() == "testing";

IResourceBuilder<IResourceWithConnectionString>? cache = null;
// if you have exist resource, you can set connection string here, without create container
//var database = builder.AddConnectionString(AppConst.Default);

#region infrastructure
var defaultName = isTesting ? "MyProjectName_test" : "MyProjectName_dev";
var devPassword = builder.AddParameter(
    "dev-password",
    value: aspireSetting.DevPassword,
    secret: true
);

// use local container
var database = builder
    .AddPostgres(name: "Database", password: devPassword, port: aspireSetting.DbPort)
    .WithImageTag("18.1-alpine")
    .WithDataVolume()
    .AddDatabase(AppConst.Default, databaseName: defaultName);

if (aspireSetting.UsesRedis)
{
    cache = builder
        .AddRedis("Cache", password: devPassword, port: aspireSetting.CachePort)
        .WithImageTag("8.2-alpine")
        .WithDataVolume()
        .WithPersistence(interval: TimeSpan.FromMinutes(5));
}

#endregion

#region services
var serviceGroup = builder.AddGroup("Services", "Globe");
var apiService = builder.AddProject<Projects.ApiService>("ApiService")
    .WithEnvironment("Components__Cache", aspireSetting.CacheType)
    .WithParentRelationship(serviceGroup);

apiService.WithReference(database).WaitFor(database);
// The Angular project is optional when the template is created with frontType=None.
if (File.Exists(Path.Combine(builder.AppHostDirectory, "..", "ClientApp", "WebApp", "package.json")))
{
    var webApp = builder.AddJavaScriptApp("frontend", "../ClientApp/WebApp", "start")
        .WithPnpm()
        .WithUrl("http://localhost:4200")
        .WaitFor(apiService)
        .WithParentRelationship(serviceGroup);
    webApp.WithReference(database).WaitFor(database);
}
if (cache != null)
{
    apiService.WithReference(cache);
}
# endregion

builder.Build().Run();
