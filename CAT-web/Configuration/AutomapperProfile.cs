using AutoMapper;

namespace CATWeb.Configuration
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CAT service
            CreateMap<CATService.TMMatch, Models.CAT.TMMatch>().ReverseMap();
            CreateMap<CATService.TBEntry, Models.CAT.TBEntry>().ReverseMap();
            CreateMap<CATService.TMAssignment, Models.CAT.TMAssignment>().ReverseMap();
        }
    }

}
