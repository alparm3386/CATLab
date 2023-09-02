﻿using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Dynamic;
using Task = CAT.Enums.Task;

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

        public async Task<Object> GetMonitoringData(DateTime dateFrom, DateTime dateTo)
        {
            //the return object
            //dynamic monitoringData = new ExpandoObject();
            var monitoringData = new
            {
                orders = new List<Object>(),
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
                                             workflowSteps = j.WorkflowSteps
                                         }).ToList();

                foreach (var dsJob in jobsWithDocuments)
                {
                    //the job object
                    var job = new
                    {
                        jobId = dsJob.jobId,
                        dateProcessed = dsJob.dateProcessed,
                        sourceLanguage = dsJob.sourceLanguage,
                        targetLanguage = dsJob.targetLanguage,
                        speciality = dsJob.speciality,
                        speed = dsJob.speed,
                        service = dsJob.service,
                        documentId = dsJob.documentId,
                        originalFileName = dsJob.originalFileName,
                        fileName = dsJob.fileName,
                        workflowSteps = new List<dynamic>()
                    };
                    order.jobs.Add(job);

                    //workflow steps
                    foreach (var dsWorkflowStep in dsJob.workflowSteps)
                    {
                        var workflowStep = new
                        {
                            task = EnumHelper.GetDisplayName((Task)dsWorkflowStep.TaskId),
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


            return monitoringData;
        }
    }
}
