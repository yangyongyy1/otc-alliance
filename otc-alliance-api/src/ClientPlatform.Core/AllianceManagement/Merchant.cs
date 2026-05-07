using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.AllianceManagement
{
    public class Merchant : FullAuditedEntity<int>
    {
        /// <summary>
        /// 联盟ID
        /// </summary>
        public int AllianceId { get; set; }

        public virtual Alliance Alliance { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// 商户ID API key
        /// </summary>
        [StringLength(50)]
        public string BusinessID { get; set; }


        /// <summary>
        /// 商户KEY API secret
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
        /// 支付设置
        /// </summary>
        public string PaySettings { get; set; }
    }
}
