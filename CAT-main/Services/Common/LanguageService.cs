using AutoMapper;
using CAT.Data;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public class LanguageService : ILanguageService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly ILogger<JobService> _logger;

        private static Dictionary<int, string> _languageCache = new Dictionary<int, string>();
        
        public LanguageService(DbContextContainer dbContextContainer, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _logger = logger;
        }

        public String GetLanguageCodeIso639_1(int languageId) 
        {
            return _languageCache[languageId];
        }

        public int GetLanguageIdFromIso639_1Code(string laguageCode)
        {
            return 1000; // _languageCache[languageId];
        }
    }
}
