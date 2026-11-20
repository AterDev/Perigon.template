using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceDefaults.Middleware;
using Share;
using Share.Exceptions;
using Perigon.AspNetCore.Models;

namespace ApiTest;

public sealed class ProblemMiddlewareTests
{
    [Test]
    public async Task BusinessException_WritesProblemDetails()
    {
        await AssertProblemAsync(
            new BusinessException("BusinessError", StatusCodes.Status422UnprocessableEntity),
            StatusCodes.Status422UnprocessableEntity,
            "BusinessError");
    }

    [Test]
    public async Task BadHttpRequestException_WritesProblemDetails()
    {
        await AssertProblemAsync(
            new BadHttpRequestException("Invalid request", StatusCodes.Status400BadRequest),
            StatusCodes.Status400BadRequest,
            "Invalid request");
    }

    [Test]
    public async Task UnhandledException_WritesProblemDetails()
    {
        await AssertProblemAsync(
            new InvalidOperationException("Unexpected failure"),
            StatusCodes.Status500InternalServerError,
            "Unexpected failure");
    }

    [Test]
    public async Task ResponseAlreadyStarted_RethrowsOriginalException()
    {
        using var services = CreateServices();
        var expected = new InvalidOperationException("late failure");
        var middleware = CreateMiddleware(services, _ => throw expected);
        var features = new FeatureCollection();
        features.Set<IHttpResponseFeature>(new StartedResponseFeature());
        var context = new DefaultHttpContext(features)
        {
            RequestServices = services,
            TraceIdentifier = "started-trace",
        };

        Exception? actual = null;
        try { await middleware.InvokeAsync(context); }
        catch (Exception exception) { actual = exception; }

        await Assert.That(context.Response.HasStarted).IsTrue();
        await Assert.That(actual).IsSameReferenceAs(expected);
    }

    [Test]
    public async Task CancelledRequest_RethrowsCancellation()
    {
        using var services = CreateServices();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var expected = new OperationCanceledException(cancellation.Token);
        var middleware = CreateMiddleware(services, _ => throw expected);
        var context = CreateContext(services, "cancelled-trace");
        context.RequestAborted = cancellation.Token;

        Exception? actual = null;
        try { await middleware.InvokeAsync(context); }
        catch (Exception exception) { actual = exception; }

        await Assert.That(actual).IsSameReferenceAs(expected);
        await Assert.That(context.Response.StatusCode).IsEqualTo(200);
    }

    [Test]
    public async Task UnsupportedAccept_StillUsesUnifiedErrorResult()
    {
        using var services = CreateServices();
        var middleware = CreateMiddleware(services, _ =>
            throw new BusinessException("BusinessError", StatusCodes.Status400BadRequest));
        var context = CreateContext(services, "plain-trace");
        context.Request.Headers.Accept = "text/plain";

        await middleware.InvokeAsync(context);

        await Assert.That(context.Response.StatusCode).IsEqualTo(StatusCodes.Status400BadRequest);
        await Assert.That(context.Response.ContentType).IsEqualTo("application/problem+json");
        context.Response.Body.Position = 0;
        var body = await JsonNode.ParseAsync(context.Response.Body);
        await Assert.That(body!["traceId"]!.GetValue<string>()).IsEqualTo(System.Diagnostics.Activity.Current?.Id ?? "plain-trace");
        await Assert.That(body["errors"]!.AsObject().Count).IsEqualTo(0);
    }

    [Test]
    public async Task EmptyErrorStatuses_UseUnifiedErrorResult()
    {
        using var services = CreateServices();
        var problemService = services.GetRequiredService<IProblemDetailsService>();
        foreach (var status in new[] { 401, 403, 429 })
        {
            var context = CreateContext(services, $"status-{status}");
            context.Response.StatusCode = status;
            context.Request.Headers.Accept = "text/plain";
            var written = await problemService.TryWriteAsync(new ProblemDetailsContext { HttpContext = context });
            await Assert.That(written).IsTrue();
            await Assert.That(context.Response.StatusCode).IsEqualTo(status);
            context.Response.Body.Position = 0;
            var body = await JsonNode.ParseAsync(context.Response.Body);
            await Assert.That(body!["status"]!.GetValue<int>()).IsEqualTo(status);
            await Assert.That(body["traceId"]).IsNotNull();
            await Assert.That(body["errors"]!.AsObject().Count).IsEqualTo(0);
        }
    }

    private static async Task AssertProblemAsync(
        Exception exception,
        int expectedStatus,
        string expectedDetail)
    {
        using var services = CreateServices();
        var middleware = CreateMiddleware(services, _ => throw exception);
        const string traceId = "problem-middleware-test";
        var context = CreateContext(services, traceId);

        await middleware.InvokeAsync(context);

        await Assert.That(context.Response.StatusCode).IsEqualTo(expectedStatus);
        await Assert.That(context.Response.ContentType)
            .Contains("application/problem+json");
        context.Response.Body.Position = 0;
        var body = await JsonNode.ParseAsync(context.Response.Body);
        await Assert.That(body!["status"]!.GetValue<int>()).IsEqualTo(expectedStatus);
        await Assert.That(body["detail"]!.GetValue<string>()).IsEqualTo(expectedDetail);
        await Assert.That(body["traceId"]!.GetValue<string>()).IsEqualTo(System.Diagnostics.Activity.Current?.Id ?? traceId);
    }

    [Test]
    public async Task ProductionUnhandledException_HidesInternalDetail()
    {
        using var services = CreateServices();
        var middleware = CreateMiddleware(services, _ =>
            throw new InvalidOperationException("secret database detail"), "Production");
        var context = CreateContext(services, "production-trace");

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await JsonNode.ParseAsync(context.Response.Body);
        await Assert.That(body!["status"]!.GetValue<int>()).IsEqualTo(500);
        await Assert.That(body["detail"]!.GetValue<string>()).IsNotEqualTo("secret database detail");
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddLocalization()
            .ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, TestJsonContext.Default))
            .AddSingleton<IProblemDetailsWriter, ErrorResultProblemDetailsWriter>()
            .AddProblemDetails();
        services.AddSingleton<Localizer>();
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext CreateContext(IServiceProvider services, string traceId)
    {
        var context = new DefaultHttpContext { RequestServices = services, TraceIdentifier = traceId };
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static GlobalExceptionMiddleware CreateMiddleware(
        IServiceProvider services,
        RequestDelegate next,
        string environmentName = "Development") =>
        new(
            next,
            services.GetRequiredService<Localizer>(),
            services.GetRequiredService<ILogger<GlobalExceptionMiddleware>>(),
            new TestHostEnvironment(environmentName));
}

internal sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
{
    public string EnvironmentName { get; set; } = environmentName;
    public string ApplicationName { get; set; } = "ApiTest";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

internal sealed class StartedResponseFeature : IHttpResponseFeature
{
    public int StatusCode { get; set; } = StatusCodes.Status200OK;
    public string? ReasonPhrase { get; set; }
    public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
    public Stream Body { get; set; } = new MemoryStream();
    public bool HasStarted => true;
    public void OnStarting(Func<object, Task> callback, object state) { }
    public void OnCompleted(Func<object, Task> callback, object state) { }
}

[JsonSerializable(typeof(ErrorResult))]
internal partial class TestJsonContext : JsonSerializerContext
{
}
