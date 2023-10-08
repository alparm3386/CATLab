using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CATService.Utils
{
    class AutoMapperConfiguration
    {
        private static MapperConfiguration _config;
        private static IMapper _mapper;
        public static IMapper Mapper
        {
            get { return _mapper; }
        }

        static AutoMapperConfiguration()
        {
            //is there a better solution in a dll than static initializer?
            _config = new MapperConfiguration(cfg => cfg.CreateMap<cat.service.TMEntry, CATService.BackupService.TMEntry>());
            _config = new MapperConfiguration(cfg => cfg.CreateMap<cat.service.TBType, CATService.BackupService.TBType>());
            _config = new MapperConfiguration(cfg => cfg.CreateMap<cat.service.TBEntry, CATService.BackupService.TBEntry>());
            _mapper = _config.CreateMapper();
        }

        public static void StaticTest()
        {
        }
    }
}
