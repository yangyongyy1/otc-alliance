using System;
using System.ComponentModel.DataAnnotations;
using Abp.Runtime.Validation;

namespace ClientPlatformUser
{
    /// <summary>
    /// C 端创建本地支付订单入参（对前端暴露）
    /// 用户只需填写：金额、币种、支付方式
    /// </summary>
    public class CreateLocalPayOrderInput : ICustomValidate
    {
        /// <summary>
        /// 支付金额
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// 币种（如：USD、AED）
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// 支付方式（如：CARD）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PaymentType { get; set; } = string.Empty;

        /// <summary>
        /// 自定义参数校验：金额 > 0 且最多两位小数
        /// </summary>
        public void AddValidationErrors(CustomValidationContext context)
        {
            if (Amount <= 0)
            {
                context.Results.Add(new ValidationResult("AmountMustBeGreaterThanZero"));
            }

            var scaled = Amount * 100m;
            if (scaled != Math.Truncate(scaled))
            {
                context.Results.Add(new ValidationResult("AmountMustHaveAtMostTwoDecimalPlaces"));
            }
        }
    }
}

