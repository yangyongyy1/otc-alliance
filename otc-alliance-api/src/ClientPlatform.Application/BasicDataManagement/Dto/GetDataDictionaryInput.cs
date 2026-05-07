using Abp.Application.Services.Dto;
using ClientPlatform.BasicDataManagement;
using ClientPlatform;

namespace ClientPlatform.BasicDataManagement.Dto
{
    public class GetDataDictionaryInput : PagedAndSortedResultRequestDto
    {
        public string DicKey { get; set; }
        public string DicValue { get; set; }
        public DataDicType? DicType { get; set; }
    }
}
