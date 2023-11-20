using AutoMapper;
using CAT.Data;
using CAT.Models.Common;
using CAT.Models.Entities.Main;
using Hangfire;

namespace CAT.Services.Common
{
    public class JobService : IJobService
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

        public FileData CreateDocument(int idJob, string userId, bool updateTM)
        {
            return _catConnector.CreateDoc(idJob, userId, updateTM);
        }
    }
}
