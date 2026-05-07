using Abp.Modules;
using Abp.Reflection.Extensions;
using ClientPlatform.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.AspNetCore.Configuration;

namespace ClientPlatform.Web.Host.Startup
{
    [DependsOn(
       typeof(ClientPlatformWebCoreModule))]
    public class ClientPlatformWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _appConfiguration;

        public ClientPlatformWebHostModule(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            //_appConfiguration = env.GetAppConfiguration();
            _appConfiguration = configuration;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(ClientPlatformWebHostModule).GetAssembly());
        }

        public override void PreInitialize()
        {
            base.PreInitialize();
            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(ClientPlatformApplicationModule).GetAssembly()
                 );
           
        }
    }
}
