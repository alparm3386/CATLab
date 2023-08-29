using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Models.Common;
using CAT.Models.Entities.Main;
using Microsoft.Extensions.Caching.Memory;
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

        public async Task<Document> CreateDocumentAsync(IFormFile formFile, DocumentType documentType)
        {
            throw new NotImplementedException();
        }
    }
}
