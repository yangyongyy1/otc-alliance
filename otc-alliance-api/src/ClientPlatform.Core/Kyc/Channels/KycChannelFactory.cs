using Abp.Dependency;
using Abp.UI;
using System.Linq;

namespace ClientPlatform.Kyc.Channels
{
    /// <summary>
    /// KYC 渠道工厂实现
    /// 单例模式，负责解析和分发具体的渠道提供商
    /// </summary>
    public class KycChannelFactory : IKycChannelFactory, ISingletonDependency
    {
        private readonly IIocManager _iocManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="iocManager">IOC 管理器</param>
        public KycChannelFactory(IIocManager iocManager)
        {
            _iocManager = iocManager;
        }

        /// <summary>
        /// 获取指定名称的渠道提供商
        /// </summary>
        /// <param name="channelName">渠道代码 (如 KycChannelCodes.Sumsub)</param>
        /// <returns>提供商实例</returns>
        /// <exception cref="UserFriendlyException">如果找不到对应渠道则抛出异常</exception>
        public IKycChannelProvider GetProvider(string channelName)
        {
            // 解析所有注册的 IKycChannelProvider 实现
            // 这里我们遍历所有实现来查找匹配 ChannelName 的那个
            // 另一种方式是使用 IOC 的 Named 注册功能 Retrieve
            
            var providers = _iocManager.ResolveAll<IKycChannelProvider>();
            var provider = providers.FirstOrDefault(p => p.ChannelName == channelName);

            if (provider == null)
            {
                throw new UserFriendlyException($"KYC 渠道提供商 '{channelName}' 未找到或未注册。");
            }
            return provider;
        }
    }
}
