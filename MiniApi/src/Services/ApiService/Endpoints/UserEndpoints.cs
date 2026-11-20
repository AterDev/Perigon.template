using Share;
using CommonMod.Managers;
using CommonMod.Models.UserDtos;
using Perigon.AspNetCore.Models;

namespace ApiService.Endpoints;

public sealed class UserEndpoints : RestEndpointBase
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = MapGroup(endpoints, "/api/users", "User");
        group.MapPost("/login", Login)
            .WithSummary("用户登录")
            .Produces<ErrorResult>(StatusCodes.Status403Forbidden, "application/problem+json")
            .Produces<ErrorResult>(StatusCodes.Status404NotFound, "application/problem+json")
            .AllowAnonymous();
    }

    public static async Task<ApiResult<UserInfoDto>> Login(
        UserLoginDto request,
        UserManager userManager,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByUserNameAsync(request.UserName!.Trim(), cancellationToken);
        if (user is null)
        {
            return NotFound(Localizer.NotFoundUser);
        }
        if (user.Password != request.Password)
        {
            return Forbid(Localizer.PasswordInvalid);
        }
        return new UserInfoDto
        {
            Id = user.Id,
            UserName = user.UserName,
            NickName = user.NickName,
        };
    }
}
