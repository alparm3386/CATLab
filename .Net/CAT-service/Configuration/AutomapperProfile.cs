using AutoMapper;
using CAT.Enums;
using CAT.Models;

namespace CAT.Configuration
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //gRPC service
            CreateMap<TMInfo, GetTMInfoResponse>()
                .ForMember(dest => dest.LastAccess, opt => opt.MapFrom(src => ((DateTimeOffset)src.lastAccess).ToUnixTimeMilliseconds()))
                .ForMember(dest => dest.TmType, opt => opt.MapFrom(src => (int)src.tmType));  // Assuming TMType is an enum and aligns with int32 values in proto
            //.ForMember(dest => dest.tmPath, opt => opt.MapFrom(src => src.tmId))
            //.ReverseMap()
            //.ForMember(dest => dest.tmId, opt => opt.MapFrom(src => src.tmPath));
            //CreateMap<CAT.Models.DTOs.QuoteDto, CAT.Models.Entities.Main.Quote>().ReverseMap();
        }
    }

}
