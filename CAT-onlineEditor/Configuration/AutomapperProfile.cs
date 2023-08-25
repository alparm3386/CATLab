using AutoMapper;
using CAT.Models;
using CAT.Models.Entities.TranslationUnits;

namespace CAT.Configuration
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CAT service
            CreateMap<CATService.TMMatch, Models.Common.TMMatch>().ReverseMap();
            CreateMap<CATService.TBEntry, Models.Common.TBEntry>().ReverseMap();
            CreateMap<CATService.TMAssignment, Models.Common.TMAssignment>().ReverseMap();
            CreateMap<TranslationUnit, TranslationUnitDTO>().ReverseMap();
        }
    }

}
