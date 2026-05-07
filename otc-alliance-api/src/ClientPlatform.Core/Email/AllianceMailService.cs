using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.Extensions.Options;
using ClientPlatform.Email.Strategies;
using ClientPlatform.Email.Templates;
using ClientPlatform.Email.Templates.Dto;

namespace ClientPlatform.Email;

/// <summary>
/// 联盟邮件服务实现
/// </summary>
public class AllianceMailService : IAllianceMailService
{
    private readonly MailConfig _mailConfig;
    private readonly Dictionary<string, IAllianceMailStrategy> _strategies;
    private readonly Dictionary<string, IMailTemplate> _templates;
    private readonly ILogger _logger;

    public AllianceMailService(IOptions<MailConfig> mailConfigOptions, ILogger logger)
    {
        _mailConfig = mailConfigOptions?.Value ?? throw new ArgumentNullException(nameof(mailConfigOptions));
        _logger = logger;
        _strategies = new Dictionary<string, IAllianceMailStrategy>();
        _templates = new Dictionary<string, IMailTemplate>();

        InitializeStrategies();
        InitializeTemplates();
    }

    /// <summary>
    /// 初始化邮件发送策略
    /// </summary>
    private void InitializeStrategies()
    {
        foreach (var kvp in _mailConfig.Alliances)
        {
            var strategy = new SmtpAllianceMailStrategy(kvp.Key, kvp.Value, _logger);
            _strategies[kvp.Key] = strategy;
        }
    }

    /// <summary>
    /// 初始化邮件模板
    /// </summary>
    private void InitializeTemplates()
    {
        foreach (var kvp in _mailConfig.Alliances)
        {
            var config = kvp.Value;
            var verificationCodeTemplate = new VerificationCodeTemplate(config.Templates.VerificationCode);
            _templates[$"{kvp.Key}_VerificationCode"] = verificationCodeTemplate;

            var registrationSuccessTemplate = new RegistrationSuccessTemplate(config.Templates.RegistrationSuccess);
            _templates[$"{kvp.Key}_RegistrationSuccess"] = registrationSuccessTemplate;
        }
    }

    /// <summary>
    /// 获取联盟邮件发送策略
    /// </summary>
    private IAllianceMailStrategy GetStrategy(string allianceKey)
    {
        if (string.IsNullOrWhiteSpace(allianceKey))
        {
            throw new ArgumentException("联盟标识不能为空", nameof(allianceKey));
        }

        if (_strategies.TryGetValue(allianceKey, out var strategy))
        {
            return strategy;
        }

        // 如果找不到指定联盟的策略，尝试使用 default 策略
        if (!allianceKey.Equals("default", StringComparison.OrdinalIgnoreCase))
        {
            if (_strategies.TryGetValue("default", out var defaultStrategy))
            {
                return defaultStrategy;
            }
        }

        throw new InvalidOperationException($"未找到联盟 '{allianceKey}' 的邮件配置，且未找到 default 配置");
    }

    /// <summary>
    /// 获取邮件模板
    /// </summary>
    private IMailTemplate GetTemplate(string allianceKey, string templateType)
    {
        var templateKey = $"{allianceKey}_{templateType}";
        if (_templates.TryGetValue(templateKey, out var template))
        {
            return template;
        }

        // 如果找不到指定联盟的模板，尝试使用 default 模板
        if (!allianceKey.Equals("default", StringComparison.OrdinalIgnoreCase))
        {
            var defaultTemplateKey = $"default_{templateType}";
            if (_templates.TryGetValue(defaultTemplateKey, out var defaultTemplate))
            {
                return defaultTemplate;
            }
        }

        throw new InvalidOperationException($"未找到联盟 '{allianceKey}' 的模板 '{templateType}'，且未找到 default 模板");
    }

    public async Task<bool> SendVerificationCodeAsync(string allianceKey, string to, string verificationCode)
    {
        try
        {
            var strategy = GetStrategy(allianceKey);
            var template = GetTemplate(allianceKey, "VerificationCode");

            var data = new VerificationCodeData { Code = verificationCode };
            var subject = template.GenerateSubject(data);
            var body = template.GenerateBody(data);

            return await strategy.SendMailAsync(to, subject, body);
        }
        catch (Exception ex)
        {
            _logger?.Error($"发送验证码邮件失败 - 联盟: {allianceKey}, 收件人: {to}, 错误: {ex.Message}", ex);
            return false;
        }
    }

    public async Task<bool> SendRegistrationSuccessAsync(string allianceKey, string to, string email, string password, string loginUrl)
    {
        try
        {
            var strategy = GetStrategy(allianceKey);
            var template = GetTemplate(allianceKey, "RegistrationSuccess");

            // 如果loginUrl为空，尝试从配置中获取
            if (string.IsNullOrWhiteSpace(loginUrl))
            {
                if (_mailConfig.Alliances.TryGetValue(allianceKey, out var allianceConfig))
                {
                    loginUrl = allianceConfig.LoginUrl;
                }
                // 如果找不到指定联盟的配置，尝试使用 default 配置
                else if (!allianceKey.Equals("default", StringComparison.OrdinalIgnoreCase))
                {
                    if (_mailConfig.Alliances.TryGetValue("default", out var defaultConfig))
                    {
                        loginUrl = defaultConfig.LoginUrl;
                    }
                }
            }

            var data = new RegistrationSuccessData
            {
                Email = email,
                Password = password,
                LoginUrl = loginUrl ?? string.Empty
            };

            var subject = template.GenerateSubject(data);
            var body = template.GenerateBody(data);

            return await strategy.SendMailAsync(to, subject, body);
        }
        catch (Exception ex)
        {
            _logger?.Error($"发送注册成功邮件失败 - 联盟: {allianceKey}, 收件人: {to}, 错误: {ex.Message}", ex);
            return false;
        }
    }
}

