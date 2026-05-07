using System.Collections.Generic;
using ClientPlatform.Pay.Models;

namespace ClientPlatform.Pay.Channels.SunPay
{
    /// <summary>
    /// SunPay 虚拟账户字段常量定义
    /// </summary>
    public static class SunPayFieldConstants
    {
        // =================================================================================
        // 1. 字段主表 (Master Dictionary)
        // =================================================================================

        /// <summary>
        /// 字段主定义表 (ID -> Definition)
        /// </summary>
        public static readonly Dictionary<int, FieldDefinition> MasterFields = new Dictionary<int, FieldDefinition>
        {
            // --- Basic Info ---
            { 1, new FieldDefinition { Key = "currency", Label = "Currency", Type = "text", Required = true } },
            { 2, new FieldDefinition { Key = "customer_id", Label = "Customer ID", Type = "text", Required = true, Description = "Internal Customer ID" } },
            { 3, new FieldDefinition { Key = "email", Label = "Email", Type = "text", Required = true, RegexPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$" } },
            { 4, new FieldDefinition { Key = "phone", Label = "Phone Number", Type = "text", Required = true } },
            { 5, new FieldDefinition { Key = "ip_address", Label = "IP Address", Type = "text", Required = false } },

            // --- Individual Info ---
            { 10, new FieldDefinition { Key = "first_name", Label = "First Name", Type = "text", Required = true } },
            { 11, new FieldDefinition { Key = "middle_name", Label = "Middle Name", Type = "text", Required = false } },
            { 12, new FieldDefinition { Key = "last_name", Label = "Last Name", Type = "text", Required = true } },
            { 13, new FieldDefinition { Key = "birth_date", Label = "Date of Birth", Type = "date", Required = true } },
            { 14, new FieldDefinition { Key = "birth_country", Label = "Country of Birth", Type = "select", Required = true } },
            { 15, new FieldDefinition { Key = "nationality", Label = "Nationality", Type = "select", Required = true } },
            { 16, new FieldDefinition { Key = "id_number", Label = "Document Number", Type = "text", Required = true } },
            { 17, new FieldDefinition { Key = "id_document_type", Label = "Document Type", Type = "select", Required = true, Options = CreateOptions("PASSPORT,NATIONAL_ID,DRIVERS_LICENSE") } },
            { 18, new FieldDefinition { Key = "id_issue_date", Label = "Issue Date", Type = "date", Required = false } },
            { 19, new FieldDefinition { Key = "id_expiration_date", Label = "Expiry Date", Type = "date", Required = false } },

            // --- Company Info ---
            { 20, new FieldDefinition { Key = "company_name", Label = "Company Name", Type = "text", Required = true } },
            { 21, new FieldDefinition { Key = "company_type", Label = "Company Type", Type = "select", Required = true, Options = CreateOptions("LIMITED_LIABILITY,SOLE_TRADER,PARTNERSHIP,OTHER") } },
            { 22, new FieldDefinition { Key = "registration_number", Label = "Registration Number", Type = "text", Required = true } },
            { 23, new FieldDefinition { Key = "company_registration_date", Label = "Incorporation Date", Type = "date", Required = true } },
            { 24, new FieldDefinition { Key = "country_of_incorporation", Label = "Country of Incorporation", Type = "select", Required = true } },
            { 25, new FieldDefinition { Key = "company_website_address", Label = "Website", Type = "text", Required = false } },
            { 26, new FieldDefinition { Key = "company_tax_reference_number", Label = "Tax Reference Number", Type = "text", Required = false } },
            { 27, new FieldDefinition { Key = "company_naics", Label = "NAICS Code", Type = "text", Required = false } },
            
            // --- Common Address ---
            { 30, new FieldDefinition { Key = "address_line", Label = "Address Line", Type = "text", Required = true } },
            { 31, new FieldDefinition { Key = "city", Label = "City", Type = "text", Required = true } },
            { 32, new FieldDefinition { Key = "post_code", Label = "Postal Code", Type = "text", Required = true } },
            { 33, new FieldDefinition { Key = "country_code", Label = "Country Code", Type = "select", Required = true } },
            { 34, new FieldDefinition { Key = "state", Label = "State / Province", Type = "text", Required = false } },

            // --- Extended / Special Fields ---
            { 40, new FieldDefinition { Key = "bank_country_code", Label = "Bank Account Country", Type = "select", Required = false, Description = "For Stables IBAN selection" } },
            { 50, new FieldDefinition { Key = "employment_status", Label = "Employment Status", Type = "select", Required = false } },
            { 51, new FieldDefinition { Key = "occupation", Label = "Occupation", Type = "text", Required = false } },
            { 52, new FieldDefinition { Key = "source_of_funds", Label = "Source of Funds", Type = "select", Required = false } },
            
            // --- File Uploads (IDs 300+) ---
            // 前端识别 file_upload_group 后自动关联这些 ID
            { 300, new FieldDefinition { Key = "passport_document_id", Label = "Passport", Type = "text", Required = false } },
            { 301, new FieldDefinition { Key = "drivers_licence_front_document_id", Label = "Driving Licence Front", Type = "text", Required = false } },
            { 302, new FieldDefinition { Key = "drivers_licence_back_document_id", Label = "Driving Licence Back", Type = "text", Required = false } },
            { 303, new FieldDefinition { Key = "utility_bill_document_id", Label = "Utility Bill (Proof of Address)", Type = "text", Required = false } },
            { 304, new FieldDefinition { Key = "bank_statement_document_id", Label = "Bank Statement", Type = "text", Required = false } },
            { 305, new FieldDefinition { Key = "selfie_document_id", Label = "Selfie", Type = "text", Required = false } },
            { 306, new FieldDefinition { Key = "company_incorporation_document_id", Label = "Certificate of Incorporation", Type = "text", Required = false } },
            { 307, new FieldDefinition { Key = "company_articles_of_incorporation_document_id", Label = "Articles of Incorporation", Type = "text", Required = false } },
            { 308, new FieldDefinition { Key = "company_beneficial_ownership_certificate_document_id", Label = "Beneficial Ownership Certificate", Type = "text", Required = false } },
            { 309, new FieldDefinition { Key = "company_bank_statement_document_id", Label = "Company Bank Statement", Type = "text", Required = false } }
        };

        // =================================================================================
        // 2. 渠道配置 (Channel Profiles)
        // =================================================================================
        /// <summary>
        /// 渠道字段 ID 组合配置 (ChannelCode -> Field IDs)
        /// </summary>
        public static readonly Dictionary<string, int[]> ChannelProfiles = new Dictionary<string, int[]>
        {
            // EUR - BCB 
            // 基础 + 个人 + 地址 + 文件
            {
                ClientPlatformConsts.VirtualAccountChannelCodes.BCB.ToUpper(),
                new[] {
                    1, 3, // currency, email (Phone removed)
                    10, 11, 12, // first, middle, last (Removed: birth_date, id fields)
                    32, 33 // post_code, country_code (Removed: address_line, city)
                    // Removed: Docs
                }
            },

            // EUR - Stables
            // 基础 + 个人 + 地址 + BankCountryCode + 文件
            {
                ClientPlatformConsts.VirtualAccountChannelCodes.Stables.ToUpper(),
                new[] {
                    1, 3, // currency, email (Phone removed)
                    10, 11, 12, 13, 15, // first, middle, last, birth_date, nationality (Removed: birth_country, id_number, id_doc_types)
                    30, 31, 32, 33 // address_line, city, post_code(Added), country_code
                    // Removed: 40 (bank_country_code), 300, 305 (Docs) - Not in Stables JSON
                }
            },

            // EUR - Weavr
            // 按 Weavr 开户必填字段：currency, customer_id, country_code, first/middle/last, email, post_code, city, address_line, birth_date, nationality
            {
                ClientPlatformConsts.VirtualAccountChannelCodes.Weavr.ToUpper(),
                new[] {
                    1, 2, 3, // currency, customer_id, email
                    10, 11, 12, 13, 15, // first, middle, last, birth_date, nationality
                    30, 31, 32, 33 // address_line, city, post_code, country_code
                }
            },

            // EUR - Mangopay
            // 按 Mangopay 开户必填字段：currency, customer_id, country_code, first/middle/last, email, post_code, city, address_line, birth_date, nationality
            {
                ClientPlatformConsts.VirtualAccountChannelCodes.Mangopay.ToUpper(),
                new[] {
                    1, 2, 3, // currency, customer_id, email
                    10, 11, 12, 13, 15, // first, middle, last, birth_date, nationality
                    30, 31, 32, 33 // address_line, city, post_code, country_code
                }
            },

            // USD (Example Placeholder)
             {
                "SUNPAY_USD",
                new[] {
                    1, 3, 4,
                    10, 11, 12, 13, 14, 15, // Individual
                    30, 31, 32, 33, 34, // Address (+State)
                    50, 52, // Employment, SourceOfFunds
                    300, 304 // Passport + BankStatement
                }
            }
        };

        // =================================================================================
        // 3. UI 分组 (UI Groups)
        // =================================================================================
        // 前端可复用的分组定义
        /// <summary>
        /// UI 分组定义
        /// </summary>
        public static readonly List<object> GroupDefinitions = new List<object>
        {
            new { Id = "BASIC_INFO", Title = "Basic Info", Fields = new[] { 1, 3, 4 } },
            new { Id = "INDIVIDUAL_INFO", Title = "Individual Info", Fields = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 } },
            new { Id = "ADDRESS_INFO", Title = "Address Info", Fields = new[] { 30, 31, 32, 33, 34, 40 } },
            new { Id = "UPLOAD_DOCS", Title = "Upload Documents", Type = "file_upload_group", Fields = new[] { 300, 301, 302, 303, 304, 305 } },
            new { Id = "COMPANY_INFO", Title = "Company Info", Condition = new { Field = "account_type", Value = "COMPANY" }, Fields = new[] { 20, 21, 22, 23, 24, 25, 26, 27, 306, 307, 308, 309 } }
        };

        public static class CustomerStatus
        {
            /// <summary>
            /// 审核中,表示商家用户的状态处于待确认阶段，可能需要进一步审核或验证
            /// </summary>
            public const string PENDING = "PENDDING";

            /// <summary>
            /// 正常,表示商家用户状态正常，可以正常使用账户进行操作
            /// </summary>
            public const string ACTIVE = "ACTIVE";

            /// <summary>
            /// 冻结,表示商家用户账户被冻结，可能由于异常活动或安全原因暂时无法使用
            /// </summary>
            public const string FROZEN = "FROZEN";

            /// <summary>
            /// 禁用,表示商家用户账户被禁用，通常是永久性限制，无法再使用
            /// </summary>
            public const string DISABLE = "DISABLE";

            /// <summary>
            /// 未通过,表示商家用户未通过审核或验证，账户可能无法激活或使用
            /// </summary>
            public const string FAILED = "FAILED";

            /// <summary>
            /// 待审核 这是渠道的创建用户的状态
            /// </summary>
            public const string REVIEWING = "REVIEWING";
        }

        private static List<FieldOption> CreateOptions(string optionsCsv)
        {
            var list = new List<FieldOption>();
            if (string.IsNullOrEmpty(optionsCsv)) return list;

            foreach (var opt in optionsCsv.Split(','))
            {
                var val = opt.Trim();
                list.Add(new FieldOption { Label = val, Value = val });
            }
            return list;
        }
    }
}
