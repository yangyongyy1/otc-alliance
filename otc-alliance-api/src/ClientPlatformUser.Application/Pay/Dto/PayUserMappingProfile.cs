using AutoMapper;
using System.Collections.Generic;
using ClientPlatform.Pay.Dto.Request;

namespace ClientPlatformUser.Pay.Dto
{
    public class PayUserMappingProfile : Profile
    {
        public PayUserMappingProfile()
        {
            CreateMap<CreateAccountInput, StandardVaCreationRequest>()
                .ForMember(d => d.ReferenceId, opt => opt.MapFrom(s => s.CustomerId))
                .ForMember(d => d.HolderName, opt => opt.MapFrom(s =>
                    s.AccountName ?? $"{s.FirstName} {s.LastName}".Trim())); ;
            // .ForMember(d => d.Purpose, opt => opt.MapFrom(s => "Salary")) // [User Removed]
            // .ForMember(d => d.RequireIban, opt => opt.MapFrom(s => true)); // [User Removed]
            // .ForMember(d => d.Details, opt => opt.MapFrom(s => s.Details ?? new Dictionary<string, object>())); // Details removed from input
        }
    }
}
