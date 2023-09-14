using AutoMapper;
using CAT.Data;
using CAT.Models.Entities.Main;
using Microsoft.EntityFrameworkCore;

namespace CAT.Services.Common
{
    public class LanguageService : ILanguageService
    {
        private readonly MainDbContextFactory _mainDbContextFactory;
        private readonly ILogger<JobService> _logger;

        private static Dictionary<int, Language> _languageIdCache = new Dictionary<int, Language>();
        private static Dictionary<string, Language> _languageCodeIso639_1Cache = new Dictionary<string, Language>();

        public LanguageService(MainDbContextFactory mainDbContextFactory, ILogger<JobService> logger)
        {
            _mainDbContextFactory = mainDbContextFactory;
            _logger = logger;
        }

        public String GetLanguageCodeIso639_1(int languageId) 
        {
            //caching
            if (_languageIdCache.Count == 0)
            {
                using (var mainDbContext = _mainDbContextFactory.CreateDbContext())
                {
                    _languageIdCache = mainDbContext.Languages.AsNoTracking().ToDictionary(l => l.Id, l => l);
                }
            }

            return _languageIdCache[languageId].ISO639_1;
        }

        public int GetLanguageIdFromIso639_1Code(string laguageCode)
        {
            //caching
            if (_languageCodeIso639_1Cache.Count == 0)
            {
                using (var mainDbContext = _mainDbContextFactory.CreateDbContext())
                {
                    _languageCodeIso639_1Cache = mainDbContext.Languages.AsNoTracking().ToDictionary(l => l.ISO639_1, l => l);
                }
            }

            return _languageCodeIso639_1Cache[laguageCode].Id;
        }
    }
}
