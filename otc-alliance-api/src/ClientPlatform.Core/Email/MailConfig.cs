using System.Collections.Generic;

namespace ClientPlatform.Email;

/// <summary>
/// 邮件配置根节点
/// </summary>
public class MailConfig
{
    /// <summary>
    /// 联盟邮件配置字典，Key 为联盟标识（如 lianmen1, lianmen2）
    /// </summary>
    public Dictionary<string, AllianceMailConfig> Alliances { get; set; } = new Dictionary<string, AllianceMailConfig>();
}

/// <summary>
/// 联盟邮件配置
/// </summary>
public class AllianceMailConfig
{
    /// <summary>
    /// SMTP 服务器地址
    /// </summary>
    public string SmtpHost { get; set; }

    /// <summary>
    /// SMTP 端口
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// 是否启用 SSL
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// 发件人邮箱地址
    /// </summary>
    public string FromAddress { get; set; }

    /// <summary>
    /// 发件人显示名称
    /// </summary>
    public string FromDisplayName { get; set; }

    /// <summary>
    /// SMTP 用户名（通常是邮箱地址）
    /// </summary>
    public string SmtpUsername { get; set; }

    /// <summary>
    /// SMTP 密码或授权码
    /// </summary>
    public string SmtpPassword { get; set; }

    /// <summary>
    /// 登录站点URL（用于注册成功邮件）
    /// </summary>
    public string LoginUrl { get; set; }

    /// <summary>
    /// 邮件模板配置
    /// </summary>
    public MailTemplateConfig Templates { get; set; } = new MailTemplateConfig();
}

/// <summary>
/// 邮件模板配置
/// </summary>
public class MailTemplateConfig
{
    /// <summary>
    /// 验证码邮件模板配置
    /// </summary>
    public VerificationCodeTemplateConfig VerificationCode { get; set; } = new VerificationCodeTemplateConfig();

    /// <summary>
    /// 注册成功邮件模板配置
    /// </summary>
    public RegistrationSuccessTemplateConfig RegistrationSuccess { get; set; } = new RegistrationSuccessTemplateConfig();
}

/// <summary>
/// 验证码邮件模板配置
/// </summary>
public class VerificationCodeTemplateConfig
{
    /// <summary>
    /// 邮件主题模板，支持占位符 {Code}
    /// </summary>
    public string Subject { get; set; } = "验证码";

    /// <summary>
    /// 邮件内容模板，支持占位符 {Code}
    /// </summary>
    public string BodyTemplate { get; set; } = "您的验证码是：{Code}，有效期10分钟。";
}

/// <summary>
/// 注册成功邮件模板配置
/// </summary>
public class RegistrationSuccessTemplateConfig
{
    /// <summary>
    /// 邮件主题模板，支持占位符 {Email}, {LoginUrl}
    /// </summary>
    public string Subject { get; set; } = "注册成功通知";

    /// <summary>
    /// 邮件内容模板，支持占位符 {Email}, {Password}, {LoginUrl}
    /// </summary>
    public string BodyTemplate { get; set; } = @"<html><body>
<h2>注册成功</h2>
<p>恭喜您注册成功！</p>
<p><strong>登录账号：</strong>{Email}</p>
<p><strong>登录密码：</strong>{Password}</p>
<p><strong>登录站点：</strong><a href='{LoginUrl}'>{LoginUrl}</a></p>
<p>请妥善保管您的账号密码，建议首次登录后修改密码。</p>
</body></html>";
}

