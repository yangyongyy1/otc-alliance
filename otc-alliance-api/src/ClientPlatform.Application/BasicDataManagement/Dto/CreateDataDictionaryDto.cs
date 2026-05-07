using Abp.AutoMapper;
using System.ComponentModel.DataAnnotations;
using ClientPlatform.BasicDataManagement;
using ClientPlatform;

namespace ClientPlatform.BasicDataManagement.Dto
{
    [AutoMapTo(typeof(DataDictionary))]
    public class CreateDataDictionaryDto
    {
        /// <summary>
        /// key
        /// </summary>
        [Required]
        public string DicKey { get; set; }

        /// <summary>
        /// value
        /// </summary>
        [Required]
        public string DicValue { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [Required]
        public DataDicType DicType { get; set; }
    }
}
