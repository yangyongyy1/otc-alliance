using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using ClientPlatform.Authorization.Users;
using ClientPlatform;
using ClientPlatform.UserManagement;

namespace ClientPlatform.Kyc
{
    /// <summary>
    /// 用户 KYC 申请记录实体
    /// 用于存储用户在 KYC 提供商（如 Sumsub）处的申请状态和详细信息
    /// </summary>
    [Table("AppKycApplicants")]
    public class KycApplicant : FullAuditedEntity<Guid>
    {

        /// <summary>
        /// 关联的用户 ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 关联的用户实体
        /// </summary>
        [ForeignKey("UserId")]
        public virtual ClientUser User { get; set; }

        /// <summary>
        /// KYC 提供商名称 (例如: Sumsub)
        /// </summary>
        public string KycProvider { get; set; }

        /// <summary>
        /// KYC 产品类型 (WebSDK, Link, API)
        /// </summary>
        public KycChannelProductTypes KycChannelProductTypes { get; set; }

        /// <summary>
        /// KYC 类型 (个人/企业)
        /// </summary>
        public BusinessUserType KycType { get; set; }

        /// <summary>
        /// 认证标准等级 (用于区分不同额度或权限的认证等级，对应 AuthStandardLevel 枚举)
        /// </summary>
        [Column("KycLevelName")]
        public AuthStandardLevel AuthStandardLevel { get; set; }

        /// <summary>
        /// KYC 验证链接 (提供给前端跳转)
        /// </summary>
        public string KycVerificationLink { get; set; }

        /// <summary>
        /// 平台业务状态 (统一状态，如: 待提交, 审核中,通过, 拒绝)
        /// </summary>
        public KycBizStatus KycBizStatus { get; set; }

        /// <summary>
        /// 是否为已关闭/历史记录（true 表示该条记录已被重置或废弃）
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// 渠道原始状态字符串 (存储提供商返回的原始状态，如: pending, approved)
        /// </summary>
        public string KycChannelStatus { get; set; }

        /* ================= 标识符 (Identifiers) ================= */

        /// <summary>
        /// Sumsub 申请人 ID (Applicant Id)
        /// </summary>
        public string ApplicantId { get; set; }

        /// <summary>
        /// 检查 ID (Inspection Id)
        /// </summary>
        public string InspectionId { get; set; }

        /// <summary>
        /// 外部用户 ID (发送给 Sumsub 的唯一标识)
        /// </summary>
        public string ExternalUserId { get; set; }

        /// <summary>
        /// 客户端 ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// API Key
        /// </summary>
        public string ApiKey { get; set; }

        /* ================= 申请人类型信息 (Applicant Type Info) ================= */

        /// <summary>
        /// 申请人类型字符串 (individual 或 company)
        /// </summary>
        public string ApplicantType { get; set; }

        /// <summary>
        /// 申请平台 (如 web, ios, android)
        /// </summary>
        public string ApplicantPlatform { get; set; }

        /// <summary>
        /// IP 所在国家
        /// </summary>
        public string IpCountry { get; set; }

        /* ================= 基础信息 (Basic Info) ================= */

        /// <summary>
        /// 名
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// 英文名
        /// </summary>
        public string FirstNameEn { get; set; }

        public string MiddleName { get; set; }
        public string MiddleNameEn { get; set; }

        /// <summary>
        /// 姓
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// 英文姓
        /// </summary>
        public string LastNameEn { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }

        /* ================= 修正信息 (Fixed/Corrected Info) ================= */

        /// <summary>
        /// 修正后的名
        /// </summary>
        public string FixedFirstName { get; set; }
        public string FixedFirstNameEn { get; set; }
        public string FixedMiddleName { get; set; }
        public string FixedMiddleNameEn { get; set; }
        public string FixedLastName { get; set; }
        public string FixedLastNameEn { get; set; }
        public DateTime? FixedDob { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string TaxResidenceCountry { get; set; }

        /* ================= 地址信息 (Address) ================= */

        /// <summary>
        /// 街道
        /// </summary>
        public string Street { get; set; }
        public string State { get; set; }
        public string BuildingNumber { get; set; }
        public string Town { get; set; }
        public string PostCode { get; set; }
        public string FormattedAddress { get; set; }

        /* ================= 企业信息 (Company Info - KYB) ================= */

        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 公司注册号
        /// </summary>
        public string CompanyRegistrationNumber { get; set; }
        public string CompanyCountry { get; set; }
        public string CompanyLegalAddress { get; set; }
        public string CompanyEmail { get; set; }

        /// <summary>
        /// 股权结构层级深度
        /// </summary>
        public int? OwnershipStructureDepth { get; set; }

        /// <summary>
        /// 跳过的受益人类型
        /// </summary>
        public string SkippedBeneficiaryTypes { get; set; }

        /* ================= 联系方式 (Contact) ================= */

        public string Email { get; set; }

        /* ================= 审核结果 (Review Results) ================= */

        /// <summary>
        /// 审核状态 (如 completed)
        /// </summary>
        public string ReviewStatus { get; set; }

        /// <summary>
        /// 审核结果 (GREEN, RED)
        /// </summary>
        public string ReviewAnswer { get; set; }

        /// <summary>
        /// 拒绝类型 (FINAL, RETRY)
        /// </summary>
        public string ReviewRejectType { get; set; }

        /// <summary>
        /// 审核评论 (后台可见)
        /// </summary>
        public string ModerationComment { get; set; }

        public string ReviewLevel { get; set; }
        public int? ReviewAttemptCount { get; set; }
        public int? Priority { get; set; }
        public DateTime? ReviewDate { get; set; }

        /// <summary>
        /// 客户端可见评论 (展示给用户)
        /// </summary>
        public string ClientComment { get; set; }

        /// <summary>
        /// 拒绝标签集合
        /// </summary>
        public string RejectLabels { get; set; }

        /* ================= 协议 (Agreement) ================= */

        public DateTime? AgreementAcceptedAt { get; set; }
        public string AgreementSource { get; set; }

        /* ================= 问卷与文档 (Questionnaire & Docs) ================= */

        public string EmploymentStatus { get; set; }
        public string AnnualIncome { get; set; }

        public string IdDocType { get; set; }
        public string IdDocCountry { get; set; }
        public string IdDocNumber { get; set; }
        public DateTime? IdDocValidUntil { get; set; }

        public bool? HasSelfie { get; set; }
        public bool? HasVideo { get; set; }
        public bool? HasProofOfAddress { get; set; }

        /* ================= 原始 JSON 数据 (JSON Data) ================= */

        /// <summary>
        /// 文件信息 JSON
        /// </summary>
        public string FileInfos { get; set; }

        /// <summary>
        /// 问卷信息 JSON
        /// </summary>
        public string QuestionnaireJson { get; set; }

        /// <summary>
        /// 所需证件 JSON
        /// </summary>
        public string RequiredIdDocsJson { get; set; }

        /// <summary>
        /// 完整原始回调数据 JSON (用于审计)
        /// </summary>
        public string RawJson { get; set; }

        /* ================= 系统时间 (System Times) ================= */

        /// <summary>
        /// 提供商系统中的创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedTime { get; set; }

        /// <summary>
        /// 最近一次已处理的 Sumsub 回调事件时间（UTC）
        /// 用于回调乱序保护，避免旧回调覆盖新状态
        /// </summary>
        public DateTime? LastCallbackEventTime { get; set; }
    }
}
