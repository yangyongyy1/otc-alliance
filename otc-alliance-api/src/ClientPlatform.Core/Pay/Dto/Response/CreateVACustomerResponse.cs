using Newtonsoft.Json;
using System;

namespace ClientPlatform.Pay.Dto.Response;

/// <summary>
/// 创建客户响应
/// </summary>
public class CreateVACustomerResponse
{
    /// <summary>
    /// 平台用户 ID
    /// </summary>
    [JsonProperty("customer_id")]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    /// <summary>
    /// 商户用户 ID
    /// </summary>
    [JsonProperty("out_user_id")]
    public string OutUserId { get; set; }

    /// <summary>
    /// 名
    /// </summary>
    [JsonProperty("first_name")]
    public string FirstName { get; set; }

    /// <summary>
    /// 中间名
    /// </summary>
    [JsonProperty("middle_name")]
    public string MiddleName { get; set; }

    /// <summary>
    /// 姓
    /// </summary>
    [JsonProperty("last_name")]
    public string LastName { get; set; }

    /// <summary>
    /// 公司名称
    /// </summary>
    [JsonProperty("company_name")]
    public string CompanyName { get; set; }

    /// <summary>
    /// 国家代码
    /// </summary>
    [JsonProperty("country_code")]
    public string CountryCode { get; set; }

    /// <summary>
    /// 客户邮箱
    /// </summary>
    [JsonProperty("customer_email")]
    public string CustomerEmail { get; set; }

    /// <summary>
    /// 客户类型（INDIVIDUAL 或 COMPANY）
    /// </summary>
    [JsonProperty("customer_type")]
    public string CustomerType { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    /// <summary>
    /// 创建时间 (Unix Timestamp)
    /// </summary>
    [JsonProperty("creation_time")]
    public long? CreationTime { get; set; }

    /// <summary>
    /// 证件类型（如 passport、national_id）
    /// </summary>
    [JsonProperty("id_type")]
    public string IdType { get; set; }

    /// <summary>
    /// 证件号码
    /// </summary>
    [JsonProperty("id_number")]
    public string IdNumber { get; set; }

    /// <summary>
    /// 职业
    /// </summary>
    [JsonProperty("occupation")]
    public string Occupation { get; set; }

    /// <summary>
    /// 企业注册号
    /// </summary>
    [JsonProperty("registration_number")]
    public string RegistrationNumber { get; set; }

    /// <summary>
    /// 公司法人名称
    /// </summary>
    [JsonProperty("company_representative_name")]
    public string CompanyRepresentativeName { get; set; }

    /// <summary>
    /// 公司法人证件类型
    /// </summary>
    [JsonProperty("company_representative_document_type")]
    public string CompanyRepresentativeDocumentType { get; set; }

    /// <summary>
    /// 公司法人证件号码
    /// </summary>
    [JsonProperty("company_representative_number")]
    public string CompanyRepresentativeNumber { get; set; }

    #region 文件字段（个人/企业均有可能需要）

    /// <summary>
    /// 证件正面文件 ID（个人必需，企业法人也需要）
    /// </summary>
    [JsonProperty("id_front_side_document_id")]
    public string IdFrontSideDocumentId { get; set; }

    /// <summary>
    /// 证件反面文件 ID（个人必需，企业法人也需要）
    /// </summary>
    [JsonProperty("id_back_side_document_id")]
    public string IdBackSideDocumentId { get; set; }

    /// <summary>
    /// 地址证明文件 ID（例如水电费账单，部分国家必需）
    /// </summary>
    [JsonProperty("address_document_id")]
    public string AddressDocumentId { get; set; }

    /// <summary>
    /// 资金来源文件 ID
    /// </summary>
    [JsonProperty("fund_source_document_id")]
    public string FundSourceDocumentId { get; set; }

    /// <summary>
    /// 企业文件 ID（如营业执照）— 企业必填
    /// </summary>
    [JsonProperty("company_document_id")]
    public string CompanyDocumentId { get; set; }

    /// <summary>
    /// 企业手持证件照 ID — 可选
    /// </summary>
    [JsonProperty("company_handheld_document_id")]
    public string CompanyHandheldDocumentId { get; set; }

    /// <summary>
    /// 其他补充文件 ID（可选）
    /// </summary>
    [JsonProperty("other_id")]
    public string OtherId { get; set; }

    #endregion
}
