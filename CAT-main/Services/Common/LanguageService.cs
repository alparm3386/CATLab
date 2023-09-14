using AutoMapper;
using CAT.Data;
using CAT.Models.Entities.Main;

namespace CAT.Services.Common
{
    public class LanguageService : ILanguageService
    {
        private readonly MainDbContext _mainDbContext;
        private readonly ILogger<JobService> _logger;

        private static Dictionary<int, string> _languageCache = new Dictionary<int, string>();
        
        public LanguageService(MainDbContext mainDbContext, ILogger<JobService> logger)
        {
            _mainDbContext = mainDbContext;
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
