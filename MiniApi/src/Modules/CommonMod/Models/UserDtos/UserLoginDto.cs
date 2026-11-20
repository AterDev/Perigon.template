using Microsoft.Extensions.Validation;

namespace CommonMod.Models.UserDtos;

#pragma warning disable ASP0029 // Required to generate validation metadata for this cross-assembly root in .NET 10.
[ValidatableType]
#pragma warning restore ASP0029
public sealed class UserLoginDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required, StringLength(50)]
    public string? UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required, StringLength(100)]
    public string? Password { get; set; }
}
