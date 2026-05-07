using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using System;
using ClientPlatform.Pay;
using ClientPlatform.Pay.Entities;
using ClientPlatform.UserManagement;

namespace ClientPlatform.VABusiness.Dot
{
    /// <summary>
    /// 支付渠道服务请求DTO
    /// </summary>
    [AutoMapFrom(typeof(PayChannelServiceRequest))]
    public class PayChannelServiceRequestDto : FullAuditedEntityDto<Guid>
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        public virtual ClientUser User { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 支付渠道提供商标识
        /// </summary>
        public string ChannelProvider { get; set; }

        /// <summary>
        /// 请求载荷
        /// </summary>
        public string RequestPayload { get; set; }

        /// <summary>
        /// 请求状态
        /// </summary>
        public PayChannelServiceRequestStatus Status { get; set; }

        /// <summary>
        /// 失败步骤
        /// </summary>
        public PayChannelServiceRequestFailStep FailStep { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        public string FailReason { get; set; }

        /// <summary>
        /// 渠道客户ID
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// 渠道账户ID
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// 客户信息相关接口的原始响应数据
        /// </summary>
        public string CustomerResponse { get; set; }

        /// <summary>
        /// 账户信息相关接口的原始响应数据
        /// </summary>
        public string AccountResponse { get; set; }

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; }
    }

    /// <summary>
    /// 支付渠道服务请求分页查询请求DTO
    /// </summary>
    public class PagedPayChannelServiceRequestResultRequestDto : PagedAndSortedResultRequestDto, IShouldNormalize
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 支付渠道提供商标识
        /// </summary>
        public string ChannelProvider { get; set; }

        /// <summary>
        /// 请求状态
        /// </summary>
        public PayChannelServiceRequestStatus? Status { get; set; }

        /// <summary>
        /// 失败步骤
        /// </summary>
        public PayChannelServiceRequestFailStep? FailStep { get; set; }

        /// <summary>
        /// 渠道客户ID
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// 渠道账户ID
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// 创建时间开始
        /// </summary>
        public DateTime? CreationTimeStart { get; set; }

        /// <summary>
        /// 创建时间结束
        /// </summary>
        public DateTime? CreationTimeEnd { get; set; }

        /// <summary>
        /// 修改时间开始
        /// </summary>
        public DateTime? ModificationTimeStart { get; set; }

        /// <summary>
        /// 修改时间结束
        /// </summary>
        public DateTime? ModificationTimeEnd { get; set; }

        /// <summary>
        /// 规范化排序字段
        /// </summary>
        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(Sorting))
            {
                Sorting = "CreationTime DESC";
            }
        }
    }
}
