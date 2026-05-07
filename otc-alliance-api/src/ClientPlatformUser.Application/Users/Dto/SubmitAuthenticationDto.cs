using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientPlatform;
using ClientPlatform.UserManagement;

namespace ClientPlatformUser.Users.Dto
{
    /// <summary>
    /// 用户认证
    /// </summary>
    [AutoMapTo(typeof(UserIdentity))]
    [AutoMapFrom(typeof(UserIdentity))]
    public class SubmitAuthenticationDto
    {
        /// <summary>
        /// 姓（Last Name）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// 中间名（Middle Name，可空）
        /// </summary>
        [StringLength(50)]
        public string MiddleName { get; set; }

        /// <summary>
        /// 名（First Name）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [Required]
        [StringLength(100)]
        public string City { get; set; }

        /// <summary>
        /// 邮政编码
        /// </summary>
        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; }

        /// <summary>
        /// 地址（详细地址）
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        /// <summary>
        /// 证件类型（护照 / 身份证等）
        /// </summary>
        [Required]
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// 证件号码
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DocumentNumber { get; set; }

        /// <summary>
        /// 证件照片（正面）
        /// </summary>
        [Required]
        [StringLength(255)]
        public string DocumentPhotoFrontUrl { get; set; }

        /// <summary>
        /// 证件照片（反面，可选）
        /// </summary>
        [StringLength(255)]
        public string DocumentPhotoBackUrl { get; set; }
    }
}
