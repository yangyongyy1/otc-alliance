using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Extensions;
using ClientPlatform.BasicDataManagement;
using ClientPlatform.Kyc;
using ClientPlatform.Pay.Dto.Request;

namespace ClientPlatform.Pay.Channels.SunPay.Builders
{
    /// <summary>
    /// SunPay 客户创建参数构建器
    /// 负责将各个来源（如 KYC 申请信息）转换为 SunPay API 所需的 CreateCustomerInput
    /// </summary>
    public class SunPayCustomerInputBuilder : ITransientDependency
    {
        private readonly IRepository<CountryInfo, int> _countryInfoRepository;

        public SunPayCustomerInputBuilder(IRepository<CountryInfo, int> countryInfoRepository)
        {
            _countryInfoRepository = countryInfoRepository;
        }

        /// <summary>
        /// 逻辑文档类型常量
        /// </summary>
        public static class DocTypes
        {
            /// <summary>身份证件正面</summary>
            public const string IdFront = "ID_FRONT";
            /// <summary>身份证件背面</summary>
            public const string IdBack = "ID_BACK";
            /// <summary>企业注册文件</summary>
            public const string CompanyDoc = "COMPANY_DOC";
            /// <summary>地址证明</summary>
            public const string Address = "ADDRESS";
            /// <summary>资金来源证明</summary>
            public const string FundSource = "FUND_SOURCE";
        }

        /// <summary>
        /// 根据 KYC 申请信息构建 CreateCustomerInput
        /// </summary>
        /// <param name="kyc">KYC 申请实体</param>
        /// <param name="fileIdMap">文件映射 (逻辑类型 -> SunPay文件ID)</param>
        /// <param name="overrides">用户手动输入的覆盖值 (Key=SnakeCase)</param>
        /// <returns>构建好的输入对象</returns>
        public CreateCustomerInput Build(KycApplicant kyc, Dictionary<string, string> fileIdMap, Dictionary<string, string> overrides = null)
        {
            if (kyc == null) throw new ArgumentNullException(nameof(kyc));
            fileIdMap ??= new Dictionary<string, string>();
            overrides ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // 1. 基础信息映射
            var input = new CreateCustomerInput
            {
                OutUserId = kyc.UserId.ToString(),
                CustomerEmail = kyc.Email ?? kyc.User?.Email, // 优先使用申请时的邮箱
                CountryCode = ConvertToIso2CountryCode(kyc.Country), // 转换为 ISO-2 格式
                CustomerType = MapCustomerType(kyc.ApplicantType),
                // 将平台侧的邀请代码传递给 SunPay 作为描述字段
                Description = kyc.User?.InviteCode
            };

            // 2. 根据客户类型分别处理
            if (input.CustomerType == "COMPANY")
            {
                BuildCompanyFields(input, kyc, fileIdMap);
            }
            else
            {
                BuildIndividualFields(input, kyc, fileIdMap);
            }

            // 3. 应用用户手动输入的覆盖值
            ApplyOverrides(input, overrides);

            return input;
        }

        private void ApplyOverrides(CreateCustomerInput input, Dictionary<string, string> overrides)
        {
            if (overrides == null || overrides.Count == 0) return;

            string GetValue(string key) => overrides.TryGetValue(key, out var val) ? val : null;

            // Common Fields
            var email = GetValue("email");
            if (!string.IsNullOrWhiteSpace(email)) input.CustomerEmail = email;

            // Individual Fields
            var firstName = GetValue("first_name");
            if (!string.IsNullOrWhiteSpace(firstName)) input.FirstName = firstName;

            var lastName = GetValue("last_name");
            if (!string.IsNullOrWhiteSpace(lastName)) input.LastName = lastName;

            var middleName = GetValue("middle_name");
            if (!string.IsNullOrWhiteSpace(middleName)) input.MiddleName = middleName;

            var birthDate = GetValue("birth_date"); // Format: yyyy-MM-dd or yyyy-MM-dd HH:mm:ss
            if (!string.IsNullOrWhiteSpace(birthDate))
            {
                // Try parsing needed? Usually API accepts string if format matches, but input.DateOfBirth is string?
                // Let's assume input format is compliant or cleaning happened upstream.
                // Ideally check format "yyyy-MM-dd"
                input.DateOfBirth = birthDate.Split(' ')[0]; // Take date part
            }

            var occupation = GetValue("occupation");
            if (!string.IsNullOrWhiteSpace(occupation)) input.Occupation = occupation;

            var address = GetValue("address_line");
            if (!string.IsNullOrWhiteSpace(address)) input.Address = address;

            var city = GetValue("city");
            if (!string.IsNullOrWhiteSpace(city)) input.City = city;

            var postCode = GetValue("post_code");
            if (!string.IsNullOrWhiteSpace(postCode)) input.PostCode = postCode;

            // Company Fields (if applicable)
            var companyName = GetValue("company_name");
            if (!string.IsNullOrWhiteSpace(companyName)) input.CompanyName = companyName;

            var registrationNumber = GetValue("registration_number");
            if (!string.IsNullOrWhiteSpace(registrationNumber)) input.RegistrationNumber = registrationNumber;

            // Add other fields as necessary based on frontend form definitions
        }

