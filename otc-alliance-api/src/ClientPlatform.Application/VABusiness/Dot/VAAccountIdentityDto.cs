using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.AllianceManagement;
using ClientPlatform.AllianceManagement.Dot;
using ClientPlatform.VABusiness;

namespace ClientPlatform.VABusiness.Dot
{
    [AutoMapFrom(typeof(VAAccountIdentity))]
    public class VAAccountIdentityDto : FullAuditedEntity<int>
    {
        /// <summary>
        /// 账户 ID（关联 Account 表）
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public int? MerchantId { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 姓（Last Name）
        /// </summary>
        [StringLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// 中间名（Middle Name，可空）
        /// </summary>
        [StringLength(50)]
        public string MiddleName { get; set; }

        /// <summary>
        /// 名（First Name）
        /// </summary>
        [StringLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [StringLength(100)]
        public string City { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        [StringLength(20)]
        public string PostalCode { get; set; }

        /// <summary>
        /// 地址（详细地址）
        /// </summary>
        [StringLength(255)]
        public string Address { get; set; }

        /// <summary>
        /// 证件类型（护照 / 身份证 / 驾照）
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// 证件号码
        /// </summary>
        [StringLength(100)]
        public string DocumentNumber { get; set; }

        /// <summary>
        /// 证件照片（正面）
        /// </summary>
        [StringLength(255)]
        public string DocumentPhotoFrontUrl { get; set; }

        /// <summary>
        /// 证件照片（反面，可选）
        /// </summary>
        [StringLength(255)]
        public string DocumentPhotoBackUrl { get; set; }

        /// <summary>
        /// 认证状态（Pending / Approved / Rejected）
        /// </summary>
        public IdentityStatus Status { get; set; }

        /// <summary>
        /// 审核拒绝原因（仅在 Rejected 时使用）
        /// </summary>
        [StringLength(255)]
        public string RejectReason { get; set; }

        public DynamicForm DynamicForm { get; set; }
    }

    public class CreateOrUpdateVAAccountIdentityDto
    {
        /// <summary>
        /// ID（更新时使用）
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 账户 ID（关联 Account 表）
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public int? MerchantId { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 姓（Last Name）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// 中间名（Middle Name，可空）
        /// </summary>
        [StringLength(50)]
        public string MiddleName { get; set; }

        /// <summary>
        /// 名（First Name）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [Required]
        [StringLength(100)]
        public string City { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }

        /// <summary>
        /// 地址（详细地址）
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        /// <summary>
        /// 证件类型（护照 / 身份证 / 驾照）
        /// </summary>
        [Required]
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// 证件号码
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DocumentNumber { get; set; }

        /// <summary>
        /// 证件照片（正面）
        /// </summary>
        [Required]
        [StringLength(255)]
        public string DocumentPhotoFrontUrl { get; set; }

        /// <summary>
        /// 证件照片（反面，可选）
        /// </summary>
        [StringLength(255)]
        public string DocumentPhotoBackUrl { get; set; }

        /// <summary>
        /// 认证状态（Pending / Approved / Rejected）
        /// </summary>
        [Required]
        public IdentityStatus Status { get; set; }

        /// <summary>
        /// 审核拒绝原因（仅在 Rejected 时使用）
        /// </summary>
        [StringLength(255)]
        public string RejectReason { get; set; }
    }

    public class PagedVAAccountIdentityResultRequestDto : PagedAndSortedResultRequestDto
    {
        public int? AccountId { get; set; }

        public int? UserId { get; set; }

        public int? AllianceId { get; set; }

        public int? MerchantId { get; set; }

        public string Currency { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DocumentNumber { get; set; }

        public DocumentType? DocumentType { get; set; }

        public IdentityStatus? Status { get; set; }

        public DateTime? CreationTimeStart { get; set; }

        public DateTime? CreationTimeEnd { get; set; }

        public DateTime? ModificationTimeStart { get; set; }

        public DateTime? ModificationTimeEnd { get; set; }
    }
}

