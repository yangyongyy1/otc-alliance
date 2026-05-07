namespace ClientPlatform.Kyc.Channels.Sumsub
{
    /// <summary>
    /// Sumsub 配置类
    /// 对应 appsettings.json 中的 SumsubConf 节点
    /// </summary>
    public class SumsubSetting
    {
        /// <summary>
        /// API 基础地址 (如 https://api.sumsub.com)
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// API 令牌
        /// </summary>
        public string ApiToken { get; set; }

        /// <summary>
        /// API 密钥 (用于签名请求)
        /// </summary>
        public string ApiSecretKey { get; set; }
        
        /// <summary>
        /// Webhook 密钥 (用于验证回调签名)
        /// </summary>
        public string WebhookSecretKey { get; set; }
    }
}
