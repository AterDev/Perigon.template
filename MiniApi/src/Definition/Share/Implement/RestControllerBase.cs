using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Share.Implement
{
    /// <summary>
    /// Base type for classes in Controllers folders that map Minimal API endpoint groups.
    /// </summary>
    public abstract class RestControllerBase
    {
        protected static RouteGroupBuilder MapGroup(
            IEndpointRouteBuilder endpoints,
            string pattern,
            string tag,
            string? authorizationPolicy = null)
        {
            var group = endpoints.MapGroup(pattern)
                .WithTags(tag);

            return string.IsNullOrWhiteSpace(authorizationPolicy)
                ? group
                : group.RequireAuthorization(authorizationPolicy);
        }

        protected static IResult Forbid(Localizer localizer, HttpContext httpContext, string? value)
        {
            var result = CreateResult(localizer, httpContext, "Forbidden", value, 403);
            Activity.Current?.SetTag("http.response.body", value);
            return Results.Json(result, statusCode: StatusCodes.Status403Forbidden);
        }

        protected static IResult NotFound(Localizer localizer, HttpContext httpContext, string? value)
        {
            var result = CreateResult(localizer, httpContext, "NotFound", value, 404);
            return Results.NotFound(result);
        }

        protected static IResult Conflict(Localizer localizer, HttpContext httpContext, string? detail)
        {
            var result = CreateResult(localizer, httpContext, "Conflict", detail, 409);
            Activity.Current?.SetTag("http.response.body", detail);
            return Results.Conflict(result);
        }

        protected static IResult Problem(
            Localizer localizer,
            HttpContext httpContext,
            string? detail = null,
            int errorCode = 0,
            params object[] arguments)
        {
            var result = CreateResult(localizer, httpContext, "Problem", detail, errorCode, arguments);
            Activity.Current?.SetTag("http.request.body", detail);
            Activity.Current?.SetTag("http.response.body", detail);
            return Results.Json(result, statusCode: StatusCodes.Status500InternalServerError);
        }

        protected static IResult BadRequest(
            Localizer localizer,
            HttpContext httpContext,
            string? error,
            params object[] arguments)
        {
            var result = CreateResult(
                localizer,
                httpContext,
                localizer.Get(Localizer.BadRequest),
                error,
                0,
                arguments);
            return Results.BadRequest(result);
        }

        private static ErrorResult CreateResult(
            Localizer localizer,
            HttpContext httpContext,
            string title,
            string? detail = null,
            int errorCode = 0,
            params object[] arguments)
        {
            var error = detail ?? string.Empty;

            if (detail != null)
            {
                error = localizer.Get(detail, arguments) ?? detail;
            }
            else if (errorCode != 0)
            {
                error = localizer.Get(errorCode.ToString()) ?? error;
            }

            return new ErrorResult(error, httpContext.TraceIdentifier, title, errorCode);
        }
    }
}
