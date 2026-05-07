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
using ClientPlatform.Settings;

namespace ClientPlatform.AllianceManagement.Dot
{
    /// <summary>
    /// 
    /// </summary>
    [AutoMapFrom(typeof(Merchant))]
    public class MerchantDto : FullAuditedEntity<int>
    {
        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public virtual AllianceDto Alliance { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        [StringLength(50)]
        public string BusinessID { get; set; }


        /// <summary>
        /// 商户KEY
        /// </summary>
        [StringLength(64)]
        public string Key { get; set; }


        /// <summary>
        /// 关联Code
        /// </summary>
        [StringLength(64)]
        public string RelationCode { get; set; }


        /// <summary>
        /// 认证类型
        /// </summary>
        public AuthType AuthType { get; set; }

        /// <summary>
        /// 认证标准（可空）
        /// </summary>
        public AuthStandardLevel? AuthStandard { get; set; }

        /// <summary>
        /// 支付设置（功能设置枚举值，逗号分隔）
        /// </summary>
        public string PaySettings { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// 
    [AutoMapTo(typeof(Merchant))]
    public class CreateMerchantDto
    {
        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// 商户ID
        /// </summary>
        [StringLength(50)]
        public string BusinessID { get; set; }


        /// <summary>
        /// 商户KEY
        /// </summary>
        [StringLength(64)]
        public string Key { get; set; }


        /// <summary>
        /// 认证类型
        /// </summary>
        public AuthType AuthType { get; set; }

        /// <summary>
        /// 认证标准（可空）
        /// </summary>
        public AuthStandardLevel? AuthStandard { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public class SettingMerchantDto
    {
        [Required]
        public int Id { get; set; }
        
        /// <summary>
        /// 认证类型
        /// </summary>
        public AuthType AuthType { get; set; }

        /// <summary>
        /// 认证标准（可空）
        /// </summary>
        public AuthStandardLevel? AuthStandard { get; set; }
    }


    public class MerchantPagedAndSortRequestDto : PagedAndSortedResultRequestDto, IShouldNormalize
    { 
        public string Name { get; set; }

        /// <summary>
        /// 联盟ID
        /// </summary>
        public int? AllianceId { get; set; }


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

    /// <summary>
    /// 更新商户功能设置（PaySettings）
    /// </summary>
    public class UpdateMerchantPaySettingsDto
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// 支付设置类型枚举值，多个以逗号隔开，如 "1,2"
        /// </summary>
        public string PaySettings { get; set; }
    }

    /// <summary>
    /// 新增或修改商户支付设置
    /// </summary>
    public class CreateOrUpdateMerchantPaySettingDto
    {
        /// <summary>
        /// 主键，有值时为更新，空时为新增。
        /// Direct: MPS-{guid}；VA: MCC-{int}
        /// </summary>
        public string Id { get; set; }

        [Required]
        public int MerchantId { get; set; }

        public MerchantPaySettingType Type { get; set; }

        [StringLength(10)]
        public string Currency { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(50)]
        public string ChannelCode { get; set; }

        public OpenClose Status { get; set; }
    }

    /// <summary>
    /// 仅更新支付设置状态（开关）
    /// </summary>
    public class UpdateMerchantPaySettingStatusDto
    {
        /// <summary>
        /// 主键：MPS-{guid} 或 MCC-{int}
        /// </summary>
        [Required]
        public string Id { get; set; }

        [Required]
        public int MerchantId { get; set; }

        public OpenClose Status { get; set; }
    }

    /// <summary>
    /// 支付设置列表项（统一 Direct + VA）
    /// </summary>
    public class MerchantPaySettingItemDto
    {
        /// <summary>
        /// 主键：Direct 为 MPS-{guid}，VA 为 MCC-{int}
        /// </summary>
        public string Id { get; set; }

        public MerchantPaySettingType Type { get; set; }

        public string Currency { get; set; }

        public string PaymentMethod { get; set; }

        public string ChannelCode { get; set; }

        public OpenClose Status { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? LastModificationTime { get; set; }

        public string Operator { get; set; }
    }
}
