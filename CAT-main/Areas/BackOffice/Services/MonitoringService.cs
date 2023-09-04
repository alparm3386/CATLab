using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Services.Common;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using Task = CAT.Enums.Task;

namespace CAT.Areas.BackOffice.Services
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

        public async Task<object> GetMonitoringData(DateTime dateFrom, DateTime dateTo)
        {
            //the return object
            //dynamic monitoringData = new ExpandoObject();
            var monitoringData = new
            {
                orders = new List<object>(),
                dateFrom,
                dateTo
            };

            //get the orders including jobs, quotes, workflow steps etc...
            var orders = await _dbContextContainer.MainContext.Orders
                    .Include(o => o.Jobs)
                        .ThenInclude(j => j.Quote)
                    .Include(o => o.Jobs)
                        .ThenInclude(j => j.WorkflowSteps).Where(o => o.DateCreated >= dateFrom && o.DateCreated < dateTo).ToListAsync();

            //process orders
            foreach (var dsOrder in orders)
            {
                //the order object
                var order = new
                {
                    id = dsOrder.Id,
                    client = dsOrder.ClientId,
                    dateCreated = dsOrder.DateCreated,
                    words = dsOrder.Words,
                    fee = dsOrder.Fee,
                    jobs = new List<dynamic>()
                };

                //join into the documents table 
                var jobsWithDocuments = (from j in dsOrder.Jobs
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
                                             originalFileName = d.OriginalFileName,
                                             fileName = d.FileName,
                                             words = j.Quote.Words,
                                             fee = j.Quote.Fee,
                                             workflowSteps = j.WorkflowSteps
                                         }).ToList();

                foreach (var dsJob in jobsWithDocuments)
                {
                    //the job object
                    var job = new
                    {
                        dsJob.jobId,
                        dsJob.dateProcessed,
                        dsJob.sourceLanguage,
                        dsJob.targetLanguage,
                        dsJob.speciality,
                        dsJob.speed,
                        dsJob.service,
                        dsJob.documentId,
                        dsJob.originalFileName,
                        dsJob.fileName,
                        dsJob.words,
                        dsJob.fee,
                        workflowSteps = new List<dynamic>()
                    };
                    order.jobs.Add(job);

                    //workflow steps
                    foreach (var dsWorkflowStep in dsJob.workflowSteps)
                    {
                        var workflowStep = new
                        {
                            task = ((Task)dsWorkflowStep.TaskId).GetDisplayName(),
                            status = dsWorkflowStep.Status,
                            startDate = dsWorkflowStep.StartDate,
                            scheduledDate = dsWorkflowStep.ScheduledDate,
                            completionDate = dsWorkflowStep.CompletionDate,
                            fee = dsWorkflowStep.Fee
                        };
                        job.workflowSteps.Add(workflowStep);
                    }
                }

                monitoringData.orders.Add(order);
            }

            monitoringData.orders.Sort((o1, o2) => (o2 as dynamic).id.CompareTo((o1 as dynamic).id));


            return monitoringData;
        }

        public async Task<object> GetJobData(int jobId)
        {
            //get the orders including jobs, quotes, workflow steps etc...
            var job = await _dbContextContainer.MainContext.Jobs
                        .Include(j => j.Quote)
                        .Include(j => j.WorkflowSteps).Where(j => j.Id == jobId).FirstOrDefaultAsync();


            //workflow steps
            var workflowSteps = new List<object>();
            foreach (var dsWorkflowStep in job!.WorkflowSteps!)
            {
                var workflowStep = new
                {
                    task = ((Task)dsWorkflowStep.TaskId).GetDisplayName(),
                    status = dsWorkflowStep.Status,
                    startDate = dsWorkflowStep.StartDate,
                    scheduledDate = dsWorkflowStep.ScheduledDate,
                    completionDate = dsWorkflowStep.CompletionDate,
                    fee = dsWorkflowStep.Fee
                };
                workflowSteps.Add(workflowStep);
            }

            //get the documents for the job
            //_dbContextContainer.MainContext.Documents.Where(d => d.)
            var jobData = new
            {
                jobId = job.Id,
                dateProcessed = job.DateProcessed,
                sourceLanguage = job.Quote!.SourceLanguage,
                targetLanguage = job.Quote.TargetLanguage,
                speciality = job.Quote.Speciality,
                speed = job.Quote.Speed,
                service = job.Quote.Service,
                //documents = ??,
                words = job.Quote.Words,
                fee = job.Quote.Fee,
                workflowSteps
            };

            return jobData;
        }
    }
}
