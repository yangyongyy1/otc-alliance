using System.Threading.Tasks;

namespace ClientPlatform.Email.Strategies;

/// <summary>
/// 联盟邮件发送策略接口
/// </summary>
public interface IAllianceMailStrategy
{
    /// <summary>
    /// 联盟标识
    /// </summary>
    string AllianceKey { get; }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="to">收件人邮箱</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="body">邮件内容</param>
    /// <returns>发送结果</returns>
    Task<bool> SendMailAsync(string to, string subject, string body);
}

