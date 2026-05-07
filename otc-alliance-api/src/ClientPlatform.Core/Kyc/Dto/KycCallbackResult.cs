using System;

namespace ClientPlatform.Kyc.Dto
{
    public class KycCallbackResult
    {
        public bool Success { get; set; }
        public bool IsApproved { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public Guid KycApplicantId { get; set; }
    }
}
