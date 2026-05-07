using System;

namespace ClientPlatform.Pay.Jobs
{
    [Serializable]
    public class CreatePayChannelCustomerJobArgs
    {
        public long UserId { get; set; }
        public Guid KycApplicantId { get; set; }
    }
}
