using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using Perigon.AspNetCore.Models;
using ServiceDefaults.Middleware;
using Share;
using Share.Implement;

namespace ApiTest;

public class ApiResultTests
{
    private sealed class TestEndpoints : RestEndpointBase
    {
        public static ProblemHttpResult RawNotFound(string detail) => NotFound(detail);

        public static ApiResult<string> ProblemResult(
            string? detail = null,
            int statusCode = 500,
            params object[] arguments) =>
            Problem(detail, statusCode, arguments);

        public static ApiResult<string> BadRequestResult(string? detail = null, params object[] arguments) =>
            BadRequest(detail, arguments);

        public static ApiResult<string> NotFoundResult(string? detail = null, params object[] arguments) =>
            NotFound(detail, arguments);

        public static ApiResult<string> ForbidResult(string? detail = null, params object[] arguments) =>
            Forbid(detail, arguments);

        public static ApiResult<string> ConflictResult(string? detail = null, params object[] arguments) =>
            Conflict(detail, arguments);

        public static ApiResult<string>[] TypedResults() =>
        [
            Microsoft.AspNetCore.Http.TypedResults.Ok("ok"),
            Microsoft.AspNetCore.Http.TypedResults.Created("/items/1", "created"),
            Microsoft.AspNetCore.Http.TypedResults.Accepted("/items/1", "accepted"),
            Microsoft.AspNetCore.Http.TypedResults.Ok(),
            Microsoft.AspNetCore.Http.TypedResults.Created(),
            Microsoft.AspNetCore.Http.TypedResults.Accepted("/items/1"),
            Microsoft.AspNetCore.Http.TypedResults.NoContent(),
            Microsoft.AspNetCore.Http.TypedResults.BadRequest(),
            Microsoft.AspNetCore.Http.TypedResults.NotFound(),
            Microsoft.AspNetCore.Http.TypedResults.Conflict(),
            Microsoft.AspNetCore.Http.TypedResults.Unauthorized(),
        ];
    }

    [Test]
    public async Task Helper_KeepsResourceKeyInProblemDetail()
    {
        var result = TestEndpoints.RawNotFound(Localizer.NotFoundUser);
        await Assert.That(result.ProblemDetails.Detail).IsEqualTo(Localizer.NotFoundUser);
        await Assert.That(result.ProblemDetails.Extensions.ContainsKey(RestEndpointBase.LocalizationArgumentsExtensionName)).IsFalse();
    }

