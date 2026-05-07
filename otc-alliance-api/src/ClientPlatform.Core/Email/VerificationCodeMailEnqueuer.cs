using Abp.Dependency;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ClientPlatform.Email;

/// <summary>
/// 验证码邮件发送：本机异步 fire-and-forget，接口立即返回，不依赖 ABP BackgroundJob
/// </summary>
public class VerificationCodeMailEnqueuer : IVerificationCodeMailEnqueuer, ITransientDependency
{
    private readonly IAllianceMailService _allianceMailService;
    private readonly ILogger<VerificationCodeMailEnqueuer> _logger;

    public VerificationCodeMailEnqueuer(IAllianceMailService allianceMailService, ILogger<VerificationCodeMailEnqueuer> logger)
    {
        _allianceMailService = allianceMailService;
        _logger = logger;
    }

    public Task EnqueueSendVerificationCodeAsync(string allianceKey, string to, string verificationCode)
    {
        _logger.LogInformation("SendVerificationCode 提交本机异步发送 - 联盟: {AllianceKey}, 收件人: {To}", allianceKey, to);

        _ = Task.Run(async () =>
        {
            try
            {
                var result = await _allianceMailService.SendVerificationCodeAsync(allianceKey, to, verificationCode);
                if (result)
                    _logger.LogInformation("SendVerificationCode 发送成功 - 收件人: {To}", to);
                else
                    _logger.LogWarning("SendVerificationCode 发送返回 false - 收件人: {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendVerificationCode 发送异常 - 收件人: {To}", to);
            }
        });

        return Task.CompletedTask;
    }
}
