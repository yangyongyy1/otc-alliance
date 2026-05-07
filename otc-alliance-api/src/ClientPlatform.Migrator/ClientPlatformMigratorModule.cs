using Abp.Events.Bus;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ClientPlatform.Configuration;
using ClientPlatform.EntityFrameworkCore;
using ClientPlatform.Migrator.DependencyInjection;
using Castle.MicroKernel.Registration;
using Microsoft.Extensions.Configuration;

namespace ClientPlatform.Migrator;

[DependsOn(typeof(ClientPlatformEntityFrameworkModule))]
public class ClientPlatformMigratorModule : AbpModule
{
    private readonly IConfigurationRoot _appConfiguration;

    public ClientPlatformMigratorModule(ClientPlatformEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbSeed = true;

        _appConfiguration = AppConfigurations.Get(
            typeof(ClientPlatformMigratorModule).GetAssembly().GetDirectoryPathOrNull()
        );
    }

    public override void PreInitialize()
    {
        Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
            ClientPlatformConsts.ConnectionStringName
        );

        Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        Configuration.ReplaceService(
            typeof(IEventBus),
            () => IocManager.IocContainer.Register(
                Component.For<IEventBus>().Instance(NullEventBus.Instance)
            )
        );
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ClientPlatformMigratorModule).GetAssembly());
        ServiceCollectionRegistrar.Register(IocManager);
    }
}
