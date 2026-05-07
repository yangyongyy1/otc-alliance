using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClientPlatform.Pay.Dto.Response
{
    /// <summary>
    /// 渠道必填字段响应
    /// </summary>
    public class ChannelRequiredFieldResponse
    {
        /// <summary>
        /// 字段名称
        /// </summary> 
        [JsonProperty("field_name")]
        public string FieldName { get; set; }

        /// <summary>
        /// 是否显示
        /// </summary> 
        [JsonProperty("display")]
        public bool Display { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary> 
        [JsonProperty("required")]
        public bool Required { get; set; }

        /// <summary>
        /// 字段类型 (如 text, select, date, file, number)
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; } = ChannelTypeName.Text;

        /// <summary>
        /// 扩展字段，例如正则表达式或选项列表
        /// </summary>
        [JsonProperty("expand")]
        public object Expand { get; set; }

        /// <summary>
        /// 默认值或预填值
        /// </summary>
        [JsonProperty("value")]
        public object Value { get; set; }

        /// <summary>
        /// 是否只读（不可修改）
        /// </summary>
        [JsonProperty("readonly")]
        public bool Readonly { get; set; }

        /// <summary>
        /// 字段说明
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 字段标签/名称 (UI显示)
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// 正则表达式
        /// </summary>
        [JsonProperty("regex")]
        public string Regex { get; set; }

        /// <summary>
        /// 验证失败提示信息
        /// </summary>
        [JsonProperty("validation_message")]
        public string ValidationMessage { get; set; }

        /// <summary>
        /// 选项列表
        /// </summary>
        [JsonProperty("options")]
        public List<ChannelRequiredFieldResponseValue> Options { get; set; }

        /// <summary>
        /// 键 (别名 FieldName)
        /// </summary>
        [JsonIgnore]
        public string Key { get => FieldName; set => FieldName = value; }

        /// <summary>
        /// 名称 (别名 Label)
        /// </summary>
        [JsonIgnore]
        public string Name { get => Label; set => Label = value; }

        /// <summary>
        /// 子字段列表
        /// </summary>
        //[JsonProperty("sub_fields")]
        //public List<ChannelRequiredFieldResponse> SubFields { get; set; } // 新增支持子字段
    }

    /// <summary>
    /// 渠道必填字段值 (包含子字段)
    /// </summary>
    public class ChannelRequiredFieldsResponseValue : ChannelRequiredFieldResponseValue
    {
        /// <summary>
        /// 子字段列表
        /// </summary>
        //[JsonProperty("childs")]
        //public List<ChannelRequiredFieldResponse> Childs { get; set; } = new List<ChannelRequiredFieldResponse>();

        /// <summary>
        /// 值数据列表
        /// </summary>
        [JsonProperty("value_data")]
        public List<ChannelRequiredFieldResponseValue> ValueData { get; set; } = new List<ChannelRequiredFieldResponseValue>();
    }

    /// <summary>
    /// 渠道必填字段值 (键值对)
    /// </summary>
    public class ChannelRequiredFieldResponseValue
    {
        /// <summary>
        /// 键
        /// </summary>
        [JsonProperty("key")]
        public string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// 渠道字段类型常量
    /// </summary>
    public class ChannelTypeName
    {
        /// <summary>
        /// 文本
        /// </summary>
        public const string Text = "text";

        /// <summary>
        /// 下拉选择
        /// </summary>
        public const string Select = "select";

        /// <summary>
        /// 日期
        /// </summary>
        public const string Date = "date";

        /// <summary>
        /// 文件
        /// </summary>
        public const string File = "file";

        /// <summary>
        /// 数字
        /// </summary>
        public const string Number = "number";
    }
}
