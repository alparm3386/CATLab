using AutoMapper;
using CAT.Data;
using CAT.Models.Entities.Main;
using Microsoft.EntityFrameworkCore;

namespace CAT.Services.Common
{
    public class LanguageService : ILanguageService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly ILogger<JobService> _logger;

        private static Dictionary<int, Language> _languageIdCache = new Dictionary<int, Language>();
        private static Dictionary<string, Language> _languageCodeIso639_1Cache = new Dictionary<string, Language>();

        public LanguageService(DbContextContainer dbContextContainer, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _logger = logger;
        }

        public async Task<String> GetLanguageCodeIso639_1(int languageId) 
        {
            //caching
            if (_languageIdCache.Count == 0)
            {
                _languageIdCache = await _dbContextContainer.MainContext.Languages.AsNoTracking().ToDictionaryAsync(l => l.Id, l => l);
            }

            return _languageIdCache[languageId].ISO639_1;
        }

        public async Task<int> GetLanguageIdFromIso639_1Code(string laguageCode)
        {
            //caching
            if (_languageCodeIso639_1Cache.Count == 0)
            {
                _languageCodeIso639_1Cache = await _dbContextContainer.MainContext.Languages.AsNoTracking().ToDictionaryAsync(l => l.ISO639_1, l => l);
            }

            return _languageCodeIso639_1Cache[laguageCode].Id;
        }

        public async Task<Dictionary<int, Language>> GetLanguages()
        {
            //caching
            if (_languageIdCache.Count == 0)
            {
                _languageIdCache = await _dbContextContainer.MainContext.Languages.AsNoTracking().ToDictionaryAsync(l => l.Id, l => l);
            }

            return _languageIdCache;
        }
    }
}
