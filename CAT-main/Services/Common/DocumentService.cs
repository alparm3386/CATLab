using AutoMapper;
using CAT.Data;
using CAT.Enums;
using CAT.Helpers;
using CAT.Models.Common;
using CAT.Models.Entities.Main;
using Microsoft.Extensions.Caching.Memory;
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

        public async Task<Document> CreateDocumentAsync(IFormFile formFile, DocumentType documentType)
        {
            try
            {
                //save the file
                var sourceFilesFolder = Path.Combine(_configuration["SourceFilesFolder"]!);
                // Generate a unique file name based on the original file name
                string fileName = FileHelper.GetUniqueFileName(formFile.FileName);

                // Combine the unique file name with the server's path to create the full path
                string filePath = Path.Combine(sourceFilesFolder, fileName);

                // Save the file to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }

                var md5 = "";
                using (var stream = formFile.OpenReadStream())
                {
                    md5 = FileHelper.CalculateMD5(stream);
                }

                //create the document
                var document = new Document()
                {
                    DocumentType = (int)DocumentType.Original,
                    FileName = fileName,
                    OriginalFileName = formFile.FileName,
                    MD5Hash = md5
                };

                await _dbContextContainer.MainContext.Documents.AddAsync(document);
                await _dbContextContainer.MainContext.SaveChangesAsync();

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateDocument ERROR: " + ex.Message);
                throw;
            }
        }
    }
}
