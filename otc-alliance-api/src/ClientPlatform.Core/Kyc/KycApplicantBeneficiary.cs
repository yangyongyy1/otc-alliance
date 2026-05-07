using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPlatform.Kyc
{
    /// <summary>
    /// KYC 受益人/代表实体 (针对企业认证 KYB)
    /// </summary>
    [Table("AppKycApplicantBeneficiaries")]
    public class KycApplicantBeneficiary : FullAuditedEntity<Guid>
    {
        /// <summary>
        /// 关联的 KYC 申请 ID
        /// </summary>
        public Guid KycApplicantId { get; set; }

        /// <summary>
        /// 关联的 KYC 申请实体
        /// </summary>
        [ForeignKey("KycApplicantId")]
        public virtual KycApplicant KycApplicant { get; set; }

        /// <summary>
        /// 提供商处的受益人申请 ID
        /// </summary>
        public string BeneficiaryApplicantId { get; set; }

        /// <summary>
        /// 受益人 ID
        /// </summary>
        public string BeneficiaryId { get; set; }

        /// <summary>
        /// 角色 (ubo: 最终受益人 / representative: 代表 / director: 董事)
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// 是否已提交
        /// </summary>
        public bool Submitted { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// 姓
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// 中间名
        /// </summary>
        public string MiddleName { get; set; }
        
        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? DateOfBirth { get; set; }
    }
}
