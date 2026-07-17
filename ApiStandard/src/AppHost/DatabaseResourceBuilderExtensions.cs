using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Aspire.Hosting;

internal static class DatabaseResourceBuilderExtensions
{
    public static IResourceBuilder<IResourceWithConnectionString> WithResetSchemaCommand(
        this IResourceBuilder<IResourceWithConnectionString> builder
    )
    {
        builder.WithCommand(
            name: "reset-schema",
            displayName: "Reset database schema",
            executeCommand: async context =>
            {
                var connectionString = await builder.Resource.GetConnectionStringAsync()
                    ?? throw new InvalidOperationException(
                        "Database connection string is unavailable."
                    );

                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync(context.CancellationToken);

                await using var command = connection.CreateCommand();
                command.CommandText = """
                    DROP SCHEMA public CASCADE;
                    CREATE SCHEMA public;
                    """;
                await command.ExecuteNonQueryAsync(context.CancellationToken);

                return CommandResults.Success("Database schema was reset.");
            },
            commandOptions: new CommandOptions
            {
                Description =
                    "Drops and recreates the public schema, including all tables and migration history.",
                ConfirmationMessage =
                    "删除并重建数据库结构？所有表和数据都将被删除，此操作不可撤销。",
                IconName = "Delete",
                IconVariant = IconVariant.Filled,
                UpdateState = context =>
                    context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
                        ? ResourceCommandState.Enabled
                        : ResourceCommandState.Disabled,
            }
        );

        return builder;
    }
}
