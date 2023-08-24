using AutoMapper;
using CAT.Data;
using CAT.Infrastructure;
using CAT.Services.CAT;

namespace CAT.Services.Quote
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

        public CAT.Models.Entities.Main.Quote CreateQuote(int ClientId, LocaleId sourceLanguage, LocaleId targetLanguage, int speciality, string filename)
        {
            return null;
        }
    }
}