        /// <summary>
        /// 识别所需的 KYC 文档并返回逻辑映射
        /// 调用者应根据此列表上传文件，生成 map 传给 Build 方法
        /// </summary>
        /// <param name="kyc">KYC 申请实体</param>
        /// <param name="docs">KYC 文档列表</param>
        /// <returns>逻辑类型 -> 文档实体 映射</returns>
        public Dictionary<string, KycApplicantDocument> IdentifyDocuments(KycApplicant kyc, List<KycApplicantDocument> docs)
        {
            var result = new Dictionary<string, KycApplicantDocument>();
            if (docs == null || !docs.Any()) return result;

            var isCompany = MapCustomerType(kyc.ApplicantType) == "COMPANY";
            var idDocType = kyc.IdDocType;

            // 1. 身份文档 (Front/Back)
            // 筛选出符合类型的文件
            var idDocs = docs.Where(d => IsMatchingDocType(d.IdDocType, idDocType)).ToList();

            // 假设列表顺序：第一个是 Front，第二个是 Back
            if (idDocs.Any())
            {
                result[DocTypes.IdFront] = idDocs.First();
                if (idDocs.Count > 1 && !IsPassport(idDocType))
                {
                    result[DocTypes.IdBack] = idDocs.Skip(1).First();
                }
            }

            // 2. 企业文档
            if (isCompany)
            {
                // 查找公司注册文件 (类型包含 COMPANY 或 REGISTRATION)
                var companyDoc = docs.FirstOrDefault(d =>
                    (d.IdDocType?.ToUpper().Contains("COMPANY") == true) ||
                    (d.IdDocType?.ToUpper().Contains("REGISTRATION") == true) ||
                    (d.IdDocType?.ToUpper().Contains("ARTICLE") == true)); // Articles of Association

                if (companyDoc != null)
                {
                    result[DocTypes.CompanyDoc] = companyDoc;
                }
            }

            // 3. 地址证明 (可选)
            var addressDoc = docs.FirstOrDefault(d => d.IdDocType?.ToUpper().Contains("ADDRESS") == true || d.IdDocType?.ToUpper().Contains("UTILITY") == true);
            if (addressDoc != null)
            {
                result[DocTypes.Address] = addressDoc;
            }

            return result;
        }

        private void BuildCompanyFields(CreateCustomerInput input, KycApplicant kyc, Dictionary<string, string> fileIdMap)
        {
            input.CompanyName = kyc.CompanyName;
            input.RegistrationNumber = kyc.CompanyRegistrationNumber;

            // 代表人信息 (通常 KYB 申请人即为代表)
            input.CompanyRepresentativeName = $"{kyc.FirstName} {kyc.LastName}".Trim();
            input.CompanyRepresentativeDocumentType = MapIdDocType(kyc.IdDocType);
            input.CompanyRepresentativeNumber = kyc.IdDocNumber;

            // 填充文件 ID
            if (fileIdMap.TryGetValue(DocTypes.CompanyDoc, out var companyDocId)) input.CompanyDocumentId = companyDocId;
            if (fileIdMap.TryGetValue(DocTypes.IdFront, out var frontId)) input.IdFrontSideDocumentId = frontId;
            if (fileIdMap.TryGetValue(DocTypes.IdBack, out var backId)) input.IdBackSideDocumentId = backId;
        }

