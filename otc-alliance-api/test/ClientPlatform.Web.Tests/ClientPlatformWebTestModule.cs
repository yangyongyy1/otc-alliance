using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using ClientPlatform.EntityFrameworkCore;
using ClientPlatform.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace ClientPlatform.Web.Tests;

[DependsOn(
    typeof(ClientPlatformWebMvcModule),
    typeof(AbpAspNetCoreTestBaseModule)
)]
public class ClientPlatformWebTestModule : AbpModule
{
    public ClientPlatformWebTestModule(ClientPlatformEntityFrameworkModule abpProjectNameEntityFrameworkModule)
    {
        abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
    }

    public override void PreInitialize()
    {
        Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ClientPlatformWebTestModule).GetAssembly());
    }

    public override void PostInitialize()
    {
        IocManager.Resolve<ApplicationPartManager>()
            .AddApplicationPartsIfNotAddedBefore(typeof(ClientPlatformWebMvcModule).Assembly);
    }
}