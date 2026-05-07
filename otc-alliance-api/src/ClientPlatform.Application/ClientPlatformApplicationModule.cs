using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ClientPlatform.Authorization;

namespace ClientPlatform;

[DependsOn(
    typeof(ClientPlatformCoreModule),
    typeof(AbpAutoMapperModule))]
public class ClientPlatformApplicationModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Authorization.Providers.Add<ClientPlatformAuthorizationProvider>();
    }

    public override void Initialize()
    {
        var thisAssembly = typeof(ClientPlatformApplicationModule).GetAssembly();

        IocManager.RegisterAssemblyByConvention(thisAssembly);

        Configuration.Modules.AbpAutoMapper().Configurators.Add(
            // Scan the assembly for classes which inherit from AutoMapper.Profile
            cfg => cfg.AddMaps(thisAssembly)
        );
    }
}
