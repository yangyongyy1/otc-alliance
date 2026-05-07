using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.Authorization;

namespace ClientPlatformUser.Application
{

    [DependsOn(
        typeof(ClientPlatformCoreModule),
        typeof(AbpAutoMapperModule))]
    public class ClientPlatformUserApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            // Configuration.Authorization.Providers.Add<ClientPlatformAuthorizationProvider>();
        }


        public override void Initialize()
        {
            var thisAssembly = typeof(ClientPlatformUserApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
