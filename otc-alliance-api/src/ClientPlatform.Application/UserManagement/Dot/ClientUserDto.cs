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
using ClientPlatform.UserManagement;
using ClientPlatform.Common;

namespace ClientPlatform.UserManagement.Dot
{
    [AutoMapFrom(typeof(ClientUser))]
    public class ClientUserDto : FullAuditedEntity<int>
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [StringLength(50)]
        public string UserName { get; set; }

        /// <summary>
        /// 姓
        /// </summary>
        [StringLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        [StringLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// 中间名
        /// </summary>
        [StringLength(50)]
        public string MiddleName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [StringLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// 邀请码（关联商户）
        /// </summary>
        [StringLength(50)]
        public string InviteCode { get; set; }

        /// <summary>
        /// 国家编码
        /// </summary>
        [StringLength(10)]
        public string CountryCode { get; set; }

        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public int? MerchantId { get; set; }

        /// <summary>
        /// 联盟
        /// </summary>
        public virtual AllianceDto Alliance { get; set; }

        /// <summary>
        /// 商户
        /// </summary>
        public virtual MerchantDto Merchant { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public UserAuthStatus UserAuthStatus { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public UserStatus UserStatus { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public AccountUserType UserType { get; set; }

        /// <summary>
        /// 认证标准等级（来自 KycApplicant.AuthStandardLevel）
        /// </summary>
        public AuthStandardLevel? AuthStandardLevel { get; set; }

        public string KycLevelCompleted { get; set; }
    }

    public class CreateOrUpdateClientUserDto
    {
        /// <summary>
        /// ID（更新时使用）
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [StringLength(50)]
        public string UserName { get; set; }

        /// <summary>
        /// 姓
        /// </summary>
        [StringLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        [StringLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// 中间名
        /// </summary>
        [StringLength(50)]
        public string MiddleName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [StringLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        [StringLength(200)]
        public string UserPassword { get; set; }

        /// <summary>
        /// 邀请码（关联商户）
        /// </summary>
        [StringLength(50)]
        public string InviteCode { get; set; }

        /// <summary>
        /// 国家编码
        /// </summary>
        [StringLength(10)]
        public string CountryCode { get; set; }

        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public int? MerchantId { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public UserAuthStatus UserAuthStatus { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public UserStatus UserStatus { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public AccountUserType UserType { get; set; }
    }

    public class PagedClientUserResultRequestDto : PagedAndSortedResultRequestDto, IShouldNormalize
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string InviteCode { get; set; }

        public int? AllianceId { get; set; }

        public int? MerchantId { get; set; }

        public KycBizStatus? UserAuthStatus { get; set; }

        public UserStatus? UserStatus { get; set; }

        public AccountUserType? UserType { get; set; }

        public DateTime? CreationTimeStart { get; set; }

        public DateTime? CreationTimeEnd { get; set; }

        public DateTime? ModificationTimeStart { get; set; }

        public DateTime? ModificationTimeEnd { get; set; }

        /// <summary>
        /// 认证标准等级（用于筛选）
        /// </summary>
        public AuthStandardLevel? AuthStandardLevel { get; set; }

        public string KycLevelName { get; set; }

        public List<CustomSearchFilter> CustomFilters { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(Sorting))
            {
                Sorting = "CreationTime DESC";
            }
        }
    }
}

