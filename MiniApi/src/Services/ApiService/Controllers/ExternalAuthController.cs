using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

namespace ApiService.Controllers
{
    public sealed class ExternalAuthController : RestControllerBase
    {
        public static void MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            var group = MapGroup(endpoints, "/api/external-auth", "ExternalAuth");
            group.MapGet("/signin-microsoft", SignInMicrosoft).AllowAnonymous();
            group.MapGet("/signin-google", SignInGoogle).AllowAnonymous();
            group.MapGet("/token", (Delegate)GetTokenAsync).AllowAnonymous();
        }

        /// <summary>
        /// Starts Microsoft account sign-in.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <response code="302">Redirects to the Microsoft sign-in challenge.</response>
        public static IResult SignInMicrosoft(HttpContext httpContext)
        {
            var returnUrl = GetQueryValue(httpContext, "returnUrl", "/");
            var redirectUri = BuildTokenRedirectUri(httpContext, MicrosoftAccountDefaults.AuthenticationScheme, returnUrl);
            var properties = new AuthenticationProperties { RedirectUri = redirectUri };
            return Results.Challenge(properties, [MicrosoftAccountDefaults.AuthenticationScheme]);
        }

        /// <summary>
        /// Starts Google account sign-in.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <response code="302">Redirects to the Google sign-in challenge.</response>
        public static IResult SignInGoogle(HttpContext httpContext)
        {
            var returnUrl = GetQueryValue(httpContext, "returnUrl", "/");
            var redirectUri = BuildTokenRedirectUri(httpContext, GoogleDefaults.AuthenticationScheme, returnUrl);
            var properties = new AuthenticationProperties { RedirectUri = redirectUri };
            return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
        }

        /// <summary>
        /// Completes external sign-in and returns external user information.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        /// <response code="200">Returns the authenticated external user information.</response>
        /// <response code="400">External authentication failed.</response>
        public static async Task<IResult> GetTokenAsync(HttpContext httpContext)
        {
            var logger = httpContext.RequestServices.GetRequiredService<ILogger<ExternalAuthController>>();
            var type = GetQueryValue(httpContext, "type", string.Empty);
            var returnUrl = GetQueryValue(httpContext, "returnUrl", "/");

            logger.LogInformation("{type} login callback initiated.", type);

            var result = await httpContext.AuthenticateAsync(type);
            if (!result.Succeeded)
            {
                logger.LogWarning("External authentication failed for type: {type}", type);
                return Results.BadRequest("External authentication failed.");
            }

            var externalUser = result.Principal;
            var email = externalUser.FindFirst(ClaimTypes.Email)?.Value;
            var name = externalUser.FindFirst(ClaimTypes.Name)?.Value;
            return Results.Ok(new { email, name, returnUrl });
        }

        private static string GetQueryValue(HttpContext httpContext, string key, string fallback)
        {
            return httpContext.Request.Query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value.ToString()
                : fallback;
        }

        private static string BuildTokenRedirectUri(HttpContext httpContext, string type, string returnUrl)
        {
            var request = httpContext.Request;
            var basePath = request.PathBase.HasValue ? request.PathBase.Value : string.Empty;
            return $"{basePath}/api/external-auth/token?type={Uri.EscapeDataString(type)}&returnUrl={Uri.EscapeDataString(returnUrl)}";
        }
    }
}
