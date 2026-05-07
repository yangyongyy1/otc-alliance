using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.BasicDataManagement
{
    /// <summary>
    /// 数据字典
    /// </summary>
    public class DataDictionary:FullAuditedEntity<int>
    {
        /// <summary>
        /// key
        /// </summary>
        public string DicKey { get; set; }


        /// <summary>
        /// value
        /// </summary>
        public string DicValue { get; set; }


        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>

        public DataDicType DicType { get; set; }
    }
}
