using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.AllianceManagement;
using ClientPlatform.Extensions;

namespace ClientPlatformUser.Users.Dto
{
    public class ClientUserInfoDto
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
        /// 
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



        public virtual Alliance Alliance { get; set; }




        public virtual Merchant Merchant { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public KycBizStatus UserAuthStatus { get; set; }

        public string UserAuthStatusName=> UserAuthStatus.GetEnumLocalizationName();

        /// <summary>
        /// 用户状态
        /// </summary>
        public UserStatus UserStatus { get; set; }

        public string UserStatusName => UserStatus.GetEnumLocalizationName();

        /// <summary>
        /// 用户类型
        /// </summary>
        public AccountUserType UserType { get; set; }

        public string UserTypeName => UserType.GetEnumLocalizationName();
    }
}
