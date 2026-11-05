using System.Diagnostics;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Perigon.AspNetCore.Models;
using Share.Implement;

namespace ServiceDefaults.Middleware;

/// <summary>Writes a consistent error contract for Problem Details and empty error responses.</summary>
public sealed class ErrorResultProblemDetailsWriter(
    IOptions<JsonOptions> jsonOptions,
    IOptions<ProblemDetailsOptions> problemOptions,
    Localizer localizer) : IProblemDetailsWriter
{
    public bool CanWrite(ProblemDetailsContext context) => true;

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var source = context.ProblemDetails;
        source.Status ??= context.HttpContext.Response.StatusCode;
        _ = TypedResults.Problem(source); // Apply the framework's standard status defaults.

        var options = jsonOptions.Value.SerializerOptions;
        var traceIdKey = options.PropertyNamingPolicy?.ConvertName("traceId") ?? "traceId";
        source.Extensions[traceIdKey] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        problemOptions.Value.CustomizeProblemDetails?.Invoke(context);
        source = context.ProblemDetails;
        source.Extensions.TryGetValue(RestEndpointBase.LocalizationArgumentsExtensionName, out var value);

        var result = new ErrorResult
        {
            Type = source.Type,
            Title = source.Title,
            Status = source.Status,
            Detail = source.Detail is { } detail ? localizer.Get(detail, value as object[] ?? []) : null,
            Instance = source.Instance,
            Errors = source is HttpValidationProblemDetails validation
                ? validation.Errors
                : new Dictionary<string, string[]>(),
            TraceId = source.Extensions.TryGetValue(traceIdKey, out var traceId) ? traceId as string : null,
        };
        foreach (var extension in source.Extensions)
        {
            if (extension.Key != traceIdKey && extension.Key != RestEndpointBase.LocalizationArgumentsExtensionName)
            {
                result.Extensions[extension.Key] = extension.Value;
            }
        }

        var typeInfo = (JsonTypeInfo<ErrorResult>)options.GetTypeInfo(typeof(ErrorResult));
        return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
            result, typeInfo, contentType: "application/problem+json"));
    }
}
