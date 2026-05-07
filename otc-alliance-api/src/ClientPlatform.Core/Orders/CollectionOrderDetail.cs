using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.Orders
{
    /// <summary>
    /// 本地支付收款订单明细实体（数据库实体）
    /// </summary>
    public class CollectionOrderDetail : FullAuditedEntity<Guid>
    {
        /// <summary>
        /// 关联的订单ID
        /// </summary>
        public Guid CollectionOrderId { get; set; }

        //付款人
        [StringLength(50)]
        public string PayerName { get; set; }

        //付款币种
        [StringLength(10)]
        public string PayerCurrency { get; set; }

        //付款人IBAN
        [StringLength(50)]
        public string PayerIban { get; set; }

        //付款人SWIFT BIC
        [StringLength(50)]
        public string PayerSwiftBic { get; set; }

        //收款人
        [StringLength(50)]
        public string RecipientName { get; set; }

        //收款币种
        [StringLength(10)]
        public string RecipientCurrency { get; set; }

        //收款人账户持有人
        [StringLength(50)]
        public string RecipientAccountHolderName { get; set; }

        //收款人账号
        [StringLength(50)]
        public string RecipientAccountNumber { get; set; }

        //收款人IBAN
        [StringLength(50)]
        public string RecipientIban { get; set; }

        //收款人SWIFT BIC
        [StringLength(50)]
        public string RecipientSwiftBic { get; set; }

        //收款人银行名称
        [StringLength(50)]
        public string RecipientBankName { get; set; }

        //收款人银行地址
        [StringLength(256)]
        public string RecipientBankAddress { get; set; }

        //收款人银行国家
        [StringLength(50)]
        public string RecipientBankCountry { get; set; }
        
        /// <summary>
        /// 收款人银行代码
        /// </summary>
        public string SortCode { get; set; }
        
        //请求信息(JSON类型)
        [Column(TypeName = "json")]
        public string RequestInfo { get; set; }

        //响应信息(JSON类型)
        [Column(TypeName = "json")]
        public string ResponseInfo { get; set; }

        /// <summary>
        /// 回调（URL 或回调内容）
        /// </summary>
        [StringLength(512)]
        public string Callback { get; set; }
    }
}
