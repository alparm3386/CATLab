using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Models.Common;
using CAT.Models.Entities.Main;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Task = CAT.Enums.Task;

namespace CAT.Services.Common
{
    public class JobService : IJobService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly ICATConnector _catConnector;
        private readonly ILanguageService _languageService;
        private readonly IDocumentService _documentService;
        private readonly IWorkflowService _workflowService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;


        public JobService(DbContextContainer dbContextContainer, IConfiguration configuration, ICATConnector catConnector,
            ILanguageService languageService, IDocumentService documentService, IWorkflowService workflowService,
            IMapper mapper, ILogger<JobService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _catConnector = catConnector;
            _languageService = languageService;
            _documentService = documentService;
            _workflowService = workflowService;
            _logger = logger;
            _mapper = mapper;
        }

        public FileData CreateDocument(int jobId, string userId, bool updateTM)
        {
            return _catConnector.CreateDoc(jobId, userId, updateTM);
        }

        public async System.Threading.Tasks.Task SubmitJobAsync(int jobId, string userId)
        {
            //get the current workflow step
            var currentWorkflowStep = await _workflowService.GetCurrentStepAsync(jobId);
            if (!CanSubmitJob((Task)currentWorkflowStep.TaskId))
                throw new Exception("The job cannot be submitted.");

            //assemble the document
            var outFile = _catConnector.CreateDoc(jobId, userId, false);

            //get the job details
            var job = await _dbContextContainer.MainContext.Jobs.Include(j => j.Quote).FirstOrDefaultAsync(j => j.Id == jobId);
            var document = await _dbContextContainer.MainContext.Documents.Where(d => d.Id == job!.SourceDocumentId).FirstOrDefaultAsync();
            var targetLanguage = await _languageService.GetLanguageCodeIso639_1Async(job!.Quote!.TargetLanguage);

            var documentType = _documentService.GetDocumentTypeForTask((Task)currentWorkflowStep.TaskId);

            //save the document
            var outDirectory = _documentService.GetDocumentFolderForDocumentType(documentType);
            var filePath = FileHelper.CreateFileNameForTask(outDirectory!, document!.OriginalFileName, targetLanguage, 
                (Task)currentWorkflowStep.TaskId);
            document = await _documentService.CreateDocumentAsync(jobId, outFile!.Content!, Path.GetFileName(filePath), documentType);

            //de-allocate the job
            var allocation = await _dbContextContainer.MainContext.Allocations
                .FirstOrDefaultAsync(a => a.JobId == jobId && a.TaskId == currentWorkflowStep.TaskId && a.ReturnUnsatisfactory == false);
            if (allocation != null)
            {
                allocation.DeallocationDate = DateTime.Now;
                allocation.DeallocatedBy = userId;
            }

            //update the completed document
            job.CompletedDocumentId = document.Id;

            //start the next step
            await _workflowService.StartNextStepAsync(jobId);

            //save changes
            await _dbContextContainer.MainContext.SaveChangesAsync();
        }

        private bool CanSubmitJob(Task task)
        {
            return true;
        }
    }
}
