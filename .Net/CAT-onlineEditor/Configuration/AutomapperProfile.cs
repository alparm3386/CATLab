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
            CreateMap<TranslationUnit, TranslationUnitDTO>().ReverseMap();
        }
    }

}
