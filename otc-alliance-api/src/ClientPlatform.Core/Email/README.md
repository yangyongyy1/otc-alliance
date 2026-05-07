# 邮件发送模块使用说明

## 概述

邮件发送模块采用策略模式和工厂模式设计，支持为不同联盟配置不同的邮件发送账号、模板等信息。添加新联盟时，只需在配置文件中添加配置即可，无需修改代码。

## 配置说明

在 `appsettings.json` 中添加 `MailConfig` 配置节点：

```json
{
  "MailConfig": {
    "Alliances": {
      "lianmen1": {
        "SmtpHost": "smtp.example.com",
        "SmtpPort": 587,
        "EnableSsl": true,
        "FromAddress": "noreply@lianmen1.com",
        "FromDisplayName": "联盟1邮件系统",
        "SmtpUsername": "noreply@lianmen1.com",
        "SmtpPassword": "your-password-here",
        "Templates": {
          "VerificationCode": {
            "Subject": "【联盟1】验证码",
            "BodyTemplate": "<html><body><h2>您的验证码</h2><p>您的验证码是：<strong>{Code}</strong></p><p>有效期10分钟，请勿泄露给他人。</p></body></html>"
          }
        }
      },
      "lianmen2": {
        "SmtpHost": "smtp.example2.com",
        "SmtpPort": 587,
        "EnableSsl": true,
        "FromAddress": "noreply@lianmen2.com",
        "FromDisplayName": "联盟2邮件系统",
        "SmtpUsername": "noreply@lianmen2.com",
        "SmtpPassword": "your-password-here",
        "Templates": {
          "VerificationCode": {
            "Subject": "【联盟2】验证码",
            "BodyTemplate": "<html><body><h2>验证码通知</h2><p>您的验证码：<strong style='color:red;font-size:20px;'>{Code}</strong></p><p>有效期10分钟。</p></body></html>"
          }
        }
      }
    }
  }
}
```

## 使用示例

在应用服务中注入 `IAllianceMailService` 并使用：

```csharp
using ClientPlatform.Email;
using System.Threading.Tasks;

public class YourAppService : AppServiceBase
{
    private readonly IAllianceMailService _allianceMailService;

    public YourAppService(IAllianceMailService allianceMailService)
    {
        _allianceMailService = allianceMailService;
    }

    public async Task SendVerificationCodeAsync(string allianceKey, string email, string code)
    {
        var result = await _allianceMailService.SendVerificationCodeAsync(allianceKey, email, code);
        if (result)
        {
            // 发送成功
        }
        else
        {
            // 发送失败
        }
    }
}
```

## 设计模式说明

1. **策略模式**：每个联盟使用独立的邮件发送策略（`IAllianceMailStrategy`），可以轻松扩展不同的发送方式（SMTP、API等）
2. **工厂模式**：`AllianceMailService` 根据联盟标识自动创建对应的策略和模板实例
3. **配置驱动**：所有配置通过配置文件管理，添加新联盟只需添加配置，无需修改代码

## 扩展新模板

如需添加新的邮件模板类型：

1. 创建模板数据类（继承或实现相应接口）
2. 创建模板实现类（实现 `IMailTemplate`）
3. 在 `MailConfig` 中添加模板配置
4. 在 `AllianceMailService` 中添加对应的发送方法

