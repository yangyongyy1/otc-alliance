using Abp.Localization;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Runtime.Security;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using ClientPlatform.Authorization.Roles;
using ClientPlatform.Authorization.Users;
using ClientPlatform.Configuration;
using ClientPlatform.Email;
using ClientPlatform.Localization;
using ClientPlatform.MultiTenancy;
using ClientPlatform.Timing;
using Castle.MicroKernel.Registration;
using ClientPlatform.Kyc;
using ClientPlatform.Kyc.Channels;
using ClientPlatform.Kyc.Channels.Sumsub;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.Net;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Abp.Configuration.Startup;
using ClientPlatform.Pay.Channels.SunPay.Builders;

namespace ClientPlatform;

[DependsOn(typeof(AbpZeroCoreModule))]
public class ClientPlatformCoreModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.Auditing.IsEnabledForAnonymousUsers = true;

        // Declare entity types
        Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
        Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
        Configuration.Modules.Zero().EntityTypes.User = typeof(User);

        ClientPlatformLocalizationConfigurer.Configure(Configuration.Localization);

        // Enable this line to create a multi-tenant application.
        Configuration.MultiTenancy.IsEnabled = ClientPlatformConsts.MultiTenancyEnabled;

        // Configure roles
        AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

        Configuration.Settings.Providers.Add<AppSettingProvider>();

        Configuration.Localization.Languages.Add(new LanguageInfo("fa", "فارسی", "famfamfam-flags ir"));

        Configuration.Settings.SettingEncryptionConfiguration.DefaultPassPhrase = ClientPlatformConsts.DefaultPassPhrase;
        SimpleStringCipher.DefaultPassPhrase = ClientPlatformConsts.DefaultPassPhrase;
    }

    public override void Initialize()
    {
        IocManager.RegisterAssemblyByConvention(typeof(ClientPlatformCoreModule).GetAssembly());

        // 注册邮件服务
        IocManager.Register<IAllianceMailService, AllianceMailService>(Abp.Dependency.DependencyLifeStyle.Singleton);

        // Register KYC Channel Factory


        // Register Sumsub Provider (Named)
        IocManager.IocContainer.Register(
            Component.For<IKycChannelProvider>()
                     .ImplementedBy<SumsubChannelProvider>()
                     .Named(KycChannelCodes.Sumsub)
                     .LifestyleTransient()
        );

        // Register SunPay Builders explicitly (Convention scanning seems to be failing or skipped)
        IocManager.IocContainer.Register(
            Component.For<ISunPayParamBuilder>()
                     .ImplementedBy<SunPayEurBuilder>()
                     .LifestyleTransient()
        );

        // Register VaCreationRequestMapper
        IocManager.Register<ClientPlatform.Pay.Mappers.VaCreationRequestMapper>(Abp.Dependency.DependencyLifeStyle.Transient);

        // Register RedLock
        RegisterRedLock();

    }

    private void RegisterRedLock()
    {
        IConfiguration appConfiguration;
        try
        {
            appConfiguration = IocManager.Resolve<IConfiguration>();
        }
        catch
        {
            // Fallback if IConfiguration is not registered (e.g. tests)
            appConfiguration = AppConfigurations.Get(typeof(ClientPlatformCoreModule).GetAssembly().GetDirectoryPathOrNull());
        }

        var redisConnectionString = appConfiguration["RedisCache:ConnectionString"];
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            redisConnectionString = appConfiguration["Abp:RedisCache:ConnectionString"];
        }

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            return;
        }

        // Use ConnectionMultiplexer to start connection. 
        // Note: For heavy usage, Singleton multiplexer is recommended.
        // RedLockFactory will manage it if we pass it in.
        var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        var distLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer> { new RedLockMultiplexer(multiplexer) });

        IocManager.IocContainer.Register(
            Component.For<RedLockNet.IDistributedLockFactory>()
                     .Instance(distLockFactory)
                     .LifestyleSingleton()
        );
    }


    public override void PostInitialize()
    {
        IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
    }
}
