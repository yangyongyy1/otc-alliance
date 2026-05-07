using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Dependency;

namespace ClientPlatform.Pay.Channels.SunPay.Builders
{
    public class SunPayVaBuilderFactory : ISingletonDependency
    {
        private readonly IEnumerable<ISunPayParamBuilder> _builders;

        // DI 容器会自动注入所有实现了 ISunPayParamBuilder 的类
        public SunPayVaBuilderFactory(IEnumerable<ISunPayParamBuilder> builders)
        {
            _builders = builders;
        }

        public ISunPayParamBuilder GetBuilder(string currency)
        {
            var builder = _builders.FirstOrDefault(b => b.CanHandle(currency));
            if (builder == null)
            {
                throw new NotSupportedException($"SunPay channel does not support currency: {currency}");
            }
            return builder;
        }
    }
}
