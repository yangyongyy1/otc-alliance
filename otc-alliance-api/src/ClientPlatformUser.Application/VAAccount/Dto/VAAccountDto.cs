using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.AllianceManagement;

namespace ClientPlatformUser.VAAccount.Dto
{
    [AutoMapFrom(typeof(ClientPlatform.VABusiness.VAAccount))]
    public class VAAccountDto:FullAuditedEntity<int>
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 账户ID
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [StringLength(10)]
        public string Currency { get; set; }

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
        /// 渠道名称
        /// </summary>
        public string ChanleCode { get; set; }
    }
}
