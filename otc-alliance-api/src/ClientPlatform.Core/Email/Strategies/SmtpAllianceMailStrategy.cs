using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Castle.Core.Logging;

namespace ClientPlatform.Email.Strategies;

/// <summary>
/// SMTP 邮件发送策略实现
/// </summary>
public class SmtpAllianceMailStrategy : IAllianceMailStrategy
{
    private readonly AllianceMailConfig _config;
    private readonly ILogger _logger;

    public SmtpAllianceMailStrategy(string allianceKey, AllianceMailConfig config, ILogger logger)
    {
        AllianceKey = allianceKey ?? throw new ArgumentNullException(nameof(allianceKey));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
    }

    public string AllianceKey { get; }

    public async Task<bool> SendMailAsync(string to, string subject, string body)
    {
        try
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(_config.SmtpHost, _config.SmtpPort, MailKit.Security.SecureSocketOptions.SslOnConnect);


                await client.AuthenticateAsync(_config.SmtpUsername, _config.SmtpPassword);

                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress(_config.FromDisplayName, _config.FromAddress));
                message.To.Add(new MimeKit.MailboxAddress(to, to));
                message.Subject = subject;
                message.Body = new MimeKit.TextPart("html") { Text = body };

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            _logger?.Info($"邮件发送成功 - 联盟: {AllianceKey}, 收件人: {to}");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.Error($"邮件发送失败 - 联盟: {AllianceKey}, 收件人: {to}, 错误: {ex.Message}", ex);
            return false;
        }
    }
}

