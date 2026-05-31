using AppHost;
using Perigon.AspNetCore.Constants;

var builder = DistributedApplication.CreateBuilder(args);
var aspireSetting = AppSettingsHelper.LoadAspireSettings(builder.Configuration);
var isTesting = builder.Configuration["ASPIRE_ENVIRONMENT"]?.ToLowerInvariant() == "testing";

IResourceBuilder<IResourceWithConnectionString>? cache = null;

// if you have exist resource, you can set connection string here, without create container
// database = builder.AddConnectionString(AppConst.Default);
// nats = builder.AddConnectionString("mq");
// qdrant = builder.AddConnectionString("qdrant");

#region infrastructure
var defaultName = isTesting ? "MyProjectName_test" : "MyProjectName_dev";
var devPassword = builder.AddParameter(
    "dev-password",
    value: aspireSetting.DevPassword,
    secret: true
);

var infrastructureGroup = builder.AddGroup("Infrastructure", "Cloud");
var database = builder
    .AddPostgres(name: "Database", password: devPassword, port: aspireSetting.DbPort)
    .WithImageTag("18.1-alpine")
    .WithDataVolume()
    .AddDatabase(AppConst.Default, databaseName: defaultName);
_ = aspireSetting.CacheType?.ToLowerInvariant() switch
{
    "memory" => null,
    _ => cache = builder
        .AddRedis("Cache", password: devPassword, port: aspireSetting.CachePort)
        .WithImageTag("8.2-alpine")
        .WithDataVolume()
        .WithPersistence(interval: TimeSpan.FromMinutes(5)),
};

devPassword.WithParentRelationship(infrastructureGroup);
database.WithParentRelationship(infrastructureGroup);
cache?.WithParentRelationship(infrastructureGroup);

#endregion

#region services
var serviceGroup = builder.AddGroup("Services", "Globe");
var apiService = builder.AddProject<Projects.ApiService>("ApiService")
    .WithParentRelationship(serviceGroup);

// run frontend app, you should install npm packages first
//var webApp = builder.AddJavaScriptApp("frontend", "../ClientApp/WebApp")
//    .WithPnpm()
//    .WithUrl("http://localhost:4200")
//    .WaitFor(apiService)
//    .WithParentRelationship(serviceGroup);

apiService.WithReference(database).WaitFor(database);
if (cache != null)
{
    apiService.WithReference(cache);
}
# endregion

builder.Build().Run();
