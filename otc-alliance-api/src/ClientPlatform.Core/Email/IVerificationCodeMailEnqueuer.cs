using System.Threading.Tasks;

namespace ClientPlatform.Email;

/// <summary>
/// 验证码邮件入队发送（后台任务，接口立即返回，避免前端长时间等待）
/// </summary>
public interface IVerificationCodeMailEnqueuer
{
    /// <summary>
    /// 将发送验证码邮件任务加入后台队列，立即返回
    /// </summary>
    Task EnqueueSendVerificationCodeAsync(string allianceKey, string to, string verificationCode);
}