    [Test]
    public async Task ProblemHelpers_LocalizeAtExecution_ForAllStatusesAndCultures()
    {
        var originalCulture = CultureInfo.CurrentUICulture;
        try
        {
            using var services = CreateServices();

            var cases = new (Func<RestEndpointBase.ApiResult<string>> Factory, int StatusCode, string Key, string Zh, string En)[]
            {
                (() => TestEndpoints.ProblemResult(Localizer.InvalidUserOrPassword), 500,
                    Localizer.InvalidUserOrPassword, "用户名或密码错误", "Invalid username or password"),
                (() => TestEndpoints.BadRequestResult(Localizer.BadRequest), 400,
                    Localizer.BadRequest, "请求参数错误", "Bad Request"),
                (() => TestEndpoints.NotFoundResult(Localizer.NotFoundUser), 404,
                    Localizer.NotFoundUser, "未找到该用户", "Not Found User"),
                (() => TestEndpoints.ForbidResult(Localizer.InvalidUserOrPassword), 403,
                    Localizer.InvalidUserOrPassword, "用户名或密码错误", "Invalid username or password"),
                (() => TestEndpoints.ConflictResult(Localizer.ConflictResource), 409,
                    Localizer.ConflictResource, "已存在的资源", "Conflict Resource"),
            };

            foreach (var (factory, statusCode, key, zh, en) in cases)
            {
                // The marker stores only the key.  The same result can therefore be
                // executed under different request cultures and localized at write time.
                var result = factory();

                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                var enResponse = await Execute(result, services, $"en-{statusCode}");
                await AssertLocalizedProblem(enResponse, statusCode, en, key);

                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
                var zhResponse = await Execute(result, services, $"zh-{statusCode}");
                await AssertLocalizedProblem(zhResponse, statusCode, zh, key);
            }
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Test]
    public async Task ProblemHelper_PassesArgumentsToLocalizedResource_AndUnknownKeysFallback()
    {
        var originalCulture = CultureInfo.CurrentUICulture;
        try
        {
            using var services = CreateParameterizedServices();
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var parameterized = await Execute(
                TestEndpoints.ProblemResult("Greeting", 400, "Ada"), services, "parameterized-en");
            await AssertLocalizedProblem(parameterized, 400, "Hello, Ada", "Greeting");

            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
            var localized = await Execute(
                TestEndpoints.ProblemResult("Greeting", 400, "Ada"), services, "parameterized-zh");
            await AssertLocalizedProblem(localized, 400, "你好，Ada", "Greeting");

            var unknown = await Execute(
                TestEndpoints.ProblemResult("Unknown.Error.Key", 400, "ignored"), services, "unknown-key");
            await AssertLocalizedProblem(unknown, 400, "Unknown.Error.Key", "Unknown.Error.Key");
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Test]
    public async Task TypedProblemDetail_LocalizesKnownKeys_AndPreservesUnknownText()
    {
        var originalCulture = CultureInfo.CurrentUICulture;
        try
        {
            using var services = CreateServices();
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
            var localizedResponse = await Execute(
                Microsoft.AspNetCore.Http.TypedResults.Problem(
                    detail: Localizer.BadRequest, statusCode: StatusCodes.Status400BadRequest),
                services,
                "raw-known-problem");
            await AssertLocalizedProblem(
                localizedResponse, StatusCodes.Status400BadRequest, "请求参数错误", Localizer.BadRequest);

            var unknownResponse = await Execute(
                Microsoft.AspNetCore.Http.TypedResults.Problem(
                    detail: "already localized", statusCode: StatusCodes.Status400BadRequest),
                services,
                "raw-unknown-problem");
            await AssertLocalizedProblem(unknownResponse, StatusCodes.Status400BadRequest, "already localized", null);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Test]
    public async Task ApiResult_SupportsCommonTypedResultsAndFromResult()
    {
        using var services = CreateServices();
        int[] expected = [200, 201, 202, 200, 201, 202, 204, 400, 404, 409, 401];
        var results = TestEndpoints.TypedResults();

        for (var i = 0; i < results.Length; i++)
        {
            var response = await Execute(results[i], services, $"typed-{i}");
            await Assert.That(response.StatusCode).IsEqualTo(expected[i]);
        }

        var wrapped = RestEndpointBase.ApiResult<string>.FromResult(
            Microsoft.AspNetCore.Http.TypedResults.UnprocessableEntity());
        var wrappedResponse = await Execute(wrapped, services, "from-result");
        await Assert.That(wrappedResponse.StatusCode).IsEqualTo(422);
        RestEndpointBase.ApiResult<string> forbidden = Microsoft.AspNetCore.Http.TypedResults.Forbid();
        await Assert.That(forbidden).IsNotNull();
    }

    private static ServiceProvider CreateServices() =>
        new ServiceCollection()
            .AddLogging()
            .AddLocalization()
            .ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, TestJsonContext.Default))
            .AddSingleton<IProblemDetailsWriter, ErrorResultProblemDetailsWriter>()
            .AddProblemDetails()
            .AddSingleton<Localizer>()
            .BuildServiceProvider();

    private static ServiceProvider CreateParameterizedServices() =>
        new ServiceCollection()
            .AddLogging()
            .ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, TestJsonContext.Default))
            .AddSingleton<IProblemDetailsWriter, ErrorResultProblemDetailsWriter>()
            .AddProblemDetails()
            .AddSingleton<IStringLocalizer<Localizer>, ParameterizedLocalizer>()
            .AddSingleton<Localizer>()
            .BuildServiceProvider();

    private static async Task<ResponseSnapshot> Execute(IResult result, IServiceProvider services, string traceId)
    {
        var context = new DefaultHttpContext { RequestServices = services, TraceIdentifier = traceId };
        context.Response.Body = new MemoryStream();
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        var body = context.Response.Body.Length == 0 ? null : await JsonNode.ParseAsync(context.Response.Body);
        return new ResponseSnapshot(context.Response.StatusCode, context.Response.ContentType, body);
    }

    private static async Task AssertLocalizedProblem(
        ResponseSnapshot response,
        int statusCode,
        string expectedDetail,
        string? detailKey)
    {
        await Assert.That(response.Body!["status"]!.GetValue<int>()).IsEqualTo(statusCode);
        await Assert.That(response.ContentType).StartsWith("application/problem+json");
        await Assert.That(response.Body["detail"]!.GetValue<string>()).IsEqualTo(expectedDetail);
        await Assert.That(response.Body.ToJsonString()).DoesNotContain(RestEndpointBase.LocalizationArgumentsExtensionName);
        if (detailKey is not null && !string.Equals(expectedDetail, detailKey, StringComparison.Ordinal))
            await Assert.That(response.Body["detail"]!.GetValue<string>()).IsNotEqualTo(detailKey);
    }

    private sealed class ParameterizedLocalizer : IStringLocalizer<Localizer>
    {
        public LocalizedString this[string name] =>
            new(name, name, resourceNotFound: true);

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (!string.Equals(name, "Greeting", StringComparison.Ordinal))
                    return new LocalizedString(name, name, resourceNotFound: true);

                var template = CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                    ? "你好，{0}"
                    : "Hello, {0}";
                return new LocalizedString(name, string.Format(CultureInfo.CurrentUICulture, template, arguments), false);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            Array.Empty<LocalizedString>();

        public IStringLocalizer WithCulture(CultureInfo culture) => this;
    }

    private sealed record ResponseSnapshot(int StatusCode, string? ContentType, JsonNode? Body);
}
