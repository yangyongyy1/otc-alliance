using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.VABusiness
{
    public class VAAccountIdentityDetail:FullAuditedAggregateRoot<int>
    {
        public int VAAccountIdentityId { get; set; }

        public virtual VAAccountIdentity VAAccountIdentity { get; set; }


        /// <summary>
        /// Name of the account holder (person or business)
        /// </summary>
        [StringLength(255)]
        public string AccountHolderName { get; set; }

        /// <summary>
        /// Account holder type: person or business
        /// </summary>
        [StringLength(20)]
        public string AccountHolderType { get; set; }

        [StringLength(255)]
        public string AccountHolderAddressLine1 { get; set; }

        [StringLength(255)]
        public string AccountHolderAddressLine2 { get; set; }

        [StringLength(255)]
        public string AccountHolderCity { get; set; }

        [StringLength(50)]
        public string AccountHolderState { get; set; }

        [StringLength(30)]
        public string AccountHolderPostalCode { get; set; }

        [StringLength(2)]
        public string AccountHolderCountryCode { get; set; }


        // =============================
        //        Bank Information
        // =============================

        [StringLength(255)]
        public string BankName { get; set; }

        [StringLength(255)]
        public string BankBranchName { get; set; }

        [StringLength(255)]
        public string BankAddressLine1 { get; set; }

        [StringLength(255)]
        public string BankAddressLine2 { get; set; }

        [StringLength(255)]
        public string BankCity { get; set; }

        [StringLength(50)]
        public string BankState { get; set; }

        [StringLength(30)]
        public string BankPostalCode { get; set; }

        [StringLength(2)]
        public string BankCountryCode { get; set; }


        // =============================
        //   Global Cross-Border Fields
        // =============================

        /// <summary>
        /// Bank SWIFT/BIC code (global wire transfer)
        /// </summary>
        [StringLength(11)]
        public string SwiftBic { get; set; }

        /// <summary>
        /// Intermediary bank name (for international wire)
        /// </summary>
        [StringLength(255)]
        public string IntermediaryBankName { get; set; }

        /// <summary>
        /// Intermediary bank SWIFT code
        /// </summary>
        [StringLength(11)]
        public string IntermediaryBankSwift { get; set; }


        // =============================
        //     United States (ACH / Wire / Fedwire)
        // =============================

        /// <summary>
        /// ACH routing number (NACHA)
        /// </summary>
        [StringLength(9)]
        public string AchRoutingNumber { get; set; }

        /// <summary>
        /// ABA routing number (checks &amp; ACH)
        /// </summary>
        [StringLength(9)]
        public string AbaRoutingNumber { get; set; }

        /// <summary>
        /// Fedwire routing number
        /// </summary>
        [StringLength(9)]
        public string FedwireRoutingNumber { get; set; }

        /// <summary>
        /// Wire routing number (may differ from ABA)
        /// </summary>
        [StringLength(9)]
        public string WireRoutingNumber { get; set; }

        /// <summary>
        /// Bank account number (US max ~17 digits; extended for IBAN accounts)
        /// </summary>
        [StringLength(34)]
        public string AccountNumber { get; set; }

        /// <summary>
        /// Checking / Savings / Other
        /// </summary>
        [StringLength(20)]
        public string AccountType { get; set; }

        /// <summary>
        /// Wire memo or payment reference (Fedwire)
        /// </summary>
        [StringLength(255)]
        public string WireMemo { get; set; }

        /// <summary>
        /// NACHA Company ID (for corporate ACH)
        /// </summary>
        [StringLength(10)]
        public string AchCompanyId { get; set; }

        /// <summary>
        /// NACHA Standard Entry Class (SEC) code: PPD, CCD, WEB etc.
        /// </summary>
        [StringLength(10)]
        public string AchStandardEntryClass { get; set; }


        // =============================
        //     Europe / UK / AU / MX
        // =============================

        /// <summary>
        /// IBAN (international bank account number)
        /// </summary>
        [StringLength(34)]
        public string Iban { get; set; }

        /// <summary>
        /// Sort code (UK)
        /// </summary>
        [StringLength(6)]
        public string SortCode { get; set; }

        /// <summary>
        /// BSB (Australia)
        /// </summary>
        [StringLength(6)]
        public string BsbCode { get; set; }

        /// <summary>
        /// Mexican CLABE account number
        /// </summary>
        [StringLength(18)]
        public string Clabe { get; set; }

        /// <summary>
        /// Bank code (ex: Germany BLZ, Japan Bank Code)
        /// </summary>
        [StringLength(20)]
        public string BankCode { get; set; }

        /// <summary>
        /// Branch code (ex: Japan Branch Code, CN 联行号)
        /// </summary>
        [StringLength(20)]
        public string BranchCode { get; set; }


        // =============================
        //       Extension JSON
        // =============================

        /// <summary>
        /// Extra JSON for advanced or custom fields
        /// </summary>
        public string Extra { get; set; }
    }
}
