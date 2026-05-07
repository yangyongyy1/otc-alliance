namespace ClientPlatform.Email.Templates;

/// <summary>
/// 邮件模板接口
/// </summary>
public interface IMailTemplate
{
    /// <summary>
    /// 模板类型名称
    /// </summary>
    string TemplateType { get; }

    /// <summary>
    /// 生成邮件主题
    /// </summary>
    /// <param name="data">模板数据</param>
    /// <returns>邮件主题</returns>
    string GenerateSubject(object data);

    /// <summary>
    /// 生成邮件内容
    /// </summary>
    /// <param name="data">模板数据</param>
    /// <returns>邮件内容</returns>
    string GenerateBody(object data);
}

