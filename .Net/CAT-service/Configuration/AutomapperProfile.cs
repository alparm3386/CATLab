using AutoMapper;
using CAT.Models;

namespace CAT.Configuration
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //gRPC service
            //CreateMap<GetTMInfoResponse, TMInfo>();
            //.ForMember(dest => dest.tmPath, opt => opt.MapFrom(src => src.tmId))
            //.ReverseMap()
            //.ForMember(dest => dest.tmId, opt => opt.MapFrom(src => src.tmPath));
            //CreateMap<CAT.Models.DTOs.QuoteDto, CAT.Models.Entities.Main.Quote>().ReverseMap();
        }
    }

}
