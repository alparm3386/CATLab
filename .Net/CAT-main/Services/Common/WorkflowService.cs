using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Models.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Task = CAT.Enums.Task;

namespace CAT.Services.Common
{
    public class WorkflowService : IWorkflowService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly IQuoteService _quoteService;
        private readonly IDocumentService _documentService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public WorkflowService(DbContextContainer dbContextContainer, IConfiguration configuration, IDocumentService documentService,
            IQuoteService quoteService, IMapper mapper, ILogger<WorkflowService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _quoteService = quoteService;
            _documentService = documentService;
            _logger = logger;
            _mapper = mapper;
        }

        public async System.Threading.Tasks.Task CreateWorkflowAsync(int orderId)
        {
            var jobs = await _dbContextContainer.MainContext.Jobs.Where(j => j.OrderId == orderId).Include(j => j.Quote).ToListAsync();

            //create the workflow for each job
            foreach (var job in jobs) 
            {
                var workflowSteps = new List<WorkflowStep>();
                var service = job.Quote!.Service;
                if (service == (int)Service.AI)
                {
                    //new job
                    var workflowStep = CreateWorkflowStep(job, null!, Task.NewJob);
                    workflowSteps.Add(workflowStep);

                    //AI process
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.AIProcess);
                    workflowSteps.Add(workflowStep);

                    //client review
                    if (job.Quote.ClientReview)
                    {
                        workflowStep = CreateWorkflowStep(job, workflowStep, Task.ClientReview);
                        workflowSteps.Add(workflowStep);
                    }

                    //job completed
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Completed);
                    workflowSteps.Add(workflowStep);

                    //credit linguists
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.CreditLinguists);
                    workflowSteps.Add(workflowStep);

                    //billing
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Billing);
                    workflowSteps.Add(workflowStep);

                    //delivery
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Delivery);
                    workflowSteps.Add(workflowStep);

                    //end
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.End);
                    workflowSteps.Add(workflowStep);
                }
                else if (service == (int)Service.AIWithRevision)
                {
                    //new job
                    var workflowStep = CreateWorkflowStep(job, null!, Task.NewJob);
                    workflowSteps.Add(workflowStep);

                    //AI process
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.AIProcess);
                    workflowSteps.Add(workflowStep);

                    //job board
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Jobboard);
                    workflowSteps.Add(workflowStep);

                    //revision
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Revision);
                    workflowSteps.Add(workflowStep);

                    //client review
                    if (job.Quote.ClientReview)
                    {
                        workflowStep = CreateWorkflowStep(job, workflowStep, Task.ClientReview);
                        workflowSteps.Add(workflowStep);
                    }

                    //job completed
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Completed);
                    workflowSteps.Add(workflowStep);

                    //credit linguists
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.CreditLinguists);
                    workflowSteps.Add(workflowStep);

                    //billing
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Billing);
                    workflowSteps.Add(workflowStep);

                    //delivery
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Delivery);
                    workflowSteps.Add(workflowStep);

                    //end
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.End);
                    workflowSteps.Add(workflowStep);
                }
                else if (service == (int)Service.AIWithTranslationAndRevision)
                {
                    //new job
                    var workflowStep = CreateWorkflowStep(job, null!, Task.NewJob);
                    workflowSteps.Add(workflowStep);

                    //AI process
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.AIProcess);
                    workflowSteps.Add(workflowStep);

                    //job board
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Jobboard);
                    workflowSteps.Add(workflowStep);

                    //translation
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Translation);
                    workflowSteps.Add(workflowStep);

                    //job board
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Jobboard);
                    workflowSteps.Add(workflowStep);

                    //revision
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Revision);
                    workflowSteps.Add(workflowStep);

                    //client review
                    if (job.Quote.ClientReview)
                    {
                        workflowStep = CreateWorkflowStep(job, workflowStep, Task.ClientReview);
                        workflowSteps.Add(workflowStep);
                    }

                    //job completed
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Completed);
                    workflowSteps.Add(workflowStep);

                    //credit linguists
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.CreditLinguists);
                    workflowSteps.Add(workflowStep);

                    //billing
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Billing);
                    workflowSteps.Add(workflowStep);

                    //delivery
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Delivery);
                    workflowSteps.Add(workflowStep);

                    //end
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.End);
                    workflowSteps.Add(workflowStep);
                }
                else if (service == (int)Service.TranslationWithRevision)
                {
                    //new job
                    var workflowStep = CreateWorkflowStep(job, null!, Task.NewJob);
                    workflowSteps.Add(workflowStep);

                    //job board
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Jobboard);
                    workflowSteps.Add(workflowStep);

                    //translation
                    workflowStep = CreateWorkflowStep(job, workflowStep!, Task.Translation);
                    workflowSteps.Add(workflowStep);

                    //job board
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Jobboard);
                    workflowSteps.Add(workflowStep);

                    //revision
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Revision);
                    workflowSteps.Add(workflowStep);

                    //client review
                    if (job.Quote.ClientReview)
                    {
                        workflowStep = CreateWorkflowStep(job, workflowStep, Task.ClientReview);
                        workflowSteps.Add(workflowStep);
                    }

                    //job completed
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Completed);
                    workflowSteps.Add(workflowStep);

                    //credit linguists
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.CreditLinguists);
                    workflowSteps.Add(workflowStep);

                    //billing
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Billing);
                    workflowSteps.Add(workflowStep);

                    //delivery
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.Delivery);
                    workflowSteps.Add(workflowStep);

                    //end
                    workflowStep = CreateWorkflowStep(job, workflowStep, Task.End);
                    workflowSteps.Add(workflowStep);
                }
                else
                    throw new NotImplementedException();

                await _dbContextContainer.MainContext.WorkflowSteps.AddRangeAsync(workflowSteps);
            }

            await _dbContextContainer.MainContext.SaveChangesAsync();
        }

        public async void StartNextStep(int jobId)
        {
            //get the workflow steps
            var workflowSteps  = await _dbContextContainer.MainContext.WorkflowSteps.Where(ws => ws.JobId == jobId).ToListAsync();
            var currentStep = workflowSteps.Select(ws => ws.Status == (int)WorkflowStatus.InProgress).FirstOrDefault();
            if (workflowSteps.All(ws => ws.Status == (int)WorkflowStatus.NotStarted))
            { 
            }

        }

        private WorkflowStep CreateWorkflowStep(Job job, WorkflowStep previousStep, Task task) 
        { 
            //calculate fields for the workflow step
            var workflowStep = new WorkflowStep() 
            {
                JobId = job.Id,
                StepOrder = previousStep != null ? previousStep.StepOrder + 1 : 0,
                TaskId = (int)task,
                Status = 0,
                StartDate = DateTime.Now,
                ScheduledDate = DateTime.Now.AddMinutes(15),
                Fee = task == Task.Revision ?  (decimal?)(job.Quote!.Words * 0.1) : 0
            };

            return workflowStep;
        }
    }
}
