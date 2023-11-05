using AutoMapper;
using CAT.Data;
using CAT.Models.Common;

namespace CAT.Services.Common
{
    public class JobService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly ICATConnector _catConnector;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;


        public JobService(DbContextContainer dbContextContainer, IConfiguration configuration, ICATConnector catConnector, 
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
            _logger = logger;
            _mapper = mapper;
        }

        public void ProcessJob(int idJob)
        {
            _catConnector.ParseDoc(idJob);
        }

        public FileData CreateDocument(int idJob)
        {
            return _catConnector.CreateDoc(idJob, Guid.NewGuid().ToString(), false);
        }
    }
}
