namespace Entity;

/// <summary>
/// 演示用用户实体，密码以明文保存，仅用于初始化示例
/// </summary>
[System.ComponentModel.DataAnnotations.Schema.Table("users")]
public sealed class User : EntityBase
{
    public User() { }
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? NickName { get; set; }
}