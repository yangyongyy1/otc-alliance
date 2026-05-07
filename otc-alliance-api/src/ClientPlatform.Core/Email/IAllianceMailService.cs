using System.Threading.Tasks;
using ClientPlatform.Email.Templates.Dto;

namespace ClientPlatform.Email;

/// <summary>
/// 联盟邮件服务接口
/// </summary>
public interface IAllianceMailService
{
    /// <summary>
    /// 发送验证码邮件
    /// </summary>
    /// <param name="allianceKey">联盟标识</param>
    /// <param name="to">收件人邮箱</param>
    /// <param name="verificationCode">验证码</param>
    /// <returns>发送结果</returns>
    Task<bool> SendVerificationCodeAsync(string allianceKey, string to, string verificationCode);

    /// <summary>
    /// 发送注册成功邮件
    /// </summary>
    /// <param name="allianceKey">联盟标识</param>
    /// <param name="to">收件人邮箱</param>
    /// <param name="email">登录账号（邮箱）</param>
    /// <param name="password">登录密码</param>
    /// <param name="loginUrl">登录站点URL</param>
    /// <returns>发送结果</returns>
    Task<bool> SendRegistrationSuccessAsync(string allianceKey, string to, string email, string password, string loginUrl);
}

