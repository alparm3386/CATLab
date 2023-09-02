using AutoMapper;
using CAT.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;

namespace CAT.Services.Common
{
    public class MonitoringService : IMonitoringService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public MonitoringService(DbContextContainer dbContextContainer, IConfiguration configuration,
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<dynamic> GetMonitoringData(DateTime dateFrom, DateTime dateTo)
        {
            //the return object
            dynamic monitoringData = new ExpandoObject();
            monitoringData.orders = new List<dynamic>();

            //get the orders including jobs, quotes, workflow steps etc...
            var orders = await _dbContextContainer.MainContext.Orders
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.Quote)
                .Include(o => o.Jobs)
                    .ThenInclude(j => j.WorkflowSteps).Where(o => o.DateCreated >= dateFrom && o.DateCreated < dateTo).ToListAsync();

            //process orders
            foreach (var order in orders)
            {
                //the order object
                dynamic dOrder = new ExpandoObject();
                dOrder.id = order.Id;
                dOrder.client = order.ClientId;
                dOrder.dateCreated = order.DateCreated;
                dOrder.jobs = new List<dynamic>();

                //join into the documents table 
                var jobsWithDocuments = (from j in order.Jobs
                                         join d in _dbContextContainer.MainContext.Documents on j.SourceDocumentId equals d.Id
                                     select new
                                     {
                                         jobId = j.Id,
                                         dateProcessed = j.DateProcessed,
                                         sourceLanguage = j.Quote!.SourceLanguage,
                                         targetLanguage = j.Quote.TargetLanguage,
                                         speciality = j.Quote.Speciality,
                                         speed = j.Quote.Speed,
                                         service = j.Quote.Service,
                                         documentId = d.Id,
                                         d.OriginalFileName,
                                         d.FileName
                                     }).ToList();

                foreach (var job in jobsWithDocuments)
                {
                    //the job object
                    dynamic dJob = new ExpandoObject();
                    //job.id = dsJob.Id;
                    //job.
                }

                monitoringData.orders.Add(order);
            }


            return monitoringData;
        }
    }
}
