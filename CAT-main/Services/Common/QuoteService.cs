using AutoMapper;
using CAT.Data;
using CAT.Infrastructure;
using CAT.Services.Common;

namespace CAT.Services.Common
{
    public class QuoteService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly CATConnector _catConnector;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public QuoteService(DbContextContainer dbContextContainer, IConfiguration configuration, CATConnector catConnector,
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
            _logger = logger;
            _mapper = mapper;
        }

        public void CreateQuote(int ClientId, LocaleId sourceLanguage, LocaleId targetLanguage, int speciality, string filename)
        {
        }
    }
}
