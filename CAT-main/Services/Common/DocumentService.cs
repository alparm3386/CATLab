using AutoMapper;
using CAT.Data;
using CAT.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace CAT.Services.Common
{
    public class DocumentService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly CATConnector _catConnector;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public DocumentService(DbContextContainer dbContextContainer, IConfiguration configuration, CATConnector catConnector, 
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
            _logger = logger;
            _mapper = mapper;
        }

        public void CreateDocument(string sourceLanguage, string targetLanguage, int speciality, string filename, DocumentType documentType)
        {
            //_dbContextContainer
        }
    }
}