        private void BuildIndividualFields(CreateCustomerInput input, KycApplicant kyc, Dictionary<string, string> fileIdMap)
        {
            // 优先使用修正后的名字（Sumsub 审核后确认的）
            input.FirstName = !string.IsNullOrEmpty(kyc.FixedFirstName) ? kyc.FixedFirstName : kyc.FirstName;
            input.LastName = !string.IsNullOrEmpty(kyc.FixedLastName) ? kyc.FixedLastName : kyc.LastName;
            input.MiddleName = !string.IsNullOrEmpty(kyc.FixedMiddleName) ? kyc.FixedMiddleName : kyc.MiddleName;

            // 证件信息
            input.IdDocumentType = MapIdDocType(kyc.IdDocType);
            input.IdDocumentNumber = kyc.IdDocNumber;

            // 职业信息（如果没有则使用默认值 "Others"）
            input.Occupation = !string.IsNullOrEmpty(kyc.EmploymentStatus) ? kyc.EmploymentStatus : "Others";

            // 填充文件 ID
            if (fileIdMap.TryGetValue(DocTypes.IdFront, out var frontId)) input.IdFrontSideDocumentId = frontId;
            if (fileIdMap.TryGetValue(DocTypes.IdBack, out var backId)) input.IdBackSideDocumentId = backId;
            if (fileIdMap.TryGetValue(DocTypes.Address, out var addressId)) input.AddressDocumentId = addressId;
        }

        private string MapCustomerType(string kycApplicantType)
        {
            if (kycApplicantType?.Equals("company", StringComparison.OrdinalIgnoreCase) == true)
                return "COMPANY";
            return "INDIVIDUAL";
        }

        private string MapIdDocType(string sumsubDocType)
        {
            if (string.IsNullOrEmpty(sumsubDocType)) return "IDCARD";

            var type = sumsubDocType.ToUpper();
            if (type.Contains("PASSPORT")) return "PASSPORT";
            if (type.Contains("DRIV") || type.Contains("DL")) return "DRIVINGLICENCE";

            return "IDCARD"; // 默认
        }

        private bool IsMatchingDocType(string docType, string targetType)
        {
            if (string.IsNullOrEmpty(docType)) return false;
            // 简单包含匹配
            if (targetType != null && docType.ToUpper().Contains(targetType.ToUpper())) return true;
            // 通用匹配
            if (targetType == "IDCARD" && docType.ToUpper().Contains("ID_CARD")) return true;
            return false;
        }

        private bool IsPassport(string type)
        {
            return type?.ToUpper().Contains("PASSPORT") == true;
        }

        /// <summary>
        /// 将国家代码转换为 ISO-2 格式 (2 字母)
        /// 支持输入：ISO-2 (DE), ISO-3 (DEU), 或完整名称
        /// </summary>
        private string ConvertToIso2CountryCode(string input)
        {
            if (string.IsNullOrEmpty(input)) return "DE"; // 默认兜底

            // 如果已经是 2 位，直接返回（假设有效）
            if (input.Length == 2) return input.ToUpper();

            // 尝试从数据库查询（按 CCA3 查找）
            if (input.Length == 3)
            {
                var country = _countryInfoRepository.GetAll()
                    .FirstOrDefault(c => c.CCA3 == input.ToUpper());
                if (country != null && !string.IsNullOrEmpty(country.CCA2))
                {
                    return country.CCA2;
                }
            }

            // 尝试按名称查找
            var countryByName = _countryInfoRepository.GetAll()
                .FirstOrDefault(c => c.Name.Equals(input, StringComparison.OrdinalIgnoreCase) ||
                                     c.CNName.Equals(input, StringComparison.OrdinalIgnoreCase));
            if (countryByName != null && !string.IsNullOrEmpty(countryByName.CCA2))
            {
                return countryByName.CCA2;
            }

            // 兜底：截取前 2 位（可能不准确，建议记录日志）
            return input.Length >= 2 ? input.Substring(0, 2).ToUpper() : "DE";
        }
    }
}
