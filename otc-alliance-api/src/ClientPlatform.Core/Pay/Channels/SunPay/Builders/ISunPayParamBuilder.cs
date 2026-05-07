using ClientPlatform.Pay.Dto.Request;
using ClientPlatform.Pay.Models;

namespace ClientPlatform.Pay.Channels.SunPay.Builders
{
    /// <summary>
    /// SunPay 参数构建器策略接口
    /// 负责将平台标准请求转换为 SunPay 特定的 CreateAccountInput
    /// </summary>
    public interface ISunPayParamBuilder
    {
        /// <summary>
        /// 是否支持该币种
        /// </summary>
        bool CanHandle(string currency);

        /// <summary>
        /// 执行构建
        /// </summary>
        CreateAccountInput Build(StandardVaCreationRequest request);
    }
}
