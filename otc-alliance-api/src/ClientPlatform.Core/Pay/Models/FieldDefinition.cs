using System.Collections.Generic;

namespace ClientPlatform.Pay.Models
{
    /// <summary>
    /// 动态表单字段定义
    /// </summary>
    public class FieldDefinition
    {
        /// <summary>
        /// 字段唯一标识 (前端 Model Key)
        ///ex: owner_name
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 字段显示标签
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 字段描述/提示信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 字段类型 (text, number, select, date, etc.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// 是否只读 (通常由商户配置覆盖)
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// 是否显示 (默认true)
        /// </summary>
        public bool Display { get; set; } = true;

        /// <summary>
        /// 验证正则
        /// </summary>
        public string RegexPattern { get; set; }

        /// <summary>
        /// 验证错误提示
        /// </summary>
        public string ValidationMessage { get; set; }

        /// <summary>
        /// 选项列表 (针对 select 类型)
        /// </summary>
        public List<FieldOption> Options { get; set; }
    }

    public class FieldOption
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }
}
