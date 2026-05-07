namespace ClientPlatform
{
    /// <summary>
    /// 错误代码常量
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// KYC 相关错误代码 (1xxx)
        /// </summary>
        public static class Kyc
        {
            /// <summary>
            /// KYC 未通过
            /// </summary>
            public const string NotApproved = "Kyc:NotApproved";

            /// <summary>
            /// KYC 未找到
            /// </summary>
            public const string NotFound = "Kyc:NotFound";
        }

        /// <summary>
        /// Pay 客户相关错误代码 (2xxx)
        /// </summary>
        public static class PayCustomer
        {
            /// <summary>
            /// 客户未创建
            /// </summary>
            public const string NotCreated = "PayCustomer:NotCreated";

            /// <summary>
            /// 客户状态异常
            /// </summary>
            public const string InvalidStatus = "PayCustomer:InvalidStatus";
        }

        /// <summary>
        /// 账户相关错误代码 (3xxx)
        /// </summary>
        public static class Account
        {
            /// <summary>
            /// 账户未找到
            /// </summary>
            public const string NotFound = "Account:NotFound";
        }

        /// <summary>
        /// 通用错误代码 (9xxx)
        /// </summary>
        public static class Common
        {
            /// <summary>
            /// 用户未登录
            /// </summary>
            public const string Unauthorized = "Common:Unauthorized";

            /// <summary>
            /// 用户未找到
            /// </summary>
            public const string UserNotFound = "Common:UserNotFound";

            /// <summary>
            /// 参数无效
            /// </summary>
            public const string InvalidParameter = "Common:InvalidParameter";

            /// <summary>
            /// 请求过于频繁
            /// </summary>
            public const string TooFrequent = "Common:TooFrequent";
        }
    }
}
