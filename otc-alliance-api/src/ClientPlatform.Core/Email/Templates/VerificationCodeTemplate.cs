using System;
using ClientPlatform.Email.Templates.Dto;

namespace ClientPlatform.Email.Templates;

/// <summary>
/// 验证码邮件模板
/// </summary>
public class VerificationCodeTemplate : IMailTemplate
{
    private readonly VerificationCodeTemplateConfig _config;

    public VerificationCodeTemplate(VerificationCodeTemplateConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public string TemplateType => "VerificationCode";

    public string GenerateSubject(object data)
    {
        if (data is VerificationCodeData verificationCodeData)
        {
            return _config.Subject?.Replace("{Code}", verificationCodeData.Code) ?? "验证码";
        }

        throw new ArgumentException($"数据类型必须是 {nameof(VerificationCodeData)}", nameof(data));
    }

    public string GenerateBody(object data)
    {
        if (data is VerificationCodeData verificationCodeData)
        {
            return _config.BodyTemplate?.Replace("{Code}", verificationCodeData.Code) ?? $"您的验证码是：{verificationCodeData.Code}";
        }

        throw new ArgumentException($"数据类型必须是 {nameof(VerificationCodeData)}", nameof(data));
    }
}

