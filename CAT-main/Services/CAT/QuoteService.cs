using AutoMapper;
using CAT.Data;
using Microsoft.Extensions.Caching.Memory;

namespace CAT.Services.CAT
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

        public void CreateQuote(string sourceLanguage, string targetLanguage, int speciality, string filename)
        {
               
        }
    }
}
