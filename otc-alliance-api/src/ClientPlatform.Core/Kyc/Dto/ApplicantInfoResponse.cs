using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClientPlatform.Kyc.Dto
{
    /// <summary>
    /// Sumsub GetApplicantsInfo API 响应
    /// </summary>
    public class ApplicantInfoResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("inspectionId")]
        public string InspectionId { get; set; }

        [JsonProperty("externalUserId")]
        public string ExternalUserId { get; set; }

        [JsonProperty("info")]
        public ApplicantInfo Info { get; set; }

        [JsonProperty("fixedInfo")]
        public ApplicantFixedInfo FixedInfo { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("applicantPlatform")]
        public string ApplicantPlatform { get; set; }

        [JsonProperty("ipCountry")]
        public string IpCountry { get; set; }

        [JsonProperty("agreement")]
        public ApplicantAgreement Agreement { get; set; }

        [JsonProperty("requiredIdDocs")]
        public RequiredIdDocs RequiredIdDocs { get; set; }

        [JsonProperty("review")]
        public ApplicantReview Review { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("questionnaires")]
        public List<Questionnaire> Questionnaires { get; set; }

        [JsonProperty("notes")]
        public List<object> Notes { get; set; }

        // 自定义属性：标识API调用是否成功
        public bool Success { get; set; }
    }

    /// <summary>
    /// 申请人基础信息
    /// </summary>
    public class ApplicantInfo
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("firstNameEn")]
        public string FirstNameEn { get; set; }

        [JsonProperty("middleName")]
        public string MiddleName { get; set; }

        [JsonProperty("middleNameEn")]
        public string MiddleNameEn { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("lastNameEn")]
        public string LastNameEn { get; set; }

        [JsonProperty("dob")]
        public string Dob { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("idDocs")]
        public List<IdDocument> IdDocs { get; set; }
    }

    /// <summary>
    /// ID 文档信息
    /// </summary>
    public class IdDocument
    {
        [JsonProperty("idDocType")]
        public string IdDocType { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("firstNameEn")]
        public string FirstNameEn { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("lastNameEn")]
        public string LastNameEn { get; set; }

        [JsonProperty("validUntil")]
        public string ValidUntil { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("dob")]
        public string Dob { get; set; }

        [JsonProperty("mrzLine1")]
        public string MrzLine1 { get; set; }

        [JsonProperty("mrzLine2")]
        public string MrzLine2 { get; set; }

        [JsonProperty("termless")]
        public bool Termless { get; set; }
    }

    /// <summary>
    /// 修正后的申请人信息
    /// </summary>
    public class ApplicantFixedInfo
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("middleName")]
        public string MiddleName { get; set; }

        [JsonProperty("firstNameEn")]
        public string FirstNameEn { get; set; }

        [JsonProperty("middleNameEn")]
        public string MiddleNameEn { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("lastNameEn")]
        public string LastNameEn { get; set; }

        [JsonProperty("dob")]
        public string Dob { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("taxResidenceCountry")]
        public string TaxResidenceCountry { get; set; }

        [JsonProperty("nationality")]
        public string Nationality { get; set; }

        [JsonProperty("addresses")]
        public List<ApplicantAddress> Addresses { get; set; }
    }

    /// <summary>
    /// 地址信息
    /// </summary>
    public class ApplicantAddress
    {
        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("streetEn")]
        public string StreetEn { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("stateEn")]
        public string StateEn { get; set; }

        [JsonProperty("buildingNumber")]
        public string BuildingNumber { get; set; }

        [JsonProperty("town")]
        public string Town { get; set; }

        [JsonProperty("townEn")]
        public string TownEn { get; set; }

        [JsonProperty("postCode")]
        public string PostCode { get; set; }

        [JsonProperty("formattedAddress")]
        public string FormattedAddress { get; set; }
    }

    /// <summary>
    /// 协议信息
    /// </summary>
    public class ApplicantAgreement
    {
        [JsonProperty("items")]
        public List<AgreementItem> Items { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("acceptedAt")]
        public string AcceptedAt { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("recordIds")]
        public List<string> RecordIds { get; set; }
    }

    /// <summary>
    /// 协议项
    /// </summary>
    public class AgreementItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("acceptedAt")]
        public string AcceptedAt { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("recordIds")]
        public List<string> RecordIds { get; set; }
    }

    /// <summary>
    /// 必需的ID文档配置
    /// </summary>
    public class RequiredIdDocs
    {
        [JsonProperty("excludedCountries")]
        public List<string> ExcludedCountries { get; set; }

        [JsonProperty("docSets")]
        public List<DocSet> DocSets { get; set; }
    }

    /// <summary>
    /// 文档集配置
    /// </summary>
    public class DocSet
    {
        [JsonProperty("idDocSetType")]
        public string IdDocSetType { get; set; }

        [JsonProperty("fields")]
        public List<DocSetField> Fields { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; }

        [JsonProperty("videoRequired")]
        public string VideoRequired { get; set; }

        [JsonProperty("nfcVerificationSettings")]
        public NfcVerificationSettings NfcVerificationSettings { get; set; }

        [JsonProperty("questionnaireDefId")]
        public string QuestionnaireDefId { get; set; }
    }

    /// <summary>
    /// 文档字段配置
    /// </summary>
    public class DocSetField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("prefill")]
        public string Prefill { get; set; }

        [JsonProperty("immutableIfPresent")]
        public bool? ImmutableIfPresent { get; set; }
    }

    /// <summary>
    /// NFC 验证设置
    /// </summary>
    public class NfcVerificationSettings
    {
        [JsonProperty("mode")]
        public string Mode { get; set; }
    }

    /// <summary>
    /// 审核信息
    /// </summary>
    public class ApplicantReview
    {
        [JsonProperty("reviewId")]
        public string ReviewId { get; set; }

        [JsonProperty("attemptId")]
        public string AttemptId { get; set; }

        [JsonProperty("attemptCnt")]
        public int AttemptCnt { get; set; }

        [JsonProperty("elapsedSincePendingMs")]
        public long ElapsedSincePendingMs { get; set; }

        [JsonProperty("elapsedSinceQueuedMs")]
        public long ElapsedSinceQueuedMs { get; set; }

        [JsonProperty("reprocessing")]
        public bool Reprocessing { get; set; }

        [JsonProperty("levelName")]
        public string LevelName { get; set; }

        [JsonProperty("levelAutoCheckMode")]
        public string LevelAutoCheckMode { get; set; }

        [JsonProperty("createDate")]
        public string CreateDate { get; set; }

        [JsonProperty("reviewDate")]
        public string ReviewDate { get; set; }

        [JsonProperty("reviewResult")]
        public ReviewResult ReviewResult { get; set; }

        [JsonProperty("reviewStatus")]
        public string ReviewStatus { get; set; }

        [JsonProperty("priority")]
        public decimal Priority { get; set; }
    }

    /// <summary>
    /// 审核结果
    /// </summary>
    public class ReviewResult
    {
        [JsonProperty("reviewAnswer")]
        public string ReviewAnswer { get; set; }

        [JsonProperty("reviewRejectType")]
        public string ReviewRejectType { get; set; }

        [JsonProperty("moderationComment")]
        public string ModerationComment { get; set; }

        [JsonProperty("clientComment")]
        public string ClientComment { get; set; }

        [JsonProperty("rejectLabels")]
        public List<string> RejectLabels { get; set; }
    }

    /// <summary>
    /// 问卷调查
    /// </summary>
    public class Questionnaire
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("sections")]
        public Dictionary<string, QuestionnaireSection> Sections { get; set; }

        [JsonProperty("score")]
        public decimal Score { get; set; }

        [JsonProperty("completionLevel")]
        public string CompletionLevel { get; set; }
    }

    /// <summary>
    /// 问卷章节
    /// </summary>
    public class QuestionnaireSection
    {
        [JsonProperty("score")]
        public decimal Score { get; set; }

        [JsonProperty("items")]
        public Dictionary<string, QuestionnaireItem> Items { get; set; }
    }

    /// <summary>
    /// 问卷项
    /// </summary>
    public class QuestionnaireItem
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
