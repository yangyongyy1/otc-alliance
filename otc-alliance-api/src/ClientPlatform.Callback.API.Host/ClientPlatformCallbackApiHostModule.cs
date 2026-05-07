using Abp.Modules;
using Abp.Reflection.Extensions;
using Microsoft.Extensions.Configuration;

namespace ClientPlatform.Callback.API.Host
{
    [DependsOn(
       typeof(ClientPlatformWebCoreModule))]
    public class ClientPlatformCallbackApiHostModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ClientPlatformCallbackApiHostModule).GetAssembly());
        }
    }
}
