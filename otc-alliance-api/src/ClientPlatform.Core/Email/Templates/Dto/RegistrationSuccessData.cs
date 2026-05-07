namespace ClientPlatform.Email.Templates.Dto;

/// <summary>
/// 注册成功邮件数据
/// </summary>
public class RegistrationSuccessData
{
    /// <summary>
    /// 登录账号（邮箱）
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 登录密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 登录站点URL
    /// </summary>
    public string LoginUrl { get; set; }
}

