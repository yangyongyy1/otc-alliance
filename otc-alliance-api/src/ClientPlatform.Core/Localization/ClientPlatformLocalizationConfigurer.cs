using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Reflection.Extensions;

namespace ClientPlatform.Localization;

public static class ClientPlatformLocalizationConfigurer
{
    public static void Configure(ILocalizationConfiguration localizationConfiguration)
    {
        localizationConfiguration.Sources.Add(
            new DictionaryBasedLocalizationSource(ClientPlatformConsts.LocalizationSourceName,
                new JsonEmbeddedFileLocalizationDictionaryProvider(
                    typeof(ClientPlatformLocalizationConfigurer).GetAssembly(),
                    "ClientPlatform.Localization.SourceFiles"
                )
            )
        );
    }
}
