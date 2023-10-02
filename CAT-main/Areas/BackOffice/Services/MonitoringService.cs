﻿using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Infrastructure;
using CAT.Models.Entities.Main;
using CAT.Services.Common;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Data;
using System.Dynamic;
using System.Xml.Linq;
using Xunit.Sdk;
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
                        .Include(j => j.WorkflowSteps)
                        .Include(j => j.Order)
                        .ThenInclude(o => o!.Client)
                        .ThenInclude(c => c.Company)
                        .Where(j => j.Id == jobId).FirstOrDefaultAsync();

            //PM
            var pmUser = await _dbContextContainer.IdentityContext.Users.Where(user => user.Id == job!.Order!.Client.Company.PMId).FirstOrDefaultAsync();
            job!.Order!.Client.Company.ProjectManager = pmUser!;

            //workflow steps
            var workflowSteps = new List<dynamic>();
            foreach (var dsWorkflowStep in job!.WorkflowSteps!)
            {
                var workflowStep = new
                {
                    //task = ((Task)dsWorkflowStep.TaskId).GetDisplayName(),
                    task = dsWorkflowStep.TaskId,
                    status = dsWorkflowStep.Status,
                    startDate = dsWorkflowStep.StartDate,
                    scheduledDate = dsWorkflowStep.ScheduledDate,
                    completionDate = dsWorkflowStep.CompletionDate,
                    fee = dsWorkflowStep.Fee,
                    stepOrder = dsWorkflowStep.StepOrder
                };
                workflowSteps.Add(workflowStep);
            }

            //sort the workflow steps
            workflowSteps = workflowSteps.OrderBy(ws => ws.stepOrder).ToList();

            //get the documents for the job
            var dsDocuments = await _dbContextContainer.MainContext.Documents.Where(d => d.JobId == jobId).ToListAsync();
            var documents = dsDocuments.Select(d => new
            {
                id = d.Id,
                jobId = d.JobId,
                fileName = d.FileName,
                originalFileName = d.OriginalFileName,
                documentType = d.DocumentType,
                documentTypeName = EnumHelper.GetDisplayName((DocumentType)d.DocumentType)
            }).ToList();

            //get the analysys
            var originalDoc = documents.FirstOrDefault(d => d.documentType == (int)DocumentType.Original);
            var analysis = await _dbContextContainer.MainContext.Analisys.AsNoTracking()
                .Where(a => a.DocumentId == originalDoc!.id).ToListAsync();

            //get the allocations
            var allocations = await _dbContextContainer.MainContext.Allocations.AsNoTracking()
                .Where(a => a.JobId == jobId).ToListAsync();

            var jobData = new
            {
                jobId = job.Id,
                orderId = job.OrderId,
                dateProcessed = job.DateProcessed,
                sourceLanguage = job.Quote!.SourceLanguage,
                targetLanguage = job.Quote.TargetLanguage,
                speciality = job.Quote.Speciality,
                specialityName = EnumHelper.GetDisplayName((Speciality)job.Quote.Speciality),
                speed = EnumHelper.GetDisplayName((ServiceSpeed)job.Quote.Speed),
                service = job.Quote.Service,
                serviceName = EnumHelper.GetDisplayName((Service)job.Quote.Service),
                documents,
                words = job.Quote.Words,
                analysis = analysis,
                fee = job.Quote.Fee,
                onlineEditorLink = UrlHelper.CreateOnlineEditorUrl(_configuration!["OnlineEditorBaseUrl"]!, job.Id, OEMode.Admin),
                workflowSteps,
                companyName = job.Order!.Client.Company.Name,
                companyId = job.Order!.Client.Company.Id,
                projectManager = pmUser!.FullName,
                allocations = allocations
            };

            return jobData;
        }

        public async System.Threading.Tasks.Task AllocateJob(int jobId, Task task, string userId)
        {
            try
            {
                //check if the task is available
                var allocatedTask = await _dbContextContainer.MainContext.Allocations
                    .Where(a => a.JobId == jobId && a.TaskId == (int)task && !a.ReturnUnsatisfactory).FirstOrDefaultAsync();

                if (allocatedTask != null)
                    throw new CATException("The job is already allocated.");

                //create the allocation
                var allocation = new Allocation()
                {
                    Status = 0,
                    Fee = 1,
                    UserId = userId,
                    JobId = jobId,
                    TaskId = (int)task,
                    ReturnUnsatisfactory = false,
                    AllocationDate = DateTime.Now,
                    CompletionDate = null
                    //WorkflowStepId = ???
                };

                //save the allocation
                _dbContextContainer.MainContext.Allocations.Add(allocation);
                await _dbContextContainer.MainContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AllocatJob ERROR:" + ex.Message);
                throw;
            }
        }

        public async System.Threading.Tasks.Task DeallocateJob(int jobId, Task task, string comment)
        {
            try
            {
                //check if the task is available
                var allocation = await _dbContextContainer.MainContext.Allocations
                    .Where(a => a.JobId == jobId && a.TaskId == (int)task && !a.ReturnUnsatisfactory).FirstOrDefaultAsync();

                if (allocation == null)
                    throw new CATException("The job is not allocated.");

                allocation.AdminComment = comment;
                allocation.ReturnUnsatisfactory = true;

                //save the allocation
                _dbContextContainer.MainContext.Allocations.Add(allocation);
                await _dbContextContainer.MainContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("AllocatJob ERROR:" + ex.Message);
                throw;
            }
        }
    }
}
