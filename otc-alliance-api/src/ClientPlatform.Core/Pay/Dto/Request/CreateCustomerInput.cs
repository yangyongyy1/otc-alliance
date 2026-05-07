using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ClientPlatform.Pay.Dto.Request;

/// <summary>
/// 创建客户输入参数
/// </summary>
public class CreateCustomerInput : BasePayRequest
{
    /// <summary>
    /// 外部用户 ID (商户端的唯一标识)
    /// </summary>
    [Required]
    [JsonProperty("out_user_id")]
    public string OutUserId { get; set; }

    /// <summary>
    /// 客户类型 (INDIVIDUAL 或 COMPANY)
    /// </summary>
    [Required]
    [JsonProperty("customer_type")]
    public string CustomerType { get; set; }

    /// <summary>
    /// 客户邮箱
    /// </summary>
    [Required]
    [JsonProperty("customer_email")]
    public string CustomerEmail { get; set; }

    /// <summary>
    /// 国家代码 (例如: DE)
    /// </summary>
    [Required]
    [JsonProperty("country_code")]
    public string CountryCode { get; set; }

    /// <summary>
    /// 名 (个人客户必填)
    /// </summary>
    [JsonProperty("first_name")]
    public string FirstName { get; set; }

    /// <summary>
    /// 中间名
    /// </summary>
    [JsonProperty("middle_name")]
    public string MiddleName { get; set; }

    /// <summary>
    /// 姓 (个人客户必填)
    /// </summary>
    [JsonProperty("last_name")]
    public string LastName { get; set; }

    /// <summary>
    /// 公司名称 (企业客户必填)
    /// </summary>
    [JsonProperty("company_name")]
    public string CompanyName { get; set; }

    /// <summary>
    /// 证件类型 (例如: PASSPORT)
    /// </summary>
    [JsonProperty("id_type")]
    public string IdType { get; set; }

    /// <summary>
    /// 证件号码
    /// </summary>
    [JsonProperty("id_number")]
    public string IdNumber { get; set; }

    /// <summary>
    /// 公司注册号 (企业客户必填)
    /// </summary>
    [JsonProperty("registration_number")]
    public string RegistrationNumber { get; set; }

    /// <summary>
    /// 公司成立日期 (企业客户必填)
    /// </summary>
    [JsonProperty("date_of_incorporation")]
    public string DateOfIncorporation { get; set; }

    /// <summary>
    /// 证件类型 (例如: PASSPORT)
    /// </summary>
    [JsonProperty("id_document_type")]
    public string IdDocumentType { get; set; }

    /// <summary>
    /// 证件号码
    /// </summary>
    [JsonProperty("id_document_number")]
    public string IdDocumentNumber { get; set; }

    /// <summary>
    /// 职业 (个人客户必填)
    /// </summary>
    [JsonProperty("occupation")]
    public string Occupation { get; set; }

    /// <summary>
    /// 证件正面 ID
    /// </summary>
    [JsonProperty("id_front_side_document_id")]
    public string IdFrontSideDocumentId { get; set; }

    /// <summary>
    /// 证件背面 ID
    /// </summary>
    [JsonProperty("id_back_side_document_id")]
    public string IdBackSideDocumentId { get; set; }

    /// <summary>
    /// 地址证明 ID
    /// </summary>
    [JsonProperty("address_document_id")]
    public string AddressDocumentId { get; set; }

    /// <summary>
    /// 资金来源证明 ID
    /// </summary>
    [JsonProperty("fund_source_document_id")]
    public string FundSourceDocumentId { get; set; }

    // Company Specific
    [JsonProperty("company_representative_name")]
    public string CompanyRepresentativeName { get; set; }

    [JsonProperty("company_representative_document_type")]
    public string CompanyRepresentativeDocumentType { get; set; }

    [JsonProperty("company_representative_number")]
    public string CompanyRepresentativeNumber { get; set; }

    [JsonProperty("company_document_id")]
    public string CompanyDocumentId { get; set; }

    [JsonProperty("company_handheld_document_id")]
    public string CompanyHandheldDocumentId { get; set; }

    // Missing Standard Fields
    /// <summary>
    /// 地址
    /// </summary>
    [JsonProperty("address")]
    public string Address { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [JsonProperty("city")]
    public string City { get; set; }

    /// <summary>
    /// 省/州
    /// </summary>
    [JsonProperty("state")]
    public string State { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    [JsonProperty("post_code")]
    public string PostCode { get; set; }

    /// <summary>
    /// 出生日期 (YYYY-MM-DD)
    /// </summary>
    [JsonProperty("date_of_birth")]
    public string DateOfBirth { get; set; }

    /// <summary>
    /// 描述（例如：用于传递平台侧的邀请代码 / 业务备注）
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// 电话号码
    /// </summary>
    [JsonProperty("phone")]
    public string Phone { get; set; }
}
