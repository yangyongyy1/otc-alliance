using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using Abp.Runtime.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.AllianceManagement;
using ClientPlatform.AllianceManagement.Dot;
using ClientPlatform.Kyc;
using ClientPlatform.UserManagement;

namespace ClientPlatform.UserManagement.Dot
{
    [AutoMapFrom(typeof(UserIdentity))]
    public class UserIdentityDto : FullAuditedEntity<int>
    {
        /// <summary>
        /// 用户 ID（关联 User 表）
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }

        public string AllianceName { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public int? MerchantId { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

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
        /// 证件类型（护照 / 身份证等）
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
        /// 审核拒绝原因（状态为 Rejected 时才需要）
        /// </summary>
        [StringLength(255)]
        public string RejectReason { get; set; }

        /// <summary>
        /// 认证方式
        /// </summary>
        public AuthType AuthType { get; set; }

        public DynamicForm DynamicForm { get; set; }


        /// <summary>
        /// 性别
        /// </summary>
        [StringLength(10)]
        public string Sex { get; set; }


        /// <summary>
        /// 生日
        /// </summary>

        public DateTime? Birthday { get; set; }


        /// <summary>
        /// 国家编码
        /// </summary>
        [StringLength(10)]
        public string CountryCode { get; set; }

        /// <summary>
        /// 洲
        /// </summary>
        [StringLength(20)]
        public string State { get; set; }
    }

    public class CreateOrUpdateUserIdentityDto
    {
        /// <summary>
        /// ID（更新时使用）
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 用户 ID（关联 User 表）
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
        /// 证件类型（护照 / 身份证等）
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
        /// 审核拒绝原因（状态为 Rejected 时才需要）
        /// </summary>
        [StringLength(255)]
        public string RejectReason { get; set; }

        /// <summary>
        /// 认证方式
        /// </summary>
        public AuthType AuthType { get; set; }
    }

    public class PagedUserIdentityResultRequestDto : PagedAndSortedResultRequestDto, IShouldNormalize
    {
        public int? UserId { get; set; }

        public int? AllianceId { get; set; }

        public int? MerchantId { get; set; }

        public string Name { get; set; }

        public string DocumentNumber { get; set; }

        public DocumentType? DocumentType { get; set; }

        public KycBizStatus? KycBizStatus { get; set; }

        public AuthType? AuthType { get; set; }

        public AuthStandardLevel? AuthStandardLevel { get; set; }

        public DateTime? CreationTimeStart { get; set; }

        public DateTime? CreationTimeEnd { get; set; }

        public DateTime? ModificationTimeStart { get; set; }

        public DateTime? ModificationTimeEnd { get; set; }

        public string Email { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(Sorting))
            {
                Sorting = "CreationTime DESC";
            }
        }
    }
}

