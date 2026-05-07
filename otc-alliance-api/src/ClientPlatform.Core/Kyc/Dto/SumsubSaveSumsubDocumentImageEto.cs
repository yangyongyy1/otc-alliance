using System;

namespace ClientPlatform.Kyc.Dto
{
    /// <summary>
    /// Sumsub 文档图片保存事件传输对象
    /// </summary>
    public class SumsubSaveSumsubDocumentImageEto
    {
        /// <summary>
        /// ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 申请人 ID
        /// </summary>
        public string applicantId { get; set; }
        /// <summary>
        /// 检查 ID
        /// </summary>
        public string inspectionId { get; set; }
    }
}
