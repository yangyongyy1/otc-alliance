

namespace ClientPlatform.AllianceManagement.Dot
{
    public class CreateMerchantSubCodeDto
    {
        public int MerchantId { get; set; }

        public string SubDescription { get; set; }
    }

    public class UpdateMerchantSubCodeDto
    {
        public int Id { get; set; }

        public string SubDescription { get; set; }
    }
    
}