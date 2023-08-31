using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Models.Common;
using CAT.Models.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace CAT.Services.Common
{
    public class DocumentService : IDocumentService
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public DocumentService(DbContextContainer dbContextContainer, IConfiguration configuration,
            IMapper mapper, ILogger<DocumentService> logger)
        {
            _dbContextContainer = dbContextContainer;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<TempDocument> CreateTempDocumentAsync(IFormFile formFile, DocumentType documentType, int filterId)
        {
            try
            {
                //save the file into the temp folder
                var tempFolder = _configuration["TempFolder"]!;
                var filePath = Path.Combine(tempFolder, formFile.FileName);
                var fileName = FileHelper.GetUniqueFileName(filePath);
                filePath = Path.Combine(tempFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }

                //get the md5 hash
                var md5Hash = "";
                using (var stream = formFile.OpenReadStream())
                {
                    md5Hash = FileHelper.CalculateMD5(filePath);
                }

                //create the document
                var document = new TempDocument()
                {
                    DocumentType = (int)documentType,
                    FileName = fileName,
                    OriginalFileName = formFile.FileName,
                    MD5Hash = md5Hash,
                    FilterId = filterId
                };

                await _dbContextContainer.MainContext.TempDocuments.AddAsync(document);
                await _dbContextContainer.MainContext.SaveChangesAsync();

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateDocument ERROR: " + ex.Message);
                throw;
            }
        }

        public async Task<Document> CreateDocumentFromTempDocumentAsync(int tempDocumentId)
        {
            var tempDocument = await _dbContextContainer.MainContext.TempDocuments.FirstOrDefaultAsync(d => d.Id == tempDocumentId);

            //check if the file exists in the source files folder
            var tempFolder = _configuration["TempFolder"]!;
            var sourceFolder = _configuration["SourceFilesFolder"]!;
            var tempDocPath = Path.Combine(tempFolder, tempDocument!.FileName);
            var docCandidate = await _dbContextContainer.MainContext.Documents.FirstOrDefaultAsync(d => d.MD5Hash == tempDocument.MD5Hash);
            var fileName = tempDocument!.OriginalFileName;
            var bCopy = false;
            if (docCandidate == null)
                bCopy = true;
            else
            {
                var docCandidatePath = Path.Combine(sourceFolder, docCandidate.FileName);
                var fiDocCandidate = new FileInfo(docCandidatePath);
                var fiTempDoc = new FileInfo(tempDocPath);
                if (fiTempDoc.Length != fiDocCandidate.Length)
                    bCopy = true;
                else
                    fileName = docCandidate.FileName;
            }

            //copy the file 
            if (bCopy)
            {
                var filePath = Path.Combine(sourceFolder, tempDocument.OriginalFileName);
                fileName = FileHelper.GetUniqueFileName(filePath);
                filePath = Path.Combine(sourceFolder, fileName);
                File.Copy(tempDocPath, filePath);
            }

            //create the document
            var document = new Document()
            {
                FileName = tempDocument!.FileName,
                OriginalFileName = tempDocument.OriginalFileName,
                DocumentType = (int)DocumentType.Original,
                MD5Hash = tempDocument.MD5Hash
            };
            _dbContextContainer.MainContext.Documents.Add(document);

            //save the analysis
            var tempQuote = await _dbContextContainer.MainContext.TempQuotes.FirstOrDefaultAsync(q => q.TempDocumentId == tempDocumentId);
            if (tempQuote != null)
            {
                if (tempQuote.Analysis != null)
                {
                    var stat = JsonConvert.DeserializeObject<Statistics>(tempQuote.Analysis);
                    var analysis = new Analysis()
                    {
                        DocumentId = document.Id,
                        Match_101 = stat!.match_101,
                        Match_100 = stat!.match_100,
                        Match_95_99 = stat!.match_95_99,
                        Match_85_94 = stat!.match_85_94,
                        Match_75_84 = stat!.match_75_84,
                        Match_50_74 = stat!.match_50_74,
                        No_match = stat!.no_match,
                        Repetitions = stat!.repetitions,
                        SourceLanguage = tempQuote.SourceLanguage,
                        TargetLanguage = stat!.targetLang,
                        Speciality = tempQuote.SpecialityId,
                        Type = AnalysisType.Normal
                    };
                    _dbContextContainer.MainContext.Analisys.Add(analysis);
                }
            }

            //save all the changes
            await _dbContextContainer.MainContext.SaveChangesAsync();

            return document;
        }
    }
}
