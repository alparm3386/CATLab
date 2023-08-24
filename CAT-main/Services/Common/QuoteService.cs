using AutoMapper;
using CAT.Data;
using CAT.Models.Common;
using CAT.Models.DTOs;
using CAT.Models.Entities.Main;
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

        public QuoteDto CreateQuote(int clientId, LocaleId sourceLanguage, LocaleId targetLanguage, int speciality, string filename)
        {
            try
            {
                //get the analisys
                //_catConnector.GetStatisticsForDocument()
                //create doc
                var quote = new Quote()
                {
                    SourceLanguage = sourceLanguage.Language,
                    TargetLanguage = targetLanguage.Language,
                    Speciality = speciality,
                    DateCreated = DateTime.Now,
                    Fee = 10.0
                };

                // Save the job
                _dbContextContainer.MainContext.Quotes.Add(quote);
                _dbContextContainer.MainContext.SaveChanges();

                return _mapper.Map<QuoteDto>(quote);
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateQuote ERROR:" + ex.Message);
                throw;
            }
        }
    }
}
