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

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 60, 60 })]
        public void ProcessJob(int jobId)
        {
            //job process
            var jobProcess = _dbContextContainer.MainContext.JobProcesses.Where(jp => jp.JobId == jobId).FirstOrDefault();

            if (jobProcess == null)
            {
                jobProcess = new JobProcess();
                jobProcess.JobId = jobId;
                jobProcess.ProcessStarted = DateTime.Now;
                jobProcess.ProcessId = "_m_" + Guid.NewGuid().ToString();
            }
            else
            {
                //check if it is parsed already
                if (jobProcess?.ProcessEnded != null)
                    throw new Exception("Already processed.");
            }

            _catConnector.ParseDoc(jobId);

            jobProcess!.ProcessEnded = DateTime.Now;
            _dbContextContainer.MainContext.JobProcesses.Add(jobProcess);
            _dbContextContainer.MainContext.SaveChanges();
        }

        public FileData CreateDocument(int idJob, string userId, bool updateTM)
        {
            return _catConnector.CreateDoc(idJob, userId, updateTM);
        }
    }
}
