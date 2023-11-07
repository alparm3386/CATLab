using AutoMapper;
using CAT.Models;

namespace CAT.Configuration
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CAT service
            //CreateMap<CAT.Models.Common.TMAssignment, CATService.TMAssignment>();
            //.ForMember(dest => dest.tmId, opt => opt.MapFrom(src => src.tmId))
            //.ReverseMap()
            //.ForMember(dest => dest.tmId, opt => opt.MapFrom(src => src.tmId));
            //CreateMap<CAT.Models.DTOs.QuoteDto, CAT.Models.Entities.Main.Quote>().ReverseMap();
        }
    }

}
