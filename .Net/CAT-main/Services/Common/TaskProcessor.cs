using CAT.Models.Entities.Main;
using CAT.Enums;
using Task = CAT.Enums.Task;
using CAT.Helpers;
using CAT.Data;
using Microsoft.EntityFrameworkCore;

namespace CAT.Services.Common
{
    public class TaskProcessor : ITaskProcessor
    {
        private readonly IConfiguration _configuration;
        private readonly IDocumentService _documentService;
        private readonly DbContextContainer _dbContextContainer;
        private readonly ICATConnector _catconnector;
        private readonly ILanguageService _languageService;

        public TaskProcessor(IConfiguration configuration, IDocumentService documentService, DbContextContainer dbContextContainer, ICATConnector catconnector,
            ILanguageService languageService)
        {
            _configuration = configuration;
            _documentService = documentService;
            _dbContextContainer = dbContextContainer;
            _catconnector = catconnector;
            _languageService = languageService;
        }

        public async Task<bool> ProcessTaskAsync(WorkflowStep workflowStep)
        {
            //parse and pre-translate the document
            if (workflowStep.TaskId == (int)Task.NewJob)
            {
                _catconnector.ParseDoc(workflowStep.JobId);
                return true;
            }

            if (workflowStep.TaskId == (int)Task.AIProcess)
            {
                //assemble the document
                var outFile = _catconnector.CreateDoc(workflowStep.JobId, "6fa6c2c7-14b1-44c1-9806-c76dac688078", false);

                //get the job details
                var job = await _dbContextContainer.MainContext.Jobs.Include(j => j.Quote).FirstOrDefaultAsync(j => j.Id == workflowStep.JobId);
                var document = await _dbContextContainer.MainContext.Documents.Where(d => d.Id == job!.SourceDocumentId).FirstOrDefaultAsync();
                var targetLanguage = await _languageService.GetLanguageCodeIso639_1Async(job!.Quote!.TargetLanguage);

                //save the document
                var outDirectory = _configuration["OutputFilesFolder"];
                var filePath = FileHelper.CreateFileNameForTask(outDirectory!, document!.OriginalFileName, targetLanguage, Task.AIProcess);
                document = await _documentService.CreateDocumentAsync(workflowStep.JobId, outFile!.Content!, filePath, DocumentType.AI);

                //update the completed document
                job.CompletedDocumentId = document.Id;

                return true;
            }

            return false;
        }
    }
}
