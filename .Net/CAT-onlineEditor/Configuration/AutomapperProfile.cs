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
            //CreateMap<Proto.TMMatch, Models.Common.TMMatch>().ReverseMap();
            //CreateMap<Proto.TBEntry, Models.Common.TBEntry>().ReverseMap();
            //CreateMap<Proto.TMAssignment, Models.Common.TMAssignment>().ReverseMap();
            CreateMap<TranslationUnit, TranslationUnitDTO>().ReverseMap();
        }
    }

}
