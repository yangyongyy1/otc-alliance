using ClientPlatform.Debugging;

namespace ClientPlatform;

public class ClientPlatformConsts
{
    public const string LocalizationSourceName = "ClientPlatform";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = false;

    public const string CacheNamespace = "ClientPlatform";


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "a62eee7b83e74d55803cb3064d26660d";

    /// <summary>
    /// 支持的现金通道代码
    /// </summary>
    public class VirtualAccountChannelCodes
    {
        public const string ZandBank = "ZandBank";
        public const string Rail = "Rail";
        public const string BCB = "BCB";
        public const string Stables = "Stables";
        public const string Weavr = "Weavr";
        public const string Mangopay = "Mangopay";
        
    }
}
