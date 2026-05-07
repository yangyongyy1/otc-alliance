using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using ClientPlatform.BasicDataManagement;
using ClientPlatform;

namespace ClientPlatform.BasicDataManagement.Dto
{
    [AutoMapFrom(typeof(DataDictionary))]
    [AutoMapTo(typeof(DataDictionary))]
    public class DataDictionaryDto : FullAuditedEntityDto<int>
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
