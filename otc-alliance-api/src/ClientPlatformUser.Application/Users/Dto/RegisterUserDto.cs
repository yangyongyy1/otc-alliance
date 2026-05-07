using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;

namespace ClientPlatformUser.Users.Dto
{

    /// <summary>
    /// 注册请求实体
    /// </summary>
    public class RegisterUserRequestDto
    {

        /// <summary>
        /// 用户类型
        /// </summary>
        public AccountUserType UserType { get; set; }


        /// <summary>
        /// 邮箱地址
        /// </summary>
        [Required]
        public string EmailAddress { get; set; }

        /// <summary>
        /// 邀请码
        /// </summary>
        /// 
        [Required]
        public string InviteCode { get; set; }

        /// <summary>
        /// 邮箱验证码
        /// </summary>
        [Required]
        public string EmailVerifyCode { get; set; }

        /// <summary>
        /// 国家代码
        /// </summary>
        [Required]
        public string CountryCode { get; set; }


       
    }
}
