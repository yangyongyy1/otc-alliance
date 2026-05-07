using System.Threading.Tasks;

namespace ClientPlatform.Kyc.Channels
{
    /// <summary>
    /// KYC 渠道工厂接口
    /// 负责根据渠道名称解析具体的提供商实例
    /// </summary>
    public interface IKycChannelFactory
    {
        /// <summary>
        /// 获取指定的 KYC 渠道提供商
        /// </summary>
        /// <param name="channelName">渠道名称 (如: "Sumsub")</param>
        /// <returns>实现 IKycChannelProvider 的实例</returns>
        IKycChannelProvider GetProvider(string channelName);
    }
}
