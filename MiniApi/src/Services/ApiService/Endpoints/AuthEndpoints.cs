using Share;
using CommonMod.Managers;

namespace ApiService.Endpoints;

public sealed class AuthEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/auth", "Auth");
        group.MapPost("/login", Login).AllowAnonymous();
    }

    /// <summary>
    /// 演示用登录接口，明文比较用户名和密码。
    /// </summary>
    /// <param name="request">登录信息。</param>
    /// <param name="userManager">用户管理。</param>
    /// <param name="localizer">本地化消息。</param>
    /// <param name="httpContext">当前请求上下文。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <response code="200">登录成功，返回用户信息。</response>
    /// <response code="400">用户名或密码错误。</response>
    public static async Task<IResult> Login(
        LoginRequest request,
        UserManager userManager,
        Localizer localizer,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(localizer, httpContext, "InvalidUserOrPassword");
        }

        var user = await userManager.FindByUserNameAsync(request.UserName.Trim(), cancellationToken);
        if (user is null || user.Password != request.Password)
        {
            return BadRequest(localizer, httpContext, "InvalidUserOrPassword");
        }

        return Results.Ok(new LoginResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            NickName = user.NickName,
        });
    }
}

public sealed class LoginRequest
{
    public string? UserName { get; set; }

    public string? Password { get; set; }
}

public sealed class LoginResponse
{
    public Guid Id { get; set; }

    public string? UserName { get; set; }

    public string? NickName { get; set; }
}
