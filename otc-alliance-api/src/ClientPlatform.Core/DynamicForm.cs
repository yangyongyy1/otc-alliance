using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform
{
    /// <summary>
    /// 存放进件，账户，订单回调，请求，的JOSN字段
    /// </summary>
    public class DynamicForm : FullAuditedEntity<int>
    {
        /// <summary>
        /// 关联ID，比如进件ID，账户ID，订单ID
        /// </summary>
        public int ReferenceId { get; set; }


        /// <summary>
        /// 动态表单JSON
        /// </summary>
        public string DyFormJson { get; set; }


        /// <summary>
        /// 表单类型
        /// </summary>
        public FormJosnType FormJosnType { get; set; }
    }
}
