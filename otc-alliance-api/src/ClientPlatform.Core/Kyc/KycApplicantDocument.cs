using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientPlatform.Kyc
{
    /// <summary>
    /// KYC 申请人证件文档实体
    /// 用于存储用户上传的图片、文档信息及审核结果
    /// </summary>
    [Table("AppKycApplicantDocuments")]
    public class KycApplicantDocument : FullAuditedEntity<Guid>
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
        /// 检查 ID
        /// </summary>
        public string InspectionId { get; set; }

        /// <summary>
        /// 文档类型 (例如: PASSPORT, ID_CARD, SELFIE)
        /// </summary>
        public string IdDocType { get; set; }

        /// <summary>
        /// 提供商处的 Image ID
        /// </summary>
        public string ImageId { get; set; }

        /// <summary>
        /// 图片槽位 (0=正面, 1=背面, 等)
        /// </summary>
        public int ImageSlot { get; set; }

        /// <summary>
        /// 国家代码
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 审核结果 (GREEN/RED)
        /// </summary>
        public string ReviewAnswer { get; set; }

        /// <summary>
        /// 管理员评论
        /// </summary>
        public string ModerationComment { get; set; }

        /// <summary>
        /// 客户端可见评论
        /// </summary>
        public string ClientComment { get; set; }

        /// <summary>
        /// 存储 URL 或 路径 (MinIO 对象名)
        /// </summary>
        public string Url { get; set; }
    }
}
