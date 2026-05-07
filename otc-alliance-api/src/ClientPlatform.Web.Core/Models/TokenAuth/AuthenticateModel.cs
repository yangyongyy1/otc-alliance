using Abp.Auditing;
using Abp.Authorization.Users;
using System.ComponentModel.DataAnnotations;

namespace ClientPlatform.Models.TokenAuth
{
    public class AuthenticateModel
    {
        [Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string UserNameOrEmailAddress { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }

        public bool RememberClient { get; set; }

        public string EmailVerifyCode { get; set; }

    }

    /// <summary>
    /// 用户登录(客户端）
    /// </summary>
    public class AuthenticateModelForUser
    {
        /// <summary>
        /// 用户名/邮箱
        /// </summary>
        [Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string UserNameOrEmailAddress { get; set; }

        /// <summary>
        /// 邮箱验证码
        /// </summary>
        public string EmailVerifyCode { get; set; }

    }
}
