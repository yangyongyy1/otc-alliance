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
using ClientPlatform.Pay;
using ClientPlatform.VABusiness;

namespace ClientPlatform.VABusiness.Dot
{
    [AutoMapFrom(typeof(VAAccount))]
    public class VAAccountDto : FullAuditedEntity<int>
    {
        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 账户ID
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public int? MerchantId { get; set; }

        /// <summary>
        /// 联盟
        /// </summary>
        public virtual AllianceDto Alliance { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [StringLength(10)]
        public string Currency { get; set; }

        /// <summary>
        /// 账户用户类型
        /// </summary>
        public AccountUserType AccountUserType { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        [StringLength(50)]
        public string AccountName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [StringLength(150)]
        public string Email { get; set; }

        /// <summary>
        /// 账户状态
        /// </summary>
        public AccountStatus AccountStatus { get; set; }


        /// <summary>
        /// 动态字段
        /// </summary>
        public DynamicForm DynamicForm { get; set; }
    }

    public class CreateOrUpdateVAAccountDto
    {
        /// <summary>
        /// ID（更新时使用）
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 账户ID
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public int? MerchantId { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [StringLength(10)]
        public string Currency { get; set; }

        /// <summary>
        /// 账户用户类型
        /// </summary>
        public AccountUserType AccountUserType { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        [StringLength(50)]
        public string AccountName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [StringLength(150)]
        public string Email { get; set; }

        /// <summary>
        /// 账户状态
        /// </summary>
        public AccountStatus AccountStatus { get; set; }
    }

    public class PagedVAAccountResultRequestDto : PagedAndSortedResultRequestDto, IShouldNormalize
    {
        public int? AllianceId { get; set; }

        public int? UserId { get; set; }

        public string AccountId { get; set; }

        public int? MerchantId { get; set; }

        public string Currency { get; set; }

        public AccountUserType? AccountUserType { get; set; }

        public string AccountName { get; set; }

        public string Email { get; set; }

        public VAStatus? Status { get; set; }

        public DateTime? CreationTimeStart { get; set; }

        public DateTime? CreationTimeEnd { get; set; }

        public DateTime? ModificationTimeStart { get; set; }

        public DateTime? ModificationTimeEnd { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(Sorting))
            {
                Sorting = "CreationTime DESC";
            }
        }
    }
}

