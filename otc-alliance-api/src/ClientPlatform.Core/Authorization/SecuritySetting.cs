using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.Authorization
{
    public class SecuritySetting
    {
        /// <summary>
        /// 是否启用手机号码验证
        /// </summary> 
        public bool IsPhoneNumberEnabled { get; set; }

        /// <summary>
        /// 是否启用谷歌账号验证
        /// </summary> 
        public bool IsGoogleAccountEnabled { get; set; }

        /// <summary>
        /// 是否已经设置资金密码
        /// </summary> 
        public bool IsSetFoudPassword { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary> 
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary> 
        public string EmailAddress { get; set; }
        /// <summary>
        /// 绑定谷歌状态
        /// </summary>
        public GoogleBindStatus? GoogleBindStatus { get; set; }
        /// <summary>
        /// 是否启用邮箱
        /// </summary>
        public bool IsEmailEnabled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClientKey { get; set; }

    }
}
