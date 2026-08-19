using AppHost;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Kubernetes.Resources;
using Perigon.AspNetCore.Constants;
using YamlDotNet.Serialization;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddKubernetesEnvironment("k8s");
var aspireSetting = AppSettingsHelper.LoadAspireSettings(builder.Configuration);
var isTesting = builder.Configuration["ASPIRE_ENVIRONMENT"]?.ToLowerInvariant() == "testing";
var isMultiTenant = builder.Configuration["Components:IsMultiTenant"] ?? "false";

IResourceBuilder<IResourceWithConnectionString>? database = null;
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
_ = aspireSetting.DatabaseType?.ToLowerInvariant() switch
{
    "postgresql" => database = builder
        .AddPostgres(name: "Database", password: devPassword, port: aspireSetting.DbPort)
        .WithImageTag("18.1-alpine")
        .WithDataVolume()
        .AddDatabase(AppConst.Default, databaseName: defaultName),
    "sqlserver" => database = builder
        .AddSqlServer(name: "Database", password: devPassword, port: aspireSetting.DbPort)
        .WithImageTag("2025-latest")
        .WithDataVolume()
        .AddDatabase(AppConst.Default, databaseName: defaultName),
    _ => null,

};
if (aspireSetting.UsesRedis)
{
    cache = builder
        .AddRedis("Cache", password: devPassword, port: aspireSetting.CachePort)
        .WithImageTag("8.2-alpine")
        .WithDataVolume()
        .WithPersistence(interval: TimeSpan.FromMinutes(5));
}

devPassword.WithParentRelationship(infrastructureGroup);
database?.WithParentRelationship(infrastructureGroup);
cache?.WithParentRelationship(infrastructureGroup);

database?.WithResetSchemaCommand();

#endregion

#region services
var serviceGroup = builder.AddGroup("Services", "Globe");
var adminService = builder.AddProject<Projects.AdminService>("AdminService")
    .WithEnvironment("Components__Cache", aspireSetting.CacheType)
    .WithEnvironment("Components__Database", aspireSetting.DatabaseType)
    .WithEnvironment("Components__IsMultiTenant", isMultiTenant)
    .WithParentRelationship(serviceGroup);

var apiService = builder.AddProject<Projects.ApiService>("ApiService")
    .WithReference(adminService)
    .WithEnvironment("Components__Cache", aspireSetting.CacheType)
    .WithEnvironment("Components__Database", aspireSetting.DatabaseType)
    .WithEnvironment("Components__IsMultiTenant", isMultiTenant)
    .WithParentRelationship(serviceGroup);

// run frontend app, you should install npm packages first
//var webApp = builder.AddJavaScriptApp("frontend", "../ClientApp/WebApp", "start")
//    .WithPnpm()
//    .WithUrl("http://localhost:4200")
//    .WaitFor(adminService)
//    .WithParentRelationship(serviceGroup);

if (database != null)
{
    apiService.WithReference(database);
    adminService.WithReference(database);
}
if (cache != null)
{
    apiService.WithReference(cache);
    adminService.WithReference(cache);
}

var apiMigrations = apiService
    .AddEFMigrations(
        "ApiService-Migrations",
        "EntityFramework.AppDbContext.DefaultDbContext"
    )
    .WithEnvironment("Components__Database", aspireSetting.DatabaseType)
    .WithEnvironment("Components__IsMultiTenant", isMultiTenant)
    .WithMigrationsProject("..\\Definition\\EntityFramework\\EntityFramework.csproj")
    .RunDatabaseUpdateOnStart()
    .PublishAsMigrationBundle(publishContainer: true)
    .PublishAsKubernetesService(resource =>
    {
        if (resource.Workload is Deployment deployment)
        {
            // Migration bundles are finite workloads. Replace Aspire's default Deployment
            // with a Job so Kubernetes does not recreate a completed migration container.
            deployment.PodTemplate.Spec.RestartPolicy = "OnFailure";
            resource.Workload = new MigrationJob
            {
                Metadata = deployment.Metadata,
                Spec = new MigrationJobSpec
                {
                    Template = deployment.Spec.Template
                }
            };
        }
    })
    .WithParentRelationship(serviceGroup);

if (database != null)
{
    apiMigrations.WithReference(database).WaitFor(database);
}

apiService.WaitForCompletion(apiMigrations);
adminService.WaitForCompletion(apiMigrations);
# endregion

builder.Build().Run();

[YamlSerializable]
internal sealed class MigrationJob : Workload
{
    public MigrationJob()
        : base("batch/v1", "Job")
    {
    }

    [YamlMember(Alias = "spec")]
    public MigrationJobSpec Spec { get; set; } = new();

    public override PodTemplateSpecV1 PodTemplate => Spec.Template;
}

[YamlSerializable]
internal sealed class MigrationJobSpec
{
    [YamlMember(Alias = "template")]
    public PodTemplateSpecV1 Template { get; set; } = new();

    [YamlMember(Alias = "backoffLimit")]
    public int BackoffLimit { get; set; } = 1;
}
