using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatformUser
{
    public class EnumDto
    {
        /// <summary>
        /// 枚举键
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 枚举int值
        /// </summary>
        public int Value { get; set; }
        /// <summary>
        /// 多语言显示名称
        /// </summary>
        public string DisplayName { get; set; }
    }
}
