using AutoMapper;
using CAT.Areas.Identity.Data;
using CAT.Data;
using CATService;
using CAT.Controllers.Api;
using CAT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using static ICSharpCode.SharpZipLib.Zip.ZipEntryFactory;

namespace CAT.Services.CAT
{
    public class JobService
    {
        private readonly IdentityDbContext _identityDBContext;
        private readonly MainDbContext _mainDbContext;
        private readonly TranslationUnitsDbContext _translationUnitsDbContext;
        private readonly IConfiguration _configuration;
        private readonly CATConnector _catConnector;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;


        public JobService(IdentityDbContext identityDBContext, MainDbContext mainDbContext, TranslationUnitsDbContext translationUnitsDbContext, 
            IConfiguration configuration, CATConnector catConnector, IMemoryCache cache, IMapper mapper, ILogger<JobService> logger)
        {
            _identityDBContext = identityDBContext;
            _mainDbContext = mainDbContext;
            _translationUnitsDbContext = translationUnitsDbContext;
            _configuration = configuration;
            _catConnector = catConnector;
            _cache = cache;
            _logger = logger;
            _mapper = mapper;
        }

        public void ProcessJob(int idJob)
        {
            _catConnector.ParseDoc(idJob);
        }
    }
}
