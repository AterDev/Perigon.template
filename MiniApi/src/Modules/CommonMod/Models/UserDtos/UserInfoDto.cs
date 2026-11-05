namespace CommonMod.Models.UserDtos;

public sealed class UserInfoDto
{
    public Guid Id { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }
}
