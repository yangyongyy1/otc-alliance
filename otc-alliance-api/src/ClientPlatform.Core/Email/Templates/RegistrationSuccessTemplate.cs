using System;
using ClientPlatform.Email.Templates.Dto;

namespace ClientPlatform.Email.Templates;

/// <summary>
/// 注册成功邮件模板
/// </summary>
public class RegistrationSuccessTemplate : IMailTemplate
{
    private readonly RegistrationSuccessTemplateConfig _config;

    public RegistrationSuccessTemplate(RegistrationSuccessTemplateConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public string TemplateType => "RegistrationSuccess";

    public string GenerateSubject(object data)
    {
        if (data is RegistrationSuccessData registrationData)
        {
            var subject = _config.Subject ?? "注册成功通知";
            return subject
                .Replace("{Email}", registrationData.Email ?? "")
                .Replace("{LoginUrl}", registrationData.LoginUrl ?? "");
        }

        throw new ArgumentException($"数据类型必须是 {nameof(RegistrationSuccessData)}", nameof(data));
    }

    public string GenerateBody(object data)
    {
        if (data is RegistrationSuccessData registrationData)
        {
            var body = _config.BodyTemplate ?? "您的账号已注册成功。";
            return body
                .Replace("{Email}", registrationData.Email ?? "")
                .Replace("{Password}", registrationData.Password ?? "")
                .Replace("{LoginUrl}", registrationData.LoginUrl ?? "");
        }

        throw new ArgumentException($"数据类型必须是 {nameof(RegistrationSuccessData)}", nameof(data));
    }
}

