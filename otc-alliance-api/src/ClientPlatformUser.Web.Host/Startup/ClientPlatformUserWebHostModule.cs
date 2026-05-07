using Abp.AspNetCore.Configuration;
using Abp.Configuration.Startup;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ClientPlatform.Configuration;
using ClientPlatformUser.Application;

namespace ClientPlatform.Web.Host.Startup
{
    [DependsOn(
       typeof(ClientPlatformWebCoreModule)
        )
        ]

    public class ClientPlatformUserWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _appConfiguration;

        public ClientPlatformUserWebHostModule(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            //_appConfiguration = env.GetAppConfiguration();
            _appConfiguration = configuration;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ClientPlatformUserWebHostModule).GetAssembly());
            IocManager.RegisterAssemblyByConvention(typeof(ClientPlatformUserApplicationModule).GetAssembly());

        }

        public override void PreInitialize()
        {
            base.PreInitialize();
            Configuration.Modules.AbpWebCommon().SendAllExceptionsToClients = false;
            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(ClientPlatformUserApplicationModule).GetAssembly()
                 );
        }

        // MQ消费者通过 IQueueHandler<T> 接口自动注册
        // 参考: RabbitMq/Handlers/CreatePayChannelCustomerHandler.cs
    }
}
