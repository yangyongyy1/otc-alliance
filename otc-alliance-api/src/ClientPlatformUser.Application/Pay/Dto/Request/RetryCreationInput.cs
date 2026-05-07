using System;
using System.ComponentModel.DataAnnotations;
using ClientPlatform.Pay.Dto.Request;

namespace ClientPlatform.Pay.Dto.Request
{
    public class RetryCreationInput : CreateAccountInput
    {
        [Required]
        public Guid RequestId { get; set; }
    }
}
